using System;
using System.Collections.Generic;

namespace GameProject02.Models;

public class MuseumObject
{
    public int MuseumSpaces { get; set; } = 1;
    public int MaxMuseumSpaces { get; set; } = 999; // ✅ MODIFIED: unlimited
    public List<MuseumItem> Items { get; set; } = new();
    public int BackgroundId { get; set; } = 0;
    public List<int> UnlockedBackgrounds { get; set; } = new() { 0 };

    public string GetBackgroundResource(int backgroundId)
    {
        return backgroundId switch
        {
            0 => "museum_background_wood",
            1 => "museum_background_pink",
            2 => "museum_background_classic",
            3 => "museum_background_laser",
            4 => "museum_background_shell",
            5 => "museum_background_forest",
            6 => "museum_background_universe",
            _ => "museum_background_wood"
        };
    }
}

public class MuseumItem
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = "بند غير معروف";
    public string ImageResource { get; set; } = "item_unknown";
    public int Quantity { get; set; } = 1;
    public int OriginalPrice { get; set; } = 0;

    // ✅ NEW: Weapon/Armor stats
    public int Damage { get; set; } = 0;
    public int Accuracy { get; set; } = 0;
    public int Defense { get; set; } = 0;
    public int Evasion { get; set; } = 0;
    public bool IsWeapon { get; set; } = false;
    public bool IsGun { get; set; } = false;
    public int GunType { get; set; } = -1;
}