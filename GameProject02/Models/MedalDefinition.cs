using System;
using System.Collections.Generic;

namespace GameProject02.Models;

public class MedalDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int MeritsReward { get; set; }
    public Func<PlayerAccount, bool> Condition { get; set; }

    public MedalDefinition(int id, string name, string desc, int merits, Func<PlayerAccount, bool> condition)
    {
        Id = id;
        Name = name;
        Description = desc;
        MeritsReward = merits;
        Condition = condition;
    }
}

public static class MedalDatabase
{
    public static readonly List<MedalDefinition> AllMedals = new()
    {
        // Crime achievements (based on old game medal IDs)
        new MedalDefinition(89, "مجرم مبتدئ", "تنفيذ أول جريمة ناجحة", 10, p => p.CrimeObject.TotalCrimesSuccessful >= 1),
        new MedalDefinition(90, "لص صغير", "تنفيذ 10 جرائم ناجحة", 20, p => p.CrimeObject.TotalCrimesSuccessful >= 10),
        new MedalDefinition(91, "لص محترف", "تنفيذ 50 جريمة ناجحة", 50, p => p.CrimeObject.TotalCrimesSuccessful >= 50),
        new MedalDefinition(92, "لص خبير", "تنفيذ 100 جريمة ناجحة", 100, p => p.CrimeObject.TotalCrimesSuccessful >= 100),
        new MedalDefinition(93, "أسطورة الجريمة", "تنفيذ 500 جريمة ناجحة", 250, p => p.CrimeObject.TotalCrimesSuccessful >= 500),

        // Level achievements
        new MedalDefinition(94, "الوصول للمستوى 5", "الوصول إلى المستوى 5", 15, p => p.Level >= 5),
        new MedalDefinition(95, "الوصول للمستوى 10", "الوصول إلى المستوى 10", 30, p => p.Level >= 10),
        new MedalDefinition(96, "الوصول للمستوى 20", "الوصول إلى المستوى 20", 60, p => p.Level >= 20),
        new MedalDefinition(97, "الوصول للمستوى 50", "الوصول إلى المستوى 50", 150, p => p.Level >= 50),
        new MedalDefinition(98, "الوصول للمستوى 100", "الوصول إلى المستوى 100", 300, p => p.Level >= 100),

        // Fight achievements (old game medal ID 113)
        new MedalDefinition(113, "مقاتل ناشئ", "الفوز في أول معركة", 15, p => p.Combat.BattlesWon >= 1),
        new MedalDefinition(114, "مقاتل شرس", "الفوز في 10 معارك", 40, p => p.Combat.BattlesWon >= 10),
        new MedalDefinition(115, "بطل القتال", "الفوز في 50 معركة", 100, p => p.Combat.BattlesWon >= 50),
        new MedalDefinition(116, "أسطورة القتال", "الفوز في 200 معركة", 250, p => p.Combat.BattlesWon >= 200),

        // Hospital visits (old game medal ID 30)
        new MedalDefinition(30, "أول زيارة للمستشفى", "دخول المستشفى", 5, p => p.CrimeObject.TotalHospitalVisits >= 1),
        new MedalDefinition(31, "مريض دائم", "دخول المستشفى 10 مرات", 20, p => p.CrimeObject.TotalHospitalVisits >= 10),
        new MedalDefinition(32, "ضيف شرف المستشفى", "دخول المستشفى 50 مرة", 60, p => p.CrimeObject.TotalHospitalVisits >= 50),

        // Prison visits (old game medal ID 33)
        new MedalDefinition(33, "أول زيارة للسجن", "دخول السجن", 5, p => p.CrimeObject.TotalPrisonVisits >= 1),
        new MedalDefinition(34, "مجرم متكرر", "دخول السجن 10 مرات", 25, p => p.CrimeObject.TotalPrisonVisits >= 10),
        new MedalDefinition(35, "محترف السجون", "دخول السجن 50 مرة", 75, p => p.CrimeObject.TotalPrisonVisits >= 50),

        // Flights (old game medal ID 37)
        new MedalDefinition(37, "أول رحلة جوية", "السفر إلى مدينة أخرى", 15, p => p.Flights >= 1),
        new MedalDefinition(38, "مسافر دائم", "السفر 10 مرات", 40, p => p.Flights >= 10),
        new MedalDefinition(39, "طيار محترف", "السفر 50 مرة", 100, p => p.Flights >= 50),

        // Herbs used (old game medal ID 93 is herbs – but we can use a different ID)
        new MedalDefinition(40, "معالج أعشاب", "استخدام 5 أعشاب", 10, p => p.HerbsUsed >= 5),
        new MedalDefinition(41, "خبير أعشاب", "استخدام 25 عشبة", 30, p => p.HerbsUsed >= 25),

        // Items found (tool rewards from crimes)
        new MedalDefinition(42, "جامع أدوات", "إيجاد 10 أدوات من الجرائم", 20, p => p.ItemsFound >= 10),

        // Gyms (training)
        new MedalDefinition(43, "رياضي مبتدئ", "إنفاق 100 طاقة في النادي", 15, p => p.Gym.LessonProgress[0] >= 100),
        new MedalDefinition(44, "رياضي محترف", "إنفاق 500 طاقة في النادي", 50, p => p.Gym.LessonProgress.Sum() >= 500),

        // Estates
        new MedalDefinition(45, "مالك عقار", "شراء أول عقار", 25, p => p.Estates.Count > 1), // excluding default shack
        new MedalDefinition(46, "مستثمر عقاري", "امتلاك 3 عقارات", 75, p => p.Estates.Count >= 3),

        // Gang medals (if gang system used)
        new MedalDefinition(47, "انضمام لعصابة", "الانضمام إلى عصابة", 30, p => !string.IsNullOrEmpty(p.GangId)),
        new MedalDefinition(48, "ولاء للعصابة", "التبرع بـ 10,000 ذهب للعصابة", 50, p => p.PersonalContribution >= 10000),

        // School medals
        new MedalDefinition(49, "طالب مجتهد", "إكمال أول درس في المدرسة", 20, p => p.School.LawLessons[0] == 2 || p.School.MilitaryLessons[0] == 2 ||
                                                                 p.School.HistoryLessons[0] == 2 || p.School.ScienceLessons[0] == 2 ||
                                                                 p.School.GymLessons[0] == 2),

        // Fighting win streaks
        new MedalDefinition(50, "سلسلة انتصارات", "الفوز في 5 معارك متتالية", 40, p => p.Combat.BattlesWon >= 5 && p.Combat.BattlesWon == p.Combat.BattlesFought),

        // Complete crime type (16 crime types) – example for first type
        new MedalDefinition(60, "مطاردة المال كاملة", "إكمال جميع جرائم مطاردة المال", 100, p => p.CrimeObject.CurrentCrimeType > 0 && p.CrimeObject.TaskProgress.Count > 0),
    };
}