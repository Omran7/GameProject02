using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class PlanePage : ContentPage
    {
        private readonly PlayerAccount _player;
        private readonly long _releaseTimeMs;
        private readonly string _destination;
        private bool _isRunning = true;
        private Random _rand = new();

        public PlanePage(PlayerAccount player, string destination, long releaseTimeMs)
        {
            InitializeComponent();
            _player = player;
            _destination = destination;
            _releaseTimeMs = releaseTimeMs;

            DestinationLabel.Text = destination;
            StartCountdown();
        }

        private void StartCountdown()
        {
            // Update UI immediately, then every second
            UpdateUI();
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                UpdateUI();
                return _isRunning;   // return false to stop the timer
            });
        }

        private void UpdateUI()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long remaining = _releaseTimeMs - now;

            if (remaining <= 0)
            {
                // Flight finished
                _isRunning = false;

                // Clear confinement state
                _player.CrimeObject.IsInPlane = false;
                _player.CrimeObject.FlightReleaseTime = 0;
                _ = FirebaseService.SavePlayerAsync(_player);

                // Notification
                NotificationService.AddGameNotification(
                    "🛬 وصلت!",
                    $"وصلت إلى {_destination}",
                    GameNotificationPriority.High, "🛬", "MainPage"
                );

                // Dismiss the modal plane page and return to main
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopModalAsync();
                });
                return;
            }

            // Display remaining time
            var ts = TimeSpan.FromMilliseconds(remaining);
            TimeLabel.Text = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

            // Random altitude and temperature (like old game)
            if (_rand.Next(10) == 0)
                AltitudeLabel.Text = $"{_rand.Next(8000, 10000)} m";
            if (_rand.Next(15) == 0)
                TemperatureLabel.Text = $"{_rand.Next(-60, -40)} °C";
        }

        // Prevent back button during flight
        protected override bool OnBackButtonPressed() => true;

        // Bottom bar actions
        private async void OnChatClicked(object sender, EventArgs e)
            => await DisplayAlert("قريباً", "المحادثة ستتوفر قريباً", "موافق");
        private async void OnNewsClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new NewsPage(), false);
        private async void OnShopClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new MarketCategoriesPage(), false);
    }
}