using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameProject02.Services;

public static class NewsService
{
    private const string ProjectId = "gameproject02-4207f";
    private const string ApiKey = "AIzaSyCM61YoJzqt9X7lOndV2oBJGeoBtU9U_Uo";
    private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
    private static readonly HttpClient _client = new();

    // ── Get ALL news documents (then filter by type) ────────────
    private static async Task<List<NewsItem>> GetAllNewsItemsAsync()
    {
        try
        {
            // ✅ Correct URL: pageSize and orderBy with encoded space
            var url = $"{BaseUrl}/news?key={ApiKey}&pageSize=100&orderBy=timestamp%20desc";
            Debug.WriteLine($"[NEWS] Request URL: {url}");

            var resp = await _client.GetAsync(url);
            var content = await resp.Content.ReadAsStringAsync();

            Debug.WriteLine($"[NEWS] Response status: {resp.StatusCode}");
            Debug.WriteLine($"[NEWS] Raw response: {content}");

            if (!resp.IsSuccessStatusCode) return new List<NewsItem>();

            return ParseNewsItems(content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NEWS] GetAllNewsItems error: {ex.Message}");
            return new List<NewsItem>();
        }
    }

    // ── Get player ads (type = 1) ──────────────────────────────
    public static async Task<List<NewsItem>> GetPlayerAdsAsync()
    {
        var all = await GetAllNewsItemsAsync();
        return all.Where(n => n.Type == NewsType.PlayerAd).ToList();
    }

    // ── Get system ads (type = 3) ──────────────────────────────
    public static async Task<List<NewsItem>> GetSystemAdsAsync()
    {
        var all = await GetAllNewsItemsAsync();
        return all.Where(n => n.Type == NewsType.SystemAlert).ToList();
    }

    // ── Get top bounty (latest type = 2) ──────────────────────
    public static async Task<NewsItem> GetLatestBountyAsync()
    {
        var all = await GetAllNewsItemsAsync();
        return all.Where(n => n.Type == NewsType.Bounty)
                  .OrderByDescending(n => n.Timestamp)
                  .FirstOrDefault();
    }

