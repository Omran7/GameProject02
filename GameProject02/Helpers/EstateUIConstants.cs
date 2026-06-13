using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace GameProject02.Helpers;

/// <summary>
/// ثوابت تصميم موحدة خاصة بنظام العقارات، تعتمد على أبعاد الشاشة الفعلية (نسب مئوية)
/// مناسبة لجميع أجهزة الموبايل (Android و iOS)
/// </summary>
public static class EstateUIConstants
{
    // الحصول على أبعاد الشاشة الحالية (بالوحدات المستقلة للكثافة - DIP)
    private static DisplayInfo DisplayInfo => DeviceDisplay.Current.MainDisplayInfo;

    /// <summary> عرض الشاشة بـ DIP </summary>
    public static double ScreenWidth => DisplayInfo.Width / DisplayInfo.Density;

    /// <summary> ارتفاع الشاشة بـ DIP </summary>
    public static double ScreenHeight => DisplayInfo.Height / DisplayInfo.Density;

    // ==========================================
    // 🃏 البطاقات (نسب مئوية من ارتفاع الشاشة)
    // ==========================================
    /// <summary> ارتفاع البطاقة الأساسية: 20% من ارتفاع الشاشة </summary>
    public static double CardHeight => ScreenHeight * 0.20;

    /// <summary> الحد الأدنى لارتفاع البطاقة: 12% من الارتفاع </summary>
    public static double CardMinHeight => ScreenHeight * 0.12;

    /// <summary> زاوية البطاقة (ثابتة) </summary>
    public const double CardCornerRadius = 0;

    /// <summary> المسافة الرأسية بين البطاقات: 0.5% من العرض </summary>
    public static double CardMarginVertical => ScreenWidth * 0.005;

    /// <summary> الحشوة الداخلية للبطاقة: 3.5% من العرض </summary>
    public static double CardContentPadding => ScreenWidth * 0.040;

    // ==========================================
    // 🖼️ الصور (نسبة من ارتفاع البطاقة)
    // ==========================================
    /// <summary> حجم الصورة: 45% من ارتفاع البطاقة </summary>
    public static double ImageSize => CardHeight * 0.45;

    /// <summary> زاوية الصورة (ثابتة) </summary>
    public const double ImageCornerRadius = 8;

    // ==========================================
    // 🔘 الأزرار (نسب مئوية من العرض والارتفاع)
    // ==========================================
    /// <summary> عرض الزر: 18% من عرض الشاشة </summary>
    public static double ButtonWidth => ScreenWidth * 0.20;

    /// <summary> ارتفاع الزر: 5% من ارتفاع الشاشة </summary>
    public static double ButtonHeight => ScreenHeight * 0.045;

    /// <summary> زوايا زر البطاقة المستطيلة (ثابتة) </summary>
    public const double ButtonCornerRadius = 0;

    /// <summary> زوايا زر دائري (ثابتة) </summary>
    public const double ButtonCornerRadiusRound = 8;

    // ==========================================
    // 📐 المسافات والهوامش (نسب مئوية)
    // ==========================================
    public static double ColumnSpacing => ScreenWidth * 0.01;
    public static double RowSpacing => ScreenHeight * 0.000;
    public static double StackSpacing => ScreenHeight * 0.000;
    public static double ButtonSpacing => ScreenWidth * 0.01;

    // ==========================================
    // 🔤 أحجام الخطوط (نسب مئوية من العرض)
    // ==========================================
    // ========== أحجام الخطوط الديناميكية (نسب مئوية من عرض الشاشة) ==========

    //  - للشارات، النصوص المساعدة، الأخطاء
    public static double FontSizeTiny => ScreenWidth * 0.025;

    //  - للإحصائيات، النصوص الوصفية، التفاصيل
    public static double FontSizeSmall => ScreenWidth * 0.030;

    //  - لعناوين البطاقات، أزرار الإجراء، العناوين الفرعية
    public static double FontSizeMedium => ScreenWidth * 0.04;

    //  - للعناوين الرئيسية، النصوص البارزة
    public static double FontSizeLarge => ScreenWidth * 0.05;

    //  - للرسائل الخاصة والتأكيدات الهامة
    public static double FontSizeXLarge => ScreenWidth * 0.06;

    public static double FontSizeButton => ScreenWidth * 0.032;   // 3.5% للأزرار

    // ==========================================
    // 🎨 الألوان الموحدة (ثابتة)
    // ==========================================
    public static readonly Color TextDark = Color.FromArgb("#000000");
    public static readonly Color TextDarkSoft = Color.FromArgb("#000000");
    public static readonly Color TextGold = Colors.Goldenrod;
    public static readonly Color TextRed = Color.FromArgb("#7D1111");
    public static readonly Color TextWhite = Colors.White;
    public static readonly Color BackgroundTransparent = Colors.Transparent;

    // ==========================================
    // ⚡ الأنيميشن (ثابتة)
    // ==========================================
    public const int AnimationPressDuration = 100;
    public const double AnimationPressScale = 0.9;
}