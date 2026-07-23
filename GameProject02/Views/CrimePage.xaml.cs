using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;

namespace GameProject02.Views;

public partial class CrimePage : ContentPage
{
    private PlayerAccount _player;

    private readonly Dictionary<int, string> _crimeTypeNames = new()
    {
        { 0,  "جرائم النشال" },
        { 1,  "جرائم اللص" },
        { 2,  "جرائم المحتال" },
        { 3,  "جرائم المأجور" },
        { 4,  "جرائم البلطجي" },
        { 5,  "جرائم القاطع" },
        { 6,  "جرائم المجرم" },
        { 7,  "جرائم العقرب" },
        { 8,  "جرائم الصياد" },
        { 9,  "جرائم القناص" },
        { 10, "جرائم القاتل" },
        { 11, "جرائم الجلاد" },
        { 12, "جرائم الشبح" },
        { 13, "جرائم الجزار" },
        { 14, "جرائم السفاح" },
        { 15, "جرائم البارون" },
        { 16, "جرائم البروفسور" }
    };

    private readonly Dictionary<int, string> _crimeTypeImages = new()
    {
        { 0,  "crime_type_1" },
        { 1,  "crime_type_2" },
        { 2,  "crime_type_3" },
        { 3,  "crime_type_4" },
        { 4,  "crime_type_5" },
        { 5,  "crime_type_6" },
        { 6,  "crime_type_7" },
        { 7,  "crime_type_8" },
        { 8,  "crime_type_9" },
        { 9,  "crime_type_10" },
        { 10, "crime_type_11" },
        { 11, "crime_type_12" },
        { 12, "crime_type_13" },
        { 13, "crime_type_14" },
        { 14, "crime_type_15" },
        { 15, "crime_type_16" },
        { 16, "crime_type_17" }
    };

