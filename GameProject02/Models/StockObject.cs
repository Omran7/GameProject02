using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GameProject02.Models;

public class StockObject
{
    public int StockSpace { get; set; } = 2000;
    public int StockFreeSpace { get; set; } = 2000;
    public int BagSpace { get; set; } = 5;
    public int BagFreeSpace { get; set; } = 5;
    public List<ShopListing> ShopListings { get; set; } = new();
    public int ShopSpaces { get; set; } = 2;
    public int MaxShopSpaces { get; set; } = 999; // ✅ MODIFIED: unlimited
    public int LockerSpace { get; set; } = 10;
    public int StallSpace { get; set; } = 4;
    public int MuseumSpace { get; set; } = 0;

    public Dictionary<string, StockItem> ItemsInStock { get; set; } = new();
    public HashSet<string> LockedItemIds { get; set; } = new();

    public void AddItem(string itemId, string name, string description, string imageResource,
                        int count, int originalPrice, int categoryId, bool usedInArming)
    {
        if (ItemsInStock.ContainsKey(itemId))
        {
            ItemsInStock[itemId].Count += count;
        }
        else
        {
            ItemsInStock[itemId] = new StockItem
            {
                ItemId = itemId,
                Name = name,
                Description = description,
                ImageResource = imageResource,
                Count = count,
                OriginalPrice = originalPrice,
                CategoryId = categoryId,
                UsedInArming = usedInArming,
                IsLocked = LockedItemIds.Contains(itemId)
            };
        }
        StockFreeSpace = Math.Max(0, StockFreeSpace - count);
    }

    public bool RemoveItem(string itemId, int count)
    {
        if (!ItemsInStock.ContainsKey(itemId) || ItemsInStock[itemId].Count < count)
            return false;

        ItemsInStock[itemId].Count -= count;
        if (ItemsInStock[itemId].Count <= 0)
        {
            ItemsInStock.Remove(itemId);
            LockedItemIds.Remove(itemId);
        }

        StockFreeSpace = Math.Min(StockSpace, StockFreeSpace + count);
        return true;
    }

    public void ToggleLockItem(string itemId)
    {
        if (LockedItemIds.Contains(itemId))
            LockedItemIds.Remove(itemId);
        else
            LockedItemIds.Add(itemId);

        if (ItemsInStock.ContainsKey(itemId))
            ItemsInStock[itemId].IsLocked = LockedItemIds.Contains(itemId);
    }

    public int CalculateSellValue(string itemId, int count)
    {
        if (!ItemsInStock.ContainsKey(itemId))
            return 0;
        return (int)(ItemsInStock[itemId].OriginalPrice * 0.5 * count);
    }

    public bool AddToBag(string itemId, int count)
    {
        if (!ItemsInStock.ContainsKey(itemId) || count <= 0)
            return false;
        var item = ItemsInStock[itemId];
        if (item.GetAvailableForBag() < count)
            return false;
        if (BagFreeSpace < count)
            return false;

        item.Count -= count;
        item.CountInBag += count;
        BagFreeSpace -= count;
        StockFreeSpace += count;
        return true;
    }

    public bool RemoveFromBag(string itemId, int count)
    {
        if (!ItemsInStock.ContainsKey(itemId))
            return false;
        var item = ItemsInStock[itemId];
        if (count <= 0 || item.CountInBag < count)
            return false;

        item.Count += count;
        item.CountInBag -= count;
        BagFreeSpace += count;
        StockFreeSpace -= count;
        return true;
    }

    public bool MoveToMuseum(string itemId)
    {
        if (!ItemsInStock.ContainsKey(itemId))
            return false;
        var item = ItemsInStock[itemId];
        int totalQuantity = item.Count;
        ItemsInStock.Remove(itemId);
        StockFreeSpace += totalQuantity;
        return true;
    }

    public bool MoveFromMuseum(string itemId, string itemName, string imageResource, int quantity, int originalPrice)
    {
        ItemsInStock[itemId] = new StockItem
        {
            ItemId = itemId,
            Name = itemName,
            ImageResource = imageResource,
            Count = quantity,
            OriginalPrice = originalPrice,
            CountInBag = 0,
            IsLocked = false
        };
        StockFreeSpace -= quantity;
        return true;
    }

    public void DebugBagState(string action, string itemId, int count) { } // keep as is
}

public class StockItem : INotifyPropertyChanged
{
    private int _count;
    private int _countInBag;

    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = "بند غير معروف";
    public string Description { get; set; } = string.Empty;
    public string ImageResource { get; set; } = "item_unknown";
    public int CategoryId { get; set; } = 0;

    // ✅ NEW: Weapon/Armor stats
    public int Damage { get; set; } = 0;
    public int Accuracy { get; set; } = 0;
    public int Defense { get; set; } = 0;
    public int Evasion { get; set; } = 0;
    public bool IsWeapon { get; set; } = false;
    public bool IsGun { get; set; } = false;
    public int GunType { get; set; } = -1;
    public int AvailableInStock => Count;

    public int Count
    {
        get => _count;
        set { if (_count != value) { _count = value; OnPropertyChanged(); } }
    }

    public int OriginalPrice { get; set; } = 0;
    public bool UsedInArming { get; set; } = false;
    public bool IsLocked { get; set; } = false;

    public int CountInBag
    {
        get => _countInBag;
        set { if (_countInBag != value) { _countInBag = value; OnPropertyChanged(); } }
    }

    public string GetDisplayName() => Count > 1 ? $"{Name} x{Count}" : Name;
    public int GetSellPrice() => (int)(OriginalPrice * 0.5);
    public int GetAvailableForBag() => Count;

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}