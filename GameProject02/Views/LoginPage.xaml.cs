using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("خطأ", "الرجاء إدخال اسم المستخدم وكلمة المرور", "موافق");
            return;
        }

        bool success = AccountService.Login(username, password);
        if (success)
        {
            var player = AccountService.GetCurrentPlayer();
            player.CrimeObject.CheckConfinementStatus();

            // ✅ التحقق من الحجز قبل فتح الصفحة الرئيسية
            if (player.CrimeObject.IsInPrison)
            {
                // الانتقال مباشرة إلى صفحة السجن كجذر (بدون MainPage)
                Application.Current.MainPage = new NavigationPage(new PrisonPage())
                {
                    BarBackgroundColor = Color.FromArgb("#2c3e50"),
                    BarTextColor = Colors.White
                };
            }
            else if (player.CrimeObject.IsInHospital)
            {
                Application.Current.MainPage = new NavigationPage(new HospitalPage())
                {
                    BarBackgroundColor = Color.FromArgb("#2c3e50"),
                    BarTextColor = Colors.White
                };
            }
            else
            {
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
        }
        else
        {
            await DisplayAlert("خطأ", "اسم المستخدم أو كلمة المرور غير صحيحة", "موافق");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Navigate to registration page
        await Navigation.PushAsync(new RegisterPage());
    }

    private void ShowError(string message)
    {
        ErrorMessage.Text = message;
        ErrorMessage.IsVisible = true;
        ErrorMessage.Opacity = 0;
        ErrorMessage.FadeTo(1, 700, Easing.CubicInOut);

        // Auto-hide after 5 seconds
        _ = Task.Delay(5000).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (ErrorMessage.IsVisible)
                {
                    ErrorMessage.FadeTo(0, 700, Easing.CubicInOut)
                        .ContinueWith(_ => ErrorMessage.IsVisible = false);
                }
            });
        });
    }
}