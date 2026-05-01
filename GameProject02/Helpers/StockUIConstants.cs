using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace GameProject02.Helpers;

/// <summary>
/// ثوابت تصميم موحدة لقسم المخزن (Stock, Bag, Shop, Museum)
/// جميع القيم الداخلية تتبع ارتفاع البطاقة (CardHeight) لضمان التجاوب الكامل.
/// </summary>
public static class StockUIConstants
{
    private static DisplayInfo DisplayInfo => DeviceDisplay.Current.MainDisplayInfo;

    // ==========================================
    // أبعاد الشاشة (للمرجع فقط)
    // ==========================================
    public static double ScreenWidth => DisplayInfo.Width / DisplayInfo.Density;
    public static double ScreenHeight => DisplayInfo.Height / DisplayInfo.Density;


    // ==========================================
    // شريط التبويبات العلوي (Tab Bar)
    // ==========================================
    public static double TabBarHeight => ScreenHeight * 0.05;          // ارتفاع شريط التبويبات )
    public static double TabButtonWidth => ScreenWidth * 0.22;         // عرض كل تبويب )
    public static double TabButtonHeight => TabBarHeight * 0.9;        // ارتفاع الزر داخل الشريط
    public static double TabCornerRadius => 0;                        // انحناء حواف الشريط (ثابت)
    public static double TabFontSize => ScreenWidth * 0.035;         // حجم خط التبويب (4% من العرض)
    public static double TabIconSize => TabBarHeight * 0.9;            // حجم الأيقونة (60% من ارتفاع الشريط)
    public static Thickness TabMargin => new Thickness(0, 0, 0, 0);    // هوامش بين الأزرار
    public static Thickness TabPadding => new Thickness(0, 0, 0, 0); // هوامش داخلية للزر

    // ==========================================
    // البطاقات (أساس التصميم)
    // ==========================================
    public static double CardHeight => ScreenHeight * 0.20;      
    public static double CardMinHeight => CardHeight;             
    public static double CardMargin => ScreenWidth * 0.000;       
    public static double CardPadding => CardHeight * 0.00;        

    // ==========================================
    // الهوامش الداخلية (كلها تابعة لارتفاع البطاقة)
    // ==========================================
    public static double TitleMarginTop => CardHeight * 0.1;          
    public static double TitleMarginHorizontal => CardHeight * 0.1;   
    public static double ContentPadding => CardHeight * 0.09;          
    public static double ButtonsPadding => CardHeight * 0.09;          

    // ==========================================
    // ارتفاع شريط العنوان داخل البطاقة
    // ==========================================
    public static double TitleBarHeight => CardHeight * 0.14;      

    // ==========================================
    // الصورة داخل البطاقة
    // ==========================================
    public static double CardImageSize => CardHeight * 0.40;       

    // ==========================================
    // الأزرار العامة (داخل البطاقة)
    // ==========================================
    public static double ButtonHeight => CardHeight * 0.18;        
    public static double ButtonWidth => CardHeight * 0.40;         
    public static double ButtonCornerRadius => 0;

    // زر اضافه وازالة متجر مخزن متحف
    public static double ShopButtonWidth => CardHeight * 0.65;
    public static double ShopButtonHeight => CardHeight * 0.18;
                                                             

    // ==========================================
    // أحجام الخطوط (تابعة لارتفاع البطاقة)
    // ==========================================
    public static double FontSizeTiny => CardHeight * 0.060;      
    public static double FontSizeSmall => CardHeight * 0.065;
    public static double FontSizeMedium => CardHeight * 0.075;
    public static double FontSizeLarge => CardHeight * 0.09;
    public static double FontSizexLarge => CardHeight * 0.11;
    public static double FontSizeButton => CardHeight * 0.065;   

    // ==========================================
    // المسافات والهوامش (نسبية لارتفاع البطاقة)
    // ==========================================
    public static double ColumnSpacing => CardHeight * 0.01;
    public static double RowSpacing => CardHeight * 0.01;
    public static double StackSpacing => CardHeight * 0.00;
    public static double ButtonSpacing => CardHeight * 0.01;


    // ==========================================
    // أزرار الفوتر (خارج البطاقة)
    // ==========================================
    public static double FooterButtonWidth => ScreenWidth * 0.18;
    public static double FooterButtonHeight => ScreenHeight * 0.045;

    // ==========================================
    // النوافذ المنبثقة (خارج البطاقة)
    // ==========================================
    public static double PopupWidth => Math.Min(ScreenWidth * 0.85, 375);
    public static double PopupMaxHeight => ScreenHeight * 0.60;
    public static double PopupCornerRadius => 0;
    public static double PopupImageSize => PopupWidth * 0.25;

    // أزرار النوافذ المنبثقة
    public static double PopupButtonWidth => ScreenWidth * 0.25;
    public static double PopupButtonHeight => ScreenHeight * 0.05;

    // ==========================================
    // شريط التمرير (Slider) وحقول الإدخال (خارج البطاقة غالباً)
    // ==========================================
    public static double SliderWidth => ScreenWidth * 0.45;
    public static double SliderHeight => ScreenHeight * 0.018;
    public static double ThumbSize => ScreenHeight * 0.03;
    public static double EntryWidth => ScreenWidth * 0.45;
    public static double EntryHeight => ScreenHeight * 0.03;
    public static double PriceEntryWidth => ScreenWidth * 0.35;

    // ==========================================
    // الألوان (ثابتة)
    // ==========================================
    public static Color TextGold => Color.FromArgb("#f39c12");
    public static Color TextWhite => Colors.WhiteSmoke;
    public static Color TextDark => Color.FromArgb("#1a1a1a");
    public static Color TextRed => Color.FromArgb("#c0392b");
    public static Color TextGreen => Color.FromArgb("#27ae60");
    public static Color BackgroundDark => Color.FromArgb("#1a1a1a");
    public static Color BorderGold => Color.FromArgb("#f39c12");
    public static Color BorderBlue => Color.FromArgb("#3498db");
    public static Color BorderOrange => Color.FromArgb("#e67e22");

    // ==========================================
    // الأنيميشن (ثابت)
    // ==========================================
    public const int AnimationPressDuration = 100;
    public const double AnimationPressScale = 0.90;
}