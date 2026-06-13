using System;
using System.Collections.Generic;
using System.Linq;
using GameProject02.Models;

namespace GameProject02.Services;

public static class RentalService
{
    private static readonly List<RentalListing> _activeListings = new();
    private static readonly List<RentalListing> _currentRentals = new();

    public static List<string> GetListedEstateInstanceIds(string ownerId) =>
        _activeListings
            .Where(l => l.OwnerId == ownerId && !l.IsRented)
            .Select(l => l.EstateInstanceId)
            .ToList();

    public static List<string> GetRentedEstateInstanceIds(string ownerId) =>
        _currentRentals
            .Where(l => l.OwnerId == ownerId)
            .Select(l => l.EstateInstanceId)
            .ToList();

    public static Dictionary<int, List<RentalListing>> GetAvailableListingsGrouped()
    {
        var available = _activeListings.Where(l => !l.IsRented).ToList();
        return available.GroupBy(l => l.EstateId)
                        .ToDictionary(g => g.Key, g => g.ToList());
    }

    public static (bool success, string message) CreateListing(PlayerAccount owner, EstateObject estate, int totalPriceFor30Days)
    {
        if (!owner.Estates.Contains(estate))
            return (false, "العقار ليس ملكك!");

        if (GetListedEstateInstanceIds(owner.PlayerId).Contains(estate.InstanceId))
            return (false, "هذا العقار معروض للإيجار بالفعل!");

        if (UsedEstateService.GetListedEstateInstanceIds(owner.PlayerId).Contains(estate.InstanceId))
            return (false, "هذا العقار معروض للبيع حالياً!");

        var maxPrice = EstateObject.EstateTypes[estate.Id].Cost / 4;
        if (totalPriceFor30Days < 1 || totalPriceFor30Days > maxPrice)
            return (false, $"السعر يجب أن يكون بين 1-{maxPrice:N0} ذهب");

        var listing = new RentalListing
        {
            OwnerId = owner.PlayerId,
            EstateId = estate.Id,
            EstateInstanceId = estate.InstanceId,
            TotalPriceFor30Days = totalPriceFor30Days,
            ListedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CurrentHappiness = estate.GetHappiness(owner),
            FixedModifications = new List<bool>(estate.FixedModifications),
            ServantContractStartTimes = new List<long>(estate.ServantContractStartTimes)
        };

        _activeListings.Add(listing);
        return (true, $"تم إدراج العقار للإيجار بنجاح!\nالسعر: {totalPriceFor30Days:N0} ذهب (لـ 30 يوم)");
    }

