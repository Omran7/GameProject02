using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace GameProject02;

[Activity(Theme = "@style/Maui.SplashTheme",
          MainLauncher = true,
          LaunchMode = LaunchMode.SingleTop,
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // تفعيل وضع ملء الشاشة الكامل
        EnableFullScreenMode();
    }

    private void EnableFullScreenMode()
    {
        // السماح للتطبيق بالرسم خلف أشرطة النظام
        WindowCompat.SetDecorFitsSystemWindows(Window, false);

        // إنشاء كائن للتحكم بأشرطة النظام
        var controller = new WindowInsetsControllerCompat(Window, Window.DecorView);

        // إخفاء شريط الحالة العلوي وشريط التنقل السفلي
        controller.Hide(WindowInsetsCompat.Type.SystemBars());

        // جعل الأشرطة تظهر مؤقتاً عند السحب من الحافة ثم تختفي
        controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
    }
}