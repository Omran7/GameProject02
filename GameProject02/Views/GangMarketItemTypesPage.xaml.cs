using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class GangMarketItemTypesPage : ContentPage
{
    private readonly int _categoryId;
    private readonly int _subCategoryId; // -1 if no subcategory
    private readonly string _categoryName;
    private double _screenWidth, _screenHeight;
    private const string _borderColor = "#f39c12";

    public GangMarketItemTypesPage(int categoryId, int subCategoryId, string categoryName)
    {
        InitializeComponent();
        _categoryId = categoryId;
        _subCategoryId = subCategoryId;
        _categoryName = categoryName;
        _screenWidth = Application.Current.MainPage.Width;
        _screenHeight = Application.Current.MainPage.Height;

        CategoryTitleLabel.Text = categoryName;
        LoadItemTypes();
    }

    private void LoadItemTypes()
    {
        var allListings = GangMarketService.GetAllListings();
        var filtered = new List<GangMarketItem>();

        if (_categoryId == 0 && _subCategoryId >= 0)
            filtered = allListings.Where(i => i.CategoryId == 0 && i.GunType == _subCategoryId).ToList();
        else
            filtered = allListings.Where(i => i.CategoryId == _categoryId).ToList();

        // Group by ItemId to get distinct item types
        var distinctItems = filtered
            .GroupBy(i => i.ItemId)
            .Select(g => g.First())
            .ToList();

        ItemsGrid.Children.Clear();
        ItemsGrid.RowDefinitions.Clear();

        int rows = (distinctItems.Count + 1) / 2;
        for (int i = 0; i < rows; i++)
            ItemsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        double cardHeight = _screenHeight * 0.24;
        double nameFontSize = _screenWidth * 0.04;

        for (int i = 0; i < distinctItems.Count; i++)
        {
            var item = distinctItems[i];
            int row = i / 2, col = i % 2;
            var card = CreateItemTypeCard(item, nameFontSize, cardHeight);
            int index = i;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) =>
            {
                await card.ScaleTo(0.92, 100);
                await Task.Delay(100);
                await card.ScaleTo(1.0, 100);
                await Navigation.PushAsync(new GangMarketSellersPage(_categoryId, _subCategoryId, item.ItemId, item.ItemName));
            };
            card.GestureRecognizers.Add(tap);
            ItemsGrid.Add(card, col, row);
        }
    }

    private Border CreateItemTypeCard(GangMarketItem item, double fontSize, double height)
    {
        var card = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            HeightRequest = height,
            Padding = 0,
            BackgroundColor = Colors.Transparent
        };
        var mainLayout = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        var outerBorder = new Border
        {
            Stroke = Color.FromArgb(_borderColor),
            StrokeThickness = 3,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            HeightRequest = height * 0.70,
            WidthRequest = height * 0.70,
            HorizontalOptions = LayoutOptions.Center,
            Padding = 4,
            BackgroundColor = Color.FromArgb("#0a0a0a")
        };
        var innerBorder = new Border
        {
            Stroke = Color.FromArgb(_borderColor),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = 0
        };
        innerBorder.Content = new Image { Source = item.ImageResource, Aspect = Aspect.Fill };
        outerBorder.Content = innerBorder;
        mainLayout.Children.Add(outerBorder);
        var nameBorder = new Border
        {
            Stroke = Color.FromArgb(_borderColor),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(15, 8),
            BackgroundColor = Color.FromArgb("#0a0a0a")
        };
        nameBorder.Content = new Label
        {
            Text = item.ItemName,
            FontFamily = "alfont_com_Alyamama-Black",
            TextColor = Colors.DarkGoldenrod,
            FontSize = fontSize,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        };
        mainLayout.Children.Add(nameBorder);
        card.Content = mainLayout;
        return card;
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnRefreshClicked(object sender, EventArgs e) { LoadItemTypes(); await DisplayAlert("تحديث", "تم تحديث قائمة العناصر", "موافق"); }
}