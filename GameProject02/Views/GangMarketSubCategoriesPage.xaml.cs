using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class GangMarketSubCategoriesPage : ContentPage
{
    private readonly int _categoryId;
    private double _screenWidth, _screenHeight;
    private const string _borderColor = "#f39c12";

    public GangMarketSubCategoriesPage(int categoryId)
    {
        InitializeComponent();
        _categoryId = categoryId;
        _screenWidth = Application.Current.MainPage.Width;
        _screenHeight = Application.Current.MainPage.Height;
        LoadSubCategories();
    }

    private void LoadSubCategories()
    {
        var subCats = GangMarketService.GetAvailableSubCategories(_categoryId);
        SubCategoriesGrid.Children.Clear();
        SubCategoriesGrid.RowDefinitions.Clear();

        int rows = (subCats.Count + 1) / 2;
        for (int i = 0; i < rows; i++)
            SubCategoriesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        double cardHeight = _screenHeight * 0.24;
        double nameFontSize = _screenWidth * 0.042;

        for (int i = 0; i < subCats.Count; i++)
        {
            var sub = subCats[i];
            int row = i / 2, col = i % 2;
            var card = CreateCategoryCard(sub.Name, sub.ImageResource, nameFontSize, cardHeight);

            // ✅ Fixed: create TapGestureRecognizer correctly
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                await card.ScaleTo(0.92, 100);
                await Task.Delay(100);
                await card.ScaleTo(1.0, 100);
                await Navigation.PushAsync(new GangMarketItemsPage(_categoryId, sub.Id, sub.Name), false);
            };
            card.GestureRecognizers.Add(tapGesture);

            SubCategoriesGrid.Add(card, col, row);
        }
    }

    private Border CreateCategoryCard(string name, string image, double fontSize, double height)
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
        innerBorder.Content = new Image { Source = image, Aspect = Aspect.Fill };
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
            Text = name,
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

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync(false);
}