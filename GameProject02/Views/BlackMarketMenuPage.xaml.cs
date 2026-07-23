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
        await Navigation.PushAsync(new BlackMarketConversionPage("Diamonds"), false);

    private async void OnEnterChecksClicked(object sender, System.EventArgs e) =>
        await Navigation.PushAsync(new BlackMarketConversionPage("Checks"), false);

    private async void OnEnterToolsClicked(object sender, System.EventArgs e) =>
        await Navigation.PushAsync(new BlackMarketConversionPage("Tools"), false);

    private async void OnEnterFoodClicked(object sender, System.EventArgs e) =>
        await Navigation.PushAsync(new BlackMarketConversionPage("Food"), false);

    private async void OnBackClicked(object sender, System.EventArgs e) => await Navigation.PopAsync(false);
    private async void OnHomeClicked(object sender, System.EventArgs e) => await Navigation.PopToRootAsync(false);
}