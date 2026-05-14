using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class GangCreatePage : ContentPage
    {
        public GangCreatePage()
        {
            InitializeComponent();
        }

        private async void OnCreateGangClicked(object sender, EventArgs e)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return;

            string name = GangNameEntry.Text?.Trim();
            string tag = GangTagEntry.Text?.Trim();

            var (isValid, message) = GangService.ValidateGangCreation(player, name, tag);
            if (!isValid)
            {
                InfoLabel.Text = message;
                return;
            }

            CreateButton.IsEnabled = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            try
            {
                // ✅ Wait for the gang to be saved to Firestore
                var gang = await GangService.CreateGangAsync(player, name, tag);

                // Also save the player (gangId updated)
                _ = FirebaseService.SavePlayerAsync(player);

                await DisplayAlert("تم", $"تم إنشاء العصابة {gang.Name} بنجاح!", "موافق");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("خطأ", "فشل إنشاء العصابة. حاول مرة أخرى.", "موافق");
                System.Diagnostics.Debug.WriteLine($"[GANG] Create error: {ex.Message}");
            }
            finally
            {
                CreateButton.IsEnabled = true;
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}