    public CrimePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        LoadCrimeData();
        SetupFooter();
    }

    // ─────────────────────────────────────────────
    //  إعداد الفوتر
    // ─────────────────────────────────────────────
    private void SetupFooter()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(30, 0)
        };

        var homeButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnHomeClicked,
            buttonImageSource: "footer_button_back.png"
        );

        grid.Add(homeButton, 1, 0);
        PageFooter.SetContent(grid);
    }

    // ─────────────────────────────────────────────
    //  تحميل البيانات
    // ─────────────────────────────────────────────
    private void LoadCrimeData()
    {
        if (_player == null) return;


        var firstCrime = CrimeDatabase.GetCrimeItem(0, 0);
        if (firstCrime != null && !_player.CrimeObject.TaskProgress.ContainsKey(0))
            _player.CrimeObject.TaskProgress[0] = firstCrime.RequiredSuccesses;

        CrimeTypesList.Children.Clear();
        for (int i = 0; i < 17; i++)
            CrimeTypesList.Children.Add(
                CreateCrimeTypeCard(i, _crimeTypeNames[i], _crimeTypeImages[i]));
    }

    // ─────────────────────────────────────────────
    //  بناء البطاقة
    // ─────────────────────────────────────────────
    private Border CreateCrimeTypeCard(int crimeTypeId, string name, string imageResource)
    {
        bool isUnlocked = crimeTypeId <= _player.CrimeObject.CurrentCrimeType;
        bool isCompleted = crimeTypeId < _player.CrimeObject.CurrentCrimeType;

        // ── الإطار الخارجي ───────────────────────────
        var mainBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
            Padding = 0,
            Margin = new Thickness(10, EstateUIConstants.CardMarginVertical),
            MinimumHeightRequest = EstateUIConstants.CardMinHeight,
            BackgroundColor = Colors.Transparent
        };

        var mainGrid = new Grid();
        mainGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 1 });

        // ── شبكة المحتوى: صورة | نص+نجوم | زر ───────
        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = EstateUIConstants.ColumnSpacing,
            Padding = new Thickness(EstateUIConstants.CardContentPadding, 0),
            VerticalOptions = LayoutOptions.Center
        };

        // ── الصورة (يمين) ────────────────────────────
        var imageBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ImageCornerRadius },
            WidthRequest = EstateUIConstants.ImageSize,
            HeightRequest = EstateUIConstants.ImageSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };
        imageBorder.Content = new Image
        {
            Source = imageResource,
            Aspect = Aspect.AspectFill,
            Opacity = isUnlocked ? 1.0 : 0.5
        };
        contentGrid.Add(imageBorder, 0);

        // ── النص والنجوم (وسط) ───────────────────────
        var detailsStack = new VerticalStackLayout
        {
            Spacing = EstateUIConstants.StackSpacing,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        detailsStack.Children.Add(new Label
        {
            Text = name,
            Style = (Style)Application.Current.Resources["CardTitle"],
            FontSize = EstateUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            Opacity = isUnlocked ? 1.0 : 0.6
        });

        // ── النجوم بصفين ─────────────────────────────
        var crimeType = CrimeDatabase.GetCrimeType(crimeTypeId);
        if (crimeType != null)
        {
            int completedTasks = _player.CrimeObject.CurrentTaskIndex
                .TryGetValue(crimeTypeId, out int idx) ? idx : (crimeTypeId == 0 ? 1 : 0);
            int totalTasks = crimeType.Crimes.Count;

            int firstRowCount = Math.Min(totalTasks, 5);
            int secondRowCount = totalTasks - firstRowCount;

            var starsWrapper = new VerticalStackLayout
            {
                Spacing = 2,
                HorizontalOptions = LayoutOptions.Center
            };

            // الصف الأول - أول 5 نجوم
            var firstRow = new HorizontalStackLayout
            {
                Spacing = 2,
                HorizontalOptions = LayoutOptions.Center
            };
            for (int i = 0; i < firstRowCount; i++)
            {
                bool isFilled = isCompleted || (isUnlocked && i < completedTasks);
                firstRow.Children.Add(new Image
                {
                    Source = isFilled ? "star_filled.png" : "star_empty.png",
                    WidthRequest = EstateUIConstants.FontSizeLarge,
                    HeightRequest = EstateUIConstants.FontSizeLarge,
                    Aspect = Aspect.AspectFit,
                    VerticalOptions = LayoutOptions.Center
                });
            }
            starsWrapper.Children.Add(firstRow);

            // الصف الثاني - الباقي إن وجد
            if (secondRowCount > 0)
            {
                var secondRow = new HorizontalStackLayout
                {
                    Spacing = 2,
                    HorizontalOptions = LayoutOptions.Center
                };
                for (int i = firstRowCount; i < totalTasks; i++)
                {
                    bool isFilled = isCompleted || (isUnlocked && i < completedTasks);
                    secondRow.Children.Add(new Image
                    {
                        Source = isFilled ? "star_filled.png" : "star_empty.png",
                        WidthRequest = EstateUIConstants.FontSizeLarge,
                        HeightRequest = EstateUIConstants.FontSizeLarge,
                        Aspect = Aspect.AspectFit,
                        VerticalOptions = LayoutOptions.Center
                    });
                }
                starsWrapper.Children.Add(secondRow);
            }

            detailsStack.Children.Add(starsWrapper);
        }

        contentGrid.Add(detailsStack, 1);

        // ── الزر (يسار) ──────────────────────────────
        var buttonBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ButtonCornerRadius },
            WidthRequest = EstateUIConstants.ButtonWidth,
            HeightRequest = EstateUIConstants.ButtonHeight,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            Padding = 0,
            BackgroundColor = Colors.Transparent
        };

        var buttonGrid = new Grid();
        buttonGrid.Add(new Image
        {
            Source = isUnlocked ? "button_background.png" : "card_background.png",
            Aspect = Aspect.Fill
        });
        buttonGrid.Add(new Label
        {
            Text = isUnlocked ? "دخول" : "مقفول",
            TextColor = isUnlocked ? Colors.Black
                                   : (Color)Application.Current.Resources["ColorError"],
            Style = (Style)Application.Current.Resources["ActionButton"],
            FontSize = EstateUIConstants.FontSizeButton
        });
        buttonBorder.Content = buttonGrid;

        var tap = new TapGestureRecognizer();
        if (isUnlocked)
        {
            tap.Tapped += async (s, e) =>
            {
                await AnimateBorder(buttonBorder);
                await Navigation.PushAsync(new CrimeTypePage(crimeTypeId), false);
            };
        }
        else
        {
            tap.Tapped += (s, e) =>
            {
                DisplayAlert("🔒 مقفول",
                    $"أنهِ جميع جرائم \"{GetCrimeTypeName(crimeTypeId - 1)}\" لفتح هذا النوع",
                    "موافق");
            };
        }
        buttonBorder.GestureRecognizers.Add(tap);

        contentGrid.Add(buttonBorder, 2);
        mainGrid.Add(contentGrid);
        mainBorder.Content = mainGrid;
        return mainBorder;
    }

    private string GetCrimeTypeName(int id) =>
        _crimeTypeNames.TryGetValue(id, out var n) ? n : "نوع جريمة غير معروف";

    // ─────────────────────────────────────────────
    //  الأحداث
    // ─────────────────────────────────────────────
    private async void OnHomeClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopToRootAsync(false);
    }

    private async Task AnimateBorder(Border border)
    {
        if (border == null) return;
        await border.ScaleTo(EstateUIConstants.AnimationPressScale,
                             EstateUIConstants.AnimationPressDuration, Easing.CubicIn);
        await border.ScaleTo(1, EstateUIConstants.AnimationPressDuration, Easing.CubicOut);
    }
}