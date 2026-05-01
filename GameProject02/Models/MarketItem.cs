namespace GameProject02.Models;

public class MarketItem
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = "بند غير معروف";
    public string Description { get; set; } = string.Empty;
    public string ImageResource { get; set; } = "item_unknown";
    public int CategoryType { get; set; } = 0; // 0=Guns, 1=Armor, 2=Grocery, etc.

    public long PriceGold { get; set; } = 0;
    public long PriceCheck { get; set; } = 0;

    public int MaxPurchaseQuantity { get; set; } = 10; // Default max 10 per transaction

    // Weapon-specific properties
    public int Damage { get; set; } = 0;
    public int Accuracy { get; set; } = 0;
    public int Defense { get; set; } = 0;
    public int Evasion { get; set; } = 0;
    public int GunType { get; set; } = -1; // -1=Not weapon
    public bool IsWeapon { get; set; } = false;
    public bool IsGun { get; set; } = false;

    // Special equipment properties
    public int SpecialEquipmentType { get; set; } = -1;
    public int Happiness { get; set; } = 0;

    // Stock management
    public int CurrentStock { get; set; } = 100;
    public int DefaultStock { get; set; } = 100;
    public DateTime LastRestockTime { get; set; } = DateTime.UtcNow;
    public int RestockMinuteOffset { get; set; } = -1; // -1 means normal hourly restock at :00
}