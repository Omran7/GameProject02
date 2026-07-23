using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class MarketPage : ContentPage
{
    private readonly int _categoryId;
    private readonly int _subCategoryId;
    private readonly string _categoryName;
    private readonly string _subCategoryName;

    private PlayerAccount _player;
    private List<MarketItem> _currentItems = new();
    private Dictionary<string, Label> _timerLabels = new();
    private bool _isTimerRunning;

    public string ArmingSlotType { get; set; } = string.Empty;

    public MarketPage(int categoryId, int subCategoryId, string categoryName, string subCategoryName)
    {
        InitializeComponent();
        _categoryId = categoryId;
        _subCategoryId = subCategoryId;
        _categoryName = categoryName;
        _subCategoryName = subCategoryName;
    }

    public MarketPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        LoadItems();
        SetupFooter();
        StartGlobalTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isTimerRunning = false;
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

    private void LoadItems()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        string title = !string.IsNullOrEmpty(_subCategoryName)
            ? $"{_categoryName} - {_subCategoryName}"
            : _categoryName;
        PageHeader.HeaderTitle = title;

        _currentItems = MarketService.GetItemsByCategory(_categoryId, _subCategoryId);
        _timerLabels.Clear();

        ItemsContainer.Children.Clear();

        if (_currentItems.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "📦 لا توجد عناصر في هذا القسم",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeMedium,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };
            ItemsContainer.Children.Add(emptyLabel);
            return;
        }

        foreach (var item in _currentItems)
        {
            var card = CreateItemCard(item);
            ItemsContainer.Children.Add(card);
        }
    }

    private Border CreateItemCard(MarketItem item)
    {
        double imageSize = EstateUIConstants.ImageSize;
        double buttonWidth = EstateUIConstants.ButtonWidth;
        double buttonHeight = EstateUIConstants.ButtonHeight;

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

        var mainGrid = new Grid();
        mainGrid.Add(new Image
        {
            Source = "card_background.png",
            Aspect = Aspect.Fill,
            Opacity = 1
        });

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

        // صورة العنصر
        var imageBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };
        imageBorder.Content = new Image
        {
            Source = item.ImageResource,
            Aspect = Aspect.AspectFill,
            WidthRequest = imageSize,
            HeightRequest = imageSize
        };
        contentGrid.Add(imageBorder, 0);

        // تفاصيل العنصر - جميع النصوص في المنتصف
        var detailsStack = new VerticalStackLayout
        {
            Spacing = EstateUIConstants.StackSpacing,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        // اسم العنصر
        var nameLabel = new Label
        {
            Text = item.Name,
            Style = (Style)Application.Current.Resources["CardTitle"],
            FontSize = EstateUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };
        detailsStack.Children.Add(nameLabel);

        // الإحصائيات (إذا كانت متوفرة)
        if (item.IsWeapon)
        {
            detailsStack.Children.Add(new Label
            {
                Text = $"⚔️ ضرر: {item.Damage}  |  🎯 دقة: {item.Accuracy}",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Color.FromArgb("#ff6b6b"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }
        else if (item.CategoryType == 1) // درع
        {
            detailsStack.Children.Add(new Label
            {
                Text = $"🛡️ دفاع: {item.Defense}  |  💨 تفادي: {item.Evasion}",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Color.FromArgb("#3498db"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }

        // المخزون أو مؤقت التجديد
        if (item.CurrentStock > 0)
        {
            detailsStack.Children.Add(new Label
            {
                Text = $"📦 المخزون: {item.CurrentStock}",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }
        else
        {
            var timerLabel = new Label
            {
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.DarkRed,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            UpdateTimerLabel(timerLabel, item);
            detailsStack.Children.Add(timerLabel);
            _timerLabels[item.ItemId] = timerLabel;
        }

        // السعر
        detailsStack.Children.Add(new Label
        {
            Text = $"💰 السعر: {NumberFormatter.FormatNumber(item.PriceGold)} ذهب",
            Style = (Style)Application.Current.Resources["CardDescription"],
            FontSize = EstateUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        });

        contentGrid.Add(detailsStack, 1);

        // زر "شراء"
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
            BackgroundColor = Colors.Transparent,
            IsEnabled = item.CurrentStock > 0
        };

        var buttonGrid = new Grid();
        buttonGrid.Add(new Image
        {
            Source = "button_background.png",
            Aspect = Aspect.Fill
        });
        buttonGrid.Add(new Label
        {
            Text = item.CurrentStock > 0 ? "شراء" : "نفذ",
            Style = (Style)Application.Current.Resources["ActionButton"],
            FontSize = EstateUIConstants.FontSizeButton,
            TextColor = item.CurrentStock > 0 ? EstateUIConstants.TextDark : Colors.Gray
        });

        buttonBorder.Content = buttonGrid;

        if (item.CurrentStock > 0)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) =>
            {
                await AnimateBorder(buttonBorder);
                await OnBuyItemClicked(item);
            };
            buttonBorder.GestureRecognizers.Add(tap);
        }

        contentGrid.Add(buttonBorder, 2);

        mainGrid.Add(contentGrid);
        mainBorder.Content = mainGrid;

        return mainBorder;
    }

    private void UpdateTimerLabel(Label label, MarketItem item)
    {
        var nextRestock = MarketService.GetNextRestockTime(item);
        var timeRemaining = nextRestock - DateTime.UtcNow;

        if (timeRemaining.TotalSeconds <= 0)
        {
            label.Text = "⏳ جاري التحديث...";
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(10); // سريع جداً
                LoadItems();
            });
            return;
        }

        string timeText;
        if (timeRemaining.TotalHours >= 1)
            timeText = $"⏳ {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
        else
            timeText = $"⏳ {timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";

        label.Text = timeText;
    }

    private void StartGlobalTimer()
    {
        _isTimerRunning = true;
        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!_isTimerRunning) return false;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var kvp in _timerLabels)
                {
                    var item = _currentItems.Find(i => i.ItemId == kvp.Key);
                    if (item != null && item.CurrentStock == 0)
                        UpdateTimerLabel(kvp.Value, item);
                }
            });
            return _isTimerRunning;
        });
    }

    private async Task OnBuyItemClicked(MarketItem item)
    {
        if (item.CurrentStock <= 0)
        {
            await ToastService.Show("هذا العنصر غير متوفر حالياً", ToastType.Error);
            return;
        }

        var (confirmed, quantity) = await StockPopupService.ShowMarketBuyPopupAsync(item, _player.Gold);
        if (confirmed)
        {
            await ProcessPurchase(item, quantity);
        }
    }

    private async Task ProcessPurchase(MarketItem item, int quantity)
    {
        long totalCost = item.PriceGold * quantity;
        if (_player.Gold < totalCost)
        {
            await ToastService.Show("الذهب غير كافي", ToastType.Error);
            return;
        }

        bool success = MarketService.TryPurchaseItem(item.ItemId, quantity, out int newStock);
        if (!success)
        {
            await ToastService.Show("الكمية المطلوبة غير متوفرة!", ToastType.Error);
            return;
        }

        _player.Gold -= (int)totalCost;

        if (!_player.StockObject.ItemsInStock.ContainsKey(item.ItemId))
        {
            _player.StockObject.ItemsInStock[item.ItemId] = new StockItem
            {
                ItemId = item.ItemId,
                Name = item.Name,
                ImageResource = item.ImageResource,
                Count = quantity,
                OriginalPrice = (int)item.PriceGold,
                CountInBag = 0,
                IsLocked = false,
                CategoryId = item.CategoryType,
                Damage = item.Damage,
                Accuracy = item.Accuracy,
                Defense = item.Defense,
                Evasion = item.Evasion,
                IsWeapon = item.IsWeapon,
                IsGun = item.IsGun,
                GunType = item.GunType
            };
            _player.StockObject.StockFreeSpace -= quantity;
        }
        else
        {
            _player.StockObject.ItemsInStock[item.ItemId].Count += quantity;
            _player.StockObject.StockFreeSpace -= quantity;
        }

        item.CurrentStock = newStock;
        LoadItems(); // ✅ تحديث الواجهة فوراً قبل الإشعار
        await ToastService.Show($"تم شراء {quantity} × {item.Name}", ToastType.Success);
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

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopAsync(false);
    }
}