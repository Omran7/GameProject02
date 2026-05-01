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

    // Train with ENERGY ALLOCATION PER STAT (not minutes!)
    // energyPerStat = [Strength, Defense, Speed, Dexterity] energy values
    public static (bool success, string message, int energySpent) Train(PlayerAccount player, int[] energyPerStat)
    {
        return player.Gym.Train(player, energyPerStat);
    }

    // Regenerate energy over time (call this on app resume/periodically)
    public static void RegenerateEnergy(PlayerAccount player)
    {
        player.RegenerateEnergy();
    }

    // Get energy percentage for UI display (0.0 to 1.0)
    public static double GetEnergyPercentage(PlayerAccount player)
    {
        return (double)player.Energy / player.MaxEnergy;
    }
}