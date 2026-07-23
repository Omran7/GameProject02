using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class EstateRentConfirmPage : ContentPage
{
    private PlayerAccount _player;
    private RentalListing _listing;

    public EstateRentConfirmPage(RentalListing listing)
    {
        InitializeComponent();
        _listing = listing;
        _player = AccountService.GetCurrentPlayer();
        ApplyDynamicSizes();
        ApplyFontSizes();
        LoadRentalData();
    }

    private void ApplyFontSizes()
    {
        SelectDurationTitle.FontSize = EstateUIConstants.FontSizeMedium;
        OwnerLabelText.FontSize = EstateUIConstants.FontSizeSmall;
        PriceLabelText.FontSize = EstateUIConstants.FontSizeSmall;
        DurationLabelText.FontSize = EstateUIConstants.FontSizeSmall;
        HappinessLabelText.FontSize = EstateUIConstants.FontSizeSmall;
        OwnerLabel.FontSize = EstateUIConstants.FontSizeSmall;
        PricePerDayLabel.FontSize = EstateUIConstants.FontSizeSmall;
        MaxDurationLabel.FontSize = EstateUIConstants.FontSizeSmall;
        HappinessLabel.FontSize = EstateUIConstants.FontSizeSmall;
        TotalPriceLabel.FontSize = EstateUIConstants.FontSizeSmall;
        SelectedDurationLabel.FontSize = EstateUIConstants.FontSizeSmall;
        RentButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
    }

    private void ApplyDynamicSizes()
    {
        var backBtn = this.FindByName<Border>("BackBtnBorder");
        if (backBtn != null)
        {
            backBtn.WidthRequest = EstateUIConstants.ButtonWidth;
            backBtn.HeightRequest = EstateUIConstants.ButtonHeight;
        }
        var imageFrame = this.FindByName<Border>("EstateImageFrame");
        if (imageFrame != null)
        {
            imageFrame.WidthRequest = EstateUIConstants.ImageSize;
            imageFrame.HeightRequest = EstateUIConstants.ImageSize;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyDynamicSizes();
        ApplyFontSizes();
        SetupFooter();
    }

    private void SetupFooter()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(25, 0)
        };

        var backButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnBackClicked,
            buttonImageSource: "footer_button_back.png"
        );

        grid.Add(backButton, 1, 0);
        PageFooter.SetContent(grid);
    }

    private void LoadRentalData()
    {
        if (_player == null || _listing == null) return;
        double dailyRate = _listing.TotalPriceFor30Days / 30.0;
        PricePerDayLabel.Text = $"{dailyRate:F2} يوم";
        MaxDurationLabel.Text = "30 يوم";
        var owner = AccountService.GetAllPlayers().FirstOrDefault(p => p.PlayerId == _listing.OwnerId);
        OwnerLabel.Text = owner?.Username ?? "لاعب مجهول";
        var estateType = EstateObject.EstateTypes[_listing.EstateId];
        HappinessLabel.Text = $"{NumberFormatter.FormatNumber(estateType.Happiness)}";
        EstateImage.Source = estateType.ImageResource;
        DaysEntry.Text = "";
        OnDaysChanged(null, null);
    }

    private void OnDaysChanged(object sender, TextChangedEventArgs e)
    {
        if (_listing == null || DaysEntry == null) return;
        if (int.TryParse(DaysEntry.Text, out int days))
        {
            if (days < 1) days = 1;
            if (days > 30) days = 30;
            SelectedDurationLabel.Text = $"{days} يوم";
            var totalPrice = _listing.CalculatePriceForDays(days);
            TotalPriceLabel.Text = $"{NumberFormatter.FormatNumber(totalPrice)} ذهب";
        }
        else
        {
            SelectedDurationLabel.Text = "1 يوم";
            TotalPriceLabel.Text = $"{NumberFormatter.FormatNumber(_listing.CalculatePriceForDays(1))} ذهب";
        }
    }

    private async void OnRentClicked(object sender, EventArgs e)
    {
        if (sender is Border rentBorder) await AnimateBorder(rentBorder);
        if (!int.TryParse(DaysEntry.Text, out int days) || days < 1 || days > 30)
        { await ToastService.Show("يرجى إدخال مدة صحيحة (1-30 يوم)!", ToastType.Error); return; }
        var totalPrice = _listing.CalculatePriceForDays(days);
        if (_player.Gold < totalPrice) { await ToastService.Show("ليس لديك ذهب كافي!", ToastType.Error); return; }
        var (success, message) = RentalService.RentEstate(_player, _listing.ListingId, days);
        if (success) { await Navigation.PopAsync(false); ToastService.Show("تم استئجار العقار بنجاح!", ToastType.Success); }
        else { await ToastService.Show($"❌ {message}", ToastType.Error); }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border backBorder) await AnimateBorder(backBorder);
        await Navigation.PopAsync(false);
    }

    private async Task AnimateBorder(Border border)
    {
        if (border == null) return;
        try { await border.ScaleTo(EstateUIConstants.AnimationPressScale, EstateUIConstants.AnimationPressDuration, Easing.CubicIn); await border.ScaleTo(1, EstateUIConstants.AnimationPressDuration, Easing.CubicOut); } catch { }
    }
}