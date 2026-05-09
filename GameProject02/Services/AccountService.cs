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

        // In‑memory fallback for non‑cloud scenarios
        private static readonly List<PlayerAccount> _allPlayers = new();

        // ── Firebase REST endpoints ──────────────────────────────────
        // For username → playerId mapping, we store a separate collection.
        private const string FirestoreBaseUrl = "https://firestore.googleapis.com/v1/projects/gameproject02-4207f/databases/(default)/documents";
        private const string WebApiKey = "AIzaSyCM61YoJzqt9X7lOndV2oBJGeoBtU9U_Uo";
        private static readonly HttpClient _httpClient = new();

        // ── Hashing ─────────────────────────────────────────────────
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        // ── REGISTRATION (async, cloud‑backed) ──────────────────────
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

            // Save player document first
            bool playerSaved = await FirebaseService.SavePlayerAsync(account);
            if (!playerSaved)
            {
                _localAccounts.Remove(account.PlayerId);
                _currentUser = null;
                await ShowAlert("Registration Failed", "Could not save player data to the cloud. Check your connection and try again.");
                return false;
            }

            // Then save username mapping
            bool mappingSaved = await SaveUsernameMappingAsync(username, account.PlayerId);
            if (!mappingSaved)
            {
                await ShowAlert("Warning", "Account created, but the username lookup may not work on other devices.");
            }

            RegenerationService.Start(_currentUser);
            return true;
        }
        // ── LOGIN (async, tries cloud if not in memory) ────────────
        public static async Task<bool> LoginAsync(string username, string password)
        {
            // 1. Local cache
            var account = _localAccounts.Values
                .FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (account == null)
            {
                // 2. Get playerId from cloud
                string playerId = await GetPlayerIdByUsernameAsync(username);
                if (playerId == null)
                {
                    await ShowAlert("Cloud Error", $"Username '{username}' not found in the database.\nCheck usernames/{username} exists with a playerId field.");
                    return false;
                }

                // 3. Load full player
                account = await FirebaseService.LoadPlayerAsync(playerId);
                if (account == null)
                {
                    await ShowAlert("Cloud Error", $"Player data for ID '{playerId}' could not be loaded.\nMake sure players/{playerId} exists and rules allow read.");
                    return false;
                }

                // Cache locally
                _localAccounts[account.PlayerId] = account;
                RegisterPlayer(account);
            }

            // 4. Check password
            string enteredHash = HashPassword(password);
            if (account.PasswordHash != enteredHash)
            {
                await ShowAlert("Login Failed", $"Incorrect password.\nExpected hash: {account.PasswordHash}\nEntered hash: {enteredHash}");
                return false;
            }

            _currentUser = account;
            EnsurePlayerRegistered(account);
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
        // ── LOGOUT (save to cloud) ─────────────────────────────────
        public static void Logout()
        {
            RegenerationService.Stop();
            var player = _currentUser;
            if (player != null)
            {
                // Save async, we don't need to await here
                _ = FirebaseService.SavePlayerAsync(player);
            }
            _currentUser = null;
        }

        // ── Current player helpers ─────────────────────────────────
        public static PlayerAccount GetCurrentPlayer() => _currentUser;
        public static bool IsLoggedIn() => _currentUser != null;

        public static int GetPlayerAgeInDays()
        {
            if (_currentUser == null) return 0;
            return (int)(DateTime.UtcNow - _currentUser.CreatedAt).TotalDays;
        }

        // ── Persistence placeholder (still used by some services) ──
        public static void SavePlayer(PlayerAccount player)
        {
            System.Diagnostics.Debug.WriteLine($"[SAVE] Player {player?.Username} state preserved");
        }

        // ── Legacy training methods (unchanged) ────────────────────
        public static void TrainAtGym()
        {
            if (_currentUser == null) return;
            _currentUser.Strength += 2;
            _currentUser.Speed += 1;
            _currentUser.Gold -= 10;
            _currentUser.CurrentXP += 5;

            if (_currentUser.CurrentXP >= _currentUser.MaxXP)
            {
                LevelUp();
            }
            else
            {
                _currentUser.LevelProgress = (double)_currentUser.CurrentXP / _currentUser.MaxXP;
            }
        }

        public static void StudyAtSchool()
        {
            if (_currentUser == null) return;
            _currentUser.Intelligence += 3;
            _currentUser.Gold -= 15;
            _currentUser.CurrentXP += 8;

            if (_currentUser.CurrentXP >= _currentUser.MaxXP)
            {
                LevelUp();
            }
            else
            {
                _currentUser.LevelProgress = (double)_currentUser.CurrentXP / _currentUser.MaxXP;
            }
        }

        private static void LevelUp()
        {
            if (_currentUser == null) return;
            _currentUser.Level++;
            _currentUser.MaxXP = _currentUser.Level * 100;
            _currentUser.CurrentXP = 0;
            _currentUser.LevelProgress = 0.0;
            _currentUser.Gold += _currentUser.Level * 50;
            _currentUser.Medals++;
        }

        // ── Multiplayer (in‑memory for now) ────────────────────────
        public static void RegisterPlayer(PlayerAccount player)
        {
            if (!_allPlayers.Any(p => p.PlayerId == player.PlayerId))
            {
                _allPlayers.Add(player);
            }
        }

        public static List<PlayerAccount> GetAllPlayers() => _allPlayers;

        public static PlayerAccount GetPlayerById(string id)
        {
            return _allPlayers.FirstOrDefault(p => p.PlayerId == id);
        }

        public static void EnsurePlayerRegistered(PlayerAccount player)
        {
            if (!_allPlayers.Contains(player))
            {
                _allPlayers.Add(player);
                System.Diagnostics.Debug.WriteLine($"[ACCOUNT] Registered player: {player.Username} (ID: {player.PlayerId})");
            }
        }

        // ── Cloud username mapping ─────────────────────────────────
        private static async Task<bool> SaveUsernameMappingAsync(string username, string playerId)
        {
            try
            {
                var doc = new
                {
                    fields = new
                    {
                        playerId = new { stringValue = playerId }
                    }
                };
                var json = JsonSerializer.Serialize(doc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{FirestoreBaseUrl}/usernames/{username}?key={WebApiKey}";
                var response = await _httpClient.PatchAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Failed to save username mapping: {ex.Message}");
                return false;
            }
        }
        private static async Task<string> GetPlayerIdByUsernameAsync(string username)
        {
            try
            {
                var url = $"{FirestoreBaseUrl}/usernames/{username}?key={WebApiKey}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH] Failed to get playerId by username: {ex.Message}");
            }
            return null;
        }
    }
}