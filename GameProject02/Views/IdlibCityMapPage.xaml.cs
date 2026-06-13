using GameProject02.Services;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class IdlibCityMapPage : ContentPage
{
    public IdlibCityMapPage()
    {
        InitializeComponent();

        // Hook into the building tapped event we added to MapView
        IdlibCityMapView.BuildingTapped += OnBuildingTapped;
    }

    private async void OnBuildingTapped(object sender, string buildingId)
    {
        Debug.WriteLine($"[CityMap] Building tapped: {buildingId}");

        try
        {
            switch (buildingId)
            {
                case "Gym":
                    await Navigation.PushAsync(new GymPage());
                    break;
                case "School":
                    await Navigation.PushAsync(new SchoolPage());
                    break;
                case "Hospital":
                    await Navigation.PushAsync(new HospitalPage());
                    break;
                case "Prison":
                    await Navigation.PushAsync(new PrisonPage());
                    break;
                case "FightClub":
                    await Navigation.PushAsync(new FightClubPage());
                    break;
                case "WorkOffice":
                    await Navigation.PushAsync(new WorkOfficePage());
                    break;
                case "CityMarket":
                    await Navigation.PushAsync(new MarketCategoriesPage());
                    break;
                case "GangMarket":
                    await Navigation.PushAsync(new GangMarketCategoriesPage());
                    break;
                case "BlackMarket":
                    await Navigation.PushAsync(new BlackMarketMenuPage());
                    break;
                case "Estate":
                    await Navigation.PushAsync(new EstatePage());
                    break;
                case "GangBase":
                    await HandleGangNavigation();
                    break;
                case "Bank":
                    await DisplayAlert("Bank", "Banking system coming soon.", "OK");
                    break;
                case "Airport":
                    await DisplayAlert("Airport", "Banking system coming soon.", "OK");
                    break;
                case "CityDatabase":
                    await DisplayAlert("Settings", "Banking system coming soon.", "OK");
                    break;
                case "Casino":
                    await DisplayAlert("Casino", $"{buildingId} system coming soon.", "OK");
                    break;
                case "LuckyWheel":
                    await DisplayAlert("Location", $"{buildingId} system coming soon.", "OK");
                    break;
                case "Hanger":
                    await DisplayAlert("Hanger", $"{buildingId} system coming soon.", "OK");
                    break;
                case "MercenaryBase":
                    await DisplayAlert("MercenaryBase", $"{buildingId} system coming soon.", "OK");
                    break;
                case "Skyscraper":
                    await DisplayAlert("Skyscraper", $"{buildingId} system coming soon.", "OK");
                    break;
                case "UpgradeLab":
                    await DisplayAlert("UpgradeLab", $"{buildingId} system coming soon.", "OK");
                    break;
                case "Cinema":
                    await DisplayAlert("Cinema", "Watch movies to gain stats - Coming soon!", "OK");
                    break;
                default:
                    // For items like "SkyScraper" or specific banners that don't have pages yet
                    // await DisplayAlert("Discovery", $"You found: {buildingId}", "OK");
                    break;
            }
        }
        catch (Exception ex)
        {
            // If the user hasn't created one of these pages yet, it might throw an error or fail to find the class
            Debug.WriteLine($"[CityMap Error] Navigation failed for {buildingId}: {ex.Message}");
            // Optional: fallback if class not found
        }
    }

    private async Task HandleGangNavigation()
    {
        // Replicating the logic from user's MainPage.xaml.cs
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        bool isInGang = player.GangObject != null && player.GangObject.IsMember(player.PlayerId);

        if (!isInGang)
        {
            bool createNew = await DisplayAlert(
                "العصابات",
                "أنت لست في عصابة. هل تريد إنشاء عصابة جديدة أو البحث عن واحدة؟",
                "إنشاء",
                "بحث");

            if (createNew)
                await Navigation.PushAsync(new Views.GangCreatePage());
            else
                await Navigation.PushAsync(new Views.GangSearchPage());
        }
        else
        {
            await Navigation.PushAsync(new Views.GangProfilePage());
        }
    }
}
