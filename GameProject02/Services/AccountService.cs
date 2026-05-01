using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GameProject02.Models;

namespace GameProject02.Services;

public static class AccountService
{
    private static readonly Dictionary<string, PlayerAccount> _localAccounts = new();
    private static PlayerAccount _currentUser;

    // Multiplayer simulation list (for in-memory testing)
    private static readonly List<PlayerAccount> _allPlayers = new();

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }

    public static bool RegisterAccount(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3) return false;
        if (string.IsNullOrWhiteSpace(password) || password.Length < 4) return false;

        if (_localAccounts.Values.Any(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return false;

        var account = new PlayerAccount
        {
            PlayerId = Guid.NewGuid().ToString(), // ✅ FORCE NEW ID HERE
            Username = username,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            CrimeObject = new CrimeObject(),
            ImageResource = "default_avatar.png"
        };

        _localAccounts[account.PlayerId] = account;
        _currentUser = account;

        RegisterPlayer(account);

        return true;
    }
    public static bool Login(string username, string password)
    {
        var account = _localAccounts.Values
            .FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (account == null) return false;

        if (account.PasswordHash == HashPassword(password))
        {
            _currentUser = account;
            EnsurePlayerRegistered(account); // ✅ CRITICAL: Register player in global list
            return true;
        }

        return false;
    }

    public static PlayerAccount GetCurrentPlayer()
    {
        return _currentUser;
    }

    public static bool IsLoggedIn() => _currentUser != null;

    // ✅ SIGN-OUT METHOD (NEW)
    public static void Logout()
    {
        _currentUser = null;
    }

    public static int GetPlayerAgeInDays()
    {
        if (_currentUser == null) return 0;
        return (int)(DateTime.UtcNow - _currentUser.CreatedAt).TotalDays;
    }
    // ✅ DUMMY METHOD FOR NOBILITY PERSISTENCE (IN-MEMORY ONLY - NO FIREBASE)
    public static void SavePlayer(PlayerAccount player)
    {
        // For future Firebase integration - currently does nothing
        // All changes are in-memory and persist while app is running
        System.Diagnostics.Debug.WriteLine($"[SAVE] Player {player?.Username} state preserved in memory");
    }

    // Training methods
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

    // Multiplayer simulation methods
    public static void RegisterPlayer(PlayerAccount player)
    {
        if (!_allPlayers.Any(p => p.PlayerId == player.PlayerId))
        {
            _allPlayers.Add(player);
        }
    }

    public static List<PlayerAccount> GetAllPlayers() => _allPlayers;

    // Optional: get player by ID (for debugging)
    public static PlayerAccount GetPlayerById(string id)
    {
        return _allPlayers.FirstOrDefault(p => p.PlayerId == id);
    }
    // ✅ ENSURE PLAYER IS REGISTERED IN GLOBAL LIST (CALL AFTER LOGIN)
    public static void EnsurePlayerRegistered(PlayerAccount player)
    {
        if (!_allPlayers.Contains(player))
        {
            _allPlayers.Add(player);
            System.Diagnostics.Debug.WriteLine($"[ACCOUNT] Registered player: {player.Username} (ID: {player.PlayerId})");
        }
    }
}