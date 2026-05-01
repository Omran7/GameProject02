using GameProject02.Models;
using System;
using System.Collections.Generic;

namespace GameProject02.Services;

public static class WorkOfficeService
{
    private static readonly List<WorkCategory> _categories = new();

    static WorkOfficeService()
    {
        // Category 0: Restaurant
        var restaurant = new WorkCategory
        {
            Id = 0,
            Name = "مطعم",
            ImageResource = "work_restaurant.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "غسيل صحون", Description = "غسل الأطباق في المطعم", SalaryGold = 1000, ExperienceReward = 25, RequiredStrength = 1000, RequiredDefense = 1000, RequiredSpeed = 1000, RequiredDexterity = 1000, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "طباخ", Description = "طهي الطعام", SalaryGold = 5000, ExperienceReward = 100, RequiredStrength = 10, RequiredDefense = 5, RequiredSpeed = 10, RequiredDexterity = 15, RequiredDaysWorked = 3 },
                new() { Level = 2, Name = "شيف", Description = "إدارة المطبخ", SalaryGold = 20000, ExperienceReward = 500, RequiredStrength = 30, RequiredDefense = 20, RequiredSpeed = 30, RequiredDexterity = 40, RequiredDaysWorked = 7 },
                new() { Level = 3, Name = "مدير مطعم", Description = "إدارة المطعم بالكامل", SalaryGold = 80000, ExperienceReward = 2000, RequiredStrength = 60, RequiredDefense = 50, RequiredSpeed = 60, RequiredDexterity = 80, RequiredDaysWorked = 14 },
                new() { Level = 4, Name = "مالك سلسلة مطاعم", Description = "تملك عدة مطاعم", SalaryGold = 300000, ExperienceReward = 8000, RequiredStrength = 100, RequiredDefense = 100, RequiredSpeed = 100, RequiredDexterity = 150, RequiredDaysWorked = 30 },
            }
        };
        _categories.Add(restaurant);

