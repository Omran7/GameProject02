using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class PrisonService
{

    private const int ESCAPE_COURAGE_COST = 10;      // Change this value as you wish
    private const int SMUGGLE_COURAGE_COST = 10;     // Change this value as you wish
    // ✅ PAY BAIL FOR CURRENT PLAYER
    public static (bool success, string message) PayBail(PlayerAccount player)
    {
        if (!player.CrimeObject.IsInPrison)
            return (false, "أنت لست في السجن حالياً!");

        int bailAmount = (int)Math.Min(int.MaxValue, player.CrimeObject.PrisonBailAmount);

        if (player.Gold < bailAmount)
            return (false, $"ليس لديك ما يكفي من المال!\nتحتاج {bailAmount:N0} ذهب لدفع الكفالة.");

        player.Gold -= bailAmount;
        player.CrimeObject.IsInPrison = false;
        player.CrimeObject.PrisonReleaseTime = 0;
        player.CrimeObject.PrisonBailAmount = 0;

        return (true, $"✅ دفعت الكفالة بنجاح!\nخرجت من السجن مبكراً.");
    }

    // ✅ ATTEMPT JAILBREAK FOR CURRENT PLAYER (50% chance)
    public static (bool success, string message) AttemptJailbreak(PlayerAccount player)
    {
        if (!player.CrimeObject.IsInPrison)
            return (false, "أنت لست في السجن حالياً!");

        // ✅ Check courage
        if (player.CrimeObject.Courage < ESCAPE_COURAGE_COST)
            return (false, $"ليس لديك ما يكفي من الشجاعة!\nتحتاج {ESCAPE_COURAGE_COST} نقطة شجاعة للهروب.");

        var random = new Random();
        int successChance = 50;
        bool success = random.Next(100) < successChance;

        if (success)
        {
            // ✅ Consume courage
            player.CrimeObject.Courage -= ESCAPE_COURAGE_COST;
            player.CrimeObject.IsInPrison = false;
            player.CrimeObject.PrisonReleaseTime = 0;
            player.CrimeObject.PrisonBailAmount = 0;
            return (true, $"✅ نجحت في الهروب من السجن!\nاستهلكت {ESCAPE_COURAGE_COST} نقطة شجاعة.");
        }
        else
        {
            // ✅ Still consume courage? Your choice – usually yes, you lose courage even on failure
            player.CrimeObject.Courage -= ESCAPE_COURAGE_COST;
            long remainingTime = player.CrimeObject.PrisonReleaseTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long extraTime = (long)(remainingTime * 0.25);
            player.CrimeObject.PrisonReleaseTime += extraTime;
            return (false, $"❌ فشلت في الهروب من السجن!\nأضاف الحراس 25% وقت إضافي لعقابك.\nخسرت {ESCAPE_COURAGE_COST} نقطة شجاعة.");
        }
    }
    // ✅ PAY BAIL FOR ANOTHER PLAYER
    public static (bool success, string message) PayBailForPlayer(PlayerAccount currentPlayer, PlayerAccount targetPlayer)
    {
        if (!targetPlayer.CrimeObject.IsInPrison)
            return (false, "هذا اللاعب ليس في السجن حالياً!");

        long bailAmount = targetPlayer.CrimeObject.PrisonBailAmount;
        if (currentPlayer.Gold < bailAmount)
            return (false, $"لا تملك {bailAmount:N0} ذهب لدفع الكفالة!");

        currentPlayer.Gold -= (int)bailAmount;
        targetPlayer.CrimeObject.IsInPrison = false;
        targetPlayer.CrimeObject.PrisonReleaseTime = 0;
        targetPlayer.CrimeObject.PrisonBailAmount = 0;

        return (true, $"✅ دفعت الكفالة عن {targetPlayer.Username} بنجاح!");
    }

    // ✅ ATTEMPT JAILBREAK FOR ANOTHER PLAYER (50% chance)
    public static (bool success, string message) AttemptJailbreakForPlayer(PlayerAccount currentPlayer, PlayerAccount targetPlayer)
    {
        if (!targetPlayer.CrimeObject.IsInPrison)
            return (false, "هذا اللاعب ليس في السجن حالياً!");

        // ✅ Check courage for the helper
        if (currentPlayer.CrimeObject.Courage < SMUGGLE_COURAGE_COST)
            return (false, $"ليس لديك ما يكفي من الشجاعة!\nتحتاج {SMUGGLE_COURAGE_COST} نقطة شجاعة للتهريب.");

        var random = new Random();
        int successChance = 50;
        bool success = random.Next(100) < successChance;

        if (success)
        {
            currentPlayer.CrimeObject.Courage -= SMUGGLE_COURAGE_COST;
            targetPlayer.CrimeObject.IsInPrison = false;
            targetPlayer.CrimeObject.PrisonReleaseTime = 0;
            targetPlayer.CrimeObject.PrisonBailAmount = 0;
            return (true, $"✅ نجحت في تهريب {targetPlayer.Username} من السجن!\nاستهلكت {SMUGGLE_COURAGE_COST} نقطة شجاعة.");
        }
        else
        {
            currentPlayer.CrimeObject.Courage -= SMUGGLE_COURAGE_COST;
            currentPlayer.CrimeObject.IsInPrison = true;
            currentPlayer.CrimeObject.PrisonReleaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)TimeSpan.FromMinutes(30).TotalMilliseconds;
            currentPlayer.CrimeObject.PrisonReason = "تم القبض عليك أثناء محاولة تهريب سجين";
            currentPlayer.CrimeObject.PrisonBailAmount = 5000;
            currentPlayer.CrimeObject.TotalPrisonVisits++;
            return (false, $"❌ فشلت محاولة التهريب! تم القبض عليك وأرسلت إلى السجن لمدة 30 دقيقة.\nخسرت {SMUGGLE_COURAGE_COST} نقطة شجاعة.");
        }
    }
    // ✅ GET ALL PLAYERS CURRENTLY IN PRISON
    public static List<PlayerAccount> GetPrisoners()
    {
        var allPlayers = AccountService.GetAllPlayers();

        var prisoners = allPlayers
            .Where(p => p.CrimeObject.IsInPrison &&
                       p.CrimeObject.PrisonReleaseTime > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            .ToList();

        System.Diagnostics.Debug.WriteLine($"[PRISON] Total players: {allPlayers.Count}");
        System.Diagnostics.Debug.WriteLine($"[PRISON] Prisoners: {prisoners.Count}");

        return prisoners;
    }

    // ✅ GET REMAINING TIME
    public static string GetRemainingTime(long releaseTime)
    {
        if (releaseTime <= 0) return "00:00:00";

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (now >= releaseTime) return "00:00:00";

        var remaining = TimeSpan.FromMilliseconds(releaseTime - now);
        return $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }
}