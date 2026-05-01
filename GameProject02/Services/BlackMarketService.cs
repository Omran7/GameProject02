using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class BlackMarketService
{
    // ✅ AUTHENTIC OLD GAME RECIPES (MAPPED EXACTLY TO YOUR REQUEST)
    public static List<BlackMarketRecipe> GetRecipesByCategory(string category)
    {
        return category switch
        {
            "Diamonds" => new List<BlackMarketRecipe>
            {
                new BlackMarketRecipe {
                    Id = "bm_flower_to_diamond", Name = "تحويل الزهور إلى ألماس", ImageResource = "market_rose_1",
                    RequiredItemType = "flower_rose", RequiredAmount = 10,
                    RewardItemType = "Diamonds", RewardAmount = 1, MinQuantity = 1, MaxQuantity = 50, MarketStock = 999
                },
                new BlackMarketRecipe {
                    Id = "bm_bomb_to_diamond", Name = "تحويل شظايا القنابل إلى ألماس", ImageResource = "market_bomb_fragment",
                    RequiredItemType = "bomb_fragment", RequiredAmount = 5,
                    RewardItemType = "Diamonds", RewardAmount = 1, MinQuantity = 1, MaxQuantity = 50, MarketStock = 999
                }
            },
            "Checks" => new List<BlackMarketRecipe>
            {
                new BlackMarketRecipe {
                    Id = "bm_crystal_to_check", Name = "تحويل الكريستال إلى شيكات", ImageResource = "market_crystal_1",
                    RequiredItemType = "crystal_quartz", RequiredAmount = 15,
                    RewardItemType = "Checks", RewardAmount = 1, MinQuantity = 1, MaxQuantity = 50, MarketStock = 999
                }
            },
            "Food" => new List<BlackMarketRecipe>
            {
                new BlackMarketRecipe {
                    Id = "bm_food_fish", Name = "طبق السمك المشوي", ImageResource = "market_food_fish",
                    RequiredItemType = "ingredient_fish", RequiredAmount = 3, RewardItemType = "food_grilled_fish", RewardAmount = 1, MinQuantity = 1, MaxQuantity = 10
                },
                new BlackMarketRecipe {
                    Id = "bm_food_seafood", Name = "طبق المأكولات البحرية", ImageResource = "market_food_seafood",
                    RequiredItemType = "ingredient_shellfish", RequiredAmount = 4, RewardItemType = "food_seafood", RewardAmount = 1, MinQuantity = 1, MaxQuantity = 10
                },
                new BlackMarketRecipe {
                    Id = "bm_food_sushi", Name = "طبق السوشي", ImageResource = "market_food_sushi",
                    RequiredItemType = "ingredient_rice", RequiredAmount = 2, RewardItemType = "food_sushi", RewardAmount = 1, MinQuantity = 1, MaxQuantity = 10
                },
                new BlackMarketRecipe {
                    Id = "bm_food_lamb", Name = "طبق اللحم", ImageResource = "market_food_lamb",
                    RequiredItemType = "ingredient_lamb", RequiredAmount = 2, RewardItemType = "food_lamb", RewardAmount = 1, MinQuantity = 1, MaxQuantity = 10
                },
                new BlackMarketRecipe {
                    Id = "bm_food_cake", Name = "كيكة الشوكولاتة", ImageResource = "market_food_cake",
                    RequiredItemType = "ingredient_flour", RequiredAmount = 5, RewardItemType = "food_chocolate_cake", RewardAmount = 1, MinQuantity = 1, MaxQuantity = 10
                }
            },
            _ => new List<BlackMarketRecipe>()
        };
    }

    // ✅ VALIDATE CONVERSION
    public static (bool canConvert, string message) CanConvert(PlayerAccount player, BlackMarketRecipe recipe, int quantity)
    {
        if (player == null || recipe == null) return (false, "بيانات غير صالحة");
        if (quantity < recipe.MinQuantity || quantity > recipe.MaxQuantity) return (false, $"الكمية يجب أن تكون بين {recipe.MinQuantity} و {recipe.MaxQuantity}");
        if (recipe.MarketStock < quantity) return (false, "الكمية غير متوفرة حالياً في السوق");

        long totalRequired = (long)recipe.RequiredAmount * quantity;

        // ✅ CHECK PLAYER STOCK
        if (!player.StockObject.ItemsInStock.TryGetValue(recipe.RequiredItemType, out var stockItem))
            return (false, "لا تملك المواد المطلوبة في المخزن");

        if (stockItem.Count < totalRequired)
            return (false, $"تحتاج {(totalRequired - stockItem.Count)} {stockItem.Name} إضافية");

        if (stockItem.UsedInArming)
            return (false, "لا يمكن تحويل المواد المجهزة حالياً");

        return (true, "");
    }

    // ✅ EXECUTE CONVERSION
    public static (bool success, string message) ExecuteConversion(PlayerAccount player, BlackMarketRecipe recipe, int quantity)
    {
        var (canConvert, errorMsg) = CanConvert(player, recipe, quantity);
        if (!canConvert) return (false, errorMsg);

        long totalRequired = (long)recipe.RequiredAmount * quantity;
        long totalReward = (long)recipe.RewardAmount * quantity;

        // ✅ DEDUCT FROM STOCK
        if (player.StockObject.ItemsInStock.TryGetValue(recipe.RequiredItemType, out var reqStock))
        {
            reqStock.Count -= (int)totalRequired;
            reqStock.UsedInArming = false;
            if (reqStock.Count <= 0) player.StockObject.ItemsInStock.Remove(reqStock.ItemId);
        }

        // ✅ ADD REWARD (Diamonds / Checks / Food)
        if (recipe.RewardItemType == "Diamonds")
        {
            player.Diamonds += (int)totalReward;
        }
        else if (recipe.RewardItemType == "Checks")
        {
            player.Checks += totalReward;
        }
        else
        {
            // Add Food Item to Stock
            if (!player.StockObject.ItemsInStock.TryGetValue(recipe.RewardItemType, out var rewardStock))
            {
                rewardStock = new StockItem
                {
                    ItemId = recipe.RewardItemType,
                    Name = recipe.Name,
                    ImageResource = recipe.ImageResource,
                    Count = 0,
                    OriginalPrice = 0,
                    CategoryId = 6, // Food category
                    UsedInArming = false
                };
                player.StockObject.ItemsInStock[recipe.RewardItemType] = rewardStock;
                player.StockObject.StockFreeSpace -= quantity;
            }
            rewardStock.Count += (int)totalReward;
        }

        recipe.MarketStock -= quantity;
        AccountService.SavePlayer(player);
        return (true, $"✅ تمت عملية التحويل بنجاح! ({quantity} وحدة)");
    }
}