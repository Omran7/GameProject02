using GameProject02.Models;
using System;

namespace GameProject02.Services;

// ✅ AUTHENTIC OLD GAME ARMING SERVICE (FROM DECOMPILED FILES)
public static class ArmingService
{
    // ✅ EQUIP ITEM TO SPECIFIC SLOT (MARKS AS USED IN STOCK)
    // ✅ EQUIP ITEM TO SPECIFIC SLOT (MARKS AS USED IN STOCK) - FIXED VERSION
    public static (bool success, string message) EquipItem(PlayerAccount player, string itemId, string slotType)
    {
        if (player == null || string.IsNullOrEmpty(itemId))
            return (false, "بيانات غير صالحة");

        // ✅ GET ITEM FROM STOCK (ALREADY HAS ALL NECESSARY PROPERTIES)
        if (!player.StockObject.ItemsInStock.TryGetValue(itemId, out var stockItem) || stockItem.Count <= 0)
            return (false, "العنصر غير موجود في المخزن");

        // ✅ VALIDATE SLOT TYPE USING STOCK ITEM PROPERTIES (NO MARKET LOOKUP NEEDED)
        bool isValid = slotType switch
        {
            "weapon" => stockItem.IsWeapon, // Weapons have IsWeapon=true
            "armor" => stockItem.CategoryId == 1,  // Armor category is 1
            "special" => stockItem.CategoryId == 3, // Special equipment category is 3
            "biochemical" => stockItem.CategoryId == 4, // Biochemical category is 4
            _ => false
        };

        if (!isValid)
            return (false, $"هذا العنصر لا يمكن تجهيزه في فتحة {slotType}");

        // ✅ UNEQUIP EXISTING ITEM IN THIS SLOT (IF ANY)
        string existingItemId = slotType switch
        {
            "weapon" => player.ArmingObject.WeaponId,
            "armor" => player.ArmingObject.ArmorId,
            "special" => player.ArmingObject.SpecialEquipmentId,
            "biochemical" => player.ArmingObject.BioChemicalId,
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(existingItemId))
        {
            UnequipItem(player, existingItemId);
        }

        // ✅ EQUIP NEW ITEM TO SLOT
        switch (slotType)
        {
            case "weapon":
                player.ArmingObject.WeaponId = itemId;
                player.ArmingObject.WeaponLevel = 0;
                break;
            case "armor":
                player.ArmingObject.ArmorId = itemId;
                player.ArmingObject.ArmorLevel = 0;
                break;
            case "special":
                player.ArmingObject.SpecialEquipmentId = itemId;
                player.ArmingObject.SpecialEquipmentLevel = 0;
                break;
            case "biochemical":
                player.ArmingObject.BioChemicalId = itemId;
                player.ArmingObject.BioChemicalLevel = 0;
                break;
        }

        // ✅ MARK ITEM AS USED IN ARMING (HIDES FROM STOCK UI)
        stockItem.UsedInArming = true;
        stockItem.Count--; // Reduce visible count (item still exists in inventory)

        // ✅ SUCCESS MESSAGE USING STOCK ITEM NAME (NO MARKET DEPENDENCY)
        return (true, $"✅ تم تجهيز {stockItem.Name} بنجاح!");
    }
    // ✅ UNEQUIP ITEM (RETURNS TO STOCK UI)
    public static (bool success, string message) UnequipItem(PlayerAccount player, string itemId)
    {
        if (player == null || string.IsNullOrEmpty(itemId))
            return (false, "بيانات غير صالحة");

        // ✅ CHECK IF ITEM IS ACTUALLY EQUIPPED
        if (!player.ArmingObject.IsItemEquipped(itemId))
            return (false, "هذا العنصر غير مجهز حالياً");

        // ✅ CLEAR SLOT
        if (player.ArmingObject.WeaponId == itemId) player.ArmingObject.WeaponId = string.Empty;
        else if (player.ArmingObject.ArmorId == itemId) player.ArmingObject.ArmorId = string.Empty;
        else if (player.ArmingObject.SpecialEquipmentId == itemId) player.ArmingObject.SpecialEquipmentId = string.Empty;
        else if (player.ArmingObject.BioChemicalId == itemId) player.ArmingObject.BioChemicalId = string.Empty;

        // ✅ RETURN ITEM TO STOCK (INCREMENT COUNT + CLEAR FLAG)
        if (player.StockObject.ItemsInStock.TryGetValue(itemId, out var stockItem))
        {
            stockItem.Count++;
            stockItem.UsedInArming = false;
        }
        else
        {
            // Recreate stock entry if missing (shouldn't happen)
            var marketItem = MarketService.GetItemById(itemId);
            if (marketItem != null)
            {
                player.StockObject.ItemsInStock[itemId] = new StockItem
                {
                    ItemId = itemId,
                    Name = marketItem.Name,
                    ImageResource = marketItem.ImageResource,
                    Count = 1,
                    UsedInArming = false,
                    OriginalPrice = (int)marketItem.PriceGold,
                    CategoryId = marketItem.CategoryType
                };
            }
        }

        return (true, "✅ تم إزالة التجهيز بنجاح!");
    }
}