using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GameProject02.Views;
public partial class TheShopPage : ContentPage
{
    public ObservableCollection<ShopItem> ShopItems { get; } = new();
    public int PlayerDiamonds { get; set; }
    public int PlayerMerits { get; set; }
    public ICommand BuyCommand { get; }
    public ICommand OpenWheelCommand { get; }

    public TheShopPage()
    {
        InitializeComponent();
        BindingContext = this;
        BuyCommand = new Command<ShopItem>(OnBuyItem);
        OpenWheelCommand = new Command(OnOpenWheel);
        LoadShop();
    }

    private void LoadShop()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;
        PlayerDiamonds = player.Diamonds;
        PlayerMerits = player.Merits;
        ShopItems.Clear();
        foreach (var item in ShopService.GetShopItems()) ShopItems.Add(item);
    }

    private async void OnBuyItem(ShopItem item)
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;
        var (success, msg) = ShopService.BuyItem(player, item);
        if (success)
        {
            PlayerDiamonds = player.Diamonds;
            PlayerMerits = player.Merits;
            OnPropertyChanged(nameof(PlayerDiamonds));
            OnPropertyChanged(nameof(PlayerMerits));
            await DisplayAlert("✅ تم الشراء", msg, "موافق");
        }
        else await DisplayAlert("❌ فشل الشراء", msg, "موافق");
    }

    private async void OnOpenWheel() => await Navigation.PushAsync(new LuckyWheelPage(), false);
}