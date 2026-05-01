using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class BlackMarketMenuPage : ContentPage
{
    public BlackMarketMenuPage()
    {
        InitializeComponent();
    }

    private async void OnEnterDiamondsClicked(object sender, System.EventArgs e) =>
        await Navigation.PushAsync(new BlackMarketConversionPage("Diamonds"));

    private async void OnEnterChecksClicked(object sender, System.EventArgs e) =>
        await Navigation.PushAsync(new BlackMarketConversionPage("Checks"));

    private async void OnEnterToolsClicked(object sender, System.EventArgs e) =>
        await Navigation.PushAsync(new BlackMarketConversionPage("Tools"));

    private async void OnEnterFoodClicked(object sender, System.EventArgs e) =>
        await Navigation.PushAsync(new BlackMarketConversionPage("Food"));

    private async void OnBackClicked(object sender, System.EventArgs e) => await Navigation.PopAsync();
    private async void OnHomeClicked(object sender, System.EventArgs e) => await Navigation.PopToRootAsync();
}