using GameProject02.Models;
using System;

namespace GameProject02.Services;

public static class EstateService
{
    // Buy an estate (player can own multiple)
    public static (bool success, string message) BuyEstate(PlayerAccount player, int estateId)
    {
        if (player.Estates.Count >= 5)
            return (false, "لقد وصلت للحد الأقصى من العقارات (5)!");

        int cost = GetEstateCost(estateId);
        if (player.Gold < cost)
            return (false, $"ليس لديك ذهب كافي! تحتاج {cost:N0} ذهب.");

        player.Gold -= cost;

        // ✅ CRITICAL FIX: Generate UNIQUE InstanceId when creating estate
        var newEstate = new EstateObject
        {
            Id = estateId,
            InstanceId = Guid.NewGuid().ToString().Substring(0, 8), // ✅ MUST SET HERE!
            EstateOwnerId = player.PlayerId,
            IsUsed = true,
            LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool> { true, false, false }
        };

        player.Estates.Add(newEstate);
        player.Estate = newEstate;
        player.EstateType = newEstate.GetEstateTypeName();
        player.EstateOwner = "انت";

        return (true, $"تم شراء {newEstate.GetEstateTypeName()} بنجاح!");
    }

    // Get estate cost (for buying) - ✅ FIXED: All costs fit in int
    private static int GetEstateCost(int estateId)
    {
        return estateId switch
        {
            0 => 0,
            1 => 1000,
            2 => 5000,
            3 => 10000,
            4 => 50000,
            5 => 1000000,
            6 => 3000000,
            7 => 5000000,
            8 => 6000000,
            9 => 8000000,
            10 => 10000000,
            11 => 20000000,
            12 => 50000000,
            13 => 1000000000, // 1B fits in int
            14 => 2000000000, // MAX INT: 2,147,483,647
            _ => 0
        };
    }
}