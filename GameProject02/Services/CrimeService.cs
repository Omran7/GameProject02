using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Views;
using System;
using System.Collections.Generic;

namespace GameProject02.Services;

public static class CrimeService
{
    private static bool IsPreviousCrimeGreen(PlayerAccount player, int crimeTypeId, int crimeItemId)
    {
        if (crimeItemId == 0) return true;
        var prevCrime = CrimeDatabase.GetCrimeItem(crimeTypeId, crimeItemId - 1);
        if (prevCrime == null) return true;
        int prevLinearIndex = CrimeDatabase.GetLinearIndex(crimeTypeId, crimeItemId - 1);
        int prevProgress = player.CrimeObject.TaskProgress.GetValueOrDefault(prevLinearIndex, 0);
        return prevProgress >= prevCrime.RequiredSuccesses;
    }

    public static (bool success, string message) AttemptCrime(PlayerAccount player, int crimeTypeId, int crimeItemId)
    {
        player.CrimeObject.CheckConfinementStatus();

        var crime = CrimeDatabase.GetCrimeItem(crimeTypeId, crimeItemId);
        if (crime == null) return (false, "جريمة غير موجودة!");

        int linearIndex = CrimeDatabase.GetLinearIndex(crimeTypeId, crimeItemId);
        if (linearIndex == -1) return (false, "خطأ في البيانات.");

        if (!IsPreviousCrimeGreen(player, crimeTypeId, crimeItemId))
            return (false, "هذه الجريمة مقفولة!\nأكمل الجريمة السابقة أولاً.");

        if (player.CrimeObject.IsInPrison)
        {
            var p = Microsoft.Maui.Controls.Application.Current.MainPage;
            p?.Dispatcher.Dispatch(async () => await p.Navigation.PushModalAsync(new PrisonPage()));
            return (false, "أنت في السجن حالياً!");
        }

        if (player.CrimeObject.IsInHospital)
            return (false, "أنت في المستشفى حالياً!");

        var consumedTools = new List<string>();
        bool toolsLost = false;

        foreach (var toolReq in crime.ToolRequirements)
        {
            if (!player.StockObject.ItemsInStock.TryGetValue(toolReq.ToolItemId, out var toolItem) ||
                toolItem.Count < toolReq.RequiredCount)
                return (false, $"لا تملك أدوات كافية!\nتحتاج {toolReq.RequiredCount} × {toolReq.ToolName}.");
        }

        var random = new Random();
        if (random.Next(100) < 40)
        {
            toolsLost = true;
            foreach (var toolReq in crime.ToolRequirements)
            {
                var toolItem = player.StockObject.ItemsInStock[toolReq.ToolItemId];
                toolItem.Count -= toolReq.RequiredCount;
                consumedTools.Add($"{toolReq.RequiredCount} × {toolReq.ToolName}");
                if (toolItem.Count <= 0)
                    player.StockObject.ItemsInStock.Remove(toolReq.ToolItemId);
            }
        }

        // ✅ SKILL #13: لا يخاف (Reduce courage cost)
        int courageCost = crime.CourageCost;
        var noFearSkill = player.Skills.FirstOrDefault(s => s.Id == 13 && s.IsEquipped);
        if (noFearSkill != null)
        {
            double reduction = Math.Max(0.60, 1.0 - (noFearSkill.Level * 0.06));
            courageCost = (int)(courageCost * reduction);
        }

        if (player.Courage < courageCost)
            return (false, $"ليس لديك ما يكفي من الشجاعة!\nتحتاج {courageCost} / لديك {player.Courage}.");

        player.Courage -= courageCost;
        player.CrimeObject.TotalCrimesAttempted++;

        int currentProgress = player.CrimeObject.TaskProgress.GetValueOrDefault(linearIndex, 0);
        double percent = crime.RequiredSuccesses > 0
                              ? Math.Clamp((double)currentProgress / crime.RequiredSuccesses, 0, 1)
                              : 1.0;

        // ✅ FIX: No color penalty for the first crime in each type (crimeItemId == 0)
        int colorBonus = 0;
        if (crimeItemId > 0) // not the first crime of this type
        {
            colorBonus = percent >= 1.0 ? 0 : percent >= 0.50 ? -10 : -20;
        }

        int baseChance = crime.BaseSuccessChance;

        // ✅ SKILL #4: خطوات الظل (Increase success chance)
        var shadowStepSkill = player.Skills.FirstOrDefault(s => s.Id == 4 && s.IsEquipped);
        if (shadowStepSkill != null)
            baseChance += (int)(shadowStepSkill.Level * 3);

        int statBonus = player.Level / 2 + player.Dexterity / 10;
        int successChance = Math.Clamp(Math.Min(95, baseChance + statBonus + colorBonus), 5, 95);
        bool success = random.Next(100) < successChance;

        if (success)
        {
            int cashReward = random.Next(crime.Reward.CashRewardMin, crime.Reward.CashRewardMax + 1);

            // ✅ SKILL #10: لص محترف (Increase crime gold)
            var proThiefSkill = player.Skills.FirstOrDefault(s => s.Id == 10 && s.IsEquipped);
            if (proThiefSkill != null)
                cashReward = (int)(cashReward * (1.0 + (proThiefSkill.Level * 0.15)));

            player.Gold += cashReward;

            int xpReward = crime.Reward.ExperienceReward + (crimeTypeId * 10);

            // ✅ SKILL #15: خبير (Increase XP gain)
            var expertSkill = player.Skills.FirstOrDefault(s => s.Id == 15 && s.IsEquipped);
            if (expertSkill != null)
                xpReward = (int)(xpReward * (1.0 + (expertSkill.Level * 0.12)));

            // ✅ FIXED: Pass 'player' argument to AddExperience
            bool leveledUp = player.MainStatesObject.AddExperience(xpReward, player);
            player.CurrentXP = player.MainStatesObject.CurrentExperience;

            var receivedItems = new List<string>();
            foreach (var itemReward in crime.Reward.ItemRewards)
            {
                int dropChance = itemReward.DropChance;

                // ✅ SKILL #16: محظوظ (Increase rare drop chance)
                var luckySkill = player.Skills.FirstOrDefault(s => s.Id == 16 && s.IsEquipped);
                if (luckySkill != null)
                    dropChance = Math.Min(100, dropChance + (luckySkill.Level * 8));

                if (random.Next(100) < dropChance)
                {
                    int itemCount = random.Next(itemReward.MinCount, itemReward.MaxCount + 1);

                    // ✅ SKILL #22: الغنيمة غنيمته (Increase loot quantity)
                    var lootSkill = player.Skills.FirstOrDefault(s => s.Id == 22 && s.IsEquipped);
                    if (lootSkill != null)
                        itemCount = (int)(itemCount * (1 + (lootSkill.Level * 0.2)));

                    if (!player.StockObject.ItemsInStock.ContainsKey(itemReward.ItemId))
                        player.StockObject.ItemsInStock[itemReward.ItemId] = new StockItem { Count = 0 };
                    player.StockObject.ItemsInStock[itemReward.ItemId].Count += itemCount;
                    receivedItems.Add($"{itemCount} × {itemReward.ItemName}");
                }
            }

            int ownProgress = player.CrimeObject.TaskProgress.GetValueOrDefault(linearIndex, 0);
            player.CrimeObject.TaskProgress[linearIndex] = ownProgress + 1;

            int nextLinearIndex = linearIndex + 1;
            var nextTask = CrimeDatabase.GetTaskByLinearIndex(nextLinearIndex);
            if (nextTask != null)
            {
                int nextProgress = player.CrimeObject.TaskProgress.GetValueOrDefault(nextLinearIndex, 0);
                player.CrimeObject.TaskProgress[nextLinearIndex] = nextProgress + 1;

                int currentCrimeTypeId = CrimeDatabase.GetCrimeTypeIdByLinearIndex(linearIndex);
                int nextCrimeTypeId = CrimeDatabase.GetCrimeTypeIdByLinearIndex(nextLinearIndex);
                if (nextCrimeTypeId > currentCrimeTypeId)
                    player.CrimeObject.CurrentCrimeType = nextCrimeTypeId;
            }

            player.CrimeObject.TotalCrimesSuccessful++;
            MissionService.OnCrimeDone(player, crimeTypeId, true);

            // ✅ Check for medals after successful crime
            MedalService.CheckAndAwardAll(player);

            NotificationService.AddGameNotification(
                title: "✅ جريمة ناجحة!",
                message: $"نجحت في {crime.Name}!\n+{cashReward:N0} ذهب، +{xpReward} خبرة",
                priority: GameNotificationPriority.High,
                icon: "💰",
                actionTarget: "CrimePage"
            );

            string message = $"نجحت في {crime.Name}!\nحصلت على {cashReward:N0} ذهب و {xpReward} خبرة.";
            if (leveledUp) message += $"\n\n🎉 تمت ترقيتك إلى المستوى {player.Level}!";
            if (toolsLost && consumedTools.Count > 0) message += $"\n\nضاعت منك الأدوات:\n{string.Join("\n", consumedTools)}";
            else if (!toolsLost && crime.ToolRequirements.Count > 0) message += "\n\nلحسن الحظ، لم تضع أدواتك هذه المرة!";
            if (receivedItems.Count > 0) message += $"\n\nالغنيمة:\n{string.Join("\n", receivedItems)}";

            return (true, message);
        }
        else
        {
            player.CrimeObject.TotalCrimesFailed++;
            int confinementMinutes = 1 + crimeTypeId;

            if (random.Next(100) < 30)
            {
                player.CrimeObject.IsInHospital = true;
                player.CrimeObject.HospitalReason = $"فشلت في {crime.Name} وجرحت نفسك";
                player.CrimeObject.TotalHospitalVisits++;

                // ✅ SKILL #8: تعافي سريع (Reduce hospital time)
                int hospitalMinutes = 2 + (crimeTypeId * 5);
                var fastRecoverySkill = player.Skills.FirstOrDefault(s => s.Id == 8 && s.IsEquipped);
                if (fastRecoverySkill != null)
                {
                    double multiplier = Math.Max(0.50, 1.0 - (fastRecoverySkill.Level * 0.08));
                    hospitalMinutes = (int)(hospitalMinutes * multiplier);
                }

                player.CrimeObject.HospitalReleaseTime =
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)TimeSpan.FromMinutes(hospitalMinutes).TotalMilliseconds;
                player.CrimeObject.HealthCurrent = Math.Max(1, player.CrimeObject.HealthCurrent - 50);

                // ✅ Check for medals after hospital admission
                MedalService.CheckAndAwardAll(player);

                NotificationService.AddGameNotification(
                    title: "🏥 في المستشفى",
                    message: $"فشلت في {crime.Name}\nالعلاج: {hospitalMinutes} دقيقة",
                    priority: GameNotificationPriority.Normal,
                    icon: "🏥",
                    actionTarget: "HospitalPage"
                );

                string message = $"فشلت في {crime.Name}!\nتم نقلك إلى المستشفى لمدة {hospitalMinutes} دقائق.";
                if (toolsLost && consumedTools.Count > 0) message += $"\n\n⚠️ ضاعت منك الأدوات:\n{string.Join("\n", consumedTools)}";
                else if (!toolsLost && crime.ToolRequirements.Count > 0) message += "\n\n✨ هربت بسلام مع أدواتك.";

                MissionService.OnCrimeDone(player, crimeTypeId, false);

                var currentPage = Microsoft.Maui.Controls.Application.Current.MainPage;
                currentPage?.Dispatcher.Dispatch(async () => await currentPage.Navigation.PushModalAsync(new HospitalPage()));

                return (false, message);
            }
            else
            {
                player.CrimeObject.IsInPrison = true;
                player.CrimeObject.PrisonReleaseTime =
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)TimeSpan.FromMinutes(confinementMinutes).TotalMilliseconds;
                player.CrimeObject.TotalPrisonVisits++;
                player.CrimeObject.PrisonBailAmount = (crimeTypeId + 1) * 1000 + (player.Level * 500);
                player.CrimeObject.PrisonReason = $"فشلت في {crime.Name}";

                // ✅ Check for medals after prison admission
                MedalService.CheckAndAwardAll(player);

                NotificationService.AddGameNotification(
                    title: "⛓️ في السجن",
                    message: $"فشلت في {crime.Name}\nالكفالة: {player.CrimeObject.PrisonBailAmount:N0} ذهب",
                    priority: GameNotificationPriority.High,
                    icon: "⛓️",
                    actionTarget: "PrisonPage"
                );

                string message = $"فشلت في {crime.Name}!\nتم سجنك لمدة {confinementMinutes} دقائق.";
                if (toolsLost && consumedTools.Count > 0) message += $"\n\n⚠️ ضاعت منك الأدوات:\n{string.Join("\n", consumedTools)}";
                else if (!toolsLost && crime.ToolRequirements.Count > 0) message += "\n\n✨ هربت بسلام مع أدواتك.";

                MissionService.OnCrimeDone(player, crimeTypeId, false);

                var currentPage = Microsoft.Maui.Controls.Application.Current.MainPage;
                currentPage?.Dispatcher.Dispatch(async () => await currentPage.Navigation.PushModalAsync(new PrisonPage()));

                return (false, message);
            }
        }
    }
}
