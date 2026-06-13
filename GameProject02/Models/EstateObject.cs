using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GameProject02.Models;

public class EstateObject
{
    public bool IsRentedOut { get; set; } = false;      // هل العقار مؤجر حالياً؟
    public long RentEndTime { get; set; } = 0;          // وقت انتهاء الإيجار
    public string RentedToPlayerId { get; set; } = "";  // معرف المستأجر
    public string RentedToPlayerName { get; set; } = ""; // اسم المستأجر


    // Core estate properties
    public int Id { get; set; } = 0;
    // ✅ UNIQUE INSTANCE IDENTIFIER (FIXES SHARED STATE BUG)
    [JsonIgnore] // Prevents serialization issues with existing saves
    public string InstanceId { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);
    public int InstanceNumber { get; set; } = 1; // Will be computed dynamically in UI
    public string EstateOwnerId { get; set; } = string.Empty;
    public bool IsUsed { get; set; } = false;
    public bool IsSpouseUsed { get; set; } = false;
    public bool IsForSale { get; set; } = false;
    public bool IsForRent { get; set; } = false;
    public bool IsRentedEstate { get; set; } = false;
    public List<string> PurchasedUpgrades { get; set; } = new();
    public List<string> ActiveContracts { get; set; } = new();
    public Dictionary<string, long> ContractStartTimes { get; set; } = new();

    // Upgrades/modifications
    public List<bool> FixedModifications { get; set; } = new List<bool> { false, false, false };

    // Servants/workers
    public List<long> ServantContractStartTimes { get; set; } = new List<long>();

    // Tax system
    public long LastTaxPaidTime { get; set; } = -1;

    // Visual
    public string EstateImageUrl { get; set; } = "no_private_domain_image";

    // Estate type definitions (from Excel file) - FIXED: Use int for costs that fit, long for large values
    public static readonly Dictionary<int, EstateType> EstateTypes = new()
{
    { 0, new EstateType("خيمة", "",
        "estate_0", 100, 0, 0, "This estate is by default owned for every new player") },
    { 1, new EstateType("كوخ قش", "",
        "estate_1", 120, 1000 , 1, "") },
    { 2, new EstateType("كوخ خشبي", "",
        "estate_2", 150, 5000, 5, "") },
    { 3, new EstateType("بيت صغير", "",
        "estate_3", 180, 10000, 10, "") },
    { 4, new EstateType("بيت ريفي", "",
        "estate_4", 200, 50000, 50, "") },
    { 5, new EstateType("بيت الشجرة", "",
        "estate_5", 450, 1000000, 1000, "") },
    { 6, new EstateType("منزل عائلي", "",
        "estate_6", 700, 3000000, 3000, "") },
    { 7, new EstateType("فيلا خشبية", "",
        "estate_7", 810, 5000000, 5000, "") },
    { 8, new EstateType("بيت السعادة", "",
        "estate_8", 850, 6000000, 6000, "") },
    { 9, new EstateType("فيلا مرفهة", "",
        "estate_9", 900, 8000000, 8000, "") },
    { 10, new EstateType("قلقة خشبية", "",
        "estate_10", 1350, 10000000, 10000, "") },
    { 11, new EstateType("فيلا بطوابق", "",
        "estate_11", 2100, 20000000, 20000, "") },
    { 12, new EstateType("قصر حجري", "",
        "estate_12", 3220, 50000000, 50000, "") },
    { 13, new EstateType("قلعة ساحرة", "",
        "estate_13", 4100, 100000000, 100000, "") }, // ✅ 1B (fits in int)
    { 14, new EstateType("قصر ملكي", "",
        "estate_14", 4500, 400000000, 400000, "") }, // ✅ CAPPED to 2B (max safe int)
    { 15, new EstateType("هرم فرعوني", "",
        "estate_private_domain", 5000, 600000000, 750000, "يمكنك تغيير صورة العقار") } // ✅ CAPPED to 2B (max safe int)
};

    // Get estate type name (Arabic)
    public string GetEstateTypeName()
    {
        return EstateTypes.TryGetValue(Id, out var type) ? type.Name : "عقار غير معروف";
    }

