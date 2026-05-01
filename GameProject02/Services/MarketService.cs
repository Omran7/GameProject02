using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class MarketService
{
    // المصدر الوحيد للحقيقة: يحتفظ بجميع العناصر التي تم توليدها
    private static Dictionary<string, MarketItem> _allItemsCache = new();
    private static Random _random = new Random();
    private static int _currentRoseCrystalMinuteOffset = -1;
    private static DateTime _lastOffsetChangeDate = DateTime.MinValue;

    private const int MAX_STOCK = 100;

    public static List<MarketItem> GetItemsByCategory(int categoryType, int subCategory = 0)
    {
        // 1. الحصول على العناصر الأساسية (إذا لم تكن موجودة، يتم إنشاؤها)
        var baseItems = GetOrCreateBaseItems(categoryType, subCategory);

        // 2. نعمل على نسخة من العناصر (حتى لا نعدل الكاش مباشرة أثناء المعالجة)
        var resultItems = baseItems.Select(item => new MarketItem
        {
            ItemId = item.ItemId,
            Name = item.Name,
            Description = item.Description,
            ImageResource = item.ImageResource,
            CategoryType = item.CategoryType,
            PriceGold = item.PriceGold,
            PriceCheck = item.PriceCheck,
            MaxPurchaseQuantity = item.MaxPurchaseQuantity,
            Damage = item.Damage,
            Accuracy = item.Accuracy,
            Defense = item.Defense,
            Evasion = item.Evasion,
            GunType = item.GunType,
            IsWeapon = item.IsWeapon,
            IsGun = item.IsGun,
            SpecialEquipmentType = item.SpecialEquipmentType,
            Happiness = item.Happiness,
            CurrentStock = item.CurrentStock,
            DefaultStock = item.DefaultStock,
            LastRestockTime = item.LastRestockTime,
            RestockMinuteOffset = item.RestockMinuteOffset
        }).ToList();

        // 3. معالجة التجديد لكل عنصر في النسخة
        foreach (var item in resultItems)
        {
            ProcessRestock(item);
        }

        // 4. تحديث الكاش بالقيم الجديدة بعد التجديد
        foreach (var item in resultItems)
        {
            if (_allItemsCache.TryGetValue(item.ItemId, out var cachedItem))
            {
                cachedItem.CurrentStock = item.CurrentStock;
                cachedItem.LastRestockTime = item.LastRestockTime;
            }
        }

        return resultItems;
    }

    private static List<MarketItem> GetOrCreateBaseItems(int categoryType, int subCategory)
    {
        // نحاول جلب العناصر من الكاش حسب الفئة
        var items = _allItemsCache.Values
            .Where(i => i.CategoryType == categoryType)
            .ToList();

        // إذا كانت الفئة فارغة في الكاش، ننشئها
        if (items.Count == 0)
        {
            items = GenerateBaseItems(categoryType, subCategory);
            foreach (var item in items)
            {
                _allItemsCache[item.ItemId] = item;
            }
        }

        return items;
    }

    private static void ProcessRestock(MarketItem item)
    {
        DateTime now = DateTime.UtcNow;

        int minuteOffset = GetMinuteOffsetForItem(item);
        DateTime nextRestock = CalculateNextRestockTime(item.LastRestockTime, minuteOffset);

        if (now >= nextRestock)
        {
            item.CurrentStock = item.DefaultStock;
            item.LastRestockTime = now;
        }
    }

    private static int GetMinuteOffsetForItem(MarketItem item)
    {
        if (item.CategoryType == 5)
        {
            return GetRoseCrystalMinuteOffset();
        }
        return 0;
    }

    private static int GetRoseCrystalMinuteOffset()
    {
        DateTime today = DateTime.UtcNow.Date;
        if (_lastOffsetChangeDate < today)
        {
            _currentRoseCrystalMinuteOffset = _random.Next(0, 60);
            _lastOffsetChangeDate = today;
        }
        return _currentRoseCrystalMinuteOffset;
    }

    public static DateTime CalculateNextRestockTime(DateTime lastRestock, int minuteOffset)
    {
        DateTime baseTime = new DateTime(lastRestock.Year, lastRestock.Month, lastRestock.Day, lastRestock.Hour, minuteOffset, 0);
        if (minuteOffset > 0 && baseTime.Minute != minuteOffset)
        {
            if (baseTime.Minute < minuteOffset)
                baseTime = baseTime.AddMinutes(minuteOffset - baseTime.Minute);
            else
                baseTime = baseTime.AddHours(1).AddMinutes(minuteOffset - 60);
        }

        if (lastRestock >= baseTime)
        {
            baseTime = baseTime.AddHours(1);
        }

        return baseTime;
    }

    public static DateTime GetNextRestockTime(MarketItem item)
    {
        if (!_allItemsCache.TryGetValue(item.ItemId, out var cachedItem))
            cachedItem = item;

        int minuteOffset = GetMinuteOffsetForItem(cachedItem);
        return CalculateNextRestockTime(cachedItem.LastRestockTime, minuteOffset);
    }

    private static List<MarketItem> GenerateBaseItems(int categoryType, int subCategory)
    {
        var items = new List<MarketItem>();

        // --- جميع العناصر كما هي بدون تغيير (نفس الكود السابق) ---
        switch (categoryType)
        {
            case 0:
                switch (subCategory)
                {
                    case 0:
                        items.Add(new MarketItem { ItemId = "gun_10001", Name = "مقلاع خشب", ImageResource = "market_gun_5", PriceGold = 7, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 0, Damage = 2, Accuracy = 2 });
                        items.Add(new MarketItem { ItemId = "gun_10002", Name = "طابة الشوك", ImageResource = "market_gun_1", PriceGold = 7, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 0, Damage = 5, Accuracy = 14 });
                        items.Add(new MarketItem { ItemId = "gun_10003", Name = "خنجر صغير", ImageResource = "market_gun_2", PriceGold = 70, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 0, Damage = 36, Accuracy = 59 });
                        items.Add(new MarketItem { ItemId = "gun_10004", Name = "سيف قديم", ImageResource = "market_gun_3", PriceGold = 70, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 0, Damage = 47, Accuracy = 44 });
                        items.Add(new MarketItem { ItemId = "gun_10005", Name = "فأس طويل", ImageResource = "market_gun_4", PriceGold = 140, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 0, Damage = 38, Accuracy = 45 });
                        break;
                    case 1:
                        items.Add(new MarketItem { ItemId = "gun_11001", Name = "8/5 روسي", ImageResource = "market_pistol_1", PriceGold = 1400, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 1, Damage = 35, Accuracy = 9 });
                        items.Add(new MarketItem { ItemId = "gun_11002", Name = "9/14 تشيكي", ImageResource = "market_pistol_2", PriceGold = 2100, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 1, Damage = 42, Accuracy = 5 });
                        items.Add(new MarketItem { ItemId = "gun_11003", Name = "قلاب 6", ImageResource = "market_pistol_3", PriceGold = 3500, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 1, Damage = 48, Accuracy = 8 });
                        items.Add(new MarketItem { ItemId = "gun_11004", Name = "7 بريتا", ImageResource = "market_pistol_4", PriceGold = 5600, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 1, Damage = 43, Accuracy = 10 });
                        break;
                    case 2:
                        items.Add(new MarketItem { ItemId = "gun_12001", Name = "اوزري", ImageResource = "market_smallmachen_1", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 2, Damage = 64, Accuracy = 19 });
                        items.Add(new MarketItem { ItemId = "gun_12002", Name = "سنوبال", ImageResource = "market_smallmachen_2", PriceGold = 3000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 2, Damage = 67, Accuracy = 13 });
                        items.Add(new MarketItem { ItemId = "gun_12003", Name = "سبيكتر", ImageResource = "market_smallmachen_3", PriceGold = 3000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 2, Damage = 53, Accuracy = 24 });
                        items.Add(new MarketItem { ItemId = "gun_12004", Name = "ميني ا ك", ImageResource = "market_smallmachen_4", PriceGold = 3000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 2, Damage = 63, Accuracy = 11 });
                        break;
                    case 3:
                        items.Add(new MarketItem { ItemId = "gun_13001", Name = "كسرية", ImageResource = "market_shotgun_1", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 3, Damage = 46, Accuracy = 19 });
                        items.Add(new MarketItem { ItemId = "gun_13002", Name = "اوتماتيك", ImageResource = "market_shotgun_2", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 3, Damage = 44, Accuracy = 20 });
                        items.Add(new MarketItem { ItemId = "gun_13003", Name = "جفت دبل", ImageResource = "market_shotgun_3", PriceGold = 3000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 3, Damage = 40, Accuracy = 24 });
                        break;
                    case 4:
                        items.Add(new MarketItem { ItemId = "gun_14001", Name = "فاماس", ImageResource = "market_machine_1", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 4, Damage = 72, Accuracy = 29 });
                        items.Add(new MarketItem { ItemId = "gun_14002", Name = "أ ك اسود", ImageResource = "market_machine_2", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 4, Damage = 60, Accuracy = 29 });
                        items.Add(new MarketItem { ItemId = "gun_14003", Name = "كلاشنكوف", ImageResource = "market_machine_3", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 4, Damage = 73, Accuracy = 19 });
                        items.Add(new MarketItem { ItemId = "gun_14004", Name = "سكار", ImageResource = "market_machine_4", PriceGold = 3000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 4, Damage = 71, Accuracy = 24 });
                        break;
                    case 5:
                        items.Add(new MarketItem { ItemId = "gun_15001", Name = "مكنظمة", ImageResource = "market_sniper_1", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 5, Damage = 54, Accuracy = 59 });
                        items.Add(new MarketItem { ItemId = "gun_15002", Name = "ماغنوم", ImageResource = "market_sniper_2", PriceGold = 3000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 5, Damage = 61, Accuracy = 49 });
                        break;
                    case 6:
                        items.Add(new MarketItem { ItemId = "gun_16001", Name = "دوشكا", ImageResource = "market_heavy_1", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 6, Damage = 99, Accuracy = 15 });
                        items.Add(new MarketItem { ItemId = "gun_16002", Name = "بي كي سي", ImageResource = "market_heavy_2", PriceGold = 3000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 6, Damage = 80, Accuracy = 32 });
                        break;
                    case 7:
                        items.Add(new MarketItem { ItemId = "gun_17001", Name = "ار بي جي", ImageResource = "market_rbg_1", PriceGold = 2000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 7, Damage = 90, Accuracy = 29 });
                        items.Add(new MarketItem { ItemId = "gun_17002", Name = "قاذف قنابل", ImageResource = "market_rbg_2", PriceGold = 3000, CategoryType = 0, IsWeapon = true, IsGun = true, GunType = 7, Damage = 82, Accuracy = 32 });
                        break;
                }
                break;
            case 1:
                items.Add(new MarketItem { ItemId = "armor_20001", Name = "درع تالف", ImageResource = "market_armor_1", PriceGold = 350, CategoryType = 1, Defense = 5, Evasion = 9 });
                items.Add(new MarketItem { ItemId = "armor_20002", Name = "خوذة الفايكنغ", ImageResource = "market_armor_2", PriceGold = 350, CategoryType = 1, Defense = 30, Evasion = 29 });
                items.Add(new MarketItem { ItemId = "armor_20003", Name = "خوذة الفارس", ImageResource = "market_armor_3", PriceGold = 350, CategoryType = 1, Defense = 35, Evasion = 33 });
                items.Add(new MarketItem { ItemId = "armor_20004", Name = "سترة جلد", ImageResource = "market_armor_4", PriceGold = 350, CategoryType = 1, Defense = 50, Evasion = 45 });
                items.Add(new MarketItem { ItemId = "armor_20005", Name = "واقي كامل", ImageResource = "market_armor_5", PriceGold = 350, CategoryType = 1, Defense = 20, Evasion = 21 });
                items.Add(new MarketItem { ItemId = "armor_20006", Name = "سترة حديدية", ImageResource = "market_armor_6", PriceGold = 700, CategoryType = 1, Defense = 40, Evasion = 37 });
                break;
            case 2:
                items.Add(new MarketItem { ItemId = "grocery_30001", Name = "خاتم ألماس", ImageResource = "market_ring_1", PriceGold = 70000, CategoryType = 2 });
                items.Add(new MarketItem { ItemId = "tool_sunglasses", Name = "نظارة شمسية", ImageResource = "tool_sunglasses", PriceGold = 18, CategoryType = 2 });
                items.Add(new MarketItem { ItemId = "tool_blank_cd", Name = "دي في دي", ImageResource = "", PriceGold = 21, CategoryType = 2 });
                items.Add(new MarketItem { ItemId = "grocery_30004", Name = "سماعات", ImageResource = "market_speakers_1", PriceGold = 70000, CategoryType = 2 });
                break;
            case 4:
                items.Add(new MarketItem { ItemId = "pharmacy_50001", Name = "لصقة طبية", ImageResource = "market_medical_1", PriceGold = 140, CategoryType = 4 });
                items.Add(new MarketItem { ItemId = "pharmacy_50002", Name = "دقيق", ImageResource = "", PriceGold = 7000, CategoryType = 4 });
                break;
            case 5:
                items.Add(new MarketItem { ItemId = "roses_&_crystal_601", Name = "أقحوان", ImageResource = "market_rose_1", PriceGold = 70, CategoryType = 5 });
                items.Add(new MarketItem { ItemId = "roses_&_crystal_602", Name = "كوارتز شفاف", ImageResource = "market_crystal_1", PriceGold = 70, CategoryType = 5 });
                break;
        }

        foreach (var item in items)
        {
            item.DefaultStock = MAX_STOCK;
            item.CurrentStock = MAX_STOCK;
        }

        return items;
    }

    public static bool TryPurchaseItem(string itemId, int quantity, out int newStock)
    {
        newStock = 0;
        if (!_allItemsCache.TryGetValue(itemId, out var item))
            return false;

        if (item.CurrentStock < quantity)
            return false;

        item.CurrentStock -= quantity;
        newStock = item.CurrentStock;
        return true;
    }

    public static string GetCategoryName(int categoryType, int subCategory = 0)
    {
        return categoryType switch
        {
            0 => subCategory switch { 0 => "أسلحة بيضاء", 1 => "مسدسات", 2 => "رشاشات صغيرة", 3 => "بنادق صيد", 4 => "رشاشات", 5 => "قناصات", 6 => "رشاشات ثقيلة", 7 => "أسلحة ثقيلة", _ => "الأسلحة" },
            1 => "الدروع",
            2 => "البقالة",
            3 => "متطلبات وظيفية",
            4 => "الصيدلية",
            5 => "ورد وكريستال",
            6 => "طعام",
            _ => ""
        };
    }

    public static MarketItem GetItemById(string itemId)
    {
        _allItemsCache.TryGetValue(itemId, out var item);
        return item;
    }
}