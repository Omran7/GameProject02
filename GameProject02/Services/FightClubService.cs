using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Services
{
    public static class FightClubService
    {
        private static Random _random = new Random();

        // ✅ MODIFIED: Async method to fetch opponents from Firestore
        public static async Task<List<FightClubPlayer>> GetEligibleOpponentsAsync(PlayerAccount currentPlayer)
        {
            if (currentPlayer == null) return new List<FightClubPlayer>();

            // ✅ Fetch all players from Firestore (not just local cache)
            var allPlayers = await AccountService.GetAllPlayersAsync();

            // Build the raw list, then deduplicate by Username (keep highest level)
            return allPlayers
                .Where(p =>
                    p.PlayerId != currentPlayer.PlayerId &&
                    !p.CrimeObject.IsInHospital &&
                    !p.CrimeObject.IsInPrison &&
                    !p.CrimeObject.IsInPlane &&          // exclude players in flight
                    p.City == currentPlayer.City)
                .Select(p => new FightClubPlayer
                {
                    PlayerId = p.PlayerId,
                    Username = p.Username,
                    ImageResource = p.ImageResource,
                    Level = p.Level,
                    HealthCurrent = p.Health,
                    HealthMax = p.MaxHealth,
                    Strength = p.Strength,
                    Defense = p.Defense,
                    Speed = p.Speed,
                    Dexterity = p.Dexterity
                })
                .GroupBy(p => p.Username)                 // remove duplicate usernames
                .Select(g => g.OrderByDescending(p => p.Level).First())
                .OrderByDescending(p => p.Level)
                .ToList();
        }

        // Keep the synchronous method for backward compatibility (optional)
        // but mark as obsolete or comment out. We'll keep it for now.
        // To avoid confusion, you can remove it or keep it as a wrapper.
        public static List<FightClubPlayer> GetEligibleOpponents(PlayerAccount currentPlayer)
        {
            // ⚠️ This sync method only returns locally cached players.
            // Use GetEligibleOpponentsAsync instead.
            if (currentPlayer == null) return new List<FightClubPlayer>();

            var allPlayers = AccountService.GetAllPlayers(); // local cache only

            return allPlayers
                .Where(p =>
                    p.PlayerId != currentPlayer.PlayerId &&
                    !p.CrimeObject.IsInHospital &&
                    !p.CrimeObject.IsInPrison &&
                    !p.CrimeObject.IsInPlane &&
                    p.City == currentPlayer.City)
                .Select(p => new FightClubPlayer
                {
                    PlayerId = p.PlayerId,
                    Username = p.Username,
                    ImageResource = p.ImageResource,
                    Level = p.Level,
                    HealthCurrent = p.Health,
                    HealthMax = p.MaxHealth,
                    Strength = p.Strength,
                    Defense = p.Defense,
                    Speed = p.Speed,
                    Dexterity = p.Dexterity
                })
                .GroupBy(p => p.Username)
                .Select(g => g.OrderByDescending(p => p.Level).First())
                .OrderByDescending(p => p.Level)
                .ToList();
        }

        private static int CalculateDamage(PlayerAccount attacker, PlayerAccount defender)
        {
            double effectiveDexterity = NobilityService.GetEffectiveDexterity(attacker);
            int baseDamage = attacker.Strength + ((int)effectiveDexterity / 2);

            if (!string.IsNullOrEmpty(attacker.ArmingObject.WeaponId) &&
                attacker.StockObject.ItemsInStock.TryGetValue(attacker.ArmingObject.WeaponId, out var weaponItem))
            {
                baseDamage += weaponItem.Damage;
            }

            int defense = defender.Defense + (defender.Speed / 3);
            if (!string.IsNullOrEmpty(defender.ArmingObject.ArmorId) &&
                defender.StockObject.ItemsInStock.TryGetValue(defender.ArmingObject.ArmorId, out var armorItem))
            {
                defense += armorItem.Defense;
            }

            int damage = baseDamage - (defense / 2);
            return Math.Max(1, damage);
        }

        public static (bool success, string message, int damage, int newHealth, int goldStolen, int statReduction)
            ExecutePlayerAction(PlayerAccount attacker, PlayerAccount defender, int actionType)
        {
            if (attacker == null || defender == null)
                return (false, "خطأ", 0, 0, 0, 0);

            if (actionType == 0) // Attack
            {
                int damage = CalculateDamage(attacker, defender);
                defender.Health = Math.Max(0, defender.Health - damage);
                return (true, $"تسبب بـ {damage} ضرر", damage, defender.Health, 0, 0);
            }
            else if (actionType == 1) // Steal
            {
                int steal = Math.Min(defender.Gold, 500 + _random.Next(500));
                attacker.Gold += steal;
                defender.Gold -= steal;

                NotificationService.AddGameNotification(
                    "💰 سرقة!",
                    $"سرقت {steal} ذهب من {defender.Username}",
                    GameNotificationPriority.Normal, "💰", "FightClub"
                );

                return (true, $"سرق {steal} ذهب", 0, defender.Health, steal, 0);
            }
            else if (actionType == 2) // Cripple
            {
                int reduction = _random.Next(5, 15);
                defender.Strength = Math.Max(1, defender.Strength - reduction);
                int damage = 10;
                defender.Health = Math.Max(0, defender.Health - damage);
                return (true, $"نقص {reduction} قوة و {damage} ضرر", damage, defender.Health, 0, reduction);
            }
            else if (actionType == 3) // Disability
            {
                defender.Health = 0;
                return (true, "تم تسبيب عاهة للخصم! (30 دقيقة مستشفى)", 0, 0, 0, 0);
            }

            return (false, "غير معروف", 0, defender.Health, 0, 0);
        }

        public static (bool success, string message, int damage, int newHealth, int goldStolen, int statReduction)
            ExecuteOpponentAction(PlayerAccount opponent, PlayerAccount player)
        {
            int action = _random.Next(100);

            if (action < 70)
            {
                int damage = CalculateDamage(opponent, player);
                player.Health = Math.Max(0, player.Health - damage);
                return (true, $"تسبب بـ {damage} ضرر", damage, player.Health, 0, 0);
            }
            else if (action < 90)
            {
                int steal = Math.Min(player.Gold, 500 + _random.Next(500));
                opponent.Gold += steal;
                player.Gold -= steal;
                return (true, $"سرق {steal} ذهب", 0, player.Health, steal, 0);
            }
            else
            {
                int reduction = _random.Next(5, 15);
                player.Strength = Math.Max(1, player.Strength - reduction);
                return (true, $"نقص {reduction} قوة", 0, player.Health, 0, reduction);
            }
        }

        public static bool CanEscape()
        {
            return _random.Next(100) < 50;
        }

        public static void SendToHospital(PlayerAccount loser, PlayerAccount winner)
        {
            loser.CrimeObject.IsInHospital = true;
            loser.CrimeObject.HospitalReason = $"هُزمت في قتال ضد {winner.Username}";
            loser.CrimeObject.TotalHospitalVisits++;

            int levelDiff = Math.Max(0, winner.Level - loser.Level);
            int minutes = 2 + (levelDiff / 2);
            minutes = Math.Min(60, Math.Max(2, minutes));

            loser.CrimeObject.HospitalReleaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                + (long)TimeSpan.FromMinutes(minutes).TotalMilliseconds;

            loser.Health = 1;
            loser.CrimeObject.HealthCurrent = 1;

            NotificationService.AddGameNotification(
                "🏥 هُزمت في القتال!",
                $"أرسلت إلى المستشفى لمدة {minutes} دقيقة بعد هزيمتك أمام {winner.Username}",
                GameNotificationPriority.High, "🏥", "HospitalPage"
            );
        }

        public static void GiveRewards(PlayerAccount winner, PlayerAccount loser)
        {
            int xp = loser.Level * 50;
            winner.CurrentXP += xp;

            NotificationService.AddGameNotification(
                "🏆 انتصار!",
                $"هزمت {loser.Username} وحصلت على {xp} خبرة",
                GameNotificationPriority.High, "🏆", "FightClub"
            );

            while (winner.MainStatesObject.CanLevelUp())
            {
                winner.MainStatesObject.LevelUp();
                NotificationService.AddGameNotification(
                    $"🎉 المستوى {winner.Level}!",
                    $"تهانينا! وصلت للمستوى {winner.Level}\n+{winner.Level * 50} ذهب مكافأة",
                    GameNotificationPriority.High, "🏆", "ProfilePage"
                );
            }

            // Check for medals (e.g., fight wins)
            MedalService.CheckAndAwardAll(winner);
        }
    }
}