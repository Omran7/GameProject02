namespace GameProject02.Models;

public class BlackMarketRecipe
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = "عنصر غير معروف";
    public string ImageResource { get; set; } = "item_unknown";
    public string Description { get; set; } = string.Empty;

    // Requirement
    public string RequiredItemType { get; set; } = string.Empty; // Stock Item ID
    public int RequiredAmount { get; set; } = 1;

    // Reward
    public string RewardItemType { get; set; } = string.Empty; // "Diamonds", "Checks", "food_x", etc.
    public int RewardAmount { get; set; } = 1;

    public int MinQuantity { get; set; } = 1;
    public int MaxQuantity { get; set; } = 50;
    public int MarketStock { get; set; } = 999;
}