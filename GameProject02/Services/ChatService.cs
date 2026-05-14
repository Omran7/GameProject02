using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameProject02.Services
{
    public static class ChatService
    {
        private const string ProjectId = "gameproject02-4207f";
        private const string ApiKey = "AIzaSyCM61YoJzqt9x7lOndV2oBJGeoBtU9U_Uo";
        private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
        private static readonly HttpClient _client = new();

        public static string GetGangChannel()
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return null;

            // 1) Try to get the gang ID from the in‑memory GangObject
            string gangId = player.GangObject?.GangId;

            // 2) If not in memory, try the stored playerGangId
            if (string.IsNullOrEmpty(gangId))
                gangId = player.GangId;

            // 3) If we have a gangId but the playerGangId field is empty, save it now
            if (!string.IsNullOrEmpty(gangId) && string.IsNullOrEmpty(player.GangId))
            {
                player.GangId = gangId;
                // Save in background – this will fix the field for future logins
                _ = FirebaseService.SavePlayerAsync(player);
            }

            return string.IsNullOrEmpty(gangId) ? null : $"gang:{gangId}";
        }

        public static string GetPlaceChannel()
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return null;
            if (player.CrimeObject.IsInHospital) return "place:hospital";
            if (player.CrimeObject.IsInPrison) return "place:prison";
            if (player.CrimeObject.IsInPlane) return "place:plane";
            return null;
        }

        public static List<string> GetAvailableChannels()
        {
            var channels = new List<string> { "worldwide" };
            var gang = GetGangChannel();
            if (gang != null) channels.Add(gang);
            var place = GetPlaceChannel();
            if (place != null) channels.Add(place);
            return channels;
        }

        public static string GetChannelDisplayName(string channel) => channel switch
        {
            "worldwide" => "العالمية",
            "place:hospital" => "المستشفى",
            "place:prison" => "السجن",
            "place:plane" => "الطائرة",
            _ => channel.StartsWith("gang:") ? "العصابة" : channel
        };

        // ── Send a message ───────────────────────────────────────────
        public static async Task<bool> SendMessageAsync(string channel, string content)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null || string.IsNullOrWhiteSpace(content)) return false;

            var msg = new ChatMessage
            {
                PlayerId = player.PlayerId,
                PlayerName = player.Username,
                Content = content.Trim(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                FromSystem = false
            };

            try
            {
                // 1) Create the channel document (empty) if it doesn't exist
                var channelDocUrl = $"{BaseUrl}/chats/{Uri.EscapeDataString(channel)}?key={ApiKey}";
                var emptyDoc = new { fields = new { } };
                await _client.PatchAsync(channelDocUrl,
                    new StringContent(JsonSerializer.Serialize(emptyDoc), Encoding.UTF8, "application/json"));

                // 2) Write the message
                var msgUrl = $"{BaseUrl}/chats/{Uri.EscapeDataString(channel)}/messages/{Uri.EscapeDataString(msg.Id)}?key={ApiKey}";
                var payload = SerializeMessage(msg);
                var resp = await _client.PatchAsync(msgUrl,
                    new StringContent(payload, Encoding.UTF8, "application/json"));

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    MainThread.BeginInvokeOnMainThread(async () =>
                        await Application.Current!.MainPage!.DisplayAlert(
                            "Chat Save Error", $"Status: {resp.StatusCode}\n{err}", "OK"));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Chat] Send error: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Chat Exception", ex.Message, "OK"));
                return false;
            }
        }

        // ── Load messages ────────────────────────────────────────────
        public static async Task<List<ChatMessage>> GetMessagesAsync(string channel, int pageSize = 30)
        {
            try
            {
                var url = $"{BaseUrl}/chats/{Uri.EscapeDataString(channel)}/messages?key={ApiKey}&orderBy=timestamp desc&pageSize={pageSize}";
                var resp = await _client.GetAsync(url);

                if (resp.StatusCode == HttpStatusCode.NotFound)
                    return new List<ChatMessage>();

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    MainThread.BeginInvokeOnMainThread(async () =>
                        await Application.Current!.MainPage!.DisplayAlert(
                            "Chat Load Error", $"Status: {resp.StatusCode}\n{err}", "OK"));
                    return new List<ChatMessage>();
                }

                var json = await resp.Content.ReadAsStringAsync();
                var messages = ParseMessages(json);
                System.Diagnostics.Debug.WriteLine($"[CHAT] Loaded {messages.Count} messages from {channel}");
                messages.Reverse();
                return messages;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Chat] Load error: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Chat Exception", ex.Message, "OK"));
                return new List<ChatMessage>();
            }
        }

        private static string SerializeMessage(ChatMessage m)
        {
            var doc = new
            {
                fields = new Dictionary<string, object>
                {
                    { "playerId",   new { stringValue  = m.PlayerId } },
                    { "playerName", new { stringValue  = m.PlayerName } },
                    { "content",    new { stringValue  = m.Content } },
                    { "timestamp",  new { integerValue = m.Timestamp } },
                    { "fromSystem", new { booleanValue = m.FromSystem } },
                    { "fromAdmin",  new { booleanValue = m.FromAdmin } }
                }
            };
            return JsonSerializer.Serialize(doc);
        }

        private static List<ChatMessage> ParseMessages(string json)
        {
            var messages = new List<ChatMessage>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("documents", out var docs))
                    return messages;

                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("name", out var nameProp)) continue;
                    var id = nameProp.GetString()?.Split('/').Last() ?? "";
                    var f = d.GetProperty("fields");

                    messages.Add(new ChatMessage
                    {
                        Id = id,
                        PlayerId = ReadStr(f, "playerId"),
                        PlayerName = ReadStr(f, "playerName"),
                        Content = ReadStr(f, "content"),
                        Timestamp = ReadLong(f, "timestamp"),
                        FromSystem = ReadBool(f, "fromSystem"),
                        FromAdmin = ReadBool(f, "fromAdmin")
                    });
                }
            }
            catch { }
            return messages;
        }

        private static string ReadStr(JsonElement f, string key) =>
            f.TryGetProperty(key, out var p) && p.TryGetProperty("stringValue", out var v)
            ? v.GetString() ?? "" : "";

        private static long ReadLong(JsonElement f, string key) =>
            f.TryGetProperty(key, out var p) && p.TryGetProperty("integerValue", out var v)
            ? (v.ValueKind == JsonValueKind.String ? long.Parse(v.GetString()!) : v.GetInt64())
            : 0;

        private static bool ReadBool(JsonElement f, string key) =>
            f.TryGetProperty(key, out var p) && p.TryGetProperty("booleanValue", out var v)
            && v.GetBoolean();
    }
}