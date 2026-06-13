using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameProject02.Views;

public partial class LuckyWheelPage : ContentPage
{
    public int PlayerDiamonds { get; set; }
    public ICommand SpinCommand { get; }
    private bool _isSpinning = false;

    public LuckyWheelPage()
    {
        InitializeComponent();
        BindingContext = this;
        SpinCommand = new Command<int>(OnSpin);
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player != null) PlayerDiamonds = player.Diamonds;
        OnPropertyChanged(nameof(PlayerDiamonds));
    }

    private async void OnSpin(int spins)
    {
        if (_isSpinning) return;
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        int costPerSpin = 10; // Diamonds per spin
        int totalCost = spins * costPerSpin;

        if (player.Diamonds < totalCost)
        {
            await DisplayAlert("خطأ", $"تحتاج {totalCost} ماس لـ {spins} دورة (لديك {player.Diamonds})", "موافق");
            return;
        }

        _isSpinning = true;
        WheelImage.Rotation = 0;
        RewardLabel.IsVisible = true;

        // Animate rotation (multiple full spins)
        double rotations = 5 + spins; // 5 full turns + spins extra
        await WheelImage.RotateTo(360 * rotations, (uint)(1000 * spins), Easing.CubicOut);

        // Deduct diamonds and apply rewards
        player.Diamonds -= totalCost;
        int totalMerits = 0;
        for (int i = 0; i < spins; i++)
        {
            var reward = LuckyWheelService.Spin();
            if (reward.Type == "Merits")
            {
                player.Merits += reward.Value;
                totalMerits += reward.Value;
            }
            else if (reward.Type == "Gold")
            {
                player.Gold += reward.Value;
            }
            else if (reward.Type == "Diamonds")
            {
                player.Diamonds += reward.Value;
            }
            else if (reward.Type == "Medals")
            {
                player.Medals += reward.Value;
            }
        }

        AccountService.SavePlayer(player);
        PlayerDiamonds = player.Diamonds;
        OnPropertyChanged(nameof(PlayerDiamonds));

        RewardLabel.Text = totalMerits > 0
            ? $"🎉 ربحت {totalMerits} استحقاق!"
            : $"🎉 تمت {spins} دورات! راجع مخزونك.";

        await DisplayAlert("نتيجة العجلة", RewardLabel.Text, "موافق");
        _isSpinning = false;
    }
}