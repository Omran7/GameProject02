using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;

namespace GameProject02.Views
{
    public partial class AirportPage : ContentPage
    {
        public ObservableCollection<AirportDestination> Destinations { get; } = new();

        public AirportPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadDestinations();
        }

        private void LoadDestinations()
        {
            var player = AccountService.GetCurrentPlayer();
            TitleLabel.Text = $"✈️ مطار {player?.City ?? "المدينة"}";

            var destinations = TravelService.GetDestinationsForCurrentCity();
            Destinations.Clear();
            foreach (var d in destinations)
                Destinations.Add(d);

            DestinationsList.ItemsSource = Destinations;
        }

        private async void OnDestinationTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.BindingContext is AirportDestination dest)
            {
                var player = AccountService.GetCurrentPlayer();
                if (player == null) return;

                bool confirm = await DisplayAlert("تأكيد السفر",
                    $"السفر إلى {dest.CityName}\n" +
                    $"التكلفة: {dest.TravelCostGold} ذهب\n" +
                    $"المدة: {dest.TravelTimeSeconds} ثانية",
                    "سافر", "إلغاء");

                if (!confirm) return;

                var (success, message) = TravelService.StartTravel(player, dest);
                if (success)
                {
                    long releaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                       + dest.TravelTimeSeconds * 1000;
                    var planePage = new PlanePage(player, dest.CityName, releaseTime);
                    await Navigation.PushModalAsync(new NavigationPage(planePage)
                    {
                        BarBackgroundColor = Color.FromArgb("#2c3e50"),
                        BarTextColor = Colors.White
                    });
                }
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
            => await Navigation.PopAsync(false);

        private async void OnMapClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new MapPage(), false);
    }
}