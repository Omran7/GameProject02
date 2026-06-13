using GameProject02.Models;
using System;

namespace GameProject02.Services;

// ✅ AUTHENTIC NOBILITY SYSTEM (FROM nobility.txt DECOMPILED CODE)
public static class NobilityService
{
    // ✅ RECOVERY RATE: 1 point per 5 minutes (300 seconds) - FROM nobility.txt LINE 300
    private const long RECOVERY_INTERVAL_MS = 300000; // 5 minutes in milliseconds

    // ✅ UPDATE NOBILITY BASED ON ELAPSED TIME (CALLED ON LOGIN/RESUME)
    public static void UpdateNobility(PlayerAccount player)
    {
        if (player.NobilityCurrent >= 100)
        {
            player.NobilityChangeTimeInMilli = -101; // Reset timer when maxed
            return;
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // ✅ FIRST TIME SETUP (FROM nobility.txt: nobilityChangeTimeInMilli = -101)
        if (player.NobilityChangeTimeInMilli == -101)
        {
            player.NobilityChangeTimeInMilli = now + RECOVERY_INTERVAL_MS;
            return;
        }

        // ✅ CALCULATE ELAPSED TIME AND RECOVER POINTS (AUTHENTIC OLD GAME)
        long timeElapsed = now - player.NobilityChangeTimeInMilli;
        if (timeElapsed < RECOVERY_INTERVAL_MS) return;

        // ✅ RECOVER MULTIPLE POINTS IF OFFLINE FOR LONG TIME
        int pointsToRecover = (int)(timeElapsed / RECOVERY_INTERVAL_MS);
        player.NobilityCurrent = Math.Min(100, player.NobilityCurrent + pointsToRecover);

        // ✅ SET NEXT RECOVERY TIME (FROM nobility.txt LINE 300 logic)
        player.NobilityChangeTimeInMilli = now + (RECOVERY_INTERVAL_MS - (timeElapsed % RECOVERY_INTERVAL_MS));

        // ✅ RESET TIMER IF MAXED (FROM nobility.txt LINE 101)
        if (player.NobilityCurrent >= 100)
        {
            player.NobilityChangeTimeInMilli = -101;
        }

        System.Diagnostics.Debug.WriteLine($"[NOBILITY] Recovered {pointsToRecover} points. Current: {player.NobilityCurrent}/100");
    }

    // ✅ APPLY NOBILITY LOSS FOR POST-FIGHT ACTIONS (FROM fight_club old 4.txt)
    public static int ApplyNobilityLoss(PlayerAccount player, string actionType)
    {
        int lossAmount = actionType switch
        {
            "steal" => 15,    // Stealing gold (FROM fight_club old 4.txt)
            "cripple" => 25,  // Crippling opponent (FROM fight_club old 4.txt)
            "leave_early" => 10, // Leaving before police arrive (FROM fight_club old 4.txt)
            _ => 0
        };

        int actualLoss = Math.Min(lossAmount, player.NobilityCurrent);
        player.NobilityCurrent = Math.Max(0, player.NobilityCurrent - actualLoss);

        // ✅ RESET RECOVERY TIMER AFTER LOSS (AUTHENTIC OLD GAME BEHAVIOR)
        if (player.NobilityCurrent < 100 && player.NobilityChangeTimeInMilli == -101)
        {
            player.NobilityChangeTimeInMilli = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + RECOVERY_INTERVAL_MS;
        }

        System.Diagnostics.Debug.WriteLine($"[NOBILITY] Lost {actualLoss} points for {actionType}. Current: {player.NobilityCurrent}/100");
        return actualLoss;
    }

    // ✅ GET EFFECTIVE DEXTERITY (APPLIES NOBILITY MULTIPLIER - FROM Experience.txt LINE 1)
    public static double GetEffectiveDexterity(PlayerAccount player)
    {
        // ✅ AUTHENTIC FORMULA FROM Experience.txt (LINE 1):
        // "if (a.t(uVar2.getAccountObject().getId()) && uVar2.getMainStatesObject().getNobilityCurrent() < 100) {
        //    dexterity *= ((double) uVar2.getMainStatesObject().getNobilityCurrent()) / 100.0d;
        // }"
        if (player.NobilityCurrent >= 100) return player.Dexterity;

        return player.Dexterity * (player.NobilityCurrent / 100.0);
    }
}