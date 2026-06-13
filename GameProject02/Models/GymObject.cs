using System;
using System.Collections.Generic;
using GameProject02.Models;

namespace GameProject02.Models;

public class GymObject
{
    // ✅ REDUCED TO 5 LESSONS (Running Gym REMOVED)
    // Lesson indices: 0=Basic, 1=Muscle, 2=Heart, 3=Speed, 4=Agility
    public int SelectedLesson { get; set; } = 0;

    // Progress for lessons 0-4 (5 lessons total)
    public List<int> LessonProgress { get; set; } = new List<int> { 0, 0, 0, 0, 0 };

    // Unlock states (lesson 0 unlocked by default, others unlocked after Basic completion)
    public List<bool> LessonUnlocked { get; set; } = new List<bool> { true, false, false, false, false };

    // ✅ GET LESSON INFO (5 lessons only - Running Gym REMOVED)
    public static LessonInfo GetLessonInfo(int lessonIndex)
    {
        return lessonIndex switch
        {
            0 => new LessonInfo("الجيم الأساسي", "💪", "كفاءة التمرين في كل المهارات متساوية", 0),
            1 => new LessonInfo("جيم العضلات", "🏋️", "يركز على زيادة القوة بشكل رئيسي", 1000),
            2 => new LessonInfo("جيم قوة القلب", "❤️", "تحسين الصحة والتحمل العام", 5000),
            3 => new LessonInfo("جيم السرعة", "🏃", "يركز على زيادة السرعة بشكل رئيسي", 20000),
            4 => new LessonInfo("جيم الرشاقة", "🤸", "يركز على زيادة المهارة بشكل رئيسي", 500000),
            _ => new LessonInfo("درس غير معروف", "?", "وصف غير معروف", 0)
        };
    }

    // ✅ TRAIN WITH AUTHENTIC CALCULATIONS (0.5 points per energy + multipliers)
    public (bool success, string message, int totalEnergySpent) Train(PlayerAccount player, int[] energyPerStat)
    {
        if (energyPerStat == null || energyPerStat.Length != 4)
            return (false, "❌ خطأ في توزيع الطاقة", 0);

        int totalEnergy = energyPerStat[0] + energyPerStat[1] + energyPerStat[2] + energyPerStat[3];

        if (player.Energy < totalEnergy)
            return (false, $"❌ ليس لديك طاقة كافية للتدريب! تحتاج {totalEnergy} طاقة وتملك فقط {player.Energy}", 0);

        // ✅ DEDUCT ENERGY FIRST (authentic behavior)
        player.Energy -= totalEnergy;

        // ✅ AUTHENTIC STAT GAIN: 0.5 points per energy point + lesson multipliers
        double baseGainPerEnergy = 0.5;
        double strengthGain = energyPerStat[0] * baseGainPerEnergy * GetStrengthMultiplier();
        double defenseGain = energyPerStat[1] * baseGainPerEnergy * GetDefenseMultiplier();
        double speedGain = energyPerStat[2] * baseGainPerEnergy * GetSpeedMultiplier();
        double dexterityGain = energyPerStat[3] * baseGainPerEnergy * GetDexterityMultiplier();

        // Update stats (rounded to nearest integer)
        player.Strength += (int)Math.Round(strengthGain);
        player.Defense += (int)Math.Round(defenseGain);
        player.Speed += (int)Math.Round(speedGain);
        player.Dexterity += (int)Math.Round(dexterityGain);

        // ✅ UPDATE PROGRESS: 1 point per energy spent
        if (SelectedLesson < LessonProgress.Count)
        {
            LessonProgress[SelectedLesson] += totalEnergy;
            if (LessonProgress[SelectedLesson] > 100)
                LessonProgress[SelectedLesson] = 100;

            // ✅ CRITICAL: UNLOCK ALL LESSONS when Basic Gym (lesson 0) completes
            if (SelectedLesson == 0 && LessonProgress[0] >= 100)
            {
                // Unlock ALL remaining lessons (1, 2, 3, 4)
                for (int i = 1; i < LessonUnlocked.Count; i++)
                {
                    LessonUnlocked[i] = true;
                }
            }
        }

        // Build success message
        string message = $"✅ تدربت بنجاح في {GetLessonInfo(SelectedLesson).Name}!\n" +
                         $"استهلكت {totalEnergy} طاقة من أصل {player.Energy + totalEnergy}\n" +
                         $"الزيادة في الإحصائيات:\n" +
                         $"💪 القوة: +{Math.Round(strengthGain)}\n" +
                         $"🛡️ الدفاع: +{Math.Round(defenseGain)}\n" +
                         $"🏃 السرعة: +{Math.Round(speedGain)}\n" +
                         $"🤸 المهارة: +{Math.Round(dexterityGain)}";

        return (true, message, totalEnergy);
    }

    // ✅ STAT MULTIPLIERS (Running Gym REMOVED - indices adjusted)
    public double GetStrengthMultiplier()
    {
        return SelectedLesson switch
        {
            1 => 2.0, // Muscle Gym: 2x Strength
            _ => 1.0  // All others: 1x Strength
        };
    }

    public double GetDefenseMultiplier()
    {
        return SelectedLesson switch
        {
            2 => 1.5, // Heart Gym: 1.5x Defense (authentic from original game)
            _ => 1.0  // All others: 1x Defense
        };
    }

    public double GetSpeedMultiplier()
    {
        return SelectedLesson switch
        {
            3 => 2.0, // Speed Gym: 2x Speed
            _ => 1.0  // All others: 1x Speed
        };
    }

    public double GetDexterityMultiplier()
    {
        return SelectedLesson switch
        {
            4 => 2.0, // Agility Gym: 2x Dexterity
            _ => 1.0  // All others: 1x Dexterity
        };
    }

    // ✅ RESET GYM (for testing/new game)
    public void ResetGym()
    {
        SelectedLesson = 0;
        LessonProgress = new List<int> { 0, 0, 0, 0, 0 };
        LessonUnlocked = new List<bool> { true, false, false, false, false };
    }

    public class LessonInfo
    {
        public string Name { get; }
        public string Icon { get; }
        public string Description { get; }
        public int UnlockRequirement { get; }

        public LessonInfo(string name, string icon, string description, int unlockRequirement)
        {
            Name = name;
            Icon = icon;
            Description = description;
            UnlockRequirement = unlockRequirement;
        }
    }
}