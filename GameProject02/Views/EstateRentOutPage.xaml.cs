using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class EstateRentOutPage : ContentPage
{
    private PlayerAccount _player;
    private EstateObject _selectedEstate;
    private int _maxTotalPriceFor30Days;

    // ─────────────────────────────────────────────
    //  أبعاد ديناميكية
    // ─────────────────────────────────────────────
    private static double ScreenWidth => DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
    private static double ScreenHeight => DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;

    private double SelectionCardWidth => ScreenWidth * 0.92;
    private double EstateImageSize => SelectionCardWidth * 0.25;
    private double SelectionButtonH => SelectionCardWidth * 0.11;
    private double InputFieldWidth => SelectionCardWidth * 0.70;
    private double InputFieldHeight => SelectionCardWidth * 0.11;
    private double ActionButtonWidth => SelectionCardWidth * 0.42;
    private double ActionButtonHeight => SelectionCardWidth * 0.12;

    // ارتفاع صور الخلفية ديناميكياً
    private double SelectionCardHeight => ScreenHeight * 0.20;
    private double PriceCardHeight => ScreenHeight * 0.43;

    private double FontSmall => ScreenHeight * 0.016;
    private double FontMedium => ScreenHeight * 0.020;
    private double FontButton => ScreenHeight * 0.018;

    public EstateRentOutPage()
    {
        InitializeComponent();
        _player = AccountService.GetCurrentPlayer();
        if (_player != null) LoadEstates();
        else ToastService.Show("خطأ في تحميل بيانات اللاعب", ToastType.Error);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        if (_player != null) LoadEstates();
        ApplyDynamicSizes();
        SetupFooter();
    }

    // ─────────────────────────────────────────────
    //  تطبيق الأبعاد الديناميكية
    // ─────────────────────────────────────────────
    private void ApplyDynamicSizes()
    {
        // ارتفاع صور الخلفية ديناميكياً
        if (SelectionCardBgImage != null)
            SelectionCardBgImage.HeightRequest = SelectionCardHeight;

        if (PriceCardBgImage != null)
            PriceCardBgImage.HeightRequest = PriceCardHeight;

        // صورة العقار
        EstateImageFrame.WidthRequest = EstateImageSize;
        EstateImageFrame.HeightRequest = EstateImageSize;

        // زر اختيار العقار
        if (EstateListButton != null)
            EstateListButton.HeightRequest = SelectionButtonH;

        // حقول السعر
        foreach (var field in new View[] { MonthlyRateBorder, DailyRateBorder })
        {
            if (field != null)
            {
                field.WidthRequest = InputFieldWidth;
                field.HeightRequest = InputFieldHeight;
            }
        }

        if (PriceEntryBorder != null)
        {
            PriceEntryBorder.WidthRequest = InputFieldWidth;
            PriceEntryBorder.HeightRequest = InputFieldHeight;
        }

        // زر التأجير
        if (RentButtonBorder != null)
        {
            RentButtonBorder.WidthRequest = ActionButtonWidth;
            RentButtonBorder.HeightRequest = ActionButtonHeight;
        }

        // الخطوط
        if (EstateNameLabel != null) EstateNameLabel.FontSize = FontMedium;
        if (HappinessLabel != null) HappinessLabel.FontSize = FontSmall;
        if (SelectedEstateLabel != null) SelectedEstateLabel.FontSize = FontSmall;
        if (InfoLabel1 != null) InfoLabel1.FontSize = FontSmall;
        if (InfoLabel2 != null) InfoLabel2.FontSize = FontSmall;
        if (MaxPriceLabel != null) MaxPriceLabel.FontSize = FontSmall;
        if (MonthlyRateLabel != null) MonthlyRateLabel.FontSize = FontSmall;
        if (DailyRateLabel != null) DailyRateLabel.FontSize = FontSmall;
        if (RentButtonLabel != null) RentButtonLabel.FontSize = FontButton;
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

    private void LoadEstates()
    {
        if (_player == null)
        {
            ToastService.Show("بيانات اللاعب غير متوفرة", ToastType.Error);
            return;
        }
    }

    private async Task AnimateBorder(Border border)
    {
        if (border == null) return;
        try
        {
            await border.ScaleTo(0.95, 80, Easing.CubicIn);
            await border.ScaleTo(1, 80, Easing.CubicOut);
        }
        catch { }
    }

    private async void OnToggleListClicked(object sender, EventArgs e)
    {
        if (sender is Border listButton) await AnimateBorder(listButton);
        if (_player == null) return;

        var listedForRent = RentalService.GetListedEstateInstanceIds(_player.PlayerId);
        var listedForSale = UsedEstateService.GetListedEstateInstanceIds(_player.PlayerId);
        var rentedOut = RentalService.GetRentedEstateInstanceIds(_player.PlayerId);

        var availableEstates = _player.Estates
            .Where(est => est.Id != 0 && est.Id != 15 &&
                          est.InstanceId != _player.PrimaryResidenceEstateInstanceId &&
                          !listedForRent.Contains(est.InstanceId) &&
                          !listedForSale.Contains(est.InstanceId) &&
                          !rentedOut.Contains(est.InstanceId) &&
                          !est.IsRentedEstate)
            .ToList();

        if (availableEstates.Count == 0)
        {
            ToastService.Show("ليس لديك عقارات متاحة للعرض للإيجار!", ToastType.Error);
            return;
        }

        var sortedEstates = availableEstates
            .OrderByDescending(e => e.GetHappiness(_player))
            .ThenBy(e => e.GetEstateTypeName())
            .ToList();

        var grouped = new Dictionary<int, List<EstateObject>>();
        foreach (var estate in sortedEstates)
        {
            if (!grouped.ContainsKey(estate.Id))
                grouped[estate.Id] = new List<EstateObject>();
            grouped[estate.Id].Add(estate);
        }

        var displayList = new List<dynamic>();
        foreach (var group in grouped)
        {
            int instanceNumber = 1;
            foreach (var estate in group.Value)
            {
                string displayName = estate.GetEstateTypeName();
                if (group.Value.Count > 1) displayName += $" #{instanceNumber}";
                displayList.Add(new
                {
                    Estate = estate,
                    DisplayName = displayName,
                    Happiness = estate.GetHappiness(_player)
                });
                instanceNumber++;
            }
        }

        PageFooter.InputTransparent = true;

        var selectedItem = await PopupService.ShowSelectionPopupWithCustomView(
            title: "أختر عقاراً",
            items: displayList,
            createItemView: CreateEstateItemView
        );

        PageFooter.InputTransparent = false;

        if (selectedItem != null)
        {
            _selectedEstate = selectedItem.Estate;
            var estateType = EstateObject.EstateTypes[_selectedEstate.Id];
            EstateNameLabel.Text = estateType.Name;
            HappinessLabel.Text = $"السعادة: {NumberFormatter.FormatNumber(_selectedEstate.GetHappiness(_player))}";
            EstateImage.Source = estateType.ImageResource;
            SelectedEstateLabel.Text = estateType.Name;
            _maxTotalPriceFor30Days = (int)(estateType.Cost / 4);
            MaxPriceLabel.Text = $"الحد الأقصى: {NumberFormatter.FormatNumber(_maxTotalPriceFor30Days)} ذهب";
            OnPriceChanged(null, null);
        }
    }

    private View CreateEstateItemView(dynamic item)
    {
        var border = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            HeightRequest = ScreenHeight * 0.075,
            BackgroundColor = Colors.Transparent,
            Padding = 0
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10,
            Padding = 0
        };

        var bg = new Image { Source = "card_background.png", Aspect = Aspect.Fill };
        grid.Add(bg, 0, 0);
        Grid.SetColumnSpan(bg, 3);

        var style = (Style)Application.Current.Resources["PopupListItem"];
        grid.Add(new Label { Text = item.DisplayName, Style = style, Margin = new Thickness(15, 0, 0, 0) }, 0, 0);
        grid.Add(new Label { Text = NumberFormatter.FormatNumber(item.Happiness), Style = style }, 1, 0);
        grid.Add(new Label { Text = "سعادة", Style = style, Margin = new Thickness(0, 0, 15, 0) }, 2, 0);

        border.Content = grid;

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await border.ScaleTo(0.95, 80, Easing.CubicIn);
            await border.ScaleTo(1, 80, Easing.CubicOut);
        };
        border.GestureRecognizers.Add(tap);
        return border;
    }

    private void OnPriceChanged(object sender, TextChangedEventArgs e)
    {
        if (_selectedEstate == null || PriceEntry == null) return;

        string cleanText = new string(PriceEntry.Text?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());
        if (cleanText.Length > 9) cleanText = cleanText.Substring(0, 9);

        if (int.TryParse(cleanText, out int price))
        {
            if (price > _maxTotalPriceFor30Days && _maxTotalPriceFor30Days > 0)
            {
                price = _maxTotalPriceFor30Days;
                PriceEntry.Text = price.ToString();
            }
            if (MonthlyRateLabel != null) MonthlyRateLabel.Text = $"{NumberFormatter.FormatNumber(price)} ذهب";
            if (DailyRateLabel != null) DailyRateLabel.Text = $"{NumberFormatter.FormatNumber((int)(price / 30.0))} ذهب";
        }
        else
        {
            if (MonthlyRateLabel != null) MonthlyRateLabel.Text = "0 ذهب";
            if (DailyRateLabel != null) DailyRateLabel.Text = "0 ذهب";
        }
    }

    private async void OnRentClicked(object sender, EventArgs e)
    {
        if (sender is Border rentBorder) await AnimateBorder(rentBorder);
        if (_selectedEstate == null)
        {
            await ToastService.Show("يرجى اختيار عقار أولاً", ToastType.Error);
            return;
        }
        if (!int.TryParse(PriceEntry.Text, out int totalPrice) || totalPrice < 1 || totalPrice > _maxTotalPriceFor30Days)
        {
            await ToastService.Show($"السعر يجب أن يكون بين 1-{NumberFormatter.FormatNumber(_maxTotalPriceFor30Days)} ذهب", ToastType.Error);
            return;
        }
        var (success, message) = RentalService.CreateListing(_player, _selectedEstate, totalPrice);
        if (success) { await Navigation.PopAsync(); ToastService.Show("تم عرض العقار للإيجار بنجاح!", ToastType.Success); }
        else { await ToastService.Show($"❌ {message}", ToastType.Error); }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border b) await AnimateBorder(b);
        await Navigation.PopAsync();
    }
}