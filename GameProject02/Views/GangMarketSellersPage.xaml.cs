using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class GangMarketSellersPage : ContentPage
{
    private readonly int _categoryId;
    private readonly int _subCategoryId;
    private readonly string _itemId;
    private readonly string _itemName;
    private PlayerAccount _player;
    private List<GangMarketItem> _sellers;
    private double _screenWidth, _screenHeight;
    private Border _notificationBorder;
    private Label _notificationLabel;

    public GangMarketSellersPage(int categoryId, int subCategoryId, string itemId, string itemName)
    {
        InitializeComponent();
        _categoryId = categoryId;
        _subCategoryId = subCategoryId;
        _itemId = itemId;
        _itemName = itemName;
        _screenWidth = Application.Current.MainPage.Width;
        _screenHeight = Application.Current.MainPage.Height;

        CategoryTitleLabel.Text = itemName;
        LoadSellers();
    }

    private void LoadSellers()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        var allListings = GangMarketService.GetAllListings();
        _sellers = allListings.Where(i => i.ItemId == _itemId).ToList();

        ItemsGrid.Children.Clear();
        ItemsGrid.RowDefinitions.Clear();

        EmptyState.IsVisible = _sellers.Count == 0;
        ItemsGrid.IsVisible = _sellers.Count > 0;

        if (_sellers.Count == 0) return;

        for (int i = 0; i < _sellers.Count; i++)
            ItemsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < _sellers.Count; i++)
        {
            var seller = _sellers[i];
            var card = CreateSellerCard(seller);
            ItemsGrid.Add(card, 0, i);
        }
    }

    private Border CreateSellerCard(GangMarketItem item)
    {
        double cardHeight = _screenHeight * 0.16;
        double imageSize = cardHeight * 0.65;
        double buttonWidth = _screenWidth * 0.22;

        var mainBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            HeightRequest = cardHeight,
            BackgroundColor = Colors.Transparent
        };

        var mainGrid = new Grid();
        mainGrid.Add(new Image { Source = "market_card_bg.png", Aspect = Aspect.Fill, Opacity = 1 });

        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(imageSize) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(buttonWidth) }
            },
            ColumnSpacing = 8,
            Padding = new Thickness(15, 15, 15, 15),
            VerticalOptions = LayoutOptions.Center
        };

        contentGrid.Add(new Image
        {
            Source = item.ImageResource,
            HeightRequest = imageSize,
            WidthRequest = imageSize,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        }, 0);

        var textLayout = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };

        textLayout.Children.Add(new Label
        {
            Text = item.ItemName,
            TextColor = Colors.White,
            FontSize = _screenWidth * 0.038,
            FontFamily = "alfont_com_Alyamama-Black",
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        });

        textLayout.Children.Add(new Label
        {
            Text = $"البائع: {item.SellerName}",
            TextColor = Color.FromArgb("#bdc3c7"),
            FontSize = _screenWidth * 0.028,
            HorizontalOptions = LayoutOptions.Center
        });

        textLayout.Children.Add(new Label
        {
            Text = $"الكمية: x{item.Quantity}",
            TextColor = Color.FromArgb("#f1c40f"),
            FontSize = _screenWidth * 0.03,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        });

        textLayout.Children.Add(new Label
        {
            Text = $"السعر: {item.PricePerItem:N0} ذهب",
            TextColor = Color.FromArgb("#2ecc71"),
            FontSize = _screenWidth * 0.03,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        });

        // Stats display
        if (item.IsWeapon)
        {
            var statsLayout = new HorizontalStackLayout { Spacing = 15, HorizontalOptions = LayoutOptions.Center };
            statsLayout.Children.Add(new Label { Text = $"⚔️ {item.Damage}", TextColor = Color.FromArgb("#ff6b6b"), FontSize = _screenWidth * 0.028 });
            statsLayout.Children.Add(new Label { Text = $"🎯 {item.Accuracy}", TextColor = Color.FromArgb("#3498db"), FontSize = _screenWidth * 0.028 });
            textLayout.Children.Add(statsLayout);
        }
        else if (item.CategoryId == 1)
        {
            var statsLayout = new HorizontalStackLayout { Spacing = 15, HorizontalOptions = LayoutOptions.Center };
            statsLayout.Children.Add(new Label { Text = $"🛡️ {item.Defense}", TextColor = Color.FromArgb("#ff6b6b"), FontSize = _screenWidth * 0.028 });
            statsLayout.Children.Add(new Label { Text = $"💨 {item.Evasion}", TextColor = Color.FromArgb("#3498db"), FontSize = _screenWidth * 0.028 });
            textLayout.Children.Add(statsLayout);
        }

        contentGrid.Add(textLayout, 1);

        var buyButtonBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            HeightRequest = _screenHeight * 0.055,
            WidthRequest = buttonWidth,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };

        var buttonGrid = new Grid();
        buttonGrid.Add(new Image { Source = "button_background.png", Aspect = Aspect.Fill, Opacity = 0.9 });
        buttonGrid.Add(new Label
        {
            Text = "شراء",
            TextColor = Colors.Black,
            FontSize = _screenWidth * 0.038,
            FontFamily = "alfont_com_Alyamama-Black",
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        });

        buyButtonBorder.Content = buttonGrid;
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await buyButtonBorder.ScaleTo(0.92, 100);
            await Task.Delay(100);
            await buyButtonBorder.ScaleTo(1.0, 100);
            await ShowBuyPopup(item);
        };
        buyButtonBorder.GestureRecognizers.Add(tap);

        contentGrid.Add(buyButtonBorder, 2);
        mainGrid.Add(contentGrid);
        mainBorder.Content = mainGrid;
        return mainBorder;
    }

    private async Task ShowBuyPopup(GangMarketItem item)
    {
        string quantityStr = await DisplayPromptAsync("شراء", $"أدخل الكمية (1-{item.Quantity}):", "شراء", "إلغاء", "1", -1, Keyboard.Numeric);
        if (string.IsNullOrEmpty(quantityStr)) return;
        if (!int.TryParse(quantityStr, out int quantity) || quantity < 1 || quantity > item.Quantity)
        {
            await ShowNotification("كمية غير صالحة", false);
            return;
        }

        int totalCost = item.PricePerItem * quantity;
        bool confirm = await DisplayAlert("تأكيد الشراء",
            $"شراء {quantity} × {item.ItemName}\nمن {item.SellerName}\nالتكلفة: {totalCost:N0} ذهب",
            "نعم", "لا");
        if (!confirm) return;

        var result = GangMarketService.BuyItem(_player, item, quantity);
        await ShowNotification(result.message, result.success);
        if (result.success) LoadSellers();
    }

    private async Task ShowNotification(string message, bool isSuccess)
    {
        if (_notificationBorder == null)
        {
            _notificationLabel = new Label
            {
                FontFamily = "alfont_com_Alyamama-Black",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.White
            };
            _notificationBorder = new Border
            {
                Stroke = Colors.White,
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(20, 15, 20, 15),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = _screenWidth * 0.60,
                Content = _notificationLabel,
                IsVisible = false
            };
            if (Content is Grid mainGrid)
            {
                mainGrid.Add(_notificationBorder);
                Grid.SetRow(_notificationBorder, 0);
                Grid.SetRowSpan(_notificationBorder, 3);
            }
        }

        _notificationLabel.Text = message;
        _notificationBorder.BackgroundColor = isSuccess ? Color.FromArgb("#CC27ae60") : Color.FromArgb("#CCc0392b");
        _notificationBorder.Stroke = isSuccess ? Color.FromArgb("#2ecc71") : Color.FromArgb("#e74c3c");

        _notificationBorder.IsVisible = true;
        _notificationBorder.Opacity = 0;
        _notificationBorder.Scale = 0.8;

        await Task.WhenAll(
            _notificationBorder.ScaleTo(1, 200, Easing.CubicOut),
            _notificationBorder.FadeTo(1, 200)
        );
        await Task.Delay(2000);
        await Task.WhenAll(
            _notificationBorder.ScaleTo(0.8, 200, Easing.CubicIn),
            _notificationBorder.FadeTo(0, 200)
        );
        _notificationBorder.IsVisible = false;
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync(false);
    private async void OnRefreshClicked(object sender, EventArgs e) { LoadSellers(); await ShowNotification("تم تحديث القائمة", true); }
}