    // ── Publish a player ad ────────────────────────────────────
    public static async Task<(bool success, string message)> PublishAdAsync(PlayerAccount player, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return (false, "الرسالة لا يمكن أن تكون فارغة.");
        if (content.Length > 50)
            return (false, "الرسالة يجب أن تكون 50 حرفاً أو أقل.");
        if (player.Gold < 20000)
            return (false, "تحتاج 20,000 ذهب لنشر إعلان.");

        player.Gold -= 20000;
        _ = FirebaseService.SavePlayerAsync(player);

        var ad = new NewsItem
        {
            Id = Guid.NewGuid().ToString(),
            Author = player.Username,
            Content = content,
            Date = DateTime.UtcNow,
            Type = NewsType.PlayerAd,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Icon = "📢"
        };

        try
        {
            var url = $"{BaseUrl}/news/{ad.Id}?key={ApiKey}";
            var json = SerializeNewsItem(ad);
            var resp = await _client.PatchAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine($"[News] Publish error: {err}");
                return (false, "فشل النشر. حاول مرة أخرى.");
            }
            return (true, "تم نشر إعلانك!");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[News] Publish error: {ex.Message}");
            return (false, "خطأ في الشبكة. حاول مرة أخرى.");
        }
    }

    // ── Serialize NewsItem to Firestore format ─────────────────
    private static string SerializeNewsItem(NewsItem n)
    {
        var doc = new
        {
            fields = new Dictionary<string, object>
            {
                { "id", new { stringValue = n.Id } },
                { "author", new { stringValue = n.Author ?? "" } },
                { "content", new { stringValue = n.Content ?? "" } },
                { "title", new { stringValue = n.Title ?? "" } },
                { "type", new { integerValue = (int)n.Type } },
                { "timestamp", new { integerValue = n.Timestamp } },
                { "icon", new { stringValue = n.Icon ?? "📡" } },
                { "date", new { timestampValue = n.Date.ToString("o") } },
                // Bounty fields
                { "bountyPlayerName", new { stringValue = n.BountyPlayerName ?? "" } },
                { "bountyLevel", new { integerValue = n.BountyLevel } },
                { "bountyGender", new { integerValue = n.BountyGender } },
                { "bountyIsVIP", new { booleanValue = n.BountyIsVIP } },
                { "bountyIsOnline", new { booleanValue = n.BountyIsOnline } },
                { "bountyPlayerId", new { stringValue = n.BountyPlayerId ?? "" } },
                { "bountyProfilePic", new { stringValue = n.BountyProfilePic ?? "" } },
                { "bountyCost", new { integerValue = n.BountyCost } },
                { "bountyPlace", new { stringValue = n.BountyPlace ?? "" } },
                { "bountyDescription", new { stringValue = n.BountyDescription ?? "" } }
            }
        };
        return JsonSerializer.Serialize(doc);
    }

    // ── Parse Firestore documents to NewsItem list ─────────────
    private static List<NewsItem> ParseNewsItems(string json)
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
                var id = nameProp.GetString()?.Split('/').Last() ?? Guid.NewGuid().ToString();
                if (!d.TryGetProperty("fields", out var fields)) continue;

                var item = new NewsItem
                {
                    Id = id,
                    Author = ReadStr(fields, "author"),
                    Content = ReadStr(fields, "content"),
                    Title = ReadStr(fields, "title"),
                    Icon = ReadStr(fields, "icon", "📡"),
                    Timestamp = ReadLong(fields, "timestamp"),
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(ReadLong(fields, "timestamp")).DateTime,
                    Type = (NewsType)ReadInt(fields, "type", (int)NewsType.Information),
                    BountyPlayerName = ReadStr(fields, "bountyPlayerName"),
                    BountyLevel = (int)ReadLong(fields, "bountyLevel"),
                    BountyGender = (int)ReadLong(fields, "bountyGender"),
                    BountyIsVIP = ReadBool(fields, "bountyIsVIP"),
                    BountyIsOnline = ReadBool(fields, "bountyIsOnline"),
                    BountyPlayerId = ReadStr(fields, "bountyPlayerId"),
                    BountyProfilePic = ReadStr(fields, "bountyProfilePic"),
                    BountyCost = ReadLong(fields, "bountyCost"),
                    BountyPlace = ReadStr(fields, "bountyPlace"),
                    BountyDescription = ReadStr(fields, "bountyDescription")
                };

                if (string.IsNullOrEmpty(item.Title) && !string.IsNullOrEmpty(item.Author))
                    item.Title = item.Author;

                items.Add(item);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[News] Parse error: {ex.Message}");
        }
        return items;
    }

    // ── Helper: Read string from Firestore fields ──────────────
    private static string ReadStr(JsonElement fields, string key, string fallback = "")
    {
        if (fields.TryGetProperty(key, out var prop) && prop.TryGetProperty("stringValue", out var v))
            return v.GetString() ?? fallback;
        return fallback;
    }

    // ── Helper: Read long from Firestore fields ────────────────
    private static long ReadLong(JsonElement fields, string key)
    {
        if (fields.TryGetProperty(key, out var prop) && prop.TryGetProperty("integerValue", out var v))
        {
            if (v.ValueKind == JsonValueKind.String && long.TryParse(v.GetString(), out var l))
                return l;
            if (v.ValueKind == JsonValueKind.Number)
                return v.GetInt64();
        }
        return 0;
    }

    // ── Helper: Read int from Firestore fields ─────────────────
    private static int ReadInt(JsonElement fields, string key, int fallback = 0)
    {
        if (fields.TryGetProperty(key, out var prop) && prop.TryGetProperty("integerValue", out var v))
        {
            if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var i))
                return i;
            if (v.ValueKind == JsonValueKind.Number)
                return v.GetInt32();
        }
        return fallback;
    }

    // ── Helper: Read bool from Firestore fields ────────────────
    private static bool ReadBool(JsonElement fields, string key)
    {
        if (fields.TryGetProperty(key, out var prop) && prop.TryGetProperty("booleanValue", out var v))
            return v.GetBoolean();
        return false;
    }
}