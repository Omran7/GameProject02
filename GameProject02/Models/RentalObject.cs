using System;
using System.Collections.Generic;
using GameProject02.Models;

namespace GameProject02.Models;

// Simple single-player rental system (no player-to-player rentals)
public static class RentalSystem
{
    // Track rented estates by player ID + estate ID
    private static readonly Dictionary<string, Dictionary<int, RentedEstateInfo>> _activeRentals = new();

    // Get available estates for rent (system-owned pool)
    public static List<RentableEstate> GetAvailableEstates()
    {
        return new List<RentableEstate>
        {
            new RentableEstate { EstateId = 1, MaxDays = 30, BasePrice = 250 },   // بدروم
            new RentableEstate { EstateId = 2, MaxDays = 30, BasePrice = 1250 },  // شقة
            new RentableEstate { EstateId = 3, MaxDays = 30, BasePrice = 2500 },  // بيت في الضواحي
            new RentableEstate { EstateId = 4, MaxDays = 30, BasePrice = 12500 }  // شاليه صغير
        };
    }

    // Rent an estate from the system
    public static bool RentEstate(PlayerAccount player, int estateId, int days, int pricePerDay)
    {
        // Validate inputs
        if (days < 1 || days > 30) return false;
        if (pricePerDay < 1) return false;

        var estateType = EstateObject.EstateTypes[estateId];
        var maxPrice = EstateObject.EstateTypes[estateId].Cost / 4;
        if (pricePerDay > maxPrice) return false;

        // Check gold
        var totalCost = pricePerDay * days;
        if (player.Gold < totalCost) return false;

        // Deduct gold
        player.Gold -= totalCost;

        // Create rented estate object
        var rentedEstate = new EstateObject
        {
            Id = estateId,
            EstateOwnerId = player.PlayerId,
            IsUsed = true,
            LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool> { true, false, false }
        };

        // Add to player's estates
        if (player.Estates == null)
            player.Estates = new List<EstateObject>();

        player.Estates.Add(rentedEstate);

        // Track rental for expiration
        if (!_activeRentals.ContainsKey(player.PlayerId))
            _activeRentals[player.PlayerId] = new Dictionary<int, RentedEstateInfo>();

        _activeRentals[player.PlayerId][estateId] = new RentedEstateInfo
        {
            DaysRented = days,
            RentStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            PricePaid = totalCost
        };

        return true;
    }

    // Check and expire rentals (call this in ProfilePage.OnAppearing)
    public static void CheckExpiredRentals(PlayerAccount player)
    {
        if (!_activeRentals.TryGetValue(player.PlayerId, out var rentals)) return;

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var estatesToRemove = new List<int>();

        foreach (var rental in rentals)
        {
            var daysPassed = (now - rental.Value.RentStartTime) / (24 * 60 * 60 * 1000);

            if (daysPassed >= rental.Value.DaysRented)
            {
                // Mark for removal
                estatesToRemove.Add(rental.Key);

                // Remove from player's estates
                var estateToRemove = player.Estates.FirstOrDefault(e => e.Id == rental.Key);
                if (estateToRemove != null)
                {
                    player.Estates.Remove(estateToRemove);
                }
            }
        }

        // Clean up expired rentals
        foreach (var estateId in estatesToRemove)
        {
            rentals.Remove(estateId);
        }

        // Remove player entry if no active rentals
        if (rentals.Count == 0)
        {
            _activeRentals.Remove(player.PlayerId);
        }
    }
}

// Rentable estate definition (system-owned)
public class RentableEstate
{
    public int EstateId { get; set; }
    public int MaxDays { get; set; } = 30;
    public int BasePrice { get; set; } // Base price per day (1/4 of original cost)

    public string GetName() => EstateObject.EstateTypes[EstateId].Name;
    public string GetDescription() => EstateObject.EstateTypes[EstateId].Description;
    public int GetMaxPrice() => EstateObject.EstateTypes[EstateId].Cost / 4;
}

// Rented estate tracking info
public class RentedEstateInfo
{
    public int DaysRented { get; set; }
    public long RentStartTime { get; set; }
    public int PricePaid { get; set; }
}