    // Calculate happiness (base + upgrades + servants)
    public int GetHappiness(PlayerAccount player)
    {
        // ✅ BASE HAPPINESS
        var baseHappiness = EstateTypes[Id].Happiness;

        // ✅ ADD UPGRADE HAPPINESS
        int upgradeHappiness = 0;
        if (EstateUpgradesDatabase.EstateUpgrades.TryGetValue(Id, out var upgrades))
        {
            upgradeHappiness = upgrades
                .Where(u => u.Type == "Upgrade" && PurchasedUpgrades.Contains(u.Name))
                .Sum(u => u.Happiness);
        }

        // ✅ ADD ACTIVE CONTRACT HAPPINESS (only if not expired)
        int contractHappiness = 0;
        if (EstateUpgradesDatabase.EstateUpgrades.TryGetValue(Id, out var contractsList))
        {
            contractHappiness = contractsList
                .Where(c => c.Type == "Contract" && ActiveContracts.Contains(c.Name))
                .Where(c =>
                {
                    if (!ContractStartTimes.TryGetValue(c.Name, out var startTime)) return false;
                    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var elapsedDays = (now - startTime) / (24 * 60 * 60 * 1000);
                    return elapsedDays < 7; // Contract valid for 7 days
                })
                .Sum(c => c.Happiness);
        }

        return baseHappiness + upgradeHappiness + contractHappiness;
    }
    // Get daily tax (1% of estate value)
    public int GetDailyTax()
    {
        int baseCost = EstateTypes.TryGetValue(Id, out var type) ? type.Cost : 0;
        return Math.Max(100, baseCost / 100);
    }

    // Get total hours owned
    public int GetTotalHoursOwned()
    {
        if (LastTaxPaidTime <= 0) return 0;

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var hoursSincePayment = (now - LastTaxPaidTime) / (60 * 60 * 1000);
        return (int)hoursSincePayment;
    }

    // Get number of completed upgrades - ✅ FIXED: Added missing method
    public int GetUpgradeCount()
    {
        return FixedModifications.Count(mod => mod);
    }

    // Get number of active servants - ✅ FIXED: Added missing method
    public int GetServantCount()
    {
        // Remove expired contracts (servants work for 7 days)
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        ServantContractStartTimes.RemoveAll(time => now - time > 7 * 24 * 60 * 60 * 1000);
        return ServantContractStartTimes.Count;
    }

    // Hire a servant (costs 500 gold/week) - ✅ FIXED: Added missing method
    public bool HireServant(PlayerAccount player)
    {
        if (player.Gold < 500)
            return false;

        // Check if we have available servant slots (based on upgrades)
        int maxServants = 1;
        if (FixedModifications.Count > 1 && FixedModifications[1]) maxServants += 1;
        if (FixedModifications.Count > 2 && FixedModifications[2]) maxServants += 1;

        if (GetServantCount() >= maxServants)
            return false;

        player.Gold -= 500;
        ServantContractStartTimes.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        return true;
    }

    // Complete an upgrade/modification - ✅ FIXED: Added missing method
    public bool CompleteUpgrade(PlayerAccount player, int upgradeIndex)
    {
        if (upgradeIndex < 0 || upgradeIndex >= FixedModifications.Count)
            return false;

        if (FixedModifications[upgradeIndex])
            return false;

        // Calculate upgrade cost (scales with estate value)
        int baseCost = EstateTypes.TryGetValue(Id, out var type) ? type.Cost : 10000;
        int cost = baseCost * (upgradeIndex + 1) / 10;

        if (player.Gold < cost)
            return false;

        player.Gold -= cost;
        FixedModifications[upgradeIndex] = true;
        return true;
    }

    // Estate type definition
    // ✅ CRITICAL FIX: Use long for costs (to support 7.5B from Excel)
    public class EstateType
    {
        public string Name { get; }
        public string Description { get; }
        public string ImageResource { get; }
        public int Happiness { get; }
        public int Cost { get; } // ✅ MUST BE int (NOT long)
        public int DailyTax { get; }
        public string Note { get; }

        // ✅ Constructor parameters MUST be int (NOT long)
        public EstateType(string name, string description, string imageResource, int happiness, int cost, int dailyTax, string note)
        {
            Name = name;
            Description = description;
            ImageResource = imageResource;
            Happiness = happiness;
            Cost = cost; // ✅ int assignment
            DailyTax = dailyTax;
            Note = note;
        }
    }

    // Get estate description (for UI)
    public string GetDescription()
    {
        return EstateTypes.TryGetValue(Id, out var type) ? type.Description : "وصف عقار غير معروف";
    }

