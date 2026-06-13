using GameProject02.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace GameProject02
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Cairo-Black.ttf", "CairoBlack");
                    fonts.AddFont("Cairo-Regular.ttf", "CairoRegular");
                    fonts.AddFont("Amiri-Bold.ttf", "AmiriBold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
                    // ✅ تسجيل الـ Handler فقط على Android (باستخدام التوجيه الشرطي)
#if ANDROID
                    handlers.AddHandler<CustomSlider, GameProject02.Platforms.Android.CustomSliderHandler>();
#endif

                    EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                    {
#if ANDROID
                        handler.PlatformView.Background = null;
                        handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif IOS
                        handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                        handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
                    });
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}