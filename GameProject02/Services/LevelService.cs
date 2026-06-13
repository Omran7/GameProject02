using GameProject02.Models;
using Microsoft.Maui.ApplicationModel;
using System;

namespace GameProject02.Services;

public static class LevelService
{
    // ✅ SHOW LEVEL-UP DIALOG (AUTHENTIC OLD GAME STYLE)
    public static async Task ShowLevelUpDialog(PlayerAccount player, int newLevel)
    {
        string message = $"🎉 تمت ترقيتك إلى المستوى {newLevel}!\n\n";
        message += $"✅ شجاعتك: {player.MainStatesObject.CourageMax}\n";
        message += $"✅ صحتك: {player.MainStatesObject.HealthMax}\n";

        if (newLevel % 5 == 0)
        {
            message += $"\n✨ حصلت على +1 خفة و +1 قوة!";
        }

        await App.Current.MainPage.DisplayAlert("🆙 تمت الترقية!", message, "استمر في التقدم!");
    }

    // ✅ GET XP REQUIRED FOR SPECIFIC LEVEL
    public static long GetXpRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        long total = 0;
        for (int i = 1; i < level; i++)
        {
            total += i * (i + 1) * 50L;
        }
        return total;
    }

    // ✅ GET CURRENT LEVEL FROM TOTAL XP
    public static int GetLevelFromTotalXp(long totalXp)
    {
        int level = 1;
        long xpSoFar = 0;

        while (true)
        {
            long xpForNext = level * (level + 1) * 50L;
            if (xpSoFar + xpForNext > totalXp)
                break;

            xpSoFar += xpForNext;
            level++;
        }

        return level;
    }
}