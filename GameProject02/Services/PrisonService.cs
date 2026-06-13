using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services
{
    public static class PrisonService
    {
        private const int ESCAPE_COURAGE_COST = 5;
        private const int SMUGGLE_COURAGE_COST = 5;

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

            // ✅ Notification
            NotificationService.AddGameNotification(
                "🔓 خرجت من السجن!",
                $"دفعت الكفالة {bailAmount:N0} ذهب وخرجت مبكراً.",
                GameNotificationPriority.Normal, "🔓", "MainPage"
            );

            return (true, "✅ دفعت الكفالة بنجاح!\nخرجت من السجن مبكراً.");
        }

        public static (bool success, string message) AttemptJailbreak(PlayerAccount player)
        {
            if (!player.CrimeObject.IsInPrison)
                return (false, "أنت لست في السجن حالياً!");

            if (player.Courage < ESCAPE_COURAGE_COST)
                return (false, $"ليس لديك ما يكفي من الشجاعة!\nتحتاج {ESCAPE_COURAGE_COST} شجاعة للهروب.");

            player.Courage -= ESCAPE_COURAGE_COST;

            bool success = new Random().Next(100) < 50;

            if (success)
            {
                player.CrimeObject.IsInPrison = false;
                player.CrimeObject.PrisonReleaseTime = 0;
                player.CrimeObject.PrisonBailAmount = 0;

                // ✅ Notification
                NotificationService.AddGameNotification(
                    "🕊️ هربت من السجن!",
                    "نجحت في الهروب، أنت حر مرة أخرى.",
                    GameNotificationPriority.High, "🕊️", "MainPage"
                );

                return (true, $"نجحت في الهروب من السجن!\nاستهلكت {ESCAPE_COURAGE_COST} شجاعة.");
            }
            else
            {
                long remainingTime = player.CrimeObject.PrisonReleaseTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                player.CrimeObject.PrisonReleaseTime += (long)(remainingTime * 0.25);

                // ✅ Notification
                NotificationService.AddGameNotification(
                    "🚔 فشل الهروب",
                    "فشلت محاولة الهروب وأضاف الحراس 25% وقت إضافي.",
                    GameNotificationPriority.High, "🚔", "PrisonPage"
                );

                return (false, $"فشلت في الهروب!\nأضاف الحراس 25% وقت إضافي.\nخسرت {ESCAPE_COURAGE_COST} شجاعة.");
            }
        }

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

            return (true, $"دفعت الكفالة عن {targetPlayer.Username} بنجاح!");
        }

        public static (bool success, string message) AttemptJailbreakForPlayer(PlayerAccount currentPlayer, PlayerAccount targetPlayer)
        {
            if (!targetPlayer.CrimeObject.IsInPrison)
                return (false, "هذا اللاعب ليس في السجن حالياً!");

            if (currentPlayer.Courage < SMUGGLE_COURAGE_COST)
                return (false, $"ليس لديك ما يكفي من الشجاعة!\nتحتاج {SMUGGLE_COURAGE_COST} شجاعة للتهريب.");

            currentPlayer.Courage -= SMUGGLE_COURAGE_COST;

            bool success = new Random().Next(100) < 50;

            if (success)
            {
                targetPlayer.CrimeObject.IsInPrison = false;
                targetPlayer.CrimeObject.PrisonReleaseTime = 0;
                targetPlayer.CrimeObject.PrisonBailAmount = 0;

                return (true, $"نجحت في تهريب {targetPlayer.Username}!\nاستهلكت {SMUGGLE_COURAGE_COST} شجاعة.");
            }
            else
            {
                currentPlayer.CrimeObject.IsInPrison = true;
                currentPlayer.CrimeObject.PrisonReleaseTime =
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() +
                    (long)TimeSpan.FromMinutes(30).TotalMilliseconds;
                currentPlayer.CrimeObject.PrisonReason = "تم القبض عليك أثناء محاولة تهريب سجين";
                currentPlayer.CrimeObject.PrisonBailAmount = 5000;
                currentPlayer.CrimeObject.TotalPrisonVisits++;

                return (false, $"❌ فشلت المحاولة! أُرسلت إلى السجن 30 دقيقة.\nخسرت {SMUGGLE_COURAGE_COST} شجاعة.");
            }
        }

        public static int GetEscapeCourageCost() => ESCAPE_COURAGE_COST;
        public static int GetSmugglerCourageCost() => SMUGGLE_COURAGE_COST;

        public static List<PlayerAccount> GetPrisoners()
        {
            var allPlayers = AccountService.GetAllPlayers();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            return allPlayers
                .Where(p => p.CrimeObject.IsInPrison &&
                            p.CrimeObject.PrisonReleaseTime > now)
                .ToList();
        }

        public static string GetRemainingTime(long releaseTime)
        {
            if (releaseTime <= 0) return "00:00:00";
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (now >= releaseTime) return "00:00:00";
            var remaining = TimeSpan.FromMilliseconds(releaseTime - now);
            return $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }
}