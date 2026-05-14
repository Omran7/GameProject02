using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameProject02.Services
{
    public static class GangDatabaseService
    {
        private const string ProjectId = "gameproject02-4207f";
        private const string ApiKey = "AIzaSyCM61YoJzqt9x7lOndV2oBJGeoBtU9U_Uo";
        private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
        private static readonly HttpClient _client = new();

        private static List<GangObject> _allGangsCache = null;
        private static DateTime _cacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

        #region Gang CRUD

        public static async Task<List<GangObject>> GetAllGangsAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _allGangsCache != null && DateTime.Now - _cacheTime < CacheDuration)
                return _allGangsCache;
            try
            {
                var url = $"{BaseUrl}/gangs?key={ApiKey}&pageSize=500";
                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<GangObject>();
                var json = await response.Content.ReadAsStringAsync();
                _allGangsCache = ParseGangs(json);
                _cacheTime = DateTime.Now;
                return _allGangsCache;
            }
            catch (Exception ex) { Debug.WriteLine($"[GANG DB] GetAll error: {ex.Message}"); return new List<GangObject>(); }
        }

        public static async Task<List<GangObject>> SearchGangsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<GangObject>();
            var all = await GetAllGangsAsync();
            string q = query.Trim().ToLower();
            return all.Where(g => g.Name.ToLower().Contains(q) || g.Tag.ToLower().Contains(q) || g.GangId.ToLower().Contains(q)).ToList();
        }

        public static async Task<bool> SaveGangAsync(GangObject gang)
        {
            if (gang == null) return false;
            try
            {
                var url = $"{BaseUrl}/gangs/{gang.GangId}?key={ApiKey}";
                var fields = GangToFirestoreFields(gang);
                var doc = new { fields };
                var json = JsonSerializer.Serialize(doc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PatchAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    _allGangsCache = null;
                    Debug.WriteLine($"[SAVE] Gang {gang.GangId} saved, members: {gang.MembersWithPositions?.Count ?? 0}");
                    return true;
                }
                return false;
            }
            catch (Exception ex) { Debug.WriteLine($"[SAVE] Error: {ex.Message}"); return false; }
        }

        public static async Task<GangObject> GetGangAsync(string gangId)
        {
            if (string.IsNullOrEmpty(gangId)) return null;
            try
            {
                var url = $"{BaseUrl}/gangs/{gangId}?key={ApiKey}";
                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("fields", out var fields)) return null;
                return ParseGangFromFields(fields, gangId);
            }
            catch (Exception ex) { Debug.WriteLine($"[GANG DB] Get error: {ex.Message}"); return null; }
        }

        public static async void DeleteGang(string gangId)
        {
            if (string.IsNullOrEmpty(gangId)) return;
            try { var url = $"{BaseUrl}/gangs/{gangId}?key={ApiKey}"; await _client.DeleteAsync(url); _allGangsCache = null; }
            catch (Exception ex) { Debug.WriteLine($"[GANG DB] Delete error: {ex.Message}"); }
        }

        #endregion

        #region Join Requests

        public static async Task<bool> SendJoinRequestAsync(string gangId, string playerId, string playerName)
        {
            if (string.IsNullOrEmpty(gangId)) throw new ArgumentException("gangId");
            if (string.IsNullOrEmpty(playerId)) throw new ArgumentException("playerId");
            try
            {
                var existing = await GetJoinRequestsAsync(gangId);
                if (existing.Any(r => r.PlayerId == playerId)) return false;
                var requestId = $"{gangId}_{playerId}";
                var url = $"{BaseUrl}/joinRequests/{requestId}?key={ApiKey}";
                var fields = new Dictionary<string, object>
                {
                    { "gangId", StringValue(gangId) },
                    { "playerId", StringValue(playerId) },
                    { "playerName", StringValue(playerName ?? "") },
                    { "timestamp", IntegerValue(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) }
                };
                var doc = new { fields };
                var json = JsonSerializer.Serialize(doc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PatchAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Debug.WriteLine($"[JOIN REQ] Send exception: {ex.Message}"); return false; }
        }

        public static async Task<List<GangJoinRequest>> GetJoinRequestsAsync(string gangId)
        {
            if (string.IsNullOrEmpty(gangId)) return new List<GangJoinRequest>();
            try
            {
                var url = $"{BaseUrl}:runQuery?key={ApiKey}";
                var query = new
                {
                    structuredQuery = new
                    {
                        from = new[] { new { collectionId = "joinRequests" } },
                        where = new
                        {
                            fieldFilter = new
                            {
                                field = new { fieldPath = "gangId" },
                                op = "EQUAL",
                                value = new { stringValue = gangId }
                            }
                        }
                    }
                };
                var json = JsonSerializer.Serialize(query);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode) return new List<GangJoinRequest>();
                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var requests = new List<GangJoinRequest>();
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.TryGetProperty("document", out var document) &&
                        document.TryGetProperty("fields", out var fields))
                    {
                        requests.Add(new GangJoinRequest
                        {
                            GangId = ReadStr(fields, "gangId"),
                            PlayerId = ReadStr(fields, "playerId"),
                            PlayerName = ReadStr(fields, "playerName"),
                            Timestamp = ReadLong(fields, "timestamp")
                        });
                    }
                }
                return requests;
            }
            catch (Exception ex) { Debug.WriteLine($"[JOIN REQ] Get exception: {ex.Message}"); return new List<GangJoinRequest>(); }
        }

        public static List<GangJoinRequest> GetJoinRequests(string gangId) =>
            Task.Run(() => GetJoinRequestsAsync(gangId)).GetAwaiter().GetResult();

        public static async Task<bool> ProcessJoinRequestAsync(string gangId, string playerId, bool accept)
        {
            if (string.IsNullOrEmpty(gangId)) throw new ArgumentException(nameof(gangId));
            if (string.IsNullOrEmpty(playerId)) throw new ArgumentException(nameof(playerId));
            if (accept)
            {
                bool added = await AddMemberToGangAsync(gangId, playerId);
                if (!added) return false;
            }
            var requestId = $"{gangId}_{playerId}";
            var url = $"{BaseUrl}/joinRequests/{requestId}?key={ApiKey}";
            var response = await _client.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        public static bool ProcessJoinRequest(string gangId, string playerId, bool accept) =>
            Task.Run(() => ProcessJoinRequestAsync(gangId, playerId, accept)).GetAwaiter().GetResult();

        #endregion

        #region Member management

        private static async Task<bool> AddMemberToGangAsync(string gangId, string playerId, GangPosition position = GangPosition.Member)
        {
            if (string.IsNullOrEmpty(gangId)) throw new ArgumentException(nameof(gangId));
            if (string.IsNullOrEmpty(playerId)) throw new ArgumentException(nameof(playerId));
            try
            {
                var gang = await GetGangAsync(gangId);
                if (gang == null) { Debug.WriteLine($"[ADD MEMBER] Gang {gangId} not found"); return false; }
                if (gang.MembersWithPositions.ContainsKey(playerId)) return false;

                gang.MembersWithPositions[playerId] = position;
                await SaveGangAsync(gang);

                var player = await FirebaseService.LoadPlayerAsync(playerId);
                if (player == null) { Debug.WriteLine($"[ADD MEMBER] Player {playerId} not found"); return false; }

                player.GangId = gangId;
                player.GangObject = gang;
                await FirebaseService.SavePlayerAsync(player);
                return true;
            }
            catch (Exception ex) { Debug.WriteLine($"[ADD MEMBER] Exception: {ex.Message}"); return false; }
        }

        public static void UpdateMemberPosition(string gangId, string playerId, GangPosition? newPosition)
        {
            Debug.WriteLine($"[GANG DB] UpdateMemberPosition: {gangId}, {playerId}, {newPosition}");
        }

        #endregion

        #region Militia & Skills

        // ✅ Converted to async to avoid deadlock
        public static async Task<JoinGangMilitiaResultObject> JoinMilitiaAsync(
            string playerId,
            GangObject gang,
            int unitId,
            int requiredCourage,
            int respectReward)
        {
            // 1. Initial Validation
            if (string.IsNullOrWhiteSpace(playerId))
                return new JoinGangMilitiaResultObject { HasError = true, ErrorMessage = "معرف اللاعب غير صالح" };

            if (gang == null)
                return new JoinGangMilitiaResultObject { HasError = true, ErrorMessage = "بيانات العصابة غير متوفرة" };

            if (gang.MilitiaMembersByUnit == null)
                gang.MilitiaMembersByUnit = new Dictionary<int, List<string>>();

            // 2. Fetch a fresh player object directly from the database
            // This ensures we have the GangId and other critical data before saving
            var player = await FirebaseService.LoadPlayerAsync(playerId);
            if (player == null)
                return new JoinGangMilitiaResultObject { HasError = true, ErrorMessage = "اللاعب غير موجود" };

            // 3. Logic Checks (Courage and Capacity)
            if (player.Courage < requiredCourage)
                return new JoinGangMilitiaResultObject { HasError = true, ErrorMessage = $"تحتاج {requiredCourage} شجاعة للانضمام" };

            if (!gang.MilitiaMembersByUnit.ContainsKey(unitId))
                gang.MilitiaMembersByUnit[unitId] = new List<string>();

            var members = gang.MilitiaMembersByUnit[unitId];
            int maxMembers = unitId * 10;
            if (members.Count >= maxMembers)
                return new JoinGangMilitiaResultObject { HasError = true, ErrorMessage = "الوحدة ممتلئة" };

            if (gang.MilitiaMembersByUnit.Any(kvp => kvp.Value.Contains(playerId)))
                return new JoinGangMilitiaResultObject { HasError = true, ErrorMessage = "أنت بالفعل عضو في ميليشيا أخرى" };

            // 4. Update the Data Objects
            members.Add(playerId);
            gang.MilitiaMembersByUnit[unitId] = members;

            // Update stats
            player.PersonalRespect += respectReward;
            player.PersonalLoyalty += respectReward / 2;
            player.Courage -= requiredCourage;

            string crystalId = unitId switch
            {
                1 => "بلورة صغيرة",
                2 => "بلورة متوسطة",
                3 => "بلورة كبيرة",
                _ => ""
            };

            if (!string.IsNullOrEmpty(crystalId))
                player.CrystalCount++;

            // Update tracking fields
            if (player.MilitiaMemberIds == null) player.MilitiaMemberIds = new List<string>();
            if (!player.MilitiaMemberIds.Contains(unitId.ToString()))
            {
                player.MilitiaMemberIds.Add(unitId.ToString());
            }
            player.MembersIdsJoinedMilitia = string.Join(",", gang.MilitiaMembersByUnit.SelectMany(kvp => kvp.Value));

            // 5. Persist to Firestore
            await SaveGangAsync(gang);
            await FirebaseService.SavePlayerAsync(player);

            // 6. Memory Sync: Update local references so the UI updates without a refresh
            if (AccountService.CurrentPlayer != null && AccountService.CurrentPlayer.PlayerId == playerId)
            {
                AccountService.CurrentPlayer.PersonalLoyalty = player.PersonalLoyalty;
                AccountService.CurrentPlayer.PersonalRespect = player.PersonalRespect;
                AccountService.CurrentPlayer.Courage = player.Courage;
                AccountService.CurrentPlayer.CrystalCount = player.CrystalCount;
                AccountService.CurrentPlayer.MilitiaMemberIds = player.MilitiaMemberIds;
                AccountService.CurrentPlayer.MembersIdsJoinedMilitia = player.MembersIdsJoinedMilitia;
                // The GangId is preserved because we loaded the player from Firestore 
                // instead of using a partial local object.
            }

            // Sync the cache used for member lists
            var cachedPlayer = AccountService.GetPlayerById(playerId);
            if (cachedPlayer != null)
            {
                cachedPlayer.PersonalLoyalty = player.PersonalLoyalty;
                cachedPlayer.PersonalRespect = player.PersonalRespect;
                cachedPlayer.Courage = player.Courage;
            }

            // 7. Refresh Gang Object to match database state
            var freshGang = await GetGangAsync(gang.GangId);
            if (freshGang != null)
            {
                gang.MembersWithPositions = freshGang.MembersWithPositions;
                gang.MilitiaMembersByUnit = freshGang.MilitiaMembersByUnit;
                gang.SkillsLevel = freshGang.SkillsLevel;
                gang.Name = freshGang.Name;
                gang.Tag = freshGang.Tag;
                gang.ImageUrl = freshGang.ImageUrl;
                gang.LeaderId = freshGang.LeaderId;
                gang.GangCash = freshGang.GangCash;
                gang.Respect = freshGang.Respect;
                gang.AvailableRespect = freshGang.AvailableRespect;
                gang.Loyalty = freshGang.Loyalty;
                gang.Contribution = freshGang.Contribution;
                gang.Level = freshGang.Level;
            }

            return new JoinGangMilitiaResultObject
            {
                IsAllProcessSuccess = true,
                IsPlayerMainStatesChanged = true,
                CrystalId = crystalId,
                MembersIdsJoinedMilitia = player.MembersIdsJoinedMilitia
            };
        }
        public static JoinGangMilitiaResultObject JoinMilitia(string playerId, GangObject gang, int unitId, int requiredCourage, int respectReward)
        {
            return Task.Run(() => JoinMilitiaAsync(playerId, gang, unitId, requiredCourage, respectReward)).GetAwaiter().GetResult();
        }

        public static JoinGangMilitiaResultObject LeaveMilitia(string playerId, GangObject gang, int unitId) => new();
        public static bool UpgradeSkill(string gangId, int skillIndex, long respectCost, long cashCost, int maxLevelPerGangLevel) => true;

        #endregion

        #region Serialization helpers

        private static Dictionary<string, object> GangToFirestoreFields(GangObject g)
        {
            string membersJson = JsonSerializer.Serialize(g.MembersWithPositions);
            string militiaJson = JsonSerializer.Serialize(g.MilitiaMembersByUnit);
            string skillsJson = JsonSerializer.Serialize(g.SkillsLevel);
            return new Dictionary<string, object>
            {
                { "gangId", StringValue(g.GangId) },
                { "name", StringValue(g.Name) },
                { "tag", StringValue(g.Tag) },
                { "imageUrl", StringValue(g.ImageUrl) },
                { "leaderId", StringValue(g.LeaderId) },
                { "gangCash", IntegerValue(g.GangCash) },
                { "respect", IntegerValue(g.Respect) },
                { "availableRespect", IntegerValue(g.AvailableRespect) },
                { "loyalty", IntegerValue(g.Loyalty) },
                { "contribution", IntegerValue(g.Contribution) },
                { "level", IntegerValue(g.Level) },
                { "createdTime", TimestampValue(g.CreatedDate) },
                { "membersWithPositions", new { stringValue = membersJson } },
                { "militiaMembersByUnit", new { stringValue = militiaJson } },
                { "skillsLevel", new { stringValue = skillsJson } }
            };
        }

        private static List<GangObject> ParseGangs(string json)
        {
            var gangs = new List<GangObject>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("documents", out var docs)) return gangs;
                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("name", out var nameProp)) continue;
                    var id = nameProp.GetString()?.Split('/').Last() ?? "";
                    var fields = d.GetProperty("fields");
                    gangs.Add(ParseGangFromFields(fields, id));
                }
            }
            catch { }
            return gangs;
        }

        private static GangObject ParseGangFromFields(JsonElement fields, string gangId)
        {
            var g = new GangObject { GangId = gangId };
            g.Name = ReadStr(fields, "name");
            g.Tag = ReadStr(fields, "tag");
            g.ImageUrl = ReadStr(fields, "imageUrl");
            g.LeaderId = ReadStr(fields, "leaderId");
            g.GangCash = ReadLong(fields, "gangCash");
            g.Respect = ReadLong(fields, "respect");
            g.AvailableRespect = ReadLong(fields, "availableRespect");
            g.Loyalty = ReadLong(fields, "loyalty");
            g.Contribution = ReadLong(fields, "contribution");
            g.Level = (int)ReadLong(fields, "level");

            string membersJson = ReadStr(fields, "membersWithPositions");
            if (!string.IsNullOrEmpty(membersJson))
            {
                try
                {
                    g.MembersWithPositions = JsonSerializer.Deserialize<Dictionary<string, GangPosition>>(membersJson) ?? new();
                    Debug.WriteLine($"[PARSE] Members deserialized: {g.MembersWithPositions.Count}");
                }
                catch (Exception ex) { Debug.WriteLine($"[PARSE] Members error: {ex.Message}"); }
            }
            else
            {
                g.MembersWithPositions = new Dictionary<string, GangPosition>();
            }

            string militiaJson = ReadStr(fields, "militiaMembersByUnit");
            if (!string.IsNullOrEmpty(militiaJson))
                g.MilitiaMembersByUnit = JsonSerializer.Deserialize<Dictionary<int, List<string>>>(militiaJson) ?? new();
            else
                g.MilitiaMembersByUnit = new Dictionary<int, List<string>>();

            string skillsJson = ReadStr(fields, "skillsLevel");
            if (!string.IsNullOrEmpty(skillsJson))
                g.SkillsLevel = JsonSerializer.Deserialize<Dictionary<string, int>>(skillsJson) ?? new();
            else
                g.SkillsLevel = new Dictionary<string, int>();

            return g;
        }

        private static object StringValue(string s) => new { stringValue = s ?? "" };
        private static object IntegerValue(long n) => new { integerValue = n };
        private static object TimestampValue(DateTime dt) => new { timestampValue = dt.ToString("o") };

        private static string ReadStr(JsonElement f, string key) =>
            f.TryGetProperty(key, out var p) && p.TryGetProperty("stringValue", out var v) ? v.GetString() ?? "" : "";

        private static long ReadLong(JsonElement f, string key) =>
            f.TryGetProperty(key, out var p) && p.TryGetProperty("integerValue", out var v)
                ? (v.ValueKind == JsonValueKind.String ? long.Parse(v.GetString()!) : v.GetInt64())
                : 0;

        #endregion
    }
}