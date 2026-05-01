using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;

namespace GameProject02.Views;

public partial class CrimeTypePage : ContentPage
{
    private PlayerAccount _player;
    private readonly int _crimeType;

    private static double ScreenWidth => DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
    private static double ScreenHeight => DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;

    private double CardHeight => ScreenHeight * 0.20;
    private double CrimeImageSize => CardHeight * 0.45;
    private double ToolImageSize => CardHeight * 0.45;
    private double TrafficLightSize => CardHeight * 0.14;
    private double TrafficLightSpacing => CardHeight * 0.10;
    private double ButtonHeight => CardHeight * 0.20;
    private double ButtonWidth => CardHeight * 0.60;
    private double InfoBoxHeight => CardHeight * 0.16;
    private double FontTitle => CardHeight * 0.10;
    private double FontLabel => CardHeight * 0.07;
    private double FontValue => CardHeight * 0.08;
    private double FontButton => CardHeight * 0.09;
    private double CardPadding => CardHeight * 0.11;
    private double CardSpacing => CardHeight * 0.05;
    private double CardMargin => CardHeight * 0.03;

    public CrimeTypePage(int crimeType)
    {
        InitializeComponent();
        _crimeType = crimeType;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        LoadCrimeData();
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
            Padding = new Thickness(30, 0)
        };

