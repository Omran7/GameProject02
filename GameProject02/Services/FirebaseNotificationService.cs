using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameProject02.Services
{
    public static class FirestoreNotificationService
    {
        private const string ProjectId = "gameproject02-4207f";
        private const string ApiKey = "AIzaSyCM61YoJzqt9x7lOndV2oBJGeoBtU9U_Uo";
        private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
        private static readonly HttpClient _client = new HttpClient();

        // ── Save a notification to Firestore ──────────────────────────
        public static async Task SaveNotificationAsync(string playerId, NotificationItem notification)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                System.Diagnostics.Debug.WriteLine("[SAVE NOTIF] Error: playerId is null");
                return;
            }

            try
            {
                var url = $"{BaseUrl}/players/{playerId}/notifications/{notification.Id}?key={ApiKey}";
                var json = ToFirestoreJson(notification);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PatchAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[SAVE NOTIF] {response.StatusCode}: {errorBody}");

                    // TEMPORARY ALERT: shows the error on the phone
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (Application.Current?.MainPage != null)
                            await Application.Current.MainPage.DisplayAlert(
                                "Save Notification Failed",
                                $"Status: {response.StatusCode}\n{errorBody}",
                                "OK");
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SAVE NOTIF] Exception: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Save Notification Error", ex.Message, "OK");
                });
            }
        }

        // ── Load notifications (last 3 days) ─────────────────────────
        public static async Task<List<NotificationItem>> GetNotificationsAsync(string playerId, bool cleanExpired = true)
        {
            try
            {
                var url = $"{BaseUrl}/players/{playerId}/notifications?key={ApiKey}&orderBy=Timestamp desc";
                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<NotificationItem>();

                var json = await response.Content.ReadAsStringAsync();
                var allItems = ParseFirestoreArray(json);

                var cutoff = DateTime.UtcNow.AddDays(-3);
                var recentItems = allItems.Where(n => n.Timestamp >= cutoff).ToList();

                if (cleanExpired)
                    _ = DeleteExpiredNotificationsAsync(playerId);

                return recentItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FIRESTORE NOTIF] Load failed: {ex.Message}");
                return new List<NotificationItem>();
            }
        }

        // ── Delete older than 3 days ─────────────────────────────────
        public static async Task DeleteExpiredNotificationsAsync(string playerId)
        {
            try
            {
                var url = $"{BaseUrl}/players/{playerId}/notifications?key={ApiKey}&orderBy=Timestamp desc";
                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                var allNotifications = ParseFirestoreArray(json);
                var cutoff = DateTime.UtcNow.AddDays(-3);

                foreach (var n in allNotifications)
                {
                    if (n.Timestamp < cutoff)
                    {
                        var deleteUrl = $"{BaseUrl}/players/{playerId}/notifications/{n.Id}?key={ApiKey}";
                        await _client.DeleteAsync(deleteUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FIRESTORE NOTIF] Cleanup failed: {ex.Message}");
            }
        }

        // ── Mark single read ─────────────────────────────────────────
        public static async Task MarkAsReadAsync(string playerId, string notificationId)
        {
            try
            {
                var url = $"{BaseUrl}/players/{playerId}/notifications/{notificationId}?key={ApiKey}&updateMask.fieldPaths=IsRead";
                var json = "{\"fields\":{\"IsRead\":{\"booleanValue\":true}}}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _client.PatchAsync(url, content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FIRESTORE NOTIF] Mark read failed: {ex.Message}");
            }
        }

        // ── Mark all read ────────────────────────────────────────────
        public static async Task MarkAllReadAsync(string playerId)
        {
            try
            {
                var notifications = await GetNotificationsAsync(playerId, cleanExpired: false);
                foreach (var n in notifications.Where(n => !n.IsRead))
                    await MarkAsReadAsync(playerId, n.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FIRESTORE NOTIF] Mark all read failed: {ex.Message}");
            }
        }

        // ── Serialization ────────────────────────────────────────────
        private static string ToFirestoreJson(NotificationItem n)
        {
            var doc = new
            {
                fields = new
                {
                    Id = new { stringValue = n.Id },
                    Title = new { stringValue = n.Title },
                    Message = new { stringValue = n.Message },
                    Timestamp = new { timestampValue = n.Timestamp.ToString("o") },
                    Category = new { integerValue = (int)n.Category },
                    Priority = new { integerValue = (int)n.Priority },
                    Icon = new { stringValue = n.Icon },
                    IsRead = new { booleanValue = n.IsRead },
                    ActionTarget = new { stringValue = n.ActionTarget },
                    PlayerId = new { stringValue = n.PlayerId }
                }
            };
            return JsonSerializer.Serialize(doc);
        }

        private static List<NotificationItem> ParseFirestoreArray(string json)
        {
            var items = new List<NotificationItem>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("documents", out var docs)) return items;

                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("name", out var nameProp)) continue;
                    var id = nameProp.GetString()?.Split('/').Last() ?? Guid.NewGuid().ToString();
                    var fields = d.GetProperty("fields");

                    items.Add(new NotificationItem
                    {
                        Id = id,
                        Title = GetStr(fields, "Title"),
                        Message = GetStr(fields, "Message"),
                        Timestamp = DateTime.TryParse(GetStr(fields, "Timestamp"), out var ts) ? ts : DateTime.UtcNow,
                        Category = (NotificationCategory)GetInt(fields, "Category", 0),
                        Priority = (GameNotificationPriority)GetInt(fields, "Priority", 1),
                        Icon = GetStr(fields, "Icon", "🔔"),
                        IsRead = GetBool(fields, "IsRead", false),
                        ActionTarget = GetStr(fields, "ActionTarget"),
                        PlayerId = GetStr(fields, "PlayerId")
                    });
                }
            }
            catch { }
            return items;
        }

        private static string GetStr(JsonElement fields, string key, string fallback = "") =>
            fields.TryGetProperty(key, out var p) && p.TryGetProperty("stringValue", out var v) ? v.GetString() ?? fallback : fallback;

        private static int GetInt(JsonElement fields, string key, int fallback = 0) =>
            fields.TryGetProperty(key, out var p) && p.TryGetProperty("integerValue", out var v) ? v.GetInt32() : fallback;

        private static bool GetBool(JsonElement fields, string key, bool fallback = false) =>
            fields.TryGetProperty(key, out var p) && p.TryGetProperty("booleanValue", out var v) ? v.GetBoolean() : fallback;
    }
}