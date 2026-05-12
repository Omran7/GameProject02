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
    public static class NewsService
    {
        private const string ProjectId = "gameproject02-4207f";
        private const string ApiKey = "AIzaSyCM61YoJzqt9X7lOndV2oBJGeoBtU9U_Uo";
        private const string BaseUrl =
            $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
        private static readonly HttpClient _client = new();

        // ── Get player ads (latest 15) ──────────────────────────────
        public static async Task<List<NewsItem>> GetPlayerAdsAsync()
        {
            try
            {
                var url = $"{BaseUrl}/news/playerAds?key={ApiKey}&orderBy=timestamp desc&limit=15";
                var resp = await _client.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return new List<NewsItem>();
                var json = await resp.Content.ReadAsStringAsync();
                return ParseNewsItems(json, NewsType.PlayerAd);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[News] Load ads error: {ex.Message}");
                return new List<NewsItem>();
            }
        }

        // ── Get top bounty (latest 1) ──────────────────────────────
        public static async Task<NewsItem> GetLatestBountyAsync()
        {
            try
            {
                var url = $"{BaseUrl}/news/bounties?key={ApiKey}&orderBy=timestamp desc&limit=1";
                var resp = await _client.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return null;
                var json = await resp.Content.ReadAsStringAsync();
                var items = ParseNewsItems(json, NewsType.Bounty);
                return items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[News] Load bounty error: {ex.Message}");
                return null;
            }
        }

        // ── Get system ads (latest 5) ──────────────────────────────
        public static async Task<List<NewsItem>> GetSystemAdsAsync()
        {
            try
            {
                var url = $"{BaseUrl}/news/systemAds?key={ApiKey}&orderBy=timestamp desc&limit=5";
                var resp = await _client.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return new List<NewsItem>();
                var json = await resp.Content.ReadAsStringAsync();
                return ParseNewsItems(json, NewsType.SystemAlert);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[News] Load system error: {ex.Message}");
                return new List<NewsItem>();
            }
        }

        // ── Publish a player ad ────────────────────────────────────
        public static async Task<(bool success, string message)> PublishAdAsync(
            PlayerAccount player, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return (false, "Message cannot be empty.");
            if (content.Length > 50)
                return (false, "Message must be 50 characters or fewer.");
            if (player.Gold < 20000)
                return (false, "You need 20,000 gold to publish an ad.");

            player.Gold -= 20000;
            _ = FirebaseService.SavePlayerAsync(player);

            var ad = new NewsItem
            {
                Id = Guid.NewGuid().ToString(),
                Author = player.Username,
                Content = content,
                Date = DateTime.UtcNow,
                Type = NewsType.PlayerAd,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            try
            {
                var url = $"{BaseUrl}/news/playerAds/{ad.Id}?key={ApiKey}";
                var json = SerializeNewsItem(ad);
                var resp = await _client.PatchAsync(url,
                    new StringContent(json, Encoding.UTF8, "application/json"));
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[News] Publish error: {err}");
                    return (false, "Failed to publish. Try again.");
                }
                return (true, "Your ad has been broadcast city‑wide!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[News] Publish error: {ex.Message}");
                return (false, "Network error. Try again.");
            }
        }

        // ── Helpers ────────────────────────────────────────────────
        private static string SerializeNewsItem(NewsItem n)
        {
            var doc = new
            {
                fields = new Dictionary<string, object>
                {
                    { "id", new { stringValue = n.Id } },
                    { "author", new { stringValue = n.Author } },
                    { "content", new { stringValue = n.Content } },
                    { "timestamp", new { integerValue = n.Timestamp } },
                    { "type", new { stringValue = n.Type.ToString() } },
                    { "title", new { stringValue = n.Title } },
                    { "playerName", new { stringValue = n.BountyPlayerName } },
                    { "level", new { integerValue = n.BountyLevel } },
                    { "gender", new { integerValue = n.BountyGender } },
                    { "isVIP", new { booleanValue = n.BountyIsVIP } },
                    { "isOnline", new { booleanValue = n.BountyIsOnline } },
                    { "playerId", new { stringValue = n.BountyPlayerId } },
                    { "profilePic", new { stringValue = n.BountyProfilePic } },
                    { "cost", new { integerValue = n.BountyCost } },
                    { "place", new { stringValue = n.BountyPlace } },
                    { "description", new { stringValue = n.BountyDescription } }
                }
            };
            return JsonSerializer.Serialize(doc);
        }

        private static List<NewsItem> ParseNewsItems(string json, NewsType fallbackType)
        {
            var items = new List<NewsItem>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("documents", out var docs))
                    return items;

                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("name", out var nameProp)) continue;
                    var id = nameProp.GetString()?.Split('/').Last()
                             ?? Guid.NewGuid().ToString();
                    var f = d.GetProperty("fields");

                    var item = new NewsItem
                    {
                        Id = id,
                        Author = ReadStr(f, "author"),
                        Content = ReadStr(f, "content"),
                        Title = ReadStr(f, "title"),
                        Timestamp = ReadLong(f, "timestamp"),
                        Date = DateTimeOffset
                            .FromUnixTimeMilliseconds(ReadLong(f, "timestamp"))
                            .DateTime,
                        Type = fallbackType,
                        BountyPlayerName = ReadStr(f, "playerName"),
                        BountyLevel = (int)ReadLong(f, "level"),
                        BountyGender = (int)ReadLong(f, "gender"),
                        BountyIsVIP = ReadBool(f, "isVIP"),
                        BountyIsOnline = ReadBool(f, "isOnline"),
                        BountyPlayerId = ReadStr(f, "playerId"),
                        BountyProfilePic = ReadStr(f, "profilePic"),
                        BountyCost = ReadLong(f, "cost"),
                        BountyPlace = ReadStr(f, "place"),
                        BountyDescription = ReadStr(f, "description")
                    };

                    if (string.IsNullOrEmpty(item.Title) && !string.IsNullOrEmpty(item.Author))
                        item.Title = item.Author;

                    items.Add(item);
                }
            }
            catch { }
            return items;
        }

        private static string ReadStr(JsonElement f, string k, string fallback = "")
            => f.TryGetProperty(k, out var p)
            && p.TryGetProperty("stringValue", out var v)
            ? v.GetString() ?? fallback : fallback;

        private static long ReadLong(JsonElement f, string k)
            => f.TryGetProperty(k, out var p)
            && p.TryGetProperty("integerValue", out var v)
            ? (v.ValueKind == JsonValueKind.String
                ? long.TryParse(v.GetString(), out var l) ? l : 0
                : v.GetInt64())
            : 0;

        private static bool ReadBool(JsonElement f, string k)
            => f.TryGetProperty(k, out var p)
            && p.TryGetProperty("booleanValue", out var v)
            && v.GetBoolean();
    }
}