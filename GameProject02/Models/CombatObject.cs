using GameProject02.Models;
using System;
using System.ComponentModel;

namespace GameProject02.Models;

public class CombatObject
{
    // Combat stats (calculated from multiple systems)
    public int TotalHealth { get; private set; } = 100;
    public int CurrentHealth { get; set; } = 100;
    public int AttackPower { get; private set; } = 10;
    public int DefensePower { get; private set; } = 5;
    public int Speed { get; private set; } = 5;
    public int CriticalChance { get; private set; } = 5; // %

    // Battle state
    public bool IsInCombat { get; set; } = false;
    public Opponent CurrentOpponent { get; set; } = null;
    public int BattlesFought { get; set; } = 0;
    public int BattlesWon { get; set; } = 0;

    // Recalculate combat stats based on player's overall progression
    // (Matches original game's d.h0() and d.q0() multi-system calculation)
    public void RecalculateStats(PlayerAccount player)
    {
        // Total Health = Base + (Strength * 8) + (Defense * 5) + (Level * 15) + (Intelligence * 2) + VIP bonus
        TotalHealth = 50 +
                     (player.Strength * 8) +
                     (player.Defense * 5) +
                     (player.Level * 15) +
                     (player.Intelligence * 2);

        if (player.IsVIP)
            TotalHealth = (int)(TotalHealth * 1.2); // 20% VIP bonus

        CurrentHealth = TotalHealth;

        // Attack Power = Strength + (Dexterity / 2) + (Level / 3)
        AttackPower = player.Strength +
                     (player.Dexterity / 2) +
                     (player.Level / 3);

        // Defense Power = Defense + (Intelligence / 3)
        DefensePower = player.Defense + (player.Intelligence / 3);

        // Speed = Base speed + Dexterity bonus
        Speed = player.Speed + (player.Dexterity / 4);

        // Critical chance = Base 5% + (Dexterity / 10)%
        CriticalChance = 5 + (player.Dexterity / 10);
        CriticalChance = Math.Min(CriticalChance, 50); // Cap at 50%
    }

    // Take damage in combat
    public void TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - damage);
    }

    // Heal after battle (hospital visit)
    public void HealFully()
    {
        CurrentHealth = TotalHealth;
    }

    // Get health percentage for UI display
    public double GetHealthPercentage()
    {
        return TotalHealth > 0 ? (double)CurrentHealth / TotalHealth : 0;
    }
}