using System;
using System.Collections.Generic;
using System.Linq;
using GameProject02.Models;

namespace GameProject02.Services;

public static class UsedEstateService
{
    private static readonly List<UsedEstateListing> _activeListings = new();

    // ✅ FIX #1: Get LISTED estates by INSTANCE ID (not type ID)
    public static List<string> GetListedEstateInstanceIds(string sellerId) =>
    _activeListings
        .Where(l => l.SellerId == sellerId && !l.IsSold)
        .Select(l => l.EstateInstanceId)
        .ToList();

    // ✅ FIX #2: Get available listings grouped by TYPE (for UI)
    public static Dictionary<int, List<UsedEstateListing>> GetAvailableListingsGrouped()
    {
        var available = _activeListings.Where(l => !l.IsSold).ToList();
        return available.GroupBy(l => l.EstateId)
                        .ToDictionary(g => g.Key, g => g.ToList());
    }

    // ✅ FIX #3: Create listing with INSTANCE ID tracking
    public static (bool success, string message) CreateListing(PlayerAccount seller, EstateObject estate, int salePrice)
    {
        // Validate estate belongs to seller
        if (!seller.Estates.Contains(estate))
            return (false, "العقار ليس ملكك!");

        // Check if already listed (by instance ID)
        if (GetListedEstateInstanceIds(seller.PlayerId).Contains(estate.InstanceId))
            return (false, "هذا العقار معروض للبيع بالفعل!");

        // Check cross-listing
        if (RentalService.GetListedEstateInstanceIds(seller.PlayerId).Contains(estate.InstanceId))
            return (false, "هذا العقار معروض للإيجار حالياً!");

        // Validate price
        var maxPrice = EstateObject.EstateTypes[estate.Id].Cost * 2;
        if (salePrice < 1 || salePrice > maxPrice)
            return (false, $"السعر يجب أن يكون بين 1-{maxPrice:N0} ذهب");

        // Create listing with estate.InstanceId
        var listing = new UsedEstateListing
        {
            SellerId = seller.PlayerId,
            EstateId = estate.Id,
            EstateInstanceId = estate.InstanceId,
            SalePrice = salePrice,
            ListedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool>(estate.FixedModifications),
            ServantContractStartTimes = new List<long>(estate.ServantContractStartTimes),
            CurrentHappiness = estate.GetHappiness(seller)
        };

        _activeListings.Add(listing);
        return (true, $"تم عرض العقار للبيع بنجاح!\nالسعر: {salePrice:N0} ذهب");
    }
    public static (bool success, string message) BuyEstate(PlayerAccount buyer, string listingId)
    {
        var listing = _activeListings.FirstOrDefault(l => l.ListingId == listingId && !l.IsSold);
        if (listing == null)
            return (false, "العقار غير متاح للشراء");

        if (buyer.Gold < listing.SalePrice)
            return (false, "ليس لديك ذهب كافي!");

        var seller = AccountService.GetAllPlayers().FirstOrDefault(p => p.PlayerId == listing.SellerId);
        if (seller == null)
            return (false, "البائع غير موجود");

        var estateToRemove = seller.Estates.FirstOrDefault(e => e.InstanceId == listing.EstateInstanceId);
        if (estateToRemove == null)
            return (false, "العقار غير موجود لدى البائع");

        var boughtEstate = new EstateObject
        {
            Id = listing.EstateId,
            InstanceId = Guid.NewGuid().ToString().Substring(0, 8), // ✅ NEW INSTANCE FOR BUYER
            EstateOwnerId = buyer.PlayerId,
            IsUsed = true,
            LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool>(listing.FixedModifications),
            ServantContractStartTimes = new List<long>(listing.ServantContractStartTimes)
        };

        buyer.Gold -= listing.SalePrice;
        seller.Gold += listing.SalePrice;

        seller.Estates.Remove(estateToRemove);
        buyer.Estates.Add(boughtEstate);

        listing.IsSold = true;
        listing.BuyerId = buyer.PlayerId;

        _activeListings.Remove(listing);

        return (true, $"تم شراء {listing.GetEstateTypeName()} بنجاح!\nالسعادة: {listing.CurrentHappiness:N0} 😊");
    }

    public static (bool success, string message) RemoveListing(string listingId)
    {
        var listing = _activeListings.FirstOrDefault(l => l.ListingId == listingId && !l.IsSold);
        if (listing == null)
            return (false, "الإعلان غير موجود أو تم بيعه بالفعل!");

        _activeListings.Remove(listing);
        return (true, "تم إزالة العقار من قائمة البيع بنجاح!");
    }
}