        var backButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnBackClicked,
            buttonImageSource: "footer_button_back.png"
        );

        grid.Add(backButton, 1, 0);
        PageFooter.SetContent(grid);
    }

    private double GetCrimePercent(int crimeItemId)
    {
        var crime = CrimeDatabase.GetCrimeItem(_crimeType, crimeItemId);
        if (crime == null) return 0;

        int li = CrimeDatabase.GetLinearIndex(_crimeType, crimeItemId);
        int progress = _player.CrimeObject.TaskProgress.GetValueOrDefault(li, 0);

        return Math.Clamp((double)progress / crime.RequiredSuccesses, 0, 1);
    }

    private bool IsCrimeUnlocked(int crimeItemId)
    {
        if (crimeItemId == 0) return true;

        var prevCrime = CrimeDatabase.GetCrimeItem(_crimeType, crimeItemId - 1);
        if (prevCrime == null) return true;

        int prevLi = CrimeDatabase.GetLinearIndex(_crimeType, crimeItemId - 1);
        int prevProgress = _player.CrimeObject.TaskProgress.GetValueOrDefault(prevLi, 0);

        return prevProgress >= prevCrime.RequiredSuccesses;
    }

    private void LoadCrimeData()
    {
        if (_player == null) return;

        _player.CrimeObject.RegenerateCourage();
        PageHeader.HeaderTitle = GetCrimeTypeName(_crimeType);

        CrimesContainer.Children.Clear();

        var crimeType = CrimeDatabase.GetCrimeType(_crimeType);
        if (crimeType == null)
        {
            CrimesContainer.Children.Add(new Label
            {
                Text = "لا توجد جرائم متاحة لهذا النوع",
                TextColor = Color.FromArgb("#bdc3c7"),
                FontSize = FontTitle,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 50, 0, 0)
            });
            return;
        }

        foreach (var crime in crimeType.Crimes)
            CrimesContainer.Children.Add(CreateCrimeCard(crime));
    }

    private string GetCrimeTypeName(int id) => id switch
    {
        0 => "جرائم النشال",
        1 => "جرائم اللص",
        2 => "جرائم المحتال",
        3 => "جرائم المأجور",
        4 => "جرائم البلطجي",
        5 => "جرائم القاطع",
        6 => "جرائم المجرم",
        7 => "جرائم العقرب",
        8 => "جرائم الصياد",
        9 => "جرائم القناص",
        10 => "جرائم القاتل",
        11 => "جرائم الجلاد",
        12 => "جرائم الشبح",
        13 => "جرائم الجزار",
        14 => "جرائم السفاح",
        15 => "جرائم البارون",
        16 => "جرائم البروفسور",
        _ => "نوع جريمة غير معروف"
    };

    private Border CreateCrimeCard(CrimeItemDefinition crime)
    {
        double percent = GetCrimePercent(crime.CrimeItemId);
        bool isUnlocked = IsCrimeUnlocked(crime.CrimeItemId);
        bool isGreen = isUnlocked && percent >= 1.0;
        bool isYellow = isUnlocked && percent >= 0.50 && percent < 1.0;
        bool isRed = isUnlocked && percent < 0.50;
        bool isExecutable = isUnlocked
                            && !_player.CrimeObject.IsInPrison
                            && !_player.CrimeObject.IsInHospital;

        string toolImage = "item_default";
        int ownedToolCount = 0;
        int requiredToolCount = 0;

        if (crime.ToolRequirements?.Count > 0)
        {
            var toolReq = crime.ToolRequirements[0];
            toolImage = toolReq.ToolItemId;
            requiredToolCount = toolReq.RequiredCount;
            if (_player.StockObject.ItemsInStock.TryGetValue(toolReq.ToolItemId, out var toolItem))
                ownedToolCount = toolItem.Count;
        }

        var mainBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            Padding = 0,
            Margin = new Thickness(5, CardMargin)
        };

        var mainGrid = new Grid();
        mainGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });

        var contentStack = new VerticalStackLayout
        {
            Spacing = CardSpacing,
            Padding = new Thickness(CardPadding),
            Opacity = isUnlocked ? 1.0 : 0.45
        };

        var mainContentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = CrimeImageSize },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = ToolImageSize }
            },
            ColumnSpacing = CardHeight * 0.04
        };

        // صورة الجريمة (يمين)
        mainContentGrid.Add(new Image
        {
            Source = crime.ImageResource,
            Aspect = Aspect.AspectFit,
            WidthRequest = CrimeImageSize,
            HeightRequest = CrimeImageSize,
            VerticalOptions = LayoutOptions.Start
        }, 0);

        // الوسط
        var middleStack = new VerticalStackLayout
        {
            Spacing = CardSpacing,
            HorizontalOptions = LayoutOptions.Fill
        };

        middleStack.Children.Add(new Label
        {
            Text = crime.Name,
            TextColor = Colors.Goldenrod,
            FontSize = FontTitle,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Start
        });

        // إشارة المرور
        var trafficLight = new HorizontalStackLayout
        {
            Spacing = TrafficLightSpacing,
            HorizontalOptions = LayoutOptions.Center
        };
        trafficLight.Children.Add(new Image
        {
            Source = "crime_traffic_green",
            WidthRequest = TrafficLightSize,
            HeightRequest = TrafficLightSize,
            Aspect = Aspect.AspectFit,
            Opacity = isGreen ? 1.0 : 0.25
        });
        trafficLight.Children.Add(new Image
        {
            Source = "crime_traffic_yellow",
            WidthRequest = TrafficLightSize,
            HeightRequest = TrafficLightSize,
            Aspect = Aspect.AspectFit,
            Opacity = isYellow ? 1.0 : 0.25
        });
        trafficLight.Children.Add(new Image
        {
            Source = "crime_traffic_red",
            WidthRequest = TrafficLightSize,
            HeightRequest = TrafficLightSize,
            Aspect = Aspect.AspectFit,
            Opacity = isRed ? 1.0 : 0.25
        });
        middleStack.Children.Add(trafficLight);

        // زر التنفيذ
        var buttonBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            HeightRequest = ButtonHeight,
            WidthRequest = ButtonWidth,
            HorizontalOptions = LayoutOptions.Center,
            Padding = 0,
            BackgroundColor = Colors.Transparent
        };
        var btnGrid = new Grid();
        btnGrid.Add(new Image
        {
            Source = isExecutable ? "button_background.png" : "card_background.png",
            Aspect = Aspect.Fill
        });
        btnGrid.Add(new Label
        {
            Text = isUnlocked ? "تنفيذ" : "مقفول 🔒",
            TextColor = !isUnlocked ? Color.FromArgb("#e74c3c") : Colors.Black,
            FontSize = FontButton,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        });
        buttonBorder.Content = btnGrid;

        if (isExecutable)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) =>
            {
                await AnimateBorder(buttonBorder);
                await ExecuteCrime(crime);
            };
            buttonBorder.GestureRecognizers.Add(tap);
        }
        middleStack.Children.Add(buttonBorder);
        mainContentGrid.Add(middleStack, 1);

        // صورة الأداة + العدد (يسار)
        var toolStack = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start
        };
        toolStack.Children.Add(new Image
        {
            Source = toolImage,
            Aspect = Aspect.AspectFit,
            WidthRequest = ToolImageSize,
            HeightRequest = ToolImageSize,
            HorizontalOptions = LayoutOptions.Center
        });
        if (crime.ToolRequirements?.Count > 0)
        {
            bool hasEnough = ownedToolCount >= requiredToolCount;
            string toolName = crime.ToolRequirements[0].ToolName;
            toolStack.Children.Add(new Label
            {
                Text = toolName,
                TextColor = Colors.Goldenrod,
                FontSize = FontLabel,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
            toolStack.Children.Add(new Label
            {
                Text = $"{ownedToolCount}",
                TextColor = hasEnough ? Colors.Goldenrod : Colors.DarkRed,
                FontSize = FontLabel,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }
        mainContentGrid.Add(toolStack, 2);
        contentStack.Children.Add(mainContentGrid);

        // الحقول الثلاثة
        var infoGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = ScreenWidth * 0.22 },
                new ColumnDefinition { Width = ScreenWidth * 0.32 },
                new ColumnDefinition { Width = ScreenWidth * 0.22 }
            },
            ColumnSpacing = CardHeight * 0.09,
            HorizontalOptions = LayoutOptions.Center
        };
        infoGrid.Add(MakeInfoBox($"{crime.CourageCost}", "icon_courage"), 0);
        infoGrid.Add(MakeInfoBox($"{NumberFormatter.FormatNumber(crime.Reward.CashRewardMin)}-{NumberFormatter.FormatNumber(crime.Reward.CashRewardMax)}", "icon_cash"), 1);
        infoGrid.Add(MakeInfoBox($"{crime.Reward.ExperienceReward}", "icon_exp"), 2);

        contentStack.Children.Add(infoGrid);
        mainGrid.Add(contentStack);
        mainBorder.Content = mainGrid;
        return mainBorder;
    }

    private Border MakeInfoBox(string value, string iconImage)
    {
        var box = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(0),
            HeightRequest = InfoBoxHeight,
            HorizontalOptions = LayoutOptions.Fill
        };
        var mainGrid = new Grid();
        mainGrid.Add(new Image { Source = "card_background", Aspect = Aspect.Fill });

        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = InfoBoxHeight * 0.8 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };
        contentGrid.Add(new Image
        {
            Source = iconImage,
            WidthRequest = InfoBoxHeight * 0.8,
            HeightRequest = InfoBoxHeight * 0.8,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(15, 0, 0, 0)
        }, 0);
        contentGrid.Add(new Label
        {
            Text = value,
            TextColor = Colors.Goldenrod,
            FontSize = FontValue,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        }, 1);

        mainGrid.Add(contentGrid);
        box.Content = mainGrid;
        return box;
    }

    // ─────────────────────────────────────────────
    //  تنفيذ الجريمة
    // ─────────────────────────────────────────────
    private async Task ExecuteCrime(CrimeItemDefinition crime)
    {
        var (success, message) = CrimeService.AttemptCrime(_player, _crimeType, crime.CrimeItemId);
        LoadCrimeData();

        if (success)
        {
            bool executeAgain = await PopupService.ShowConfirmAsync(
                title: "نجاح!",
                message: message,
                operationType: PopupOperationType.Confirm,
                overridePositiveText: "تنفيذ",
                overrideNegativeText: "إلغاء"
            );

            if (executeAgain)
                await ExecuteCrime(crime);
        }
        // عند الفشل: CrimeService يفتح السجن/المستشفى تلقائياً
        // والرسالة ستظهر هناك
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border b) await AnimateBorder(b);
        await Navigation.PopAsync();
    }

    private async Task AnimateBorder(Border border)
    {
        if (border == null) return;
        await border.ScaleTo(0.95, 100, Easing.CubicIn);
        await border.ScaleTo(1.0, 100, Easing.CubicOut);
    }
}