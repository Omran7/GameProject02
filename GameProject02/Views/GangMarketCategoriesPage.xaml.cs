using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class GangMarketCategoriesPage : ContentPage
{
    private double _screenWidth;
    private double _screenHeight;
    private readonly string _uniformBorderColor = "#f39c12";

    // Weapon subcategories list
    private readonly List<WeaponSubCategory> _weaponSubCategories = new()
    {
        new WeaponSubCategory { Id = 0, Name = "سلاح ابيض", SubCategoryId = 0 },
        new WeaponSubCategory { Id = 1, Name = "المسدسات", SubCategoryId = 1 },
        new WeaponSubCategory { Id = 2, Name = "الرشاشات الصغيرة", SubCategoryId = 2 },
        new WeaponSubCategory { Id = 3, Name = "بنادق الصيد", SubCategoryId = 3 },
        new WeaponSubCategory { Id = 4, Name = "رشاشات", SubCategoryId = 4 },
        new WeaponSubCategory { Id = 5, Name = "القناصات", SubCategoryId = 5 },
        new WeaponSubCategory { Id = 6, Name = "رشاشات ثقيلة", SubCategoryId = 6 },
        new WeaponSubCategory { Id = 7, Name = "قواذف", SubCategoryId = 7 }
    };

    public GangMarketCategoriesPage()
    {
        InitializeComponent();
        _screenWidth = Application.Current.MainPage.Width;
        _screenHeight = Application.Current.MainPage.Height;
        LoadCategories();
    }

    private void LoadCategories()
    {
        var categories = new List<CategoryInfo>
        {
            new CategoryInfo { Id = 0, Name = "الأسلحة", ImageResource = "market_weapons.png", HasSubCategories = true },
            new CategoryInfo { Id = 1, Name = "الدروع", ImageResource = "market_armors.png", HasSubCategories = false },
            new CategoryInfo { Id = 2, Name = "البقالة", ImageResource = "market_grocery.png", HasSubCategories = false },
            new CategoryInfo { Id = 4, Name = "الصيدلية", ImageResource = "market_pharmacy.png", HasSubCategories = false },
            new CategoryInfo { Id = 5, Name = "ورد وكريستال", ImageResource = "market_crystal.png", HasSubCategories = false }
        };

        // Filter only categories that have at least one item in gang market
        var allItems = GangMarketService.GetAllListings();
        var availableCategories = new List<CategoryInfo>();
        foreach (var cat in categories)
        {
            bool hasItems = false;
            if (cat.Id == 0) // Weapons: check any weapon subcategory
            {
                for (int sub = 0; sub <= 7; sub++)
                {
                    if (GangMarketService.GetItemsByCategoryAndSubCategory(cat.Id, sub).Count > 0)
                    {
                        hasItems = true;
                        break;
                    }
                }
            }
            else
            {
                hasItems = allItems.Exists(i => i.CategoryId == cat.Id);
            }
            if (hasItems)
                availableCategories.Add(cat);
        }

        CategoryCardsGrid.Children.Clear();
        CategoryCardsGrid.RowDefinitions.Clear();

        int rows = (availableCategories.Count + 1) / 2;
        for (int i = 0; i < rows; i++)
            CategoryCardsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        double cardHeight = _screenHeight * 0.24;
        double nameFontSize = _screenWidth * 0.042;

        for (int i = 0; i < availableCategories.Count; i++)
        {
            var category = availableCategories[i];
            int row = i / 2, col = i % 2;
            var card = CreateCategoryCard(category, nameFontSize, cardHeight);
            CategoryCardsGrid.Add(card, col, row);
        }
    }

    private Border CreateCategoryCard(CategoryInfo category, double nameFontSize, double cardHeight)
    {
        var card = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            HeightRequest = cardHeight,
            Padding = new Thickness(0),
            BackgroundColor = Colors.Transparent
        };

        var mainLayout = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var outerBorder = new Border
        {
            Stroke = Color.FromArgb(_uniformBorderColor),
            StrokeThickness = 3,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            HeightRequest = cardHeight * 0.70,
            WidthRequest = cardHeight * 0.70,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(4),
            BackgroundColor = Color.FromArgb("#0a0a0a")
        };

        var innerBorder = new Border
        {
            Stroke = Color.FromArgb(_uniformBorderColor),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(0)
        };

        var bgImage = new Image
        {
            Source = category.ImageResource,
            Aspect = Aspect.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        innerBorder.Content = bgImage;
        outerBorder.Content = innerBorder;
        mainLayout.Children.Add(outerBorder);

        var nameOuterBorder = new Border
        {
            Stroke = Color.FromArgb(_uniformBorderColor),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(15, 8, 15, 8),
            BackgroundColor = Color.FromArgb("#0a0a0a")
        };

        var nameLabel = new Label
        {
            Text = category.Name,
            FontFamily = "alfont_com_Alyamama-Black",
            TextColor = Colors.DarkGoldenrod,
            FontSize = nameFontSize,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        nameOuterBorder.Content = nameLabel;
        mainLayout.Children.Add(nameOuterBorder);

        card.Content = mainLayout;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) => await OnCardTapped(card, category);
        card.GestureRecognizers.Add(tapGesture);

        return card;
    }

    private async Task OnCardTapped(Border card, CategoryInfo category)
    {
        await card.ScaleTo(0.92, 100, Easing.CubicOut);
        await Task.Delay(100);
        await card.ScaleTo(1.0, 100, Easing.CubicIn);

        if (category.HasSubCategories)
        {
            // Show weapon subcategories popup
            await ShowSubcategoriesPopup(category.Name);
        }
        else
        {
            // Navigate to item types page (distinct items) for this category
            await Navigation.PushAsync(new GangMarketItemTypesPage(category.Id, -1, category.Name));
        }
    }

    private async Task ShowSubcategoriesPopup(string categoryName)
    {
        PopupTitle.Text = $"اختر نوع السلاح";
        SubCategoriesList.Children.Clear();

        double subCardHeight = _screenHeight * 0.065;
        double subFontSize = _screenWidth * 0.035;

        foreach (var sub in _weaponSubCategories)
        {
            // Check if this subcategory has any items in the market
            var itemsInSub = GangMarketService.GetItemsByCategoryAndSubCategory(0, sub.SubCategoryId);
            if (itemsInSub.Count == 0) continue; // Skip empty subcategories

            var card = CreateSubCategoryCard(sub.Name, sub.SubCategoryId, subCardHeight, subFontSize);
            SubCategoriesList.Children.Add(card);
        }

        PopupOverlay.InputTransparent = false;
        PopupOverlay.IsVisible = true;

        PopupContent.Scale = 0.5;
        PopupContent.Opacity = 0;

        await Task.WhenAll(
            PopupContent.FadeTo(1, 250, Easing.CubicIn),
            PopupContent.ScaleTo(1.0, 250, Easing.CubicOut)
        );
    }

    private Border CreateSubCategoryCard(string subCategoryName, int subCategoryId, double cardHeight, double fontSize)
    {
        var card = new Border
        {
            Stroke = Color.FromArgb(_uniformBorderColor),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            BackgroundColor = Color.FromArgb("#0a0a0a"),
            Padding = new Thickness(15, 12),
            HeightRequest = cardHeight
        };

        var nameLabel = new Label
        {
            Text = subCategoryName,
            TextColor = Colors.Goldenrod,
            FontFamily = "alfont_com_Alyamama-Black",
            FontSize = fontSize,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        card.Content = nameLabel;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            await card.ScaleTo(0.95, 50);
            await Task.Delay(50);
            await card.ScaleTo(1.0, 50);
            await HidePopup();
            // Navigate to item types page for this weapon subcategory
            await Navigation.PushAsync(new GangMarketItemTypesPage(0, subCategoryId, subCategoryName));
        };
        card.GestureRecognizers.Add(tapGesture);

        return card;
    }

    private async Task HidePopup()
    {
        await Task.WhenAll(
            PopupContent.FadeTo(0, 150, Easing.CubicOut),
            PopupContent.ScaleTo(0.8, 150, Easing.CubicIn)
        );

        PopupOverlay.IsVisible = false;
        PopupOverlay.InputTransparent = true;
    }

    private async void OnClosePopupClicked(object sender, EventArgs e) => await HidePopup();
    private async void OnOverlayClicked(object sender, EventArgs e) => await HidePopup();
    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadCategories();
        await DisplayAlert("تحديث", "تم تحديث قائمة الأقسام", "موافق");
    }
}

public class WeaponSubCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SubCategoryId { get; set; }
}