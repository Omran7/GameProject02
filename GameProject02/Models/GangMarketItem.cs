namespace GameProject02.Models;

public class GangMarketItem
{
    public string SellerId { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ImageResource { get; set; } = "item_unknown";
    public int Quantity { get; set; }
    public int PricePerItem { get; set; }
    public int OriginalPrice { get; set; }
    public int CategoryId { get; set; } // 0=weapons, 1=armor, 2=grocery, 4=pharmacy, 5=crystal
    public int SubCategoryId { get; set; } = -1; // For weapons: 0=white,1=pistols,2=smg,3=shotgun,4=rifles,5=sniper,6=heavy,7=launchers
    public string CategoryName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public bool IsWeapon { get; set; }
    public bool IsGun { get; set; }
    public int Damage { get; set; }
    public int Accuracy { get; set; }
    public int Defense { get; set; }
    public int Evasion { get; set; }
    public int GunType { get; set; } = -1;

    public string DisplayPrice => $"{PricePerItem:N0} ذهب";
    public string DisplayQuantity => $"x{Quantity}";
    public string DisplaySeller => $"البائع: {SellerName}";
}