        // Category 1: Bank
        var bank = new WorkCategory
        {
            Id = 1,
            Name = "بنك",
            ImageResource = "work_bank.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "حارس أمن", Description = "حراسة البنك", SalaryGold = 1500, ExperienceReward = 30, RequiredStrength = 5, RequiredDefense = 5, RequiredSpeed = 5, RequiredDexterity = 5, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "صراف", Description = "خدمة العملاء", SalaryGold = 8000, ExperienceReward = 200, RequiredStrength = 10, RequiredDefense = 10, RequiredSpeed = 15, RequiredDexterity = 20, RequiredDaysWorked = 5 },
                new() { Level = 2, Name = "محلل مالي", Description = "تحليل الأصول", SalaryGold = 35000, ExperienceReward = 800, RequiredStrength = 20, RequiredDefense = 20, RequiredSpeed = 30, RequiredDexterity = 50, RequiredDaysWorked = 10 },
                new() { Level = 3, Name = "مدير فرع", Description = "إدارة فرع البنك", SalaryGold = 150000, ExperienceReward = 3000, RequiredStrength = 40, RequiredDefense = 40, RequiredSpeed = 50, RequiredDexterity = 80, RequiredDaysWorked = 20 },
                new() { Level = 4, Name = "نائب رئيس", Description = "إدارة إقليمية", SalaryGold = 600000, ExperienceReward = 12000, RequiredStrength = 80, RequiredDefense = 80, RequiredSpeed = 100, RequiredDexterity = 150, RequiredDaysWorked = 40 },
            }
        };
        _categories.Add(bank);

        // Category 2: Cinema
        var cinema = new WorkCategory
        {
            Id = 2,
            Name = "سينما",
            ImageResource = "work_cinema.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "موزع تذاكر", Description = "بيع التذاكر", SalaryGold = 1200, ExperienceReward = 25, RequiredStrength = 0, RequiredDefense = 0, RequiredSpeed = 5, RequiredDexterity = 5, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "مساعد إدارة", Description = "مساعدة إدارة الصالة", SalaryGold = 6000, ExperienceReward = 150, RequiredStrength = 5, RequiredDefense = 5, RequiredSpeed = 15, RequiredDexterity = 20, RequiredDaysWorked = 4 },
                new() { Level = 2, Name = "مدير صالة", Description = "إدارة الصالة", SalaryGold = 25000, ExperienceReward = 600, RequiredStrength = 20, RequiredDefense = 15, RequiredSpeed = 30, RequiredDexterity = 40, RequiredDaysWorked = 8 },
                new() { Level = 3, Name = "مدير عام", Description = "إدارة السينما", SalaryGold = 100000, ExperienceReward = 2500, RequiredStrength = 40, RequiredDefense = 30, RequiredSpeed = 50, RequiredDexterity = 70, RequiredDaysWorked = 15 },
            }
        };
        _categories.Add(cinema);

        // Category 3: Science Lab
        var scienceLab = new WorkCategory
        {
            Id = 3,
            Name = "مختبر علوم",
            ImageResource = "work_science_lab.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "مساعد مختبر", Description = "تنظيف الأدوات", SalaryGold = 2000, ExperienceReward = 50, RequiredStrength = 0, RequiredDefense = 0, RequiredSpeed = 5, RequiredDexterity = 10, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "فني", Description = "تحضير العينات", SalaryGold = 10000, ExperienceReward = 250, RequiredStrength = 5, RequiredDefense = 5, RequiredSpeed = 15, RequiredDexterity = 30, RequiredDaysWorked = 6 },
                new() { Level = 2, Name = "باحث", Description = "إجراء التجارب", SalaryGold = 40000, ExperienceReward = 1000, RequiredStrength = 15, RequiredDefense = 15, RequiredSpeed = 30, RequiredDexterity = 60, RequiredDaysWorked = 12 },
                new() { Level = 3, Name = "عالم", Description = "أبحاث متقدمة", SalaryGold = 180000, ExperienceReward = 4500, RequiredStrength = 30, RequiredDefense = 30, RequiredSpeed = 50, RequiredDexterity = 100, RequiredDaysWorked = 25 },
            }
        };
        _categories.Add(scienceLab);

        // Category 4: Army Camp
        var army = new WorkCategory
        {
            Id = 4,
            Name = "معسكر الجيش",
            ImageResource = "work_army.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "جندي", Description = "تدريبات أساسية", SalaryGold = 2000, ExperienceReward = 60, RequiredStrength = 10, RequiredDefense = 10, RequiredSpeed = 10, RequiredDexterity = 5, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "رقيب", Description = "قيادة فريق", SalaryGold = 10000, ExperienceReward = 300, RequiredStrength = 30, RequiredDefense = 20, RequiredSpeed = 20, RequiredDexterity = 15, RequiredDaysWorked = 5 },
                new() { Level = 2, Name = "ملازم", Description = "قيادة فصيلة", SalaryGold = 45000, ExperienceReward = 1200, RequiredStrength = 60, RequiredDefense = 40, RequiredSpeed = 40, RequiredDexterity = 30, RequiredDaysWorked = 10 },
                new() { Level = 3, Name = "نقيب", Description = "قيادة سرية", SalaryGold = 200000, ExperienceReward = 5000, RequiredStrength = 100, RequiredDefense = 70, RequiredSpeed = 70, RequiredDexterity = 50, RequiredDaysWorked = 20 },
                new() { Level = 4, Name = "عقيد", Description = "قيادة كتيبة", SalaryGold = 800000, ExperienceReward = 20000, RequiredStrength = 150, RequiredDefense = 100, RequiredSpeed = 100, RequiredDexterity = 80, RequiredDaysWorked = 40 },
            }
        };
        _categories.Add(army);

        // Category 5: Hospital
        var hospitalWork = new WorkCategory
        {
            Id = 5,
            Name = "مستشفى (وظيفة)",
            ImageResource = "work_hospital.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "منظف", Description = "تنظيف الغرف", SalaryGold = 1000, ExperienceReward = 25, RequiredStrength = 0, RequiredDefense = 0, RequiredSpeed = 5, RequiredDexterity = 5, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "مساعد تمريض", Description = "مساعدة الممرضين", SalaryGold = 5000, ExperienceReward = 120, RequiredStrength = 5, RequiredDefense = 5, RequiredSpeed = 10, RequiredDexterity = 15, RequiredDaysWorked = 4 },
                new() { Level = 2, Name = "ممرض", Description = "رعاية المرضى", SalaryGold = 20000, ExperienceReward = 500, RequiredStrength = 10, RequiredDefense = 10, RequiredSpeed = 20, RequiredDexterity = 30, RequiredDaysWorked = 8 },
                new() { Level = 3, Name = "طبيب", Description = "تشخيص وعلاج", SalaryGold = 90000, ExperienceReward = 2000, RequiredStrength = 20, RequiredDefense = 20, RequiredSpeed = 40, RequiredDexterity = 60, RequiredDaysWorked = 15 },
                new() { Level = 4, Name = "جراح", Description = "عمليات جراحية", SalaryGold = 350000, ExperienceReward = 8000, RequiredStrength = 40, RequiredDefense = 40, RequiredSpeed = 60, RequiredDexterity = 100, RequiredDaysWorked = 30 },
            }
        };
        _categories.Add(hospitalWork);

        // Category 6: Coal Mining
        var coalMining = new WorkCategory
        {
            Id = 6,
            Name = "منجم فحم",
            ImageResource = "work_coal_mining.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "عامل منجم", Description = "استخراج الفحم", SalaryGold = 3000, ExperienceReward = 80, RequiredStrength = 15, RequiredDefense = 5, RequiredSpeed = 5, RequiredDexterity = 5, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "مشرف عمال", Description = "إدارة العمال", SalaryGold = 15000, ExperienceReward = 400, RequiredStrength = 40, RequiredDefense = 20, RequiredSpeed = 15, RequiredDexterity = 15, RequiredDaysWorked = 7 },
                new() { Level = 2, Name = "مهندس تعدين", Description = "تخطيط الأنفاق", SalaryGold = 60000, ExperienceReward = 1500, RequiredStrength = 70, RequiredDefense = 40, RequiredSpeed = 30, RequiredDexterity = 40, RequiredDaysWorked = 14 },
                new() { Level = 3, Name = "مدير منجم", Description = "إدارة المنجم", SalaryGold = 250000, ExperienceReward = 6000, RequiredStrength = 120, RequiredDefense = 70, RequiredSpeed = 50, RequiredDexterity = 70, RequiredDaysWorked = 25 },
            }
        };
        _categories.Add(coalMining);

        // Category 7: Freelancer
        var freelancer = new WorkCategory
        {
            Id = 7,
            Name = "مستقل",
            ImageResource = "work_freelancer.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "ساعي", Description = "توصيل الطلبات", SalaryGold = 800, ExperienceReward = 20, RequiredStrength = 2, RequiredDefense = 0, RequiredSpeed = 10, RequiredDexterity = 5, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "مصمم جرافيك", Description = "تصميم شعارات", SalaryGold = 12000, ExperienceReward = 300, RequiredStrength = 5, RequiredDefense = 5, RequiredSpeed = 20, RequiredDexterity = 40, RequiredDaysWorked = 5 },
                new() { Level = 2, Name = "مطور برامج", Description = "برمجة تطبيقات", SalaryGold = 50000, ExperienceReward = 1200, RequiredStrength = 10, RequiredDefense = 10, RequiredSpeed = 40, RequiredDexterity = 80, RequiredDaysWorked = 10 },
                new() { Level = 3, Name = "مدير مشاريع", Description = "إدارة فرق العمل", SalaryGold = 200000, ExperienceReward = 5000, RequiredStrength = 30, RequiredDefense = 30, RequiredSpeed = 60, RequiredDexterity = 120, RequiredDaysWorked = 20 },
            }
        };
        _categories.Add(freelancer);

        // Category 8: School
        var school = new WorkCategory
        {
            Id = 8,
            Name = "مدرسة",
            ImageResource = "work_school.png",
            Jobs = new List<JobDefinition>
            {
                new() { Level = 0, Name = "حارس مدرسة", Description = "حراسة المدرسة", SalaryGold = 1200, ExperienceReward = 30, RequiredStrength = 5, RequiredDefense = 3, RequiredSpeed = 5, RequiredDexterity = 3, RequiredDaysWorked = 0 },
                new() { Level = 1, Name = "مساعد معلم", Description = "مساعدة المعلمين", SalaryGold = 6000, ExperienceReward = 150, RequiredStrength = 5, RequiredDefense = 5, RequiredSpeed = 10, RequiredDexterity = 15, RequiredDaysWorked = 5 },
                new() { Level = 2, Name = "معلم", Description = "تدريس الطلاب", SalaryGold = 25000, ExperienceReward = 600, RequiredStrength = 10, RequiredDefense = 10, RequiredSpeed = 20, RequiredDexterity = 30, RequiredDaysWorked = 10 },
                new() { Level = 3, Name = "مدير مدرسة", Description = "إدارة المدرسة", SalaryGold = 100000, ExperienceReward = 2500, RequiredStrength = 25, RequiredDefense = 25, RequiredSpeed = 40, RequiredDexterity = 60, RequiredDaysWorked = 20 },
            }
        };
        _categories.Add(school);
    }

    public static List<WorkCategory> GetAllCategories() => _categories;

    public static WorkCategory GetCategoryById(int id) => _categories.Find(c => c.Id == id);

    public static JobDefinition GetJob(int categoryId, int jobLevel)
    {
        var category = GetCategoryById(categoryId);
        if (category == null || jobLevel < 0 || jobLevel >= category.Jobs.Count)
            return null;
        return category.Jobs[jobLevel];
    }

    public static bool CanPromote(PlayerAccount player, int categoryId, int currentJobLevel)
    {
        var nextJob = GetJob(categoryId, currentJobLevel + 1);
        if (nextJob == null) return false;

        if (player.Strength < nextJob.RequiredStrength) return false;
        if (player.Defense < nextJob.RequiredDefense) return false;
        if (player.Speed < nextJob.RequiredSpeed) return false;
        if (player.Dexterity < nextJob.RequiredDexterity) return false;

        if (player.WorkObject.JobStartTimeMilli > 0)
        {
            double daysWorked = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - player.WorkObject.JobStartTimeMilli) / (86400.0 * 1000.0);
            if (daysWorked < nextJob.RequiredDaysWorked) return false;
        }
        else if (nextJob.RequiredDaysWorked > 0) return false;

        if (!string.IsNullOrEmpty(nextJob.RequiredCertificateItemId))
        {
            if (!player.StockObject.ItemsInStock.ContainsKey(nextJob.RequiredCertificateItemId) ||
                player.StockObject.ItemsInStock[nextJob.RequiredCertificateItemId].Count <= 0)
                return false;
        }

        return true;
    }

    public static bool CanCollectSalary(PlayerAccount player)
    {
        if (player.WorkObject.JobStartTimeMilli == 0)
            return false;

        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long startTime = player.WorkObject.JobStartTimeMilli;
        long lastCollect = player.WorkObject.JobGotSalaryTimeMilli;

        // الوقت الأصلي: 24 ساعة
        long cooldown = 24 * 60 * 60 * 1000;

        if (lastCollect == 0)
        {
            return (now - startTime) >= cooldown;
        }
        else
        {
            return (now - lastCollect) >= cooldown;
        }
    }

    public static (bool success, string message, long goldGained, int xpGained) CollectSalary(PlayerAccount player)
    {
        if (!CanCollectSalary(player))
            return (false, "لقد حصلت على راتبك اليوم بالفعل! انتظر حتى الغد.", 0, 0);

        if (player.WorkObject.WorkType < 0 || player.WorkObject.JobLevel < 0)
            return (false, "أنت لا تعمل في أي وظيفة حالياً!", 0, 0);

        var job = GetJob(player.WorkObject.WorkType, player.WorkObject.JobLevel);
        if (job == null) return (false, "خطأ في بيانات الوظيفة", 0, 0);

        var random = new Random();
        double bonus = 1.0 + (random.NextDouble() * 0.04);
        long goldGained = (long)(job.SalaryGold * bonus);
        int xpGained = job.ExperienceReward;

        player.Gold += (int)goldGained;
        player.CurrentXP += xpGained;
        player.WorkObject.JobGotSalaryTimeMilli = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        while (player.MainStatesObject.CanLevelUp())
            player.MainStatesObject.LevelUp();

        return (true, $"حصلت على راتب {goldGained:N0} ذهب و {xpGained} خبرة", goldGained, xpGained);
    }

    public static (bool success, string message) Promote(PlayerAccount player, int categoryId, int currentJobLevel)
    {
        if (!CanPromote(player, categoryId, currentJobLevel))
            return (false, "لا تستوفي متطلبات الترقية!");

        var nextJob = GetJob(categoryId, currentJobLevel + 1);
        if (nextJob == null) return (false, "لا توجد ترقية أعلى");

        if (!string.IsNullOrEmpty(nextJob.RequiredCertificateItemId))
        {
            if (!player.StockObject.ItemsInStock.TryGetValue(nextJob.RequiredCertificateItemId, out var certItem) || certItem.Count <= 0)
                return (false, "تحتاج إلى الشهادة المطلوبة للترقية");

            certItem.Count--;
            if (certItem.Count <= 0)
                player.StockObject.ItemsInStock.Remove(nextJob.RequiredCertificateItemId);
            player.StockObject.StockFreeSpace += 1;
        }

        player.WorkObject.WorkType = categoryId;
        player.WorkObject.JobLevel = currentJobLevel + 1;
        player.WorkObject.JobStartTimeMilli = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        player.WorkObject.JobGotSalaryTimeMilli = 0;

        return (true, $"تهانينا! تمت ترقيتك إلى {nextJob.Name}!");
    }

    public static (bool success, string message) StartJob(PlayerAccount player, int categoryId)
    {
        var category = GetCategoryById(categoryId);
        if (category == null || category.Jobs.Count == 0)
            return (false, "وظيفة غير صالحة");

        var firstJob = category.Jobs[0];
        if (firstJob == null) return (false, "خطأ في البيانات");

        player.WorkObject.WorkType = categoryId;
        player.WorkObject.JobLevel = 0;
        player.WorkObject.JobStartTimeMilli = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        player.WorkObject.JobGotSalaryTimeMilli = 0;

        return (true, $"بدأت العمل كـ {firstJob.Name} في {category.Name}");
    }

    public static (bool hasJob, WorkCategory category, JobDefinition job, double daysWorked) GetCurrentJob(PlayerAccount player)
    {
        if (player.WorkObject.WorkType < 0 || player.WorkObject.JobLevel < 0)
            return (false, null, null, 0);

        var category = GetCategoryById(player.WorkObject.WorkType);
        if (category == null) return (false, null, null, 0);

        var job = GetJob(category.Id, player.WorkObject.JobLevel);
        if (job == null) return (false, null, null, 0);

        double daysWorked = 0;
        if (player.WorkObject.JobStartTimeMilli > 0)
            daysWorked = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - player.WorkObject.JobStartTimeMilli) / (86400.0 * 1000.0);

        return (true, category, job, daysWorked);
    }
}