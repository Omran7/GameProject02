using GameProject02.Helpers;
using GameProject02.Services;
using GameProject02.Views;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views
{
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

            LoginButton.IsEnabled = false;
            LoadingIndicator.IsRunning = true;

            try
            {
                bool success = await AccountService.LoginAsync(username, password);

                if (success)
                {
                    var player = AccountService.GetCurrentPlayer();
                    player.CrimeObject.CheckConfinementStatus();
                    MedalService.CheckAndAwardAll(player);

                    // Set the main page based on confinement
                    if (player.CrimeObject.IsInPlane &&
                        player.CrimeObject.FlightReleaseTime > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    {
                        var planePage = new PlanePage(player, player.City, player.CrimeObject.FlightReleaseTime);
                        Application.Current.MainPage = new NavigationPage(new MainPage());
                        await Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(planePage)
                        {
                            BarBackgroundColor = Color.FromArgb("#2c3e50"),
                            BarTextColor = Colors.White
                        });
                    }
                    else if (player.CrimeObject.IsInPrison)
                    {
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

                    // ✅ Show ban alert AFTER page change
                    await Task.Delay(300);
                    await BanHelper.ShowBansOnLogin(player);
                }
                else
                {
                    await DisplayAlert("خطأ", "اسم المستخدم أو كلمة المرور غير صحيحة", "موافق");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("خطأ", "تعذر الاتصال بالخادم، حاول مرة أخرى لاحقاً", "موافق");
                System.Diagnostics.Debug.WriteLine($"[Login] Error: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e) =>
            await Navigation.PushAsync(new RegisterPage());

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.IsVisible = true;
            ErrorMessage.Opacity = 0;
            ErrorMessage.FadeTo(1, 700, Easing.CubicInOut);
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
}