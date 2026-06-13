using GameProject02.Models;
using System;

namespace GameProject02.Services;

public static class CombatService
{
    private static readonly Random _random = new Random();

    // Start a new battle
    public static void StartBattle(PlayerAccount player, Opponent opponent)
    {
        player.Combat.IsInCombat = true;
        player.Combat.CurrentOpponent = opponent;
        player.Combat.RecalculateStats(player);
        player.Combat.BattlesFought++;
    }

    // Player attacks opponent
    public static (bool hit, int damage, bool critical) PlayerAttack(PlayerAccount player)
    {
        if (!player.Combat.IsInCombat || player.Combat.CurrentOpponent == null)
            return (false, 0, false);

        var opponent = player.Combat.CurrentOpponent;

        // Calculate hit chance based on player Dexterity vs opponent Speed
        int hitChance = 70 + (player.Dexterity - opponent.Speed);
        hitChance = Math.Max(30, Math.Min(95, hitChance)); // Clamp between 30-95%

        bool hit = _random.Next(100) < hitChance;
        if (!hit)
            return (false, 0, false);

        // Calculate base damage
        int damage = player.Combat.AttackPower - (opponent.DefensePower / 2);
        damage = Math.Max(1, damage); // Minimum 1 damage

        // Check for critical hit
        bool critical = _random.Next(100) < player.Combat.CriticalChance;
        if (critical)
            damage *= 2;

        // Apply damage
        opponent.CurrentHealth = Math.Max(0, opponent.CurrentHealth - damage);

        return (true, damage, critical);
    }

    // Opponent attacks player
    public static (bool hit, int damage, bool critical) OpponentAttack(PlayerAccount player)
    {
        if (!player.Combat.IsInCombat || player.Combat.CurrentOpponent == null)
            return (false, 0, false);

        var opponent = player.Combat.CurrentOpponent;

        // Calculate hit chance based on opponent stats vs player Defense
        int hitChance = 65 + (opponent.AttackPower - player.Combat.DefensePower);
        hitChance = Math.Max(25, Math.Min(90, hitChance));

        bool hit = _random.Next(100) < hitChance;
        if (!hit)
            return (false, 0, false);

        // Calculate damage
        int damage = opponent.AttackPower - (player.Combat.DefensePower / 2);
        damage = Math.Max(1, damage);

        // Critical hit chance for opponent (simpler)
        bool critical = _random.Next(100) < 10; // 10% base crit chance
        if (critical)
            damage *= 2;

        // Apply damage to player
        player.Combat.TakeDamage(damage);

        return (true, damage, critical);
    }

    // Check battle outcome
    public static BattleResult CheckBattleResult(PlayerAccount player)
    {
        if (!player.Combat.IsInCombat || player.Combat.CurrentOpponent == null)
            return BattleResult.None;

        var opponent = player.Combat.CurrentOpponent;

        if (opponent.CurrentHealth <= 0)
        {
            player.Combat.BattlesWon++;
            player.Gold += opponent.GoldReward;

            // Award XP and check level up
            player.CurrentXP += opponent.XPReward;
            if (player.CurrentXP >= player.MaxXP)
            {
                AccountService.GetCurrentPlayer().Level++;
                AccountService.GetCurrentPlayer().MaxXP = player.Level * 100;
                AccountService.GetCurrentPlayer().CurrentXP = 0;
                AccountService.GetCurrentPlayer().LevelProgress = 0.0;
                AccountService.GetCurrentPlayer().Gold += player.Level * 50; // Level up bonus
            }
            else
            {
                player.LevelProgress = (double)player.CurrentXP / player.MaxXP;
            }

            player.Combat.IsInCombat = false;
            player.Combat.CurrentOpponent = null;
            return BattleResult.Victory;
        }

        if (player.Combat.CurrentHealth <= 0)
        {
            player.HospitalVisits++; // Track in profile stats!
            player.Combat.IsInCombat = false;
            player.Combat.CurrentOpponent = null;
            player.Combat.HealFully(); // Auto-heal after defeat (with hospital visit penalty)
            return BattleResult.Defeat;
        }

        return BattleResult.Ongoing;
    }

    public enum BattleResult
    {
        None,
        Ongoing,
        Victory,
        Defeat
    }
}