    public static (bool success, string message) RentEstate(PlayerAccount tenant, string listingId, int daysToRent)
    {
        var listing = _activeListings.FirstOrDefault(l => l.ListingId == listingId && !l.IsRented);
        if (listing == null)
            return (false, "العقار غير متاح للإيجار");

        if (daysToRent < 1 || daysToRent > 30)
            return (false, "المدة يجب أن تكون بين 1-30 يوم");

        if (tenant.Gold < listing.CalculatePriceForDays(daysToRent))
            return (false, "ليس لديك ذهب كافي!");

        var owner = AccountService.GetAllPlayers().FirstOrDefault(p => p.PlayerId == listing.OwnerId);
        if (owner == null)
            return (false, "مالك العقار غير موجود");

        var estateToRemove = owner.Estates.FirstOrDefault(e => e.InstanceId == listing.EstateInstanceId);
        if (estateToRemove == null)
            return (false, "العقار غير موجود لدى المالك");

        var totalPrice = listing.CalculatePriceForDays(daysToRent);
        tenant.Gold -= totalPrice;
        owner.Gold += totalPrice;

        // ✅ إنشاء سجل مؤقت للمالك (عقار مؤجر)
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var endTime = now + (daysToRent * 24L * 60 * 60 * 1000);
        var ownerPlaceholder = new EstateObject
        {
            Id = estateToRemove.Id,
            InstanceId = Guid.NewGuid().ToString().Substring(0, 8),
            EstateOwnerId = owner.PlayerId,
            IsUsed = true,
            IsRentedOut = true,
            RentEndTime = endTime,
            RentedToPlayerId = tenant.PlayerId,
            RentedToPlayerName = tenant.Username,
            FixedModifications = new List<bool>(listing.FixedModifications),
            ServantContractStartTimes = new List<long>(listing.ServantContractStartTimes)
        };
        owner.Estates.Add(ownerPlaceholder);

        // إنشاء النسخة المستأجرة للمستأجر
        var rentedEstate = new EstateObject
        {
            Id = listing.EstateId,
            InstanceId = Guid.NewGuid().ToString().Substring(0, 8),
            EstateOwnerId = tenant.PlayerId,
            IsUsed = true,
            IsRentedEstate = true,
            LastTaxPaidTime = now,
            FixedModifications = new List<bool>(listing.FixedModifications),
            ServantContractStartTimes = new List<long>(listing.ServantContractStartTimes)
        };

        owner.Estates.Remove(estateToRemove);
        tenant.Estates.Add(rentedEstate);

        listing.IsRented = true;
        listing.TenantId = tenant.PlayerId;
        listing.RentStartTime = now;
        listing.ActualDaysRented = daysToRent;

        _activeListings.Remove(listing);
        _currentRentals.Add(listing);

        return (true, $"تم استئجار {listing.GetEstateTypeName()} لمدة {daysToRent} يوم بنجاح!\nالسعر: {totalPrice:N0} ذهب");
    }

    public static (bool success, string message) RemoveListing(string listingId)
    {
        var listing = _activeListings.FirstOrDefault(l => l.ListingId == listingId && !l.IsRented);
        if (listing == null)
            return (false, "الإعلان غير موجود أو تم تأجيره بالفعل!");

        _activeListings.Remove(listing);
        return (true, "تم إزالة العقار من قائمة الإيجار بنجاح!");
    }

    public static void ProcessExpiredRentals()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var expired = _currentRentals.Where(r =>
            (now - r.RentStartTime) / (24 * 60 * 60 * 1000) >= r.ActualDaysRented
        ).ToList();

        foreach (var rental in expired)
        {
            var tenant = AccountService.GetAllPlayers().FirstOrDefault(p => p.PlayerId == rental.TenantId);
            var owner = AccountService.GetAllPlayers().FirstOrDefault(p => p.PlayerId == rental.OwnerId);

            if (tenant != null && owner != null)
            {
                // إزالة العقار من المستأجر
                var rentedEstate = tenant.Estates.FirstOrDefault(e => e.InstanceId == rental.EstateInstanceId && e.IsRentedEstate);
                if (rentedEstate != null)
                    tenant.Estates.Remove(rentedEstate);

                // ✅ إزالة السجل المؤقت من المالك
                var ownerPlaceholder = owner.Estates.FirstOrDefault(e => e.IsRentedOut && e.RentedToPlayerId == tenant.PlayerId && e.Id == rental.EstateId);
                if (ownerPlaceholder != null)
                    owner.Estates.Remove(ownerPlaceholder);

                // إعادة العقار للمالك (كعقار عادي)
                var returnedEstate = new EstateObject
                {
                    Id = rental.EstateId,
                    InstanceId = Guid.NewGuid().ToString().Substring(0, 8),
                    EstateOwnerId = owner.PlayerId,
                    IsUsed = true,
                    IsRentedOut = false,
                    LastTaxPaidTime = now,
                    FixedModifications = new List<bool>(rental.FixedModifications),
                    ServantContractStartTimes = new List<long>(rental.ServantContractStartTimes)
                };
                owner.Estates.Add(returnedEstate);
            }

            _currentRentals.Remove(rental);
        }
    }
}