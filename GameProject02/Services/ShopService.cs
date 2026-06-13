using GameProject02.Models;
using System.Collections.Generic;

namespace GameProject02.Services;

public static class ShopService
{
    // 🛍️ Shop Catalog (Based on Old Game Packages)
    public static List<ShopItem> GetShopItems() => new()
    {
        new ShopItem { Id = "merit_s", Name = "حزمة استحقاقات صغيرة", Description = "20 استحقاق لتفعيل المهارات", CostDiamonds = 50, RewardMerits = 20, ImageResource = "shop_merit_small" },
        new ShopItem { Id = "merit_m", Name = "حزمة استحقاقات متوسطة", Description = "100 استحقاق + 5 ميداليات", CostDiamonds = 200, RewardMerits = 100, RewardMedals = 5, ImageResource = "shop_merit_med" },
        new ShopItem { Id = "merit_l", Name = "حزمة استحقاقات كبيرة", Description = "300 استحقاق + 15 ميدالية", CostDiamonds = 500, RewardMerits = 300, RewardMedals = 15, ImageResource = "shop_merit_large" },
        new ShopItem { Id = "merit_xl", Name = "حزمة استحقاقات ضخمة", Description = "700 استحقاق + 40 ميدالية", CostDiamonds = 1000, RewardMerits = 700, RewardMedals = 40, ImageResource = "shop_merit_mega" },
        new ShopItem { Id = "diamond_pack", Name = "حزمة ماس صغيرة", Description = "50 ماس", CostDiamonds = 0, RewardDiamonds = 50, ImageResource = "shop_diamonds" },
    };

    // 💰 Purchase Logic
    public static (bool success, string message) BuyItem(PlayerAccount player, ShopItem item)
    {
        if (player.Diamonds < item.CostDiamonds)
            return (false, $"لا تملك ما يكفي من الماس!\nتحتاج {item.CostDiamonds} ولديك {player.Diamonds}.");

        // Deduct cost & Apply rewards
        player.Diamonds -= item.CostDiamonds;
        player.Merits += item.RewardMerits;
        player.Medals += item.RewardMedals;
        player.Diamonds += item.RewardDiamonds;
        player.Gold += item.RewardGold;

        AccountService.SavePlayer(player);
        return (true, $"✅ تم الشراء بنجاح!\nحصلت على:\n{FormatRewards(item)}");
    }

    private static string FormatRewards(ShopItem item)
    {
        var list = new List<string>();
        if (item.RewardMerits > 0) list.Add($"{item.RewardMerits} استحقاق");
        if (item.RewardMedals > 0) list.Add($"{item.RewardMedals} ميدالية");
        if (item.RewardDiamonds > 0) list.Add($"{item.RewardDiamonds} ماس");
        if (item.RewardGold > 0) list.Add($"{item.RewardGold:N0} ذهب");
        return string.Join("\n", list);
    }
}