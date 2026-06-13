using System;
using System.Collections.Generic;
using GameProject02.Models;

namespace GameProject02.Models;

public class SchoolObject
{
    // Lesson progress trackers (0=locked, 1=unlocked/available, 2=completed)
    public List<int> LawLessons { get; set; } = new List<int> { 1, 0, 0, 0, 0, 0, 0 };
    public List<int> MilitaryLessons { get; set; } = new List<int> { 1, 0, 0, 0, 0, 0, 0 };
    public List<int> HistoryLessons { get; set; } = new List<int> { 1, 0, 0, 0, 0 };
    public List<int> ScienceLessons { get; set; } = new List<int> { 1, 0, 0, 0, 0, 0, 0 };
    public List<int> GymLessons { get; set; } = new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 };

    // Study state
    public bool IsStudying { get; set; } = false;
    public long StartStudyingTimeInMilli { get; set; } = 0;
    public int CurrentCategory { get; set; } = 0;
    public int CurrentLesson { get; set; } = 0;

    public string GetLessonName(int category, int lesson)
    {
        return category switch
        {
            0 => lesson switch
            {
                0 => "مقدمة القانون",
                1 => "القانون الاقتصادي",
                2 => "القانون المدني",
                3 => "القانون الجنائي",
                4 => "القانون الاقتصادي المتقدم",
                5 => "القانون المدني المتقدم",
                6 => "القانون الجنائي المتقدم",
                _ => "درس قانون"
            },
            1 => lesson switch
            {
                0 => "مقدمة العلوم العسكرية",
                1 => "علم القواذف",
                2 => "علم الاسلحة البيضاء",
                3 => "علم الحراسة العسكرية",
                4 => "علم القواذف المتقدم",
                5 => "علم السلاح الابيض المتقدم",
                6 => "علم الحراسة المتقدم",
                _ => "درس عسكري"
            },
            2 => lesson switch
            {
                0 => "مقدمة التاريخ",
                1 => "علم الاثار",
                2 => "علم التفاوض",
                3 => "علم الاثار المتقدم",
                4 => "علم التفاوض المتقدم",
                _ => "درس تاريخ"
            },
            3 => lesson switch
            {
                0 => "مقدمة العلوم",
                1 => "علم التكنولوجيا",
                2 => "علم الميكانيكا",
                3 => "علم الهندسة",
                4 => "علم التكنولوجيا المتقدم",
                5 => "علم الميكانيكا المتقدم",
                6 => "علم الهندسة المتقدم",
                _ => "درس علوم"
            },
            4 => lesson switch
            {
                0 => "مقدمة الجيم",
                1 => "رفع اثقال",
                2 => "سباحة",
                3 => "ركض",
                4 => "مرونة",
                5 => "رفع اثقال مكثفة",
                6 => "سباحة مكثفة",
                7 => "ركض مكثف",
                8 => "مرونة مكثفة",
                _ => "درس لياقة"
            },
            _ => "درس"
        };
    }

    public string GetLessonDescription(int category, int lesson)
    {
        return category switch
        {
            0 => lesson switch
            {
                0 => "زيادة معدل نجاح الجريمة  5%",
                1 => "زيادة محصول فلوس الجريمة  5%",
                2 => "زيادة محصول الخبرة من الجريمة  5%",
                3 => "تقليل الوقت عند دخولك السجن او المستشفي عند ارتكاب جريمة  25%",
                4 => "زيادة محصول فلوس الجريمة  15%",
                5 => "زيادة محصول الخبرة من الجريمة  15%",
                6 => "تقليل الوقت عند دخولك السجن او المستشفي عند ارتكاب جريمة  50%",
                _ => "وصف قانون"
            },
            1 => lesson switch
            {
                0 => "زيادة وقت قضاء خصمك بالمشفي  5%",
                1 => "زيادة كفاءة السلاح الناري  5%",
                2 => "زيادة كفاءة السلاح الابيض  5%",
                3 => "زيادة الـ hp عندما تكون حارس شخصي  5%",
                4 => "زيادة كفاءة السلاح الناري  10%",
                5 => "زيادة كفاءة السلاح الابيض  10%",
                6 => "زيادة الـ hp عندما تكون حارس شخصي  10%",
                _ => "وصف عسكري"
            },
            2 => lesson switch
            {
                0 => "زيادة احتمالية ايجاد غنيمة في الصناديق  10%",
                1 => "زيادة نجاح جريمة تجارة الاثار  10%",
                2 => "تقليل معدل الضرايب عند الشراء من محلك  1%",
                3 => "زيادة نجاح جريمة تجارة الاثار  15%",
                4 => "تقليل الضرايب المدفوعة عند الشراء من محلك  2%",
                _ => "وصف تاريخ"
            },
            3 => lesson switch
            {
                0 => "تقليل الاضرار المتلقية من اعدائك  10%",
                1 => "زيادة نجاح جرايم التهكير  5%",
                2 => "زيادة نجاح جرايم السيارات  5%",
                3 => "تقليل تكاليف تعديلات العقارات  10%",
                4 => "زيادة نجاح جرايم التهكير  10%",
                5 => "اكتساب سعادة مضاعفة عند استخدام المعدات النادرة",
                6 => "زيادة السعادة من تعديلات العقار  20%",
                _ => "وصف علمي"
            },
            4 => lesson switch
            {
                0 => "زيادة كفاءة التمرين  5%",
                2 => "زيادة الدفاع  5%",
                3 => "زيادة السرعة  5%",
                4 => "زيادة المهارة  5%",
                5 => "زيادة القوة  15%",
                6 => "زيادة الدفاع  15%",
                7 => "زيادة السرعة  15%",
                8 => "زيادة المهارة  15%",
                _ => "وصف لياقة"
            },
            _ => "وصف الدرس"
        };
    }

    // ✅✅ MODIFIED: Get study time in MINUTES based on course level
    public int GetStudyTimeInMinutes(int category, int lesson)
    {
        if (lesson == 0)
        {
            return 7 * 24 * 60; // 7 أيام
        }
        else if (lesson >= 1 && lesson <= 3)
        {
            return 15 * 24 * 60; // 15 يوم
        }
        else
        {
            return 30 * 24 * 60; // 30 يوم
        }
    }

    // ✅✅ MODIFIED: Get study cost based on course level
    public int GetStudyCost(int category, int lesson)
    {
        if (lesson == 0)
        {
            return 5_000_000; // 5 مليون
        }
        else if (lesson >= 1 && lesson <= 3)
        {
            return 15_000_000; // 15 مليون
        }
        else
        {
            return 35_000_000; // 35 مليون
        }
    }

    // ✅✅ دالة تنسيق الأرقام (K, M, B) - 1 ألف = 1K, 1 مليون = 1M
    public string FormatNumber(long number)
    {
        if (number >= 1000000000)
            return (number / 1000000000.0).ToString("0.##") + "B";
        else if (number >= 1000000)
            return (number / 1000000.0).ToString("0.##") + "M";
        else if (number >= 1000)
            return (number / 1000.0).ToString("0.##") + "K";
        else
            return number.ToString();
    }

    public int GetStudyTimeInDays(int category, int lesson)
    {
        if (lesson == 0) return 7;
        else if (lesson >= 1 && lesson <= 3) return 15;
        else return 30;
    }

    public bool StartStudying(PlayerAccount player, int category, int lesson)
    {
        var lessons = GetLessonsForCategory(category);

        if (lesson < 0 || lesson >= lessons.Count || lessons[lesson] != 1)
            return false;

        int cost = GetStudyCost(category, lesson);
        if (player.Gold < cost)
            return false;

        player.Gold -= cost;

        IsStudying = true;
        CurrentCategory = category;
        CurrentLesson = lesson;
        StartStudyingTimeInMilli = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return true;
    }

    // ✅ MODIFIED: Check if studying is complete
    public bool IsStudyComplete(PlayerAccount player)
    {
        if (!IsStudying) return false;

        int studyMinutes = GetStudyTimeInMinutes(CurrentCategory, CurrentLesson);
        long studyMillis = studyMinutes * 60L * 1000L;

        if (player.IsVIP)
            studyMillis = (long)(studyMillis * 0.8);

        long elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - StartStudyingTimeInMilli;
        return elapsedTime >= studyMillis;
    }

    public void CancelStudying()
    {
        if (!IsStudying) return;

        IsStudying = false;
        StartStudyingTimeInMilli = 0;
    }

    public void CompleteStudy(PlayerAccount player)
    {
        if (!IsStudying) return;

        var lessons = GetLessonsForCategory(CurrentCategory);
        if (CurrentLesson < lessons.Count)
        {
            lessons[CurrentLesson] = 2;
            UnlockNextLessons(lessons, CurrentCategory, CurrentLesson);
        }

        ApplyStatBonuses(player, CurrentCategory, CurrentLesson);

        IsStudying = false;
        StartStudyingTimeInMilli = 0;
    }

    private void UnlockNextLessons(List<int> lessons, int category, int currentLesson)
    {
        if (category == 0)
        {
            if (currentLesson == 0)
            {
                UnlockLesson(lessons, 1);
                UnlockLesson(lessons, 2);
                UnlockLesson(lessons, 3);
            }
            else if (currentLesson == 1) UnlockLesson(lessons, 4);
            else if (currentLesson == 2) UnlockLesson(lessons, 5);
            else if (currentLesson == 3) UnlockLesson(lessons, 6);
        }
        else if (category == 1)
        {
            if (currentLesson == 0)
            {
                UnlockLesson(lessons, 1);
                UnlockLesson(lessons, 2);
                UnlockLesson(lessons, 3);
            }
            else if (currentLesson == 1) UnlockLesson(lessons, 4);
            else if (currentLesson == 2) UnlockLesson(lessons, 5);
            else if (currentLesson == 3) UnlockLesson(lessons, 6);
        }
        else if (category == 2)
        {
            if (currentLesson == 0)
            {
                UnlockLesson(lessons, 1);
                UnlockLesson(lessons, 2);
            }
            else if (currentLesson == 1) UnlockLesson(lessons, 3);
            else if (currentLesson == 2) UnlockLesson(lessons, 4);
        }
        else if (category == 3)
        {
            if (currentLesson == 0)
            {
                UnlockLesson(lessons, 1);
                UnlockLesson(lessons, 2);
                UnlockLesson(lessons, 3);
            }
            else if (currentLesson == 1) UnlockLesson(lessons, 4);
            else if (currentLesson == 2) UnlockLesson(lessons, 5);
            else if (currentLesson == 3) UnlockLesson(lessons, 6);
        }
        else if (category == 4)
        {
            if (currentLesson == 0)
            {
                UnlockLesson(lessons, 1);
                UnlockLesson(lessons, 2);
                UnlockLesson(lessons, 3);
                UnlockLesson(lessons, 4);
            }
            else if (currentLesson == 1) UnlockLesson(lessons, 5);
            else if (currentLesson == 2) UnlockLesson(lessons, 6);
            else if (currentLesson == 3) UnlockLesson(lessons, 7);
            else if (currentLesson == 4) UnlockLesson(lessons, 8);
        }
    }

    private void UnlockLesson(List<int> lessons, int lessonIndex)
    {
        if (lessonIndex >= 0 && lessonIndex < lessons.Count)
        {
            if (lessons[lessonIndex] == 0)
            {
                lessons[lessonIndex] = 1;
            }
        }
    }

    private void ApplyStatBonuses(PlayerAccount player, int category, int lesson)
    {
        switch (category)
        {
            case 0:
                if (lesson == 0) player.CrimeSuccessRate += 5;
                if (lesson == 1) player.CrimeGoldYield += 5;
                if (lesson == 2) player.CrimeExperienceYield += 5;
                if (lesson == 3) player.CrimePunishmentReduction += 25;
                if (lesson == 4) player.CrimeGoldYield += 10;
                if (lesson == 5) player.CrimeExperienceYield += 10;
                if (lesson == 6) player.CrimePunishmentReduction += 25;
                break;
            case 1:
                if (lesson == 0) player.HospitalTimeMultiplier += 5;
                if (lesson == 1) player.FirearmEfficiency += 5;
                if (lesson == 2) player.MeleeWeaponEfficiency += 5;
                if (lesson == 3) player.BodyguardHPBonus += 5;
                if (lesson == 4) player.FirearmEfficiency += 5;
                if (lesson == 5) player.MeleeWeaponEfficiency += 5;
                if (lesson == 6) player.BodyguardHPBonus += 5;
                break;
            case 2:
                if (lesson == 0) player.LootBoxChance += 10;
                if (lesson == 1) player.ArtifactCrimeSuccess += 10;
                if (lesson == 2) player.StallTaxReduction += 1;
                if (lesson == 3) player.ArtifactCrimeSuccess += 5;
                if (lesson == 4) player.StallTaxReduction += 1;
                break;
            case 3:
                if (lesson == 0) player.DamageReduction += 10;
                if (lesson == 1) player.HackingCrimeSuccess += 5;
                if (lesson == 2) player.CarCrimeSuccess += 5;
                if (lesson == 3) player.EstateModificationCostReduction += 10;
                if (lesson == 4) player.HackingCrimeSuccess += 5;
                if (lesson == 5) player.HappinessMultiplier += 2;
                if (lesson == 6) player.EstateHappinessBonus += 20;
                break;
            case 4:
                if (lesson == 0) player.GymEfficiency += 5;
                if (lesson == 1) player.Strength += (int)(player.Strength * 0.05);
                if (lesson == 2) player.Defense += (int)(player.Defense * 0.05);
                if (lesson == 3) player.Speed += (int)(player.Speed * 0.05);
                if (lesson == 4) player.Dexterity += (int)(player.Dexterity * 0.05);
                if (lesson == 5) player.Strength += (int)(player.Strength * 0.15);
                if (lesson == 6) player.Defense += (int)(player.Defense * 0.15);
                if (lesson == 7) player.Speed += (int)(player.Speed * 0.15);
                if (lesson == 8) player.Dexterity += (int)(player.Dexterity * 0.15);
                break;
        }
    }

    public List<int> GetLessonsForCategory(int category)
    {
        return category switch
        {
            0 => LawLessons,
            1 => MilitaryLessons,
            2 => HistoryLessons,
            3 => ScienceLessons,
            4 => GymLessons,
            _ => new List<int>()
        };
    }

    // ✅✅ MODIFIED: Get remaining study time (days and hours ONLY)
    public string GetRemainingStudyTime(PlayerAccount player)
    {
        if (!IsStudying) return "ليس قيد الدراسة";

        int studyMinutes = GetStudyTimeInMinutes(CurrentCategory, CurrentLesson);
        long studyMillis = studyMinutes * 60L * 1000L;

        if (player.IsVIP)
            studyMillis = (long)(studyMillis * 0.8);

        long elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - StartStudyingTimeInMilli;
        long remainingMillis = Math.Max(0, studyMillis - elapsedTime);

        // ✅ حساب الأيام والساعات فقط
        int totalMinutes = (int)(remainingMillis / (60 * 1000));
        int days = totalMinutes / (24 * 60);
        int hours = (totalMinutes % (24 * 60)) / 60;

        // ✅ عرض الأيام والساعات فقط
        if (days > 0)
        {
            if (hours > 0)
                return $"{days} يوم {hours} ساعة";
            return $"{days} يوم";
        }
        else if (hours > 0)
        {
            return $"{hours} ساعة";
        }
        return "يكتمل قريباً";
    }
}