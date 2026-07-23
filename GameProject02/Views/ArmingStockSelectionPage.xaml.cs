using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class ArmingStockSelectionPage : ContentPage
{
    private readonly string _slotType; // "weapon", "armor", "special", "biochemical"
    private readonly int _categoryType; // 0=weapons, 1=armor, 3=special, 4=biochemical
    private PlayerAccount _player;
    private double _screenWidth;
    private double _screenHeight;

    public ArmingStockSelectionPage(string slotType, int categoryType)
    {
        InitializeComponent();
        _slotType = slotType;
        _categoryType = categoryType;

        // Set title based on slot type
        TitleLabel.Text = slotType switch
        {
            "weapon" => "اختيار سلاح",
            "armor" => "اختيار درع",
            "special" => "اختيار معدات خاصة",
            "biochemical" => "اختيار كيمياء حيوية",
            _ => "اختيار عنصر"
        };

        _screenWidth = Application.Current.MainPage.Width;
        _screenHeight = Application.Current.MainPage.Height;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadStockItems();
    }

    private void LoadStockItems()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        // ✅ FILTER STOCK ITEMS: Category + Available (count > 0) + NOT already equipped
        var availableItems = _player.StockObject.ItemsInStock
            .Where(kvp => kvp.Value.CategoryId == _categoryType &&
                         kvp.Value.Count > 0 &&
                         !kvp.Value.UsedInArming) // Critical: Only show items NOT in arming
            .Select(kvp => kvp.Value)
            .ToList();

        ItemsContainer.Children.Clear();
        EmptyState.IsVisible = availableItems.Count == 0;

        if (availableItems.Count == 0)
        {
            EmptyState.IsVisible = true;
            return;
        }

        foreach (var stockItem in availableItems)
        {
            var itemControl = CreateStockItemControl(stockItem);
            ItemsContainer.Children.Add(itemControl);
        }
    }

    private Border CreateStockItemControl(StockItem stockItem)
    {
        double cardHeight = _screenHeight * 0.12;
        double imageSize = cardHeight * 0.6;
        double buttonWidth = _screenWidth * 0.25;

        var border = new Border
        {
            Stroke = Color.FromArgb("#3498db"),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            BackgroundColor = Color.FromArgb("#2c2c2c"),
            Padding = new Thickness(10),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(imageSize) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(buttonWidth) }
            },
            ColumnSpacing = 10,
            VerticalOptions = LayoutOptions.Center
        };

        // Item image
        var image = new Image
        {
            Source = stockItem.ImageResource,
            HeightRequest = imageSize,
            WidthRequest = imageSize,
            Aspect = Aspect.AspectFit
        };
        grid.Add(image, 0, 0);

        // Item info
        var infoStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };

        var nameLabel = new Label
        {
            Text = stockItem.Name,
            TextColor = Colors.White,
            FontSize = _screenWidth * 0.035,
            FontAttributes = FontAttributes.Bold
        };
        infoStack.Children.Add(nameLabel);

        var countLabel = new Label
        {
            Text = $"الكمية: {stockItem.Count}",
            TextColor = Color.FromArgb("#2ecc71"),
            FontSize = _screenWidth * 0.028
        };
        infoStack.Children.Add(countLabel);

        // Add stats if weapon/armor
        if (_categoryType == 0) // Weapon
        {
            var weapon = MarketService.GetItemById(stockItem.ItemId);
            if (weapon != null)
            {
                var statsLabel = new Label
                {
                    Text = $"ضرر: {weapon.Damage} | دقة: {weapon.Accuracy}",
                    TextColor = Color.FromArgb("#bdc3c7"),
                    FontSize = _screenWidth * 0.025
                };
                infoStack.Children.Add(statsLabel);
            }
        }
        else if (_categoryType == 1) // Armor
        {
            var armor = MarketService.GetItemById(stockItem.ItemId);
            if (armor != null)
            {
                var statsLabel = new Label
                {
                    Text = $"دفاع: {armor.Defense} | تفادي: {armor.Evasion}",
                    TextColor = Color.FromArgb("#bdc3c7"),
                    FontSize = _screenWidth * 0.025
                };
                infoStack.Children.Add(statsLabel);
            }
        }

        grid.Add(infoStack, 1, 0);

        // Equip button
        var equipButton = new Button
        {
            Text = "⚔️ تجهيز",
            BackgroundColor = Color.FromArgb("#8e44ad"),
            TextColor = Colors.White,
            CornerRadius = 8,
            FontSize = _screenWidth * 0.03,
            HeightRequest = cardHeight * 0.6,
            WidthRequest = buttonWidth
        };
        equipButton.Clicked += async (s, e) => await OnEquipClicked(stockItem);
        grid.Add(equipButton, 2, 0);

        border.Content = grid;
        return border;
    }

    private async Task OnEquipClicked(StockItem stockItem)
    {
        // ✅ EQUIP ITEM DIRECTLY FROM STOCK (NO MARKET INVOLVED)
        var result = ArmingService.EquipItem(_player, stockItem.ItemId, _slotType);

        await DisplayAlert(
            result.success ? "✅ نجاح" : "❌ فشل",
            result.message,
            "موافق"
        );

        if (result.success)
        {
            // Refresh to show updated stock
            LoadStockItems();

            // Optional: Auto-close after 800ms
            await Task.Delay(800);
            await Navigation.PopAsync(false);
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(false);
    }
}