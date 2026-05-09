using GameProject02.Models;
using System;

namespace GameProject02.Services;

public static class GymService
{
    // Check if player can start training (has energy)
    public static bool CanStartTraining(PlayerAccount player)
    {
        return player.Energy > 0;
    }

    // Train with ENERGY ALLOCATION PER STAT
    // energyPerStat = [Strength, Defense, Speed, Dexterity]
    public static (bool success, string message, int energySpent) Train(PlayerAccount player, int[] energyPerStat)
    {
        return player.Gym.Train(player, energyPerStat);
    }

    // Get energy percentage for UI display (0.0 to 1.0)
    public static double GetEnergyPercentage(PlayerAccount player)
    {
        return player.MaxEnergy > 0 ? (double)player.Energy / player.MaxEnergy : 0;
    }

    // ── RegenerateEnergy حُذفت — يتولى RegenerationService هذه المهمة الآن ──
}