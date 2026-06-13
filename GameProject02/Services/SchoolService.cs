using GameProject02.Models;
using System;

namespace GameProject02.Services;

public static class SchoolService
{
    public static bool CanStartStudying(PlayerAccount player, int category, int lesson)
    {
        var school = player.School;
        var lessons = school.GetLessonsForCategory(category);

        if (lesson < 0 || lesson >= lessons.Count || lessons[lesson] != 1)
            return false;

        int cost = school.GetStudyCost(category, lesson);
        return player.Gold >= cost;
    }

    public static bool StartStudying(PlayerAccount player, int category, int lesson)
    {
        return player.School.StartStudying(player, category, lesson);
    }

    public static bool IsStudyComplete(PlayerAccount player)
    {
        return player.School.IsStudyComplete(player);
    }

    public static void CompleteStudy(PlayerAccount player)
    {
        player.School.CompleteStudy(player);
    }

    // ✅ MODIFIED: Get study progress percentage (supports minutes)
    public static double GetStudyProgress(PlayerAccount player)
    {
        var school = player.School;
        if (!school.IsStudying) return 0;

        int studyMinutes = school.GetStudyTimeInMinutes(school.CurrentCategory, school.CurrentLesson);
        long studyMillis = studyMinutes * 60L * 1000L;

        if (player.IsVIP)
            studyMillis = (long)(studyMillis * 0.8);

        long elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - school.StartStudyingTimeInMilli;
        return Math.Min(1.0, (double)elapsedTime / studyMillis);
    }

    public static void CancelStudying(PlayerAccount player)
    {
        player.School.CancelStudying();
    }
}