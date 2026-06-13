using GameProject02.Services;

namespace GameProject02.Models;

// ✅ AUTHENTIC OLD GAME ARMING STRUCTURE (FROM DECOMPILED FILES)
public class ArmingObject
{
    // Weapon slot (from market category 0)
    public string WeaponId { get; set; } = string.Empty;
    public int WeaponLevel { get; set; } = 0;

    // Armor slot (from market category 1)
    public string ArmorId { get; set; } = string.Empty;
    public int ArmorLevel { get; set; } = 0;

    // Special Equipment slot (from market category 3)
    public string SpecialEquipmentId { get; set; } = string.Empty;
    public int SpecialEquipmentLevel { get; set; } = 0;

    // Biochemical slot (from market category 4)
    public string BioChemicalId { get; set; } = string.Empty;
    public int BioChemicalLevel { get; set; } = 0;

    // ✅ CHECK IF ITEM IS EQUIPPED (FOR STOCK FILTERING)
    public bool IsItemEquipped(string itemId)
    {
        return WeaponId == itemId ||
               ArmorId == itemId ||
               SpecialEquipmentId == itemId ||
               BioChemicalId == itemId;
    }

    // ✅ GET TOTAL WEAPON DAMAGE (FOR FIGHT CALCULATIONS)
    public int GetTotalWeaponDamage()
    {
        if (string.IsNullOrEmpty(WeaponId)) return 0;
        var weapon = MarketService.GetItemById(WeaponId);
        return weapon?.Damage ?? 0;
    }

    // ✅ GET TOTAL ARMOR DEFENSE (FOR FIGHT CALCULATIONS)
    public int GetTotalArmorDefense()
    {
        if (string.IsNullOrEmpty(ArmorId)) return 0;
        var armor = MarketService.GetItemById(ArmorId);
        return armor?.Defense ?? 0;
    }
}