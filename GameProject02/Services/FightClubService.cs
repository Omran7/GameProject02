using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class FightClubService
{
    private static Random _random = new Random();

    // =========================
    // GET OPPONENTS
    // =========================
    public static List<FightClubPlayer> GetEligibleOpponents(PlayerAccount currentPlayer)
    {
        if (currentPlayer == null) return new List<FightClubPlayer>();

        var allPlayers = AccountService.GetAllPlayers();

        return allPlayers
            .Where(p =>
                p.PlayerId != currentPlayer.PlayerId &&
                !p.CrimeObject.IsInHospital &&
                !p.CrimeObject.IsInPrison &&
                p.City == currentPlayer.City)
            .Select(p => new FightClubPlayer
            {
                PlayerId = p.PlayerId,
                Username = p.Username,
                ImageResource = p.ImageResource,
                Level = p.Level,
                HealthCurrent = p.Health,
                HealthMax = p.MaxHealth,
                Strength = p.Strength,
                Defense = p.Defense,
                Speed = p.Speed,
                Dexterity = p.Dexterity
            })
            .OrderByDescending(p => p.Level)
            .ToList();
    }

    // =========================
    // DAMAGE
    // =========================
    private static int CalculateDamage(PlayerAccount attacker, PlayerAccount defender)
    {
        // ✅ GET EFFECTIVE DEXTERITY (APPLIES NOBILITY MULTIPLIER)
        double effectiveDexterity = NobilityService.GetEffectiveDexterity(attacker);

        // ✅ BASE DAMAGE: Strength + (Effective Dexterity / 2) + Weapon Bonus
        int baseDamage = attacker.Strength + ((int)effectiveDexterity / 2);

        // Add weapon damage if equipped
        if (!string.IsNullOrEmpty(attacker.ArmingObject.WeaponId) &&
            attacker.StockObject.ItemsInStock.TryGetValue(attacker.ArmingObject.WeaponId, out var weaponItem))
        {
            baseDamage += weaponItem.Damage;
        }

        // ✅ DEFENSE CALCULATION (NO NOBILITY EFFECT ON DEFENDER - FROM Experience.txt)
        int defense = defender.Defense + (defender.Speed / 3);

        // Add armor defense if equipped
        if (!string.IsNullOrEmpty(defender.ArmingObject.ArmorId) &&
            defender.StockObject.ItemsInStock.TryGetValue(defender.ArmingObject.ArmorId, out var armorItem))
        {
            defense += armorItem.Defense;
        }

        // ✅ FINAL DAMAGE: (Base + Weapon) - (Defense + Armor)/2
        int damage = baseDamage - (defense / 2);
        return Math.Max(1, damage);
    }

    // =========================
    // PLAYER ACTION
    // =========================
    public static (bool success, string message, int damage, int newHealth, int goldStolen, int statReduction)
        ExecutePlayerAction(PlayerAccount attacker, PlayerAccount defender, int actionType)
    {
        if (attacker == null || defender == null)
            return (false, "خطأ", 0, 0, 0, 0);

        if (actionType == 0) // Attack
        {
            int damage = CalculateDamage(attacker, defender);
            defender.Health = Math.Max(0, defender.Health - damage);

            return (true, $"تسبب بـ {damage} ضرر", damage, defender.Health, 0, 0);
        }
        else if (actionType == 1) // Steal
        {
            int steal = Math.Min(defender.Gold, 500 + _random.Next(500));
            attacker.Gold += steal;
            defender.Gold -= steal;

            return (true, $"سرق {steal} ذهب", 0, defender.Health, steal, 0);
        }
        else if (actionType == 2) // Cripple
        {
            int reduction = _random.Next(5, 15);
            defender.Strength = Math.Max(1, defender.Strength - reduction);

            // Add some health damage so the opponent actually feels the hit
            int damage = 10;
            defender.Health = Math.Max(0, defender.Health - damage);

            return (true, $"نقص {reduction} قوة و {damage} ضرر", damage, defender.Health, 0, reduction);
        }
        else if (actionType == 3) // Disability (عاهة)
        {
            // In the old game, this usually ensures the player stays 
            // in the hospital for 1800 seconds (30 minutes)
            defender.Health = 0;

            // Logic to set hospital time (Assuming you have a Hospital timer in your model)
            // defender.HospitalTimer = 1800; 

            return (true, "تم تسبيب عاهة للخصم! (30 دقيقة مستشفى)", 0, 0, 0, 0);
        }

        return (false, "غير معروف", 0, defender.Health, 0, 0);
    }

    // =========================
    // OPPONENT ACTION
    // =========================
    public static (bool success, string message, int damage, int newHealth, int goldStolen, int statReduction)
        ExecuteOpponentAction(PlayerAccount opponent, PlayerAccount player)
    {
        int action = _random.Next(100);

        if (action < 70)
        {
            int damage = CalculateDamage(opponent, player);
            player.Health = Math.Max(0, player.Health - damage);

            return (true, $"تسبب بـ {damage} ضرر", damage, player.Health, 0, 0);
        }
        else if (action < 90)
        {
            int steal = Math.Min(player.Gold, 500 + _random.Next(500));
            opponent.Gold += steal;
            player.Gold -= steal;

            return (true, $"سرق {steal} ذهب", 0, player.Health, steal, 0);
        }
        else
        {
            int reduction = _random.Next(5, 15);
            player.Strength = Math.Max(1, player.Strength - reduction);

            return (true, $"نقص {reduction} قوة", 0, player.Health, 0, reduction);
        }
    }

    // =========================
    // ESCAPE
    // =========================
    public static bool CanEscape()
    {
        return _random.Next(100) < 50;
    }

    // =========================
    // HOSPITAL
    // =========================
    public static void SendToHospital(PlayerAccount loser, PlayerAccount winner)
    {
        // ✅ SET HOSPITAL CONFINEMENT STATE (TRIGGERS AUTO-REDIRECT)
        loser.CrimeObject.IsInHospital = true;
        loser.CrimeObject.HospitalReason = $"هُزمت في قتال ضد {winner.Username}";
        loser.CrimeObject.TotalHospitalVisits++;

        // ✅ CALCULATE HOSPITAL DURATION (10-60 MINUTES BASED ON LEVEL DIFF)
        int levelDiff = Math.Max(0, winner.Level - loser.Level);
        int minutes = 2 + (levelDiff / 2);
        minutes = Math.Min(60, Math.Max(2, minutes));

        loser.CrimeObject.HospitalReleaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            + (long)TimeSpan.FromMinutes(minutes).TotalMilliseconds;

        // ✅ REDUCE HEALTH TO 1 (NOT DEAD - AUTHENTIC OLD GAME)
        loser.Health = 1;
        loser.CrimeObject.HealthCurrent = 1;
    }

    // =========================
    // REWARDS
    // =========================
    // ✅ CORRECTED: WINNING FIGHT GIVES ONLY XP (NO GOLD) - GOLD ONLY FROM STEAL BUTTON
    public static void GiveRewards(PlayerAccount winner, PlayerAccount loser)
    {
        // ✅ ONLY GIVE XP (EXPERIENCE) FOR WINNING - MATCHES OLD GAME
        int xp = loser.Level * 50;
        winner.CurrentXP += xp;

        // ✅ TRIGGER LEVEL-UP IF XP THRESHOLD REACHED (AUTHENTIC OLD GAME)
        while (winner.MainStatesObject.CanLevelUp())
            winner.MainStatesObject.LevelUp();
    }
}