    // Get image source (for UI)
    public string GetImageSource()
    {
        // If this is the special estate (Id = 15) and a custom image is set, use it
        if (Id == 15 && !string.IsNullOrEmpty(EstateImageUrl) && EstateImageUrl != "no_private_domain_image")
        {
            return EstateImageUrl;  // This is an absolute file path in app's private storage
        }
        // Otherwise fall back to the default type image
        return EstateTypes.TryGetValue(Id, out var type) ? type.ImageResource : "default_estate.png";
    }

    // Get upgrade data for current estate
    public List<UpgradeItem> GetUpgrades()
    {
        var upgrades = new List<UpgradeItem>();

        switch (Id)
        {
            case 0: // Shack
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 11, Cost = 900.0m, StatusText = "لم تتم إضافته بعد", Description = "جرس صوتي للشقة الخاصة بك", ImageSource = "doorbell.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 10, Cost = 180.0m, StatusText = "لم تتم إضافته بعد", Description = "ليزر بريدي للمنزل", ImageSource = "lightbulb.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 13, Cost = 1800.0m, StatusText = "لم تتم إضافته بعد", Description = "اضافة صندوق بريد ام بيتك", ImageSource = "mailbox.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "تعاقد", Happiness = 50, Cost = 450.0m, StatusText = "غير متعاقد معه", Description = "مساعدة لتسهيل حياتك", ImageSource = "cleaner.png" });
                break;

            case 1: // Basement
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 10, Cost = 180.0m, StatusText = "لم تتم إضافته بعد", Description = "ليزر بريدي للمنزل", ImageSource = "lightbulb.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 13, Cost = 1800.0m, StatusText = "لم تتم إضافته بعد", Description = "اضافة صندوق بريد ام بيتك", ImageSource = "mailbox.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "تعاقد", Happiness = 50, Cost = 450.0m, StatusText = "غير متعاقد معه", Description = "مساعدة لتسهيل حياتك", ImageSource = "cleaner.png" });
                break;

            case 2: // Apartment
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 11, Cost = 900.0m, StatusText = "لم تتم إضافته بعد", Description = "جرس صوتي للشقة الخاصة بك", ImageSource = "doorbell.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 10, Cost = 180.0m, StatusText = "لم تتم إضافته بعد", Description = "ليزر بريدي للمنزل", ImageSource = "lightbulb.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 13, Cost = 1800.0m, StatusText = "لم تتم إضافته بعد", Description = "اضافة صندوق بريد ام بيتك", ImageSource = "mailbox.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "تعاقد", Happiness = 50, Cost = 450.0m, StatusText = "غير متعاقد معه", Description = "مساعدة لتسهيل حياتك", ImageSource = "cleaner.png" });
                break;

            case 3: // Suburban House
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 15, Cost = 1100.0m, StatusText = "لم تتم إضافته بعد", Description = "أضافة نظام أمان للمنزل", ImageSource = "security_system.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 12, Cost = 850.0m, StatusText = "لم تتم إضافته بعد", Description = "أضافة نظام إنذار للحريق", ImageSource = "fire_alarm.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "تعاقد", Happiness = 60, Cost = 500.0m, StatusText = "غير متعاقد معه", Description = "خدمات تنظيف منزل", ImageSource = "cleaner.png" });
                break;

            case 4: // Chalet
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 18, Cost = 1350.0m, StatusText = "لم تتم إضافته بعد", Description = "أضافة مسبح خاص", ImageSource = "pool.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "إضافة", Happiness = 14, Cost = 950.0m, StatusText = "لم تتم إضافته بعد", Description = "أضافة نظام ترفيه", ImageSource = "entertainment_system.png" });
                upgrades.Add(new UpgradeItem { ButtonText = "تعاقد", Happiness = 70, Cost = 550.0m, StatusText = "غير متعاقد معه", Description = "خدمات حماية منزل", ImageSource = "security_guard.png" });
                break;
        }

        return upgrades;
    }

    // Upgrade data class
    public class UpgradeItem
    {
        public string ButtonText { get; set; }
        public int Happiness { get; set; }
        public decimal Cost { get; set; }
        public string StatusText { get; set; }
        public string Description { get; set; }
        public string ImageSource { get; set; }

        public string FormattedCost => Cost >= 1000 ? $"{Cost / 1000:F1}k" : $"{Cost:F1}";
    }
}