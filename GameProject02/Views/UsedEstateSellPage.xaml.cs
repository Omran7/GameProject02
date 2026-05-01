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

public partial class UsedEstateSellPage : ContentPage
{
    private PlayerAccount _player;
    private EstateObject _selectedEstate;
    private int _maxSalePrice;
    private bool _isUpdatingEntry = false;

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

    // ارتفاع البطاقة ديناميكياً
    private double SelectionCardHeight => ScreenHeight * 0.20;
    private double PriceCardHeight => ScreenHeight * 0.34;

    private double FontSmall => ScreenHeight * 0.016;
    private double FontMedium => ScreenHeight * 0.020;
    private double FontButton => ScreenHeight * 0.018;

    public UsedEstateSellPage()
    {
        InitializeComponent();
        _player = AccountService.GetCurrentPlayer();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        ApplyDynamicSizes();
        LoadEstateData();
        SetupFooter();
    }

    // ─────────────────────────────────────────────
    //  تطبيق الأبعاد الديناميكية
    // ─────────────────────────────────────────────
    private void ApplyDynamicSizes()
    {
        // ارتفاع صور الخلفية = ارتفاع البطاقة ديناميكياً
        if (SelectionCardBgImage != null)
            SelectionCardBgImage.HeightRequest = SelectionCardHeight;

        if (PriceCardBgImage != null)
            PriceCardBgImage.HeightRequest = PriceCardHeight;

        // صورة العقار
        EstateImageFrame.WidthRequest = EstateImageSize;
        EstateImageFrame.HeightRequest = EstateImageSize;

        // زر اختيار العقار
        if (SelectEstateBtnBorder != null)
            SelectEstateBtnBorder.HeightRequest = SelectionButtonH;

        // حقل عرض السعر
        if (PriceDisplayBorder != null)
        {
            PriceDisplayBorder.WidthRequest = InputFieldWidth;
            PriceDisplayBorder.HeightRequest = InputFieldHeight;
        }

        // حقل إدخال السعر
        if (PriceEntryBorder != null)
        {
            PriceEntryBorder.WidthRequest = InputFieldWidth;
            PriceEntryBorder.HeightRequest = InputFieldHeight;
        }

        // زر البيع
        if (SellBtnBorder != null)
        {
            SellBtnBorder.WidthRequest = ActionButtonWidth;
            SellBtnBorder.HeightRequest = ActionButtonHeight;
        }

        // الخطوط
        if (EstateNameLabel != null) EstateNameLabel.FontSize = FontMedium;
        if (HappinessLabel != null) HappinessLabel.FontSize = FontSmall;
        if (SelectEstateLabel != null) SelectEstateLabel.FontSize = FontSmall;
        if (InfoLabel1 != null) InfoLabel1.FontSize = FontSmall;
        if (InfoLabel2 != null) InfoLabel2.FontSize = FontSmall;
        if (MaxPriceLabel != null) MaxPriceLabel.FontSize = FontSmall;
        if (PriceLabel != null) PriceLabel.FontSize = FontSmall;
        if (SellButtonLabel != null) SellButtonLabel.FontSize = FontButton;
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

    private void LoadEstateData()
    {
        if (_selectedEstate == null) return;

        EstateNameLabel.Text = _selectedEstate.GetEstateTypeName();
        EstateImage.Source = _selectedEstate.GetImageSource();

        var currentHappiness = _selectedEstate.GetHappiness(_player);
        HappinessLabel.Text = $"السعادة: {NumberFormatter.FormatNumber(currentHappiness)}";

        var estateType = EstateObject.EstateTypes[_selectedEstate.Id];
        _maxSalePrice = estateType.Cost * 2;
        if (MaxPriceLabel != null)
            MaxPriceLabel.Text = $"الحد الأقصى: {NumberFormatter.FormatNumber(_maxSalePrice)} ذهب";

        _isUpdatingEntry = true;
        if (PriceEntry != null) PriceEntry.Text = "";
        _isUpdatingEntry = false;

        OnPriceTextChanged(null, null);
    }

    private void OnPriceTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingEntry) return;
        if (_selectedEstate == null || PriceEntry == null) return;

        if (string.IsNullOrEmpty(e?.NewTextValue))
        {
            if (PriceLabel != null) PriceLabel.Text = "السعر: 0 ذهب";
            return;
        }

        if (long.TryParse(e.NewTextValue, out long priceLong))
        {
            if (priceLong < 1)
            {
                _isUpdatingEntry = true; priceLong = 1;
                PriceEntry.Text = priceLong.ToString();
                _isUpdatingEntry = false;
            }
            else if (priceLong > _maxSalePrice)
            {
                _isUpdatingEntry = true; priceLong = _maxSalePrice;
                PriceEntry.Text = priceLong.ToString();
                _isUpdatingEntry = false;
            }
            if (PriceLabel != null)
                PriceLabel.Text = $"السعر: {NumberFormatter.FormatNumber(priceLong)} ذهب";
            if (PriceErrorLabel != null) PriceErrorLabel.IsVisible = false;
        }
        else
        {
            _isUpdatingEntry = true;
            PriceEntry.Text = _maxSalePrice.ToString();
            _isUpdatingEntry = false;
            if (PriceLabel != null)
                PriceLabel.Text = $"السعر: {NumberFormatter.FormatNumber(_maxSalePrice)} ذهب";
        }
    }

    private async void OnSelectEstateClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        if (_player == null) return;

        var listedForSale = UsedEstateService.GetListedEstateInstanceIds(_player.PlayerId);
        var listedForRent = RentalService.GetListedEstateInstanceIds(_player.PlayerId);
        var rentedOut = RentalService.GetRentedEstateInstanceIds(_player.PlayerId);

        var availableEstates = _player.Estates
            .Where(est => est.Id != 0 && est.Id != 15 &&
                          est.InstanceId != _player.PrimaryResidenceEstateInstanceId &&
                          !listedForSale.Contains(est.InstanceId) &&
                          !listedForRent.Contains(est.InstanceId) &&
                          !rentedOut.Contains(est.InstanceId) &&
                          !est.IsRentedEstate)
            .ToList();

        if (availableEstates.Count == 0)
        {
            ToastService.Show("ليس لديك عقارات متاحة للعرض!", ToastType.Error);
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
            LoadEstateData();
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

    private async void OnSellClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        if (_selectedEstate == null)
        {
            await ToastService.Show("الرجاء اختيار عقار أولاً!", ToastType.Error);
            return;
        }
        if (!long.TryParse(PriceEntry.Text, out long salePriceLong))
        {
            await ToastService.Show("الرجاء إدخال السعر!", ToastType.Error);
            return;
        }
        if (salePriceLong < 1 || salePriceLong > _maxSalePrice)
        {
            await ToastService.Show($"السعر يجب أن يكون بين 1-{NumberFormatter.FormatNumber(_maxSalePrice)} ذهب", ToastType.Error);
            return;
        }
        var (success, message) = UsedEstateService.CreateListing(_player, _selectedEstate, (int)salePriceLong);
        if (success) { await Navigation.PopAsync(); ToastService.Show("تم عرض العقار للبيع بنجاح!", ToastType.Success); }
        else { ToastService.Show($"❌ {message}", ToastType.Error); }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border b) await AnimateBorder(b);
        await Navigation.PopAsync();
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
}