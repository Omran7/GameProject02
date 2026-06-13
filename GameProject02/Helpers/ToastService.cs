using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameProject02.Helpers;

public enum ToastType { Success, Error, Info }

public static class ToastService
{
    private static View? currentToast;
    private static CancellationTokenSource? currentCts;

    public static async Task Show(string message, ToastType type = ToastType.Success)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // إلغاء الإشعار السابق
            currentCts?.Cancel();
            currentCts?.Dispose();
            currentCts = new CancellationTokenSource();

            if (currentToast != null)
            {
                var parent = currentToast.Parent as Grid;
                parent?.Children.Remove(currentToast);
                currentToast = null;
            }

            var page = GetActivePage();
            if (page == null) return;

            // التأكد من أن جذر الصفحة Grid
            if (page.Content is not Grid rootGrid)
            {
                var oldContent = page.Content;
                rootGrid = new Grid { BackgroundColor = Colors.Transparent };
                rootGrid.Add(oldContent);
                page.Content = rootGrid;
            }

            // إنشاء الإشعار
            var toast = CreateToastUI(message, type);

            toast.Opacity = 0;
            toast.Scale = 0.8;

            // ✅ wrapper بـ LTR لضمان التمركز الصحيح بغض النظر عن FlowDirection الصفحة
            var wrapper = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackgroundColor = Colors.Transparent,
                InputTransparent = true
            };
            wrapper.Add(toast);

            currentToast = wrapper;
            rootGrid.Add(wrapper);

            // تغطية كل مساحة الصفحة
            Grid.SetRow(wrapper, 0);
            if (rootGrid.RowDefinitions.Count > 0)
                Grid.SetRowSpan(wrapper, rootGrid.RowDefinitions.Count);

            Grid.SetColumn(wrapper, 0);
            if (rootGrid.ColumnDefinitions.Count > 0)
                Grid.SetColumnSpan(wrapper, rootGrid.ColumnDefinitions.Count);

            try
            {
                // أنيميشن الظهور
                await Task.WhenAll(
                    toast.FadeTo(1, 200, Easing.CubicOut),
                    toast.ScaleTo(1, 200, Easing.CubicOut)
                );

                // وميض النص (3 مرات)
                if (toast is Border border && border.Content is Label textLabel)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (currentCts.Token.IsCancellationRequested) break;
                        await textLabel.FadeTo(0.4, 150, Easing.Linear);
                        await textLabel.FadeTo(1, 150, Easing.Linear);
                    }
                }

                await Task.Delay(800, currentCts.Token);

                // أنيميشن الاختفاء
                await Task.WhenAll(
                    toast.FadeTo(0, 200, Easing.CubicIn),
                    toast.ScaleTo(0.8, 200, Easing.CubicIn)
                );

                if (rootGrid.Children.Contains(wrapper))
                    rootGrid.Children.Remove(wrapper);
                currentToast = null;
            }
            catch
            {
                if (rootGrid.Children.Contains(wrapper))
                    rootGrid.Children.Remove(wrapper);
                currentToast = null;
            }
        });
    }

    private static View CreateToastUI(string message, ToastType type)
    {
        Color bgColor = type switch
        {
            ToastType.Success => Color.FromArgb("#CC27ae60"),
            ToastType.Error => Color.FromArgb("#CCc0392b"),
            _ => Color.FromArgb("#CC2c3e50")
        };

        double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        double maxWidth = Math.Min(screenWidth * 0.85, 380);

        var border = new Border
        {
            BackgroundColor = bgColor,
            Stroke = Colors.Black,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(10, 10),
            MaximumWidthRequest = maxWidth,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            FlowDirection = FlowDirection.RightToLeft,
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Radius = 10,
                Opacity = 0.4f,
                Offset = new Point(0, 5)
            }
        };

        var label = new Label
        {
            Text = message,
            TextColor = Colors.White,
            FontFamily = "Cairo-Black",
            FontSize = 16,
            HorizontalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.WordWrap
        };

        border.Content = label;
        return border;
    }

    private static ContentPage? GetActivePage()
    {
        var page = Application.Current?.MainPage;
        if (page is NavigationPage nav) return nav.CurrentPage as ContentPage;
        return page as ContentPage;
    }
}