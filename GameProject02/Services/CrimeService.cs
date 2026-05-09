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

    public static (bool success, string message) AttemptCrime(
        PlayerAccount player, int crimeTypeId, int crimeItemId)
    {
        // ── تحقق من وضع السجن/المستشفى ────────────────────────────────────────
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
            p?.Dispatcher.Dispatch(async () =>
                await p.Navigation.PushModalAsync(new PrisonPage()));
            return (false, "أنت في السجن حالياً!");
        }

        if (player.CrimeObject.IsInHospital)
            return (false, "أنت في المستشفى حالياً!");

        // ── الأدوات ────────────────────────────────────────────────────────────
        var consumedTools = new List<string>();
        bool toolsLost = false;

        foreach (var toolReq in crime.ToolRequirements)
        {
            if (!player.StockObject.ItemsInStock.TryGetValue(toolReq.ToolItemId, out var toolItem) ||
                toolItem.Count < toolReq.RequiredCount)
                return (false,
                    $"لا تملك أدوات كافية!\nتحتاج {toolReq.RequiredCount} × {toolReq.ToolName}.");
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

        // ── الشجاعة ────────────────────────────────────────────────────────────
        // ✅ نستخدم player.Courage (المصدر الموحد) وليس CrimeObject.Courage
        if (player.Courage < crime.CourageCost)
            return (false,
                $"ليس لديك ما يكفي من الشجاعة!\nتحتاج {crime.CourageCost} / لديك {player.Courage}.");

        // ✅ استهلاك الشجاعة من PlayerAccount مباشرة
        player.Courage -= crime.CourageCost;
        player.CrimeObject.TotalCrimesAttempted++;

        // ── نسبة النجاح ────────────────────────────────────────────────────────
        int currentProgress = player.CrimeObject.TaskProgress.GetValueOrDefault(linearIndex, 0);
        double percent = crime.RequiredSuccesses > 0
                              ? Math.Clamp((double)currentProgress / crime.RequiredSuccesses, 0, 1)
                              : 1.0;

        int colorBonus = percent >= 1.0 ? 0 : percent >= 0.50 ? -10 : -20;
        int baseChance = crime.BaseSuccessChance;
        int statBonus = player.Level / 2 + player.Dexterity / 10;
        int successChance = Math.Clamp(Math.Min(95, baseChance + statBonus + colorBonus), 5, 95);
        bool success = random.Next(100) < successChance;

        if (success)
        {
            // ── مكافآت النجاح ──────────────────────────────────────────────────
            int cashReward = random.Next(crime.Reward.CashRewardMin, crime.Reward.CashRewardMax + 1);
            player.Gold += cashReward;

            int xpReward = crime.Reward.ExperienceReward + (crimeTypeId * 10);
            bool leveledUp = player.MainStatesObject.AddExperience(xpReward);
            player.CurrentXP = player.MainStatesObject.CurrentExperience;

            var receivedItems = new List<string>();
            foreach (var itemReward in crime.Reward.ItemRewards)
            {
                if (random.Next(100) < itemReward.DropChance)
                {
                    int itemCount = random.Next(itemReward.MinCount, itemReward.MaxCount + 1);
                    if (!player.StockObject.ItemsInStock.ContainsKey(itemReward.ItemId))
                        player.StockObject.ItemsInStock[itemReward.ItemId] = new StockItem { Count = 0 };
                    player.StockObject.ItemsInStock[itemReward.ItemId].Count += itemCount;
                    receivedItems.Add($"{itemCount} × {itemReward.ItemName}");
                }
            }

            // ── تقدم السلسلة ────────────────────────────────────────────────────
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

            string message = $"نجحت في {crime.Name}!\nحصلت على {cashReward:N0} ذهب و {xpReward} خبرة.";
            if (leveledUp)
                message += $"\n\n🎉 تمت ترقيتك إلى المستوى {player.Level}!";
            if (toolsLost && consumedTools.Count > 0)
                message += $"\n\nضاعت منك الأدوات:\n{string.Join("\n", consumedTools)}";
            else if (!toolsLost && crime.ToolRequirements.Count > 0)
                message += "\n\nلحسن الحظ، لم تضع أدواتك هذه المرة!";
            if (receivedItems.Count > 0)
                message += $"\n\nالغنيمة:\n{string.Join("\n", receivedItems)}";

            return (true, message);
        }
        else
        {
            player.CrimeObject.TotalCrimesFailed++;
            int confinementMinutes = 1 + crimeTypeId;

            if (random.Next(100) < 30)
            {
                // ── مستشفى ─────────────────────────────────────────────────────
                player.CrimeObject.IsInHospital = true;
                player.CrimeObject.HospitalReason = $"فشلت في {crime.Name} وجرحت نفسك";
                player.CrimeObject.TotalHospitalVisits++;

                int hospitalMinutes = 2 + (crimeTypeId * 5);
                player.CrimeObject.HospitalReleaseTime =
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() +
                    (long)TimeSpan.FromMinutes(hospitalMinutes).TotalMilliseconds;
                player.CrimeObject.HealthCurrent =
                    Math.Max(1, player.CrimeObject.HealthCurrent - 50);

                string message = $"فشلت في {crime.Name}!\nتم نقلك إلى المستشفى لمدة {hospitalMinutes} دقائق.";
                if (toolsLost && consumedTools.Count > 0)
                    message += $"\n\n⚠️ ضاعت منك الأدوات:\n{string.Join("\n", consumedTools)}";
                else if (!toolsLost && crime.ToolRequirements.Count > 0)
                    message += "\n\n✨ هربت بسلام مع أدواتك.";

                MissionService.OnCrimeDone(player, crimeTypeId, false);

                var currentPage = Microsoft.Maui.Controls.Application.Current.MainPage;
                currentPage?.Dispatcher.Dispatch(async () =>
                    await currentPage.Navigation.PushModalAsync(new HospitalPage()));

                return (false, message);
            }
            else
            {
                // ── سجن ────────────────────────────────────────────────────────
                player.CrimeObject.IsInPrison = true;
                player.CrimeObject.PrisonReleaseTime =
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() +
                    (long)TimeSpan.FromMinutes(confinementMinutes).TotalMilliseconds;
                player.CrimeObject.TotalPrisonVisits++;
                player.CrimeObject.PrisonBailAmount =
                    (crimeTypeId + 1) * 1000 + (player.Level * 500);
                player.CrimeObject.PrisonReason = $"فشلت في {crime.Name}";

                string message = $"فشلت في {crime.Name}!\nتم سجنك لمدة {confinementMinutes} دقائق.";
                if (toolsLost && consumedTools.Count > 0)
                    message += $"\n\n⚠️ ضاعت منك الأدوات:\n{string.Join("\n", consumedTools)}";
                else if (!toolsLost && crime.ToolRequirements.Count > 0)
                    message += "\n\n✨ هربت بسلام مع أدواتك.";

                MissionService.OnCrimeDone(player, crimeTypeId, false);

                var currentPage = Microsoft.Maui.Controls.Application.Current.MainPage;
                currentPage?.Dispatcher.Dispatch(async () =>
                    await currentPage.Navigation.PushModalAsync(new PrisonPage()));

                return (false, message);
            }
        }
    }
}