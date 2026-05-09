using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GameProject02.Models;

namespace GameProject02.Services
{
    public static class AccountService
    {
        private static readonly Dictionary<string, PlayerAccount> _localAccounts = new();
        private static PlayerAccount _currentUser;
        private static readonly List<PlayerAccount> _allPlayers = new();

        private const string FirestoreBaseUrl = "https://firestore.googleapis.com/v1/projects/gameproject02-4207f/databases/(default)/documents";
        private const string WebApiKey = "AIzaSyCM61YoJzqt9x7lOndV2oBJGeoBtU9U_Uo";
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
            RegisterPlayer(account);

            bool saved = await FirebaseService.SavePlayerAsync(account);
            if (!saved)
            {
                _localAccounts.Remove(account.PlayerId);
                _currentUser = null;
                await ShowAlert("Registration Failed", "Could not save player data to the cloud.");
                return false;
            }

            bool mappingSaved = await SaveUsernameMappingAsync(username, account.PlayerId);
            if (!mappingSaved)
                await ShowAlert("Warning", "Account created, but username lookup may not work on other devices.");

            // ✅ CLEAR any leftover notifications from previous user, then add the welcome
            NotificationService.ClearAll();

            // ✅ WELCOME NOTIFICATION
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

        // ... (rest of AccountService code unchanged until LoginAsync)

        public static async Task<bool> LoginAsync(string username, string password)
        {
            var account = _localAccounts.Values
                .FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (account == null)
            {
                string playerId = await GetPlayerIdByUsernameAsync(username);
                if (playerId == null)
                {
                    await ShowAlert("Cloud Error", $"Username '{username}' not found.");
                    return false;
                }

                account = await FirebaseService.LoadPlayerAsync(playerId);
                if (account == null)
                {
                    await ShowAlert("Cloud Error", "Player data could not be loaded.");
                    return false;
                }

                _localAccounts[account.PlayerId] = account;
                RegisterPlayer(account);
            }

            if (account.PasswordHash != HashPassword(password))
            {
                await ShowAlert("Login Failed", "Incorrect password.");
                return false;
            }

            _currentUser = account;
            EnsurePlayerRegistered(account);

            // No need to clear or load notifications – they are part of the player object

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

            // ✅ Clear notifications (already there, but good to keep)
            NotificationService.ClearAll();
            _currentUser = null;
        }

        public static PlayerAccount GetCurrentPlayer() => _currentUser;
        public static bool IsLoggedIn() => _currentUser != null;

        public static int GetPlayerAgeInDays()
        {
            if (_currentUser == null) return 0;
            return (int)(DateTime.UtcNow - _currentUser.CreatedAt).TotalDays;
        }

        public static void SavePlayer(PlayerAccount player)
        {
            System.Diagnostics.Debug.WriteLine($"[SAVE] Player {player?.Username} state preserved");
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

                // ✅ LEVEL UP NOTIFICATION
                NotificationService.AddGameNotification(
                    title: $"🎉 المستوى {_currentUser.Level}!",
                    message: $"تهانينا! وصلت للمستوى {_currentUser.Level}\n+{_currentUser.Level * 50} ذهب مكافأة",
                    priority: GameNotificationPriority.High,
                    icon: "🏆",
                    actionTarget: "ProfilePage"
                );
            }
            else
            {
                _currentUser.LevelProgress = (double)_currentUser.CurrentXP / _currentUser.MaxXP;
            }
        }

        public static void RegisterPlayer(PlayerAccount player)
        {
            if (!_allPlayers.Any(p => p.PlayerId == player.PlayerId))
                _allPlayers.Add(player);
        }

        public static List<PlayerAccount> GetAllPlayers() => _allPlayers;
        public static PlayerAccount GetPlayerById(string id) => _allPlayers.FirstOrDefault(p => p.PlayerId == id);
        public static void EnsurePlayerRegistered(PlayerAccount player)
        {
            if (!_allPlayers.Contains(player))
            {
                _allPlayers.Add(player);
                System.Diagnostics.Debug.WriteLine($"[ACCOUNT] Registered player: {player.Username} (ID: {player.PlayerId})");
            }
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

        private static async Task<string> GetPlayerIdByUsernameAsync(string username)
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