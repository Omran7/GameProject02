namespace GameProject02.Models;

// 🛍️ Shop Item Model
public class ShopItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageResource { get; set; } = "shop_default";
    public int CostDiamonds { get; set; } = 0;
    public int RewardMerits { get; set; } = 0;
    public int RewardMedals { get; set; } = 0;
    public int RewardDiamonds { get; set; } = 0;
    public int RewardGold { get; set; } = 0;
    public bool IsPurchasable => CostDiamonds > 0 || RewardMerits > 0;
}

// 🎡 Lucky Wheel Reward Model
public class WheelReward
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageResource { get; set; } = "wheel_default";
    public string Type { get; set; } = "Merits"; // Merits, Diamonds, Gold, Medals
    public int Value { get; set; } = 0;
    public int Weight { get; set; } = 10; // Probability weight
    public bool IsCurrency => Type is "Merits" or "Diamonds" or "Gold" or "Medals";
}