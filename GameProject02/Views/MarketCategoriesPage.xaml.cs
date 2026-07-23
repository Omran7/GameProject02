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

public partial class MarketCategoriesPage : ContentPage
{
    private PlayerAccount _player;

    private readonly List<CategoryInfo> _categories = new()
    {
        new CategoryInfo { Id = 0, Name = "متجر الاسلحة", ImageResource = "market_weapons.png", HasSubCategories = true },
        new CategoryInfo { Id = 1, Name = "متجر الدروع", ImageResource = "market_armors.png", HasSubCategories = false },
        new CategoryInfo { Id = 2, Name = "ادوات المدينة", ImageResource = "market_grocery.png", HasSubCategories = false },
        new CategoryInfo { Id = 4, Name = "الصيدلية", ImageResource = "market_pharmacy.png", HasSubCategories = false },
        new CategoryInfo { Id = 5, Name = "ورد وكريستال", ImageResource = "market_crystal.png", HasSubCategories = false }
    };

    private readonly string[] _weaponSubCategories = new[]
    {
        "سلاح ابيض", "المسدسات", "الرشاشات الصغيرة",
        "بنادق الصيد", "رشاشات", "القناصات",
        "رشاشات ثقيلة", "قواذف"
    };

    private int _selectedCategoryId = 0;

    public MarketCategoriesPage()
    {
        InitializeComponent();
        LoadCategories();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
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

        var homeButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnHomeClicked,
            buttonImageSource: "footer_button_back.png"
        );

        grid.Add(homeButton, 1, 0);
        PageFooter.SetContent(grid);
    }

    private void LoadCategories()
    {
        CategoriesContainer.Children.Clear();

        foreach (var category in _categories)
        {
            var card = CreateCategoryCard(category);
            CategoriesContainer.Children.Add(card);
        }
    }

    private Border CreateCategoryCard(CategoryInfo category)
    {
        double buttonWidth = EstateUIConstants.ButtonWidth;
        double buttonHeight = EstateUIConstants.ButtonHeight;
        double imageSize = EstateUIConstants.ImageSize;

        var mainBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
            Padding = 0,
            Margin = new Thickness(0, EstateUIConstants.CardMarginVertical),
            MinimumHeightRequest = EstateUIConstants.CardMinHeight,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill
        };

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto }
            }
        };
        mainGrid.Add(new Image
        {
            Source = "card_background.png",
            Aspect = Aspect.Fill,
            Opacity = 1
        });

        // شبكة المحتوى: صورة | تفاصيل | زر
        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = EstateUIConstants.ColumnSpacing,
            Padding = new Thickness(EstateUIConstants.CardContentPadding, EstateUIConstants.CardContentPadding * 0.5),
            VerticalOptions = LayoutOptions.Center
        };

        // صورة الفئة
        var imageBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ImageCornerRadius },
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };
        imageBorder.Content = new Image
        {
            Source = category.ImageResource,
            Aspect = Aspect.Fill,
            WidthRequest = imageSize,
            HeightRequest = imageSize
        };
        contentGrid.Add(imageBorder, 0);

        // اسم الفئة
        var nameLabel = new Label
        {
            Text = category.Name,
            Style = (Style)Application.Current.Resources["CardTitle"],
            FontSize = EstateUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.WordWrap,
            MaxLines = 2
        };
        contentGrid.Add(nameLabel, 1);

        // زر "دخول"
        var buttonBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ButtonCornerRadius },
            WidthRequest = buttonWidth,
            HeightRequest = buttonHeight,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            Padding = 0,
            BackgroundColor = Colors.Transparent
        };

        var buttonGrid = new Grid();
        buttonGrid.Add(new Image
        {
            Source = "button_background.png",
            Aspect = Aspect.Fill
        });
        buttonGrid.Add(new Label
        {
            Text = "دخول",
            Style = (Style)Application.Current.Resources["ActionButton"],
            FontSize = EstateUIConstants.FontSizeButton
        });

        buttonBorder.Content = buttonGrid;

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await AnimateBorder(buttonBorder);
            await OnCategoryTapped(category);
        };
        buttonBorder.GestureRecognizers.Add(tap);

        contentGrid.Add(buttonBorder, 2);

        mainGrid.Add(contentGrid);
        mainBorder.Content = mainGrid;

        return mainBorder;
    }

    private async Task OnCategoryTapped(CategoryInfo category)
    {
        if (category.HasSubCategories)
        {
            _selectedCategoryId = category.Id;

            var selectedSubCategory = await PopupService.ShowSelectionPopupWithCustomView(
                title: $" {category.Name}",
                items: _weaponSubCategories.Select((name, index) => new { Index = index, Name = name }).ToList(),
                createItemView: (item) => CreateSubCategoryItemView(item.Name)
            );

            if (selectedSubCategory != null)
            {
                await NavigateToItemsPage(category.Id, selectedSubCategory.Index, category.Name, selectedSubCategory.Name);
            }
        }
        else
        {
            await NavigateToItemsPage(category.Id, 0, category.Name, "");
        }
    }

    private View CreateSubCategoryItemView(string subCategoryName)
    {
        var border = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            HeightRequest = 50,
            BackgroundColor = Colors.Transparent,
            Padding = 0
        };

        var grid = new Grid();
        grid.Add(new Image
        {
            Source = "card_background.png",
            Aspect = Aspect.Fill,
            Opacity = 1,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        });

        var label = new Label
        {
            Text = subCategoryName,
            Style = (Style)Application.Current.Resources["PopupListItem"],
            FontSize = EstateUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        grid.Add(label);

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

    private async Task NavigateToItemsPage(int categoryId, int subCategoryId, string categoryName, string subCategoryName)
    {
        var itemsPage = new MarketPage(categoryId, subCategoryId, categoryName, subCategoryName);
        await Navigation.PushAsync(itemsPage, false);
    }

    private async Task AnimateBorder(Border border)
    {
        if (border == null) return;
        try
        {
            await border.ScaleTo(EstateUIConstants.AnimationPressScale, EstateUIConstants.AnimationPressDuration, Easing.CubicIn);
            await border.ScaleTo(1, EstateUIConstants.AnimationPressDuration, Easing.CubicOut);
        }
        catch { }
    }

    private async Task NavigateBackToConfinementOrHome()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null)
        {
            await Navigation.PopToRootAsync(false);
            return;
        }

        player.CrimeObject.CheckConfinementStatus();

        if (player.CrimeObject.IsInPrison)
        {
            if (Navigation.ModalStack.Any(p => p is PrisonPage))
                await Navigation.PopModalAsync();
            else
                await Navigation.PushModalAsync(new PrisonPage());
            return;
        }

        if (player.CrimeObject.IsInHospital)
        {
            if (Navigation.ModalStack.Any(p => p is HospitalPage))
                await Navigation.PopModalAsync();
            else
                await Navigation.PushModalAsync(new HospitalPage());
            return;
        }

        await Navigation.PopToRootAsync(false);
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await NavigateBackToConfinementOrHome();
    }
}

public class CategoryInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ImageResource { get; set; } = "";
    public bool HasSubCategories { get; set; }
}