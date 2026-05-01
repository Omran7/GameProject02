using System;
using System.Globalization;

namespace GameProject02.Services;

public static class LanguageManager
{
    public enum Language { Arabic, English }

    private static Language _currentLanguage = Language.Arabic;

    public static Language CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            // Set culture for number formatting, dates, etc.
            CultureInfo culture = value == Language.Arabic ?
                new CultureInfo("ar") : new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }

    // Arabic text
    public static string Level => _currentLanguage == Language.Arabic ? "اللفل" : "Level";
    public static string City => _currentLanguage == Language.Arabic ? "المدينة" : "City";
    public static string VIP => _currentLanguage == Language.Arabic ? "في أي بي" : "VIP";
    public static string NotVIP => _currentLanguage == Language.Arabic ? "لست في أي بي" : "Not VIP";
    public static string IsVIP => _currentLanguage == Language.Arabic ? "في أي بي" : "VIP";
    public static string Gold => _currentLanguage == Language.Arabic ? "الذهب" : "Gold";
    public static string Diamonds => _currentLanguage == Language.Arabic ? "الألماس" : "Diamonds";
    public static string Energy => _currentLanguage == Language.Arabic ? "الطاقة" : "Energy";
    public static string Health => _currentLanguage == Language.Arabic ? "الصحة" : "Health";
    public static string Strength => _currentLanguage == Language.Arabic ? "القوة" : "Strength";
    public static string Defense => _currentLanguage == Language.Arabic ? "الدفاع" : "Defense";
    public static string Speed => _currentLanguage == Language.Arabic ? "السرعة" : "Speed";
    public static string Dexterity => _currentLanguage == Language.Arabic ? "الخفة" : "Dexterity";
    public static string Intelligence => _currentLanguage == Language.Arabic ? "الذكاء" : "Intelligence";
    public static string Medals => _currentLanguage == Language.Arabic ? "الميداليات" : "Medals";
    public static string AchievementPoints => _currentLanguage == Language.Arabic ? "نقاط الإنجاز" : "Achievement Points";
    public static string BattleStats => _currentLanguage == Language.Arabic ? "إحصائيات المعركة" : "Battle Stats";
    public static string Skills => _currentLanguage == Language.Arabic ? "مهارات مكتسبة" : "Acquired Skills";
    public static string GeneralStats => _currentLanguage == Language.Arabic ? "إحصائيات عامة" : "General Stats";
    public static string EstateInfo => _currentLanguage == Language.Arabic ? "معلومات العقار" : "Estate Information";
    public static string TrainGym => _currentLanguage == Language.Arabic ? "💪 التدريب في الصالة الرياضية" : "💪 TRAIN AT GYM";
    public static string StudySchool => _currentLanguage == Language.Arabic ? "🎓 الدراسة في المدرسة" : "🎓 STUDY AT SCHOOL";
    public static string Profile => _currentLanguage == Language.Arabic ? "👤 الملف الشخصي" : "👤 PROFILE";
    public static string Back => _currentLanguage == Language.Arabic ? "رجوع" : "Back";
    public static string Stock => _currentLanguage == Language.Arabic ? "📦 مخزن" : "Stock";

}