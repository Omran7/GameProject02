using GameProject02.Helpers;
using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameProject02.Services
{
    public static class AccountService
    {
        private static readonly Dictionary<string, PlayerAccount> _localAccounts = new();
        private static PlayerAccount _currentUser;
        private static readonly List<PlayerAccount> _allPlayers = new();
        private static bool _isLoggedIn = false;

        public static PlayerAccount CurrentPlayer
        {
            get => _currentUser;
            set
            {
                if (value == null)
                    Debug.WriteLine("[AccountService] Warning: Setting CurrentPlayer to null");
                _currentUser = value;
            }
        }

        public static void SetCurrentPlayer(PlayerAccount player) => CurrentPlayer = player;
        public static bool IsLoggedIn() => _isLoggedIn;

        private const string FirestoreBaseUrl = "https://firestore.googleapis.com/v1/projects/gameproject02-4207f/databases/(default)/documents";
        private const string WebApiKey = "AIzaSyCM61YoJzqt9X7lOndV2oBJGeoBtU9U_Uo";
        private static readonly HttpClient _httpClient = new();

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        public static async Task<bool> RegisterAccountAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3) return false;
            if (string.IsNullOrWhiteSpace(password) || password.Length < 4) return false;

            if (_localAccounts.Values.Any(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return false;

            string existingPlayerId = await GetPlayerIdByUsernameAsync(username);
            if (existingPlayerId != null)
            {
                await ShowAlert("Registration Failed", "This username is already taken. Please choose another.");
                return false;
            }

            var account = new PlayerAccount
            {
                PlayerId = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                CrimeObject = new CrimeObject(),
                ImageResource = "default_avatar.png"
            };

            _localAccounts[account.PlayerId] = account;
            _currentUser = account;
            _isLoggedIn = true;
            RegisterPlayer(account);

            bool saved = await FirebaseService.SavePlayerAsync(account);
            if (!saved)
            {
                _localAccounts.Remove(account.PlayerId);
                _currentUser = null;
                _isLoggedIn = false;
                await ShowAlert("Registration Failed", "Could not save player data to the cloud.");
                return false;
            }

            bool mappingSaved = await SaveUsernameMappingAsync(username, account.PlayerId);
            if (!mappingSaved)
                await ShowAlert("Warning", "Account created, but username lookup may not work on other devices.");

            NotificationService.AddGameNotification(
                title: "🎉 مرحباً!",
                message: $"أهلاً بك {username}! ابدأ رحلتك في عالم الجريمة",
                priority: GameNotificationPriority.High,
                icon: "🎮",
                actionTarget: "MainPage"
            );

            RegenerationService.Start(_currentUser);
            return true;
        }

        public static async Task<bool> LoginAsync(string username, string password)
        {
            var localAccount = _localAccounts.Values
                .FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            string playerId = localAccount?.PlayerId ?? await GetPlayerIdByUsernameAsync(username);
            if (string.IsNullOrEmpty(playerId))
            {
                await ShowAlert("Cloud Error", $"Username '{username}' not found.");
                return false;
            }

            var cloudAccount = await FirebaseService.LoadPlayerAsync(playerId);
            if (cloudAccount == null)
            {
                await ShowAlert("Cloud Error", "Player data could not be loaded.");
                return false;
            }

            // Log ban flags for debugging
            Debug.WriteLine($"[LOGIN] Bans loaded for {cloudAccount.Username}: Chat={cloudAccount.IsBannedFromChat}, Profile={cloudAccount.IsBannedFromChangeProfilePic}, News={cloudAccount.IsBannedFromNews}, Messages={cloudAccount.IsBannedFromPrivateMessages}");

            if (cloudAccount.PasswordHash != HashPassword(password))
            {
                await ShowAlert("Login Failed", "Incorrect password.");
                return false;
            }

            // Load gang
            if (!string.IsNullOrEmpty(cloudAccount.GangId))
            {
                try
                {
                    cloudAccount.GangObject = await GangDatabaseService.GetGangAsync(cloudAccount.GangId);
                    if (cloudAccount.GangObject == null)
                    {
                        cloudAccount.GangId = string.Empty;
                        await FirebaseService.SavePlayerAsync(cloudAccount);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[LOGIN] Failed to load gang: {ex.Message}");
                    cloudAccount.GangObject = null;
                }
            }

            _localAccounts[cloudAccount.PlayerId] = cloudAccount;
            if (localAccount != null && localAccount.PlayerId != cloudAccount.PlayerId)
                _localAccounts.Remove(localAccount.PlayerId);

            _currentUser = cloudAccount;
            _isLoggedIn = true;
            RegisterPlayer(cloudAccount);
            EnsurePlayerRegistered(cloudAccount);

            RegenerationService.Start(_currentUser);

            return true;
        }

        private static async Task ShowAlert(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            });
        }

        public static void Logout()
        {
            RegenerationService.Stop();
            var player = _currentUser;
            if (player != null)
                _ = FirebaseService.SavePlayerAsync(player);

            NotificationService.ClearAll();
            _currentUser = null;
            _isLoggedIn = false;
        }

        public static PlayerAccount GetCurrentPlayer() => _currentUser;

        public static int GetPlayerAgeInDays()
        {
            if (_currentUser == null) return 0;
            return (int)(DateTime.UtcNow - _currentUser.CreatedAt).TotalDays;
        }

        public static void SavePlayer(PlayerAccount player)
        {
            Debug.WriteLine($"[SAVE] Player {player?.Username} state preserved");
        }

        public static void TrainAtGym()
        {
            if (_currentUser == null) return;
            _currentUser.Strength += 2;
            _currentUser.Speed += 1;
            _currentUser.Gold -= 10;
            _currentUser.CurrentXP += 5;
            CheckLevelUp();
        }

        public static void StudyAtSchool()
        {
            if (_currentUser == null) return;
            _currentUser.Intelligence += 3;
            _currentUser.Gold -= 15;
            _currentUser.CurrentXP += 8;
            CheckLevelUp();
        }

        private static void CheckLevelUp()
        {
            if (_currentUser == null) return;
            if (_currentUser.CurrentXP >= _currentUser.MaxXP)
            {
                _currentUser.Level++;
                _currentUser.MaxXP = _currentUser.Level * 100;
                _currentUser.CurrentXP = 0;
                _currentUser.LevelProgress = 0.0;
                _currentUser.Gold += _currentUser.Level * 50;
                _currentUser.Medals++;

                _currentUser.SkillPoints += 3;

                NotificationService.AddGameNotification(
                    title: $"🎉 المستوى {_currentUser.Level}!",
                    message: $"تهانينا! وصلت للمستوى {_currentUser.Level}\n+{_currentUser.Level * 50} ذهب مكافأة",
                    priority: GameNotificationPriority.High,
                    icon: "🏆",
                    actionTarget: "ProfilePage"
                );

                _ = FirebaseService.SavePlayerAsync(_currentUser);
            }
            else
            {
                _currentUser.LevelProgress = (double)_currentUser.CurrentXP / _currentUser.MaxXP;
            }
            MedalService.CheckAndAwardAll(_currentUser);
        }

        public static void RegisterPlayer(PlayerAccount player)
        {
            if (!_allPlayers.Any(p => p.PlayerId == player.PlayerId))
                _allPlayers.Add(player);
        }

        public static async Task<PlayerAccount> GetPlayerByIdAsync(string playerId)
        {
            var player = _allPlayers.FirstOrDefault(p => p.PlayerId == playerId);
            if (player != null) return player;
            return await FirebaseService.LoadPlayerAsync(playerId);
        }

        public static List<PlayerAccount> GetAllPlayers() => _allPlayers;
        public static PlayerAccount GetPlayerById(string id) => _allPlayers.FirstOrDefault(p => p.PlayerId == id);

        public static void EnsurePlayerRegistered(PlayerAccount player)
        {
            if (!_allPlayers.Contains(player))
            {
                _allPlayers.Add(player);
                Debug.WriteLine($"[ACCOUNT] Registered player: {player.Username} (ID: {player.PlayerId})");
            }
        }

        public static async Task<PlayerAccount> GetPlayerByUsernameAsync(string username)
        {
            string playerId = await GetPlayerIdByUsernameAsync(username);
            if (string.IsNullOrEmpty(playerId)) return null;
            return await FirebaseService.LoadPlayerAsync(playerId);
        }

        public static async Task<List<PlayerAccount>> GetAllPlayersAsync()
        {
            try
            {
                var url = $"{FirestoreBaseUrl}/players?key={WebApiKey}&pageSize=500";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<PlayerAccount>();
                var json = await response.Content.ReadAsStringAsync();
                return ParsePlayersList(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccountService] GetAllPlayers error: {ex.Message}");
                return new List<PlayerAccount>();
            }
        }

        private static List<PlayerAccount> ParsePlayersList(string json)
        {
            var players = new List<PlayerAccount>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("documents", out var docs)) return players;
                foreach (var d in docs.EnumerateArray())
                {
                    if (!d.TryGetProperty("name", out var nameProp)) continue;
                    var playerId = nameProp.GetString()?.Split('/').Last() ?? "";
                    var fields = d.GetProperty("fields");
                    var player = ParsePlayerFromFirestoreFields(fields, playerId);
                    if (player != null) players.Add(player);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccountService] ParsePlayersList error: {ex.Message}");
            }
            return players;
        }

        private static PlayerAccount ParsePlayerFromFirestoreFields(JsonElement fields, string playerId)
        {
            var player = new PlayerAccount { PlayerId = playerId };

            player.Username = GetString(fields, "username") ?? "";
            player.PasswordHash = GetString(fields, "passwordHash") ?? "";
            player.AvatarPath = GetString(fields, "avatarPath") ?? player.AvatarPath;
            player.Gender = GetString(fields, "gender") ?? "ذكر";
            player.City = GetString(fields, "city") ?? "مدينة العصابات";
            player.IsVIP = GetBoolean(fields, "isVIP");
            player.AchievementPoints = GetInt32(fields, "achievementPoints");
            player.Medals = GetInt32(fields, "medals");
            player.Level = GetInt32(fields, "level");
            player.Gold = GetInt32(fields, "gold");
            player.Diamonds = GetInt32(fields, "diamonds");
            player.Checks = GetInt32(fields, "checks");
            player.Energy = GetInt32(fields, "energy");
            player.MaxEnergy = GetInt32(fields, "maxEnergy");
            player.Courage = GetInt32(fields, "courage");
            player.MaxCourage = GetInt32(fields, "maxCourage");
            player.NobilityCurrent = GetInt32(fields, "nobilityCurrent");
            player.Health = GetInt32(fields, "health");
            player.MaxHealth = GetInt32(fields, "maxHealth");
            player.Strength = GetInt32(fields, "strength");
            player.Defense = GetInt32(fields, "defense");
            player.Speed = GetInt32(fields, "speed");
            player.Dexterity = GetInt32(fields, "dexterity");
            player.Intelligence = GetInt32(fields, "intelligence");
            player.CurrentXP = GetInt32(fields, "currentXP");
            player.MaxXP = GetInt32(fields, "maxXP");
            player.PersonalContribution = GetInt32(fields, "personalContribution");
            player.PersonalLoyalty = GetInt64(fields, "personalLoyalty");
            player.PersonalRespect = GetInt64(fields, "personalRespect");
            player.CrystalCount = GetInt32(fields, "crystalCount");
            player.CrimeAttempts = GetInt32(fields, "crimeAttempts");
            player.Shovels = GetInt32(fields, "shovels");
            player.HospitalVisits = GetInt32(fields, "hospitalVisits");
            player.JailTimes = GetInt32(fields, "jailTimes");
            player.Flights = GetInt32(fields, "flights");
            player.HerbsUsed = GetInt32(fields, "herbsUsed");
            player.ItemsFound = GetInt32(fields, "itemsFound");
            player.GangId = GetString(fields, "gangId") ?? "";
            player.EstateType = GetString(fields, "estateType") ?? player.EstateType;
            player.EstateOwner = GetString(fields, "estateOwner") ?? player.EstateOwner;
            player.EstateHours = GetInt32(fields, "estateHours");
            player.EstateUpgrades = GetInt32(fields, "estateUpgrades");
            player.EstateWorkers = GetInt32(fields, "estateWorkers");
            player.ImageResource = GetString(fields, "imageResource") ?? player.ImageResource;

            return player;
        }

        private static string GetString(JsonElement fields, string key)
        {
            if (fields.TryGetProperty(key, out var prop) && prop.TryGetProperty("stringValue", out var v))
                return v.GetString();
            return null;
        }

        private static int GetInt32(JsonElement fields, string key)
        {
            if (fields.TryGetProperty(key, out var prop) && prop.TryGetProperty("integerValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out int result))
                    return result;
                if (v.ValueKind == JsonValueKind.Number)
                    return v.GetInt32();
            }
            return 0;
        }

        private static long GetInt64(JsonElement fields, string key)
        {
            if (fields.TryGetProperty(key, out var prop) && prop.TryGetProperty("integerValue", out var v))
            {
                if (v.ValueKind == JsonValueKind.String && long.TryParse(v.GetString(), out long result))
                    return result;
                if (v.ValueKind == JsonValueKind.Number)
                    return v.GetInt64();
            }
            return 0;
        }

        private static bool GetBoolean(JsonElement fields, string key)
        {
            if (fields.TryGetProperty(key, out var prop) && prop.TryGetProperty("booleanValue", out var v))
                return v.GetBoolean();
            return false;
        }

        private static async Task<bool> SaveUsernameMappingAsync(string username, string playerId)
        {
            try
            {
                var doc = new { fields = new { playerId = new { stringValue = playerId } } };
                var json = JsonSerializer.Serialize(doc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"{FirestoreBaseUrl}/usernames/{username}?key={WebApiKey}";
                var response = await _httpClient.PatchAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> GetPlayerIdByUsernameAsync(string username)
        {
            try
            {
                var url = $"{FirestoreBaseUrl}/usernames/{username}?key={WebApiKey}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("fields", out var fields) &&
                    fields.TryGetProperty("playerId", out var pidProp) &&
                    pidProp.TryGetProperty("stringValue", out var val))
                {
                    return val.GetString();
                }
            }
            catch { }
            return null;
        }
    }
}