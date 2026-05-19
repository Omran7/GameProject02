namespace GameProject02.Models;

// ✅ UNIFIED SKILL CLASS - Combines old game structure + new system properties
public class Skill
{
    // ✅ NEW SYSTEM PROPERTIES
    public int Id { get; set; } = 0;
    public int Level { get; set; } = 0;
    public int Cards { get; set; } = 0; // Points spent on this skill
    public bool IsEquipped { get; set; } = false;
    public int GetUpgradeCost() => (Level + 1) * 10;


    // ✅ OLD GAME PROPERTIES (for backward compatibility)
    public string Name { get; set; } = string.Empty;
    public int Percentage { get; set; } = 0;
    public int BaseValue { get; set; } = 0;
    public int BonusValue { get; set; } = 0;

    // ✅ CONSTRUCTORS
    public Skill() { }

    // Old game constructor (keep for compatibility)
    public Skill(string name, int percentage, int baseValue, int bonusValue)
    {
        Name = name;
        Percentage = percentage;
        BaseValue = baseValue;
        BonusValue = bonusValue;
    }

    // New system constructor
    public Skill(int id, string name, int level = 0)
    {
        Id = id;
        Name = name;
        Level = level;
    }

    // ✅ HELPER: Get multiplier for this skill level
    public double GetMultiplier(double baseMultiplier = 1.0, double perLevelBonus = 0.1)
    {
        return baseMultiplier + (Level * perLevelBonus);
    }
}