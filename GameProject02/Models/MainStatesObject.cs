using System;

namespace GameProject02.Models;

public class MainStatesObject
{
    // ✅ AUTHENTIC OLD GAME FIELDS
    public int Level { get; set; } = 1;
    public long CurrentExperience { get; set; } = 0;

    // Resource stats
    public int CourageCurrent { get; set; } = 100;
    public int CourageMax { get; set; } = 100;
    public int HealthCurrent { get; set; } = 100;
    public int HealthMax { get; set; } = 100;

    // Player stats affecting crime success
    public int Dexterity { get; set; } = 10;
    public int Strength { get; set; } = 10;

    // ✅ GET XP REQUIRED FOR NEXT LEVEL (AUTHENTIC FORMULA)
    public long GetXpRequiredForNextLevel()
    {
        // Level 1 → 100 XP, Level 2 → 300 XP, Level 3 → 600 XP...
        return Level * (Level + 1) * 50L;
    }

    // ✅ CHECK IF LEVEL UP POSSIBLE
    public bool CanLevelUp()
    {
        return CurrentExperience >= GetXpRequiredForNextLevel();
    }

    // ✅ PERFORM LEVEL UP (AUTHENTIC OLD GAME BEHAVIOR)
    public void LevelUp()
    {
        if (!CanLevelUp()) return;

        Level++;
        CurrentExperience -= GetXpRequiredForNextLevel();

        // ✅ RESTORE COURAGE TO MAX (AUTHENTIC OLD GAME)
        CourageCurrent = CourageMax;

        // ✅ INCREASE MAX COURAGE (2 points per level)
        CourageMax += 2;

        // ✅ INCREASE MAX HEALTH (5 points per level)
        HealthMax += 5;

        // ✅ INCREASE STATS (1 point per 5 levels)
        if (Level % 5 == 0)
        {
            Dexterity++;
            Strength++;
        }
    }

    // ✅ ADD EXPERIENCE (TRIGGERS AUTO LEVEL-UP)
    public bool AddExperience(long amount)
    {
        if (amount <= 0) return false;

        CurrentExperience += amount;
        bool leveledUp = false;

        // ✅ HANDLE MULTIPLE LEVEL-UPS
        while (CanLevelUp())
        {
            LevelUp();
            leveledUp = true;
        }

        return leveledUp;
    }

    // ✅ GET LEVEL PROGRESS PERCENTAGE (FOR UI)
    public double GetLevelProgressPercentage()
    {
        long required = GetXpRequiredForNextLevel();
        if (required <= 0) return 100.0;

        return Math.Min(100.0, (CurrentExperience * 100.0) / required);
    }
}