using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class HospitalService
{
    // ✅ أقسام العمل في المستشفى (من اللعبة القديمة)
    private static readonly List<HospitalWorkDefinition> _hospitalWorks = new()
    {
        new HospitalWorkDefinition
        {
            WorkLevel = 3,
            Name = "تنظيف الغرف",
            Description = "ساعد في تنظيف غرف المرضى واستلم راتباً بسيطاً",
            RequiredCash = 100000,
            RequiredCourage = 10,
            HealthRestored = 75,
            ExperienceReward = 75,
            RequiredPlayerLevel = 0
        },
        new HospitalWorkDefinition
        {
            WorkLevel = 4,
            Name = "الصيدلية",
            Description = "ساعد في توزيع الأدوية واحصل على راتب أفضل",
            RequiredCash = 300000,
            RequiredCourage = 15,
            HealthRestored = 250,
            ExperienceReward = 250,
            RequiredPlayerLevel = 103
        },
        new HospitalWorkDefinition
        {
            WorkLevel = 5,
            Name = "المختبر الطبي",
            Description = "ساعد في تحليل العينات واحصل على راتب ممتاز",
            RequiredCash = 400000,
            RequiredCourage = 20,
            HealthRestored = 500,
            ExperienceReward = 500,
            RequiredPlayerLevel = 119
        }
    };

    public static List<HospitalWorkDefinition> GetAvailableWorks(PlayerAccount player)
    {
        return _hospitalWorks
            .Where(work => player.Level >= work.RequiredPlayerLevel)
            .ToList();
    }

    public static (bool success, string message) PerformWork(PlayerAccount player, int workLevel)
    {
        if (!player.CrimeObject.IsInHospital)
            return (false, "أنت لست في المستشفى حالياً!");

        var work = _hospitalWorks.FirstOrDefault(w => w.WorkLevel == workLevel);
        if (work == null)
            return (false, "عمل غير موجود!");

        if (player.Level < work.RequiredPlayerLevel)
            return (false, $"تحتاج إلى المستوى {work.RequiredPlayerLevel} لتنفيذ هذا العمل.");

        if (player.Gold < work.RequiredCash)
            return (false, $"ليس لديك ما يكفي من المال!\nتحتاج {work.RequiredCash:N0} ذهب.");

        if (player.CrimeObject.Courage < work.RequiredCourage)
            return (false, $"ليس لديك ما يكفي من الشجاعة!\nتحتاج {work.RequiredCourage} نقطة.");

        // خصم التكاليف
        player.Gold -= work.RequiredCash;
        player.CrimeObject.Courage -= work.RequiredCourage;

        // استعادة الصحة
        int healthBefore = player.CrimeObject.HealthCurrent;
        player.CrimeObject.HealthCurrent = Math.Min(
            player.CrimeObject.HealthMax,
            player.CrimeObject.HealthCurrent + work.HealthRestored
        );
        int healthGained = player.CrimeObject.HealthCurrent - healthBefore;

        // منح الخبرة
        player.CurrentXP += work.ExperienceReward;

        // تقليل وقت المستشفى (25% لكل عمل)
        if (player.CrimeObject.HospitalReleaseTime > 0)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long remainingTime = player.CrimeObject.HospitalReleaseTime - now;
            long timeReduction = (long)(remainingTime * 0.25);
            player.CrimeObject.HospitalReleaseTime = Math.Max(
                now,
                player.CrimeObject.HospitalReleaseTime - timeReduction
            );
        }

        string message = $"أكملت العمل بنجاح في المستشفى!\n" +
                       $"الصحة المستعادة: +{healthGained}\n" +
                       $"الخبرة: +{work.ExperienceReward}\n" +
                       $"الوقت المتبقي: {GetRemainingTime(player.CrimeObject.HospitalReleaseTime)}";

        return (true, message);
    }

    public static (bool success, string message) UseHealthTank(PlayerAccount player)
    {
        if (!player.CrimeObject.IsInHospital)
            return (false, "لا يمكنك استخدام خزان الصحة إلا وأنت في المستشفى!");

        const string healthTankId = "health_tank";
        if (!player.StockObject.ItemsInStock.TryGetValue(healthTankId, out var tankItem)
            || tankItem.Count < 1)
            return (false, "ليس لديك خزان صحة كافي!\nاشترِ خزان صحة من الصيدلية.");

        // استهلاك الخزان
        tankItem.Count--;
        if (tankItem.Count <= 0)
            player.StockObject.ItemsInStock.Remove(healthTankId);

        // خروج فوري واستعادة الصحة
        player.CrimeObject.IsInHospital = false;
        player.CrimeObject.HospitalReleaseTime = 0;
        player.CrimeObject.HealthCurrent = player.CrimeObject.HealthMax;

        return (true, "✅ تم استخدام خزان الصحة!\nخرجت من المستشفى فوراً مع استعادة كامل صحتك.");
    }

    // ✅ دالة لجلب جميع المرضى الحاليين (لحالة الزائر)
    public static List<PlayerAccount> GetPatients()
    {
        var allPlayers = AccountService.GetAllPlayers();

        // تصفية اللاعبين الموجودين حالياً في المستشفى ولم تنته مدتهم
        var patients = allPlayers
            .Where(p => p.CrimeObject.IsInHospital &&
                       p.CrimeObject.HospitalReleaseTime > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            .ToList();

        System.Diagnostics.Debug.WriteLine($"[HOSPITAL] Total players: {allPlayers.Count}");
        System.Diagnostics.Debug.WriteLine($"[HOSPITAL] Patients: {patients.Count}");

        return patients;
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

// تعريف فئة العمل (موجودة أصلاً في ملف منفصل، نضعها هنا للتكامل)
public class HospitalWorkDefinition
{
    public int WorkLevel { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageResource { get; set; } = "work_hospital";
    public int RequiredCash { get; set; }
    public int RequiredCourage { get; set; }
    public int HealthRestored { get; set; }
    public int ExperienceReward { get; set; }
    public int RequiredPlayerLevel { get; set; } = 0;
}