namespace GameProject02.Models;

public class ShopListing
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = "بند غير معروف";
    public string ImageResource { get; set; } = "item_unknown";
    public int Quantity { get; set; } = 0; // Max 25 per listing
    public int PricePerItem { get; set; } = 0; // Player-set price (no limits)
    public int OriginalPrice { get; set; } = 0; // For reference only
    public bool IsActive { get; set; } = true;
}