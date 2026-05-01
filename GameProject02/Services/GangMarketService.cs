using GameProject02.Models;
using GameProject02.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class GangMarketService
{
    // Weapon subcategories mapping (same as MarketService)
    private static readonly Dictionary<int, string> _weaponSubCategories = new()
    {
        { 0, "سلاح ابيض" },
        { 1, "المسدسات" },
        { 2, "الرشاشات الصغيرة" },
        { 3, "بنادق الصيد" },
        { 4, "رشاشات" },
        { 5, "القناصات" },
        { 6, "رشاشات ثقيلة" },
        { 7, "قواذف" }
    };

    public static List<CategoryInfo> GetAvailableCategories()
    {
        var allListings = GetAllListings();
        var categories = new Dictionary<int, string>
        {
            { 0, "الأسلحة" },
            { 1, "الدروع" },
            { 2, "البقالة" },
            { 4, "الصيدلية" },
            { 5, "ورد وكريستال" }
        };

        var available = new List<CategoryInfo>();
        foreach (var cat in categories)
        {
            if (allListings.Any(l => l.CategoryId == cat.Key))
            {
                available.Add(new CategoryInfo
                {
                    Id = cat.Key,
                    Name = cat.Value,
                    ImageResource = GetCategoryImage(cat.Key),
                    HasSubCategories = (cat.Key == 0) // Only weapons have subcategories
                });
            }
        }
        return available;
    }

    public static List<SubCategoryInfo> GetAvailableSubCategories(int categoryId)
    {
        if (categoryId != 0) return new List<SubCategoryInfo>();

        var allListings = GetAllListings();
        var availableSubs = new List<SubCategoryInfo>();

        foreach (var sub in _weaponSubCategories)
        {
            if (allListings.Any(l => l.CategoryId == 0 && l.SubCategoryId == sub.Key))
            {
                availableSubs.Add(new SubCategoryInfo
                {
                    Id = sub.Key,
                    Name = sub.Value,
                    ImageResource = GetSubCategoryImage(sub.Key)
                });
            }
        }
        return availableSubs;
    }

    public static List<GangMarketItem> GetItemsByCategory(int categoryId, int subCategoryId = -1)
    {
        var all = GetAllListings();
        if (categoryId == 0 && subCategoryId >= 0)
            return all.Where(l => l.CategoryId == categoryId && l.SubCategoryId == subCategoryId).ToList();
        return all.Where(l => l.CategoryId == categoryId).ToList();
    }

    private static string GetCategoryImage(int categoryId) => categoryId switch
    {
        0 => "market_weapons.png",
        1 => "market_armors.png",
        2 => "market_grocery.png",
        4 => "market_pharmacy.png",
        5 => "market_crystal.png",
        _ => "market_weapons.png"
    };

    private static string GetSubCategoryImage(int subId) => "market_weapons.png"; // Could differentiate later

    public static List<GangMarketItem> GetAllListings()
    {
        var allPlayers = AccountService.GetAllPlayers();
        var listings = new List<GangMarketItem>();

        foreach (var player in allPlayers)
        {
            if (player.StockObject?.ShopListings == null) continue;

            for (int i = 0; i < player.StockObject.ShopListings.Count; i++)
            {
                var listing = player.StockObject.ShopListings[i];
                if (listing == null || !listing.IsActive || listing.Quantity <= 0) continue;

                // Find stock item to get stats and category
                StockItem? stockItem = null;
                if (player.StockObject.ItemsInStock.TryGetValue(listing.ItemId, out var item))
                    stockItem = item;

                int categoryId = stockItem?.CategoryId ?? GetCategoryFromItemId(listing.ItemId);
                int subCategoryId = -1;
                if (categoryId == 0 && stockItem != null && stockItem.IsGun)
                {
                    // Map GunType to subcategory (0-7)
                    subCategoryId = stockItem.GunType >= 0 ? stockItem.GunType : 0;
                }

                var marketItem = new GangMarketItem
                {
                    SellerId = player.PlayerId,
                    SellerName = player.Username,
                    ItemId = listing.ItemId,
                    ItemName = listing.ItemName,
                    ImageResource = listing.ImageResource,
                    Quantity = listing.Quantity,
                    PricePerItem = listing.PricePerItem,
                    OriginalPrice = listing.OriginalPrice,
                    CategoryId = categoryId,
                    SubCategoryId = subCategoryId,
                    SubCategoryName = subCategoryId >= 0 && _weaponSubCategories.ContainsKey(subCategoryId) ? _weaponSubCategories[subCategoryId] : "",
                    IsWeapon = stockItem?.IsWeapon ?? false,
                    IsGun = stockItem?.IsGun ?? false,
                    Damage = stockItem?.Damage ?? 0,
                    Accuracy = stockItem?.Accuracy ?? 0,
                    Defense = stockItem?.Defense ?? 0,
                    Evasion = stockItem?.Evasion ?? 0,
                    GunType = stockItem?.GunType ?? -1
                };
                listings.Add(marketItem);
            }
        }
        return listings;
    }
    public static List<GangMarketItem> GetItemsByCategoryAndSubCategory(int categoryId, int subCategoryId)
    {
        var allItems = GetAllListings();
        if (categoryId != 0) return allItems.Where(i => i.CategoryId == categoryId).ToList();

        // For weapons, filter by GunType (0-7) to match subcategory
        return allItems.Where(i => i.CategoryId == 0 && i.GunType == subCategoryId).ToList();
    }

    private static int GetCategoryFromItemId(string itemId)
    {
        if (itemId.StartsWith("gun_")) return 0;
        if (itemId.StartsWith("armor_")) return 1;
        if (itemId.StartsWith("grocery_")) return 2;
        if (itemId.StartsWith("pharmacy_")) return 4;
        if (itemId.Contains("crystal") || itemId.Contains("rose")) return 5;
        return 0;
    }

    public static (bool success, string message) BuyItem(PlayerAccount buyer, GangMarketItem item, int quantity)
    {
        // Same as before...
        if (quantity <= 0 || quantity > item.Quantity)
            return (false, "كمية غير صالحة");

        int totalCost = item.PricePerItem * quantity;
        if (buyer.Gold < totalCost)
            return (false, $"ليس لديك {totalCost:N0} ذهب");

        var seller = AccountService.GetPlayerById(item.SellerId);
        if (seller == null)
            return (false, "البائع غير متواجد حالياً");

        var shopListing = seller.StockObject.ShopListings
            .FirstOrDefault(l => l != null && l.ItemId == item.ItemId && l.PricePerItem == item.PricePerItem);
        if (shopListing == null || shopListing.Quantity < quantity)
            return (false, "العنصر غير متوفر حالياً");

        buyer.Gold -= totalCost;
        seller.Gold += totalCost;

        shopListing.Quantity -= quantity;
        if (shopListing.Quantity <= 0)
        {
            int index = seller.StockObject.ShopListings.IndexOf(shopListing);
            seller.StockObject.ShopListings[index] = null;
        }

        if (buyer.StockObject.ItemsInStock.TryGetValue(item.ItemId, out var buyerItem))
        {
            buyerItem.Count += quantity;
        }
        else
        {
            buyer.StockObject.ItemsInStock[item.ItemId] = new StockItem
            {
                ItemId = item.ItemId,
                Name = item.ItemName,
                ImageResource = item.ImageResource,
                Count = quantity,
                OriginalPrice = item.OriginalPrice,
                CategoryId = item.CategoryId,
                IsWeapon = item.IsWeapon,
                IsGun = item.IsGun,
                Damage = item.Damage,
                Accuracy = item.Accuracy,
                Defense = item.Defense,
                Evasion = item.Evasion,
                GunType = item.GunType,
                CountInBag = 0,
                IsLocked = false
            };
        }
        buyer.StockObject.StockFreeSpace -= quantity;

        return (true, $"✅ تم شراء {quantity} × {item.ItemName} من {seller.Username}");
    }
}

public class SubCategoryInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageResource { get; set; } = string.Empty;
}