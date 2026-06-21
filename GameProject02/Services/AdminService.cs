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

public static class AdminService
{
    private const string ProjectId = "gameproject02-4207f";
    private const string ApiKey = "AIzaSyCM61YoJzqt9X7lOndV2oBJGeoBtU9U_Uo";
    private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
    private static readonly HttpClient _client = new();

    // ── Role Check ──────────────────────────────────────────────────────
    public static bool IsPlayerAdmin(PlayerAccount player) =>
        player?.IsAdmin == true || player?.IsTemporaryAdmin == true;

    public static bool IsPlayerManager(PlayerAccount player) =>
        player?.IsManager == true && !player.IsAdmin;

    // ── Public helper to get role string ──────────────────────────────
    public static string GetPlayerRole(PlayerAccount player)
    {
        if (player.IsAdmin) return "مسؤول";
        if (player.IsTemporaryAdmin) return "مسؤول مؤقت";
        if (player.IsManager) return "مدير";
        return "لاعب عادي";
    }

    // ── Get all players directly from Firestore (no caching) ──────────
    public static async Task<List<PlayerAccount>> GetAllPlayersFreshAsync()
    {
        var players = new List<PlayerAccount>();
        try
        {
            var url = $"{BaseUrl}/players?key={ApiKey}";
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[ADMIN] GetAllPlayersFresh failed: {response.StatusCode}");
                return players;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("documents", out var docs))
                return players;

            foreach (var d in docs.EnumerateArray())
            {
                if (!d.TryGetProperty("fields", out var fields)) continue;
                var playerId = d.TryGetProperty("name", out var nameProp)
                    ? nameProp.GetString()?.Split('/').Last()
                    : Guid.NewGuid().ToString();
                var player = FirebaseService.ParsePlayerFromFirestoreFields(fields, playerId);
                if (player != null)
                    players.Add(player);
            }
            Debug.WriteLine($"[ADMIN] Fresh load: {players.Count} players");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ADMIN] GetAllPlayersFresh error: {ex.Message}");
        }
        return players;
    }

    // ── Get All Admins and Managers ──────────────────────────────────
    public static async Task<List<PlayerAccount>> GetAllAdminsAndManagersAsync()
    {
        var all = await GetAllPlayersFreshAsync();
        return all.Where(p => p.IsAdmin || p.IsManager).ToList();
    }

    // ── Promote/Demote Manager ──────────────────────────────────────
    public static async Task<bool> PromoteToManagerAsync(string playerId)
    {
        try
        {
            var url = $"{BaseUrl}/players/{playerId}?key={ApiKey}&updateMask.fieldPaths=isManager";
            var fields = new { fields = new { isManager = new { booleanValue = true } } };
            var json = JsonSerializer.Serialize(fields);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public static async Task<bool> DemoteManagerAsync(string playerId)
    {
        try
        {
            var url = $"{BaseUrl}/players/{playerId}?key={ApiKey}&updateMask.fieldPaths=isManager";
            var fields = new { fields = new { isManager = new { booleanValue = false } } };
            var json = JsonSerializer.Serialize(fields);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── Promote to Admin ──────────────────────────────────────────────
    public static async Task<bool> PromoteToAdminAsync(string playerId)
    {
        try
        {
            var url = $"{BaseUrl}/players/{playerId}?key={ApiKey}&updateMask.fieldPaths=isAdmin,isTemporaryAdmin,hasAdminRequestNotReviewed";
            var fields = new
            {
                fields = new
                {
                    isAdmin = new { booleanValue = true },
                    isTemporaryAdmin = new { booleanValue = false },
                    hasAdminRequestNotReviewed = new { booleanValue = false }
                }
            };
            var json = JsonSerializer.Serialize(fields);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public static async Task<bool> RemoveAdminFlagAsync(string playerId)
    {
        try
        {
            var url = $"{BaseUrl}/players/{playerId}?key={ApiKey}&updateMask.fieldPaths=isAdmin,isTemporaryAdmin";
            var fields = new { fields = new { isAdmin = new { booleanValue = false }, isTemporaryAdmin = new { booleanValue = false } } };
            var json = JsonSerializer.Serialize(fields);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── Unified demotion ──────────────────────────────────────────────
    public static async Task<bool> DemotePlayerAsync(string playerId)
    {
        try
        {
            var all = await GetAllPlayersFreshAsync();
            var player = all.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null) return false;

            if (player.IsAdmin || player.IsTemporaryAdmin)
                return await RemoveAdminFlagAsync(playerId);
            else if (player.IsManager)
                return await DemoteManagerAsync(playerId);
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ADMIN] DemotePlayer error: {ex.Message}");
            return false;
        }
    }

    // ── Submit Promotion Request ─────────────────────────────────────
    public static async Task<bool> SubmitPromotionRequestAsync(PlayerAccount requester, string reason, string imageBase64 = "")
    {
        var request = new AdminRequest
        {
            Id = Guid.NewGuid().ToString(),
            PlayerId = requester.PlayerId,
            PlayerName = requester.Username,
            RequestType = AdminRequestType.AdminPromotion,
            Reason = reason,
            ImageBase64 = imageBase64,
            Timestamp = DateTime.UtcNow
        };

        var url = $"{BaseUrl}/adminRequests/{request.Id}?key={ApiKey}";
        var doc = new { fields = ConvertAdminRequestToFields(request) };
        var json = JsonSerializer.Serialize(doc);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync(url, content);
        return response.IsSuccessStatusCode;
    }

    // ── Submit Ban Request ────────────────────────────────────────────
    public static async Task<bool> SubmitBanRequestAsync(PlayerAccount requester, string targetPlayerId, string targetPlayerName, string reason, string imageBase64 = "")
    {
        var request = new AdminRequest
        {
            Id = Guid.NewGuid().ToString(),
            PlayerId = requester.PlayerId,
            PlayerName = requester.Username,
            RequestType = AdminRequestType.BanRequest,
            Reason = reason,
            TargetPlayerId = targetPlayerId,
            TargetPlayerName = targetPlayerName,
            ImageBase64 = imageBase64,
            Timestamp = DateTime.UtcNow
        };

        var url = $"{BaseUrl}/adminRequests/{request.Id}?key={ApiKey}";
        var doc = new { fields = ConvertAdminRequestToFields(request) };
        var json = JsonSerializer.Serialize(doc);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync(url, content);
        return response.IsSuccessStatusCode;
    }

    // ── Get Pending Requests ──────────────────────────────────────────
    public static async Task<List<AdminRequest>> GetPendingRequestsAsync()
    {
        var requests = new List<AdminRequest>();
        try
        {
            var url = $"{BaseUrl}/adminRequests?key={ApiKey}&orderBy=timestamp desc";
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return requests;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("documents", out var docs))
            {
                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("fields", out var fields)) continue;
                    // Get the document ID from the "name" field
                    string docId = null;
                    if (d.TryGetProperty("name", out var nameProp))
                    {
                        var fullName = nameProp.GetString();
                        docId = fullName?.Split('/').Last();
                    }
                    var req = ParseAdminRequest(fields);
                    if (req != null && !req.IsReviewed)
                    {
                        req.Id = docId ?? Guid.NewGuid().ToString(); // Set the ID
                        requests.Add(req);
                    }
                }
            }
        }
        catch (Exception ex) { Debug.WriteLine($"[ADMIN] GetPendingRequests error: {ex.Message}"); }
        return requests;
    }

    // ── Get All Requests (history) ──────────────────────────────────
    public static async Task<List<AdminRequest>> GetAllRequestsAsync()
    {
        var requests = new List<AdminRequest>();
        try
        {
            var url = $"{BaseUrl}/adminRequests?key={ApiKey}&orderBy=timestamp desc";
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return requests;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("documents", out var docs))
            {
                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("fields", out var fields)) continue;
                    string docId = null;
                    if (d.TryGetProperty("name", out var nameProp))
                    {
                        var fullName = nameProp.GetString();
                        docId = fullName?.Split('/').Last();
                    }
                    var req = ParseAdminRequest(fields);
                    if (req != null)
                    {
                        req.Id = docId ?? Guid.NewGuid().ToString();
                        requests.Add(req);
                    }
                }
            }
        }
        catch (Exception ex) { Debug.WriteLine($"[ADMIN] GetAllRequests error: {ex.Message}"); }
        return requests;
    }

    // ── Review Request ────────────────────────────────────────────────
    public static async Task<bool> ReviewAdminRequestAsync(
        string requestId,
        string reviewedByPlayerId,
        bool isApproved,
        string note = "",
        List<string> banTypes = null)
    {
        try
        {
            // 1. Load the request (optional, we could skip this if we trust the caller)
            var reqUrl = $"{BaseUrl}/adminRequests/{requestId}?key={ApiKey}";
            var getResp = await _client.GetAsync(reqUrl);
            if (!getResp.IsSuccessStatusCode)
            {
                var error = await getResp.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ADMIN] Load request failed: {getResp.StatusCode} - {error}");
                return false;
            }
            var json = await getResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("fields", out var fields)) return false;
            var request = ParseAdminRequest(fields);
            if (request == null) return false;

            // 2. Update request status
            var updateFields = new
            {
                fields = new
                {
                    isReviewed = new { booleanValue = true },
                    isApproved = new { booleanValue = isApproved },
                    reviewNote = new { stringValue = note ?? "" },
                    reviewedBy = new { stringValue = reviewedByPlayerId },
                    reviewedAt = new { timestampValue = DateTime.UtcNow.ToString("o") }
                }
            };
            var reqJson = JsonSerializer.Serialize(updateFields);
            var reqContent = new StringContent(reqJson, Encoding.UTF8, "application/json");
            var reqResponse = await _client.PatchAsync(reqUrl, reqContent);
            if (!reqResponse.IsSuccessStatusCode)
            {
                var error = await reqResponse.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ADMIN] Update request failed: {reqResponse.StatusCode} - {error}");
                return false;
            }

            // 3. If approved, perform the action
            if (isApproved)
            {
                if (request.RequestType == AdminRequestType.AdminPromotion)
                {
                    await PromoteToAdminAsync(request.PlayerId);
                }
                else if (request.RequestType == AdminRequestType.BanRequest && !string.IsNullOrEmpty(request.TargetPlayerId))
                {
                    if (banTypes == null || banTypes.Count == 0)
                        banTypes = new List<string> { "chat", "profile", "news", "messages" };
                    await ApplyBanFromRequestAsync(request.TargetPlayerId, banTypes);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ADMIN] ReviewAdminRequest error: {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> ApplyBanFromRequestAsync(string targetPlayerId, List<string> banTypes)
    {
        bool success = true;
        foreach (var banType in banTypes)
        {
            if (!await BanPlayerAsync(targetPlayerId, banType, true))
                success = false;
        }
        return success;
    }

    // ── Ban / Unban ────────────────────────────────────────────────────
    public static async Task<bool> BanPlayerAsync(string playerId, string banType, bool ban)
    {
        try
        {
            string field = banType switch
            {
                "chat" => "isBannedFromChat",
                "profile" => "isBannedFromChangeProfilePic",
                "news" => "isBannedFromNews",
                "messages" => "isBannedFromPrivateMessages",
                _ => throw new ArgumentException("Invalid ban type")
            };

            var url = $"{BaseUrl}/players/{playerId}?key={ApiKey}&updateMask.fieldPaths={field}";
            var updateFields = new
            {
                fields = new Dictionary<string, object> { { field, new { booleanValue = ban } } }
            };
            var json = JsonSerializer.Serialize(updateFields);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ADMIN] BanPlayer error: {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> UnbanPlayerAsync(string playerId, string banType)
        => await BanPlayerAsync(playerId, banType, false);

    public static async Task<bool> UnbanAllAsync(string playerId)
    {
        var types = new[] { "chat", "profile", "news", "messages" };
        bool success = true;
        foreach (var t in types)
        {
            if (!await UnbanPlayerAsync(playerId, t))
                success = false;
        }
        return success;
    }

    // ── Send System Announcement ──────────────────────────────────────
    public static async Task<(bool success, string message)> SendSystemAnnouncementAsync(string message, string adminName)
    {
        try
        {
            var id = Guid.NewGuid().ToString();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var url = $"{BaseUrl}/news?key={ApiKey}";

            var announcement = new
            {
                fields = new
                {
                    id = new { stringValue = id },
                    title = new { stringValue = "🛡️ إدارة النظام" },
                    content = new { stringValue = message },
                    author = new { stringValue = adminName },
                    type = new { integerValue = 3 },
                    timestamp = new { integerValue = now },
                    icon = new { stringValue = "🛡️" },
                    isRead = new { booleanValue = false }
                }
            };

            var json = JsonSerializer.Serialize(announcement);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
                return (true, "✅ تم نشر الإعلان بنجاح!");
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return (false, $"❌ فشل النشر: {errorBody}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"❌ استثناء: {ex.Message}");
        }
    }

    // ── Admin Menu Items ──────────────────────────────────────────────
    public static List<AdminMenuItem> GetAdminMenuItems(AdminPermission permissions)
    {
        var menu = new List<AdminMenuItem>
        {
            new AdminMenuItem { Title = "لوحة التحكم", Icon = "📊", Action = "Dashboard", RequiredPermission = 0, IsVisible = true },
            new AdminMenuItem { Title = "الطلبات المعلقة", Icon = "📨", Action = "PendingRequests", RequiredPermission = AdminPermission.ReviewAdminRequests, IsVisible = permissions.HasFlag(AdminPermission.ReviewAdminRequests) },
            new AdminMenuItem { Title = "إدارة اللاعبين", Icon = "👥", Action = "ManagePlayers", RequiredPermission = 0, IsVisible = true },
            new AdminMenuItem { Title = "إعلان نظام", Icon = "📢", Action = "SendAnnouncement", RequiredPermission = AdminPermission.SendSystemAnnouncement, IsVisible = permissions.HasFlag(AdminPermission.SendSystemAnnouncement) },
            new AdminMenuItem { Title = "سجل الإجراءات", Icon = "📜", Action = "ActionLog", RequiredPermission = 0, IsVisible = true },
            new AdminMenuItem { Title = "إدارة المسؤولين", Icon = "⚙️", Action = "ManageAdmins", RequiredPermission = AdminPermission.ManageOtherAdmins, IsVisible = permissions.HasFlag(AdminPermission.ManageOtherAdmins) }
        };
        return menu.Where(m => m.IsVisible).ToList();
    }

    // ── Serialization helpers ────────────────────────────────────────
    private static Dictionary<string, object> ConvertAdminRequestToFields(AdminRequest r) => new()
    {
        ["playerId"] = new { stringValue = r.PlayerId },
        ["playerName"] = new { stringValue = r.PlayerName },
        ["requestType"] = new { integerValue = (int)r.RequestType },
        ["reason"] = new { stringValue = r.Reason },
        ["targetPlayerId"] = new { stringValue = r.TargetPlayerId ?? "" },
        ["targetPlayerName"] = new { stringValue = r.TargetPlayerName ?? "" },
        ["imageBase64"] = new { stringValue = r.ImageBase64 ?? "" },
        ["timestamp"] = new { timestampValue = r.Timestamp.ToString("o") },
        ["isReviewed"] = new { booleanValue = r.IsReviewed },
        ["isApproved"] = new { booleanValue = r.IsApproved },
        ["reviewNote"] = new { stringValue = r.ReviewNote ?? "" },
        ["reviewedBy"] = new { stringValue = r.ReviewedBy ?? "" },
        ["reviewedAt"] = r.ReviewedAt.HasValue
            ? new { timestampValue = r.ReviewedAt.Value.ToString("o") }
            : new { nullValue = (object)null }
    };

    private static AdminRequest ParseAdminRequest(JsonElement fields)
    {
        try
        {
            return new AdminRequest
            {
                // Id is set separately from the document name
                PlayerId = GetStr(fields, "playerId"),
                PlayerName = GetStr(fields, "playerName"),
                RequestType = (AdminRequestType)GetInt(fields, "requestType"),
                Reason = GetStr(fields, "reason"),
                TargetPlayerId = GetStr(fields, "targetPlayerId"),
                TargetPlayerName = GetStr(fields, "targetPlayerName"),
                ImageBase64 = GetStr(fields, "imageBase64"),
                Timestamp = GetTime(fields, "timestamp"),
                IsReviewed = GetBool(fields, "isReviewed"),
                IsApproved = GetBool(fields, "isApproved"),
                ReviewNote = GetStr(fields, "reviewNote"),
                ReviewedBy = GetStr(fields, "reviewedBy"),
                ReviewedAt = GetNullableTime(fields, "reviewedAt")
            };
        }
        catch { return null; }
    }

    private static string GetStr(JsonElement f, string key, string fallback = "") =>
        f.TryGetProperty(key, out var p) && p.TryGetProperty("stringValue", out var v) ? v.GetString() ?? fallback : fallback;

    private static int GetInt(JsonElement f, string key)
    {
        if (f.TryGetProperty(key, out var p) && p.TryGetProperty("integerValue", out var v))
            return v.ValueKind == JsonValueKind.String ? int.Parse(v.GetString()) : v.GetInt32();
        return 0;
    }

    private static bool GetBool(JsonElement f, string key)
    {
        if (f.TryGetProperty(key, out var p) && p.TryGetProperty("booleanValue", out var v))
            return v.GetBoolean();
        return false;
    }

    private static DateTime GetTime(JsonElement f, string key)
    {
        if (f.TryGetProperty(key, out var p) && p.TryGetProperty("timestampValue", out var v) &&
            DateTime.TryParse(v.GetString(), out var dt))
            return dt;
        return DateTime.UtcNow;
    }

    private static DateTime? GetNullableTime(JsonElement f, string key)
    {
        if (f.TryGetProperty(key, out var p) && p.TryGetProperty("timestampValue", out var v) &&
            DateTime.TryParse(v.GetString(), out var dt))
            return dt;
        return null;
    }
}