using GameProject02.Models;
using System.Linq;

namespace GameProject02.Services;

public static class MedalService
{
    public static void CheckAndAwardAll(PlayerAccount player)
    {
        if (player == null) return;

        bool changed = false;
        foreach (var medal in MedalDatabase.AllMedals)
        {
            if (!player.EarnedMedalIds.Contains(medal.Id) && medal.Condition(player))
            {
                AwardMedal(player, medal);
                changed = true;
            }
        }
        if (changed)
        {
            AccountService.SavePlayer(player);
        }
    }

    private static void AwardMedal(PlayerAccount player, MedalDefinition medal)
    {
        player.EarnedMedalIds.Add(medal.Id);
        player.Medals++;
        player.Merits += medal.MeritsReward;

        NotificationService.AddGameNotification(
            $"🏅 ميدالية جديدة: {medal.Name}",
            $"حصلت على {medal.MeritsReward} استحقاق!",
            GameNotificationPriority.High,
            "🏅",
            "ProfilePage"
        );
    }
}