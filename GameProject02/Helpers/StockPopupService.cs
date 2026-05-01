using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Threading.Tasks;

namespace GameProject02.Services;

public static class StockPopupService
{
    #region 📦 الدالة الأساسية (تصميم 3 أجزاء - محتوى متوسّط)

    private static async Task<T?> ShowPopupAsync<T>(
        string title,
        View content,
        Func<TaskCompletionSource<T?>, Task> configureAndWait)
    {
        var tcs = new TaskCompletionSource<T?>();
        var activePage = GetActivePage();
        if (activePage == null) { tcs.TrySetResult(default); return default; }

        Grid? rootGrid = activePage.Content as Grid;
        if (rootGrid == null) { tcs.TrySetResult(default); return default; }

        double popupWidth = StockUIConstants.PopupWidth;
        double maxPopupHeight = StockUIConstants.PopupMaxHeight;
        double topHeight = Math.Max(40, popupWidth * 0.12);
        double bottomHeight = Math.Max(35, popupWidth * 0.10);

        var overlay = new BoxView
        {
            BackgroundColor = Colors.Black.WithAlpha(0.7f),
            Opacity = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        overlay.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { }) });

        var popupBorder = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupContainer"],
            WidthRequest = popupWidth,
            MaximumHeightRequest = maxPopupHeight,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Opacity = 0,
            Scale = 0.8
        };

        var popupContainer = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = topHeight },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = bottomHeight }
            }
        };

        // الصف العلوي
        var topGrid = new Grid();
        topGrid.Add(new Image { Source = "popup_top", Aspect = Aspect.Fill });
        topGrid.Add(new Label
        {
            Text = title,
            Style = (Style)Application.Current.Resources["StockPopupTitle"]
        });
        popupContainer.Add(topGrid, 0, 0);

        // الصف الأوسط
        var middleGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            VerticalOptions = LayoutOptions.Start
        };
        var middleBg = new Image { Source = "popup_middle", Aspect = Aspect.Fill };
        Grid.SetRowSpan(middleBg, 2);
        middleGrid.Add(middleBg);

        var contentWrapper = new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Spacing = 0,
            Children = { content }
        };

        var scrollView = new ScrollView
        {
            Content = contentWrapper,
            Padding = new Thickness(20, 10, 20, 0),
            MaximumHeightRequest = maxPopupHeight - topHeight - bottomHeight - 60,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
        };
        middleGrid.Add(scrollView, 0, 0);

        await configureAndWait(tcs);

        popupContainer.Add(middleGrid, 0, 1);

        // الصف السفلي
        var bottomGrid = new Grid();
        bottomGrid.Add(new Image { Source = "popup_bottom", Aspect = Aspect.Fill });
        popupContainer.Add(bottomGrid, 0, 2);

        popupBorder.Content = popupContainer;

        rootGrid.Add(overlay);
        Grid.SetRowSpan(overlay, rootGrid.RowDefinitions.Count > 0 ? rootGrid.RowDefinitions.Count : 1);
        Grid.SetColumnSpan(overlay, rootGrid.ColumnDefinitions.Count > 0 ? rootGrid.ColumnDefinitions.Count : 1);
        rootGrid.Add(popupBorder);
        Grid.SetRowSpan(popupBorder, rootGrid.RowDefinitions.Count > 0 ? rootGrid.RowDefinitions.Count : 1);
        Grid.SetColumnSpan(popupBorder, rootGrid.ColumnDefinitions.Count > 0 ? rootGrid.ColumnDefinitions.Count : 1);

        overlay.IsVisible = true;
        popupBorder.IsVisible = true;
        await Task.WhenAll(
            overlay.FadeTo(0.7, 200),
            popupBorder.FadeTo(1, 200),
            popupBorder.ScaleTo(1, 200, Easing.CubicOut)
        );

        var result = await tcs.Task;

        await Task.WhenAll(
            overlay.FadeTo(0, 200),
            popupBorder.FadeTo(0, 200),
            popupBorder.ScaleTo(0.8, 200, Easing.CubicIn)
        );

        rootGrid.Children.Remove(overlay);
        rootGrid.Children.Remove(popupBorder);

        return result;
    }

    private static ContentPage? GetActivePage()
    {
        var page = Application.Current?.MainPage;
        if (page is NavigationPage nav) return nav.CurrentPage as ContentPage;
        return page as ContentPage;
    }

    #endregion

    #region 🔘 أزرار النوافذ (موحدة)

    private static View CreatePopupButtons(
        string confirmText,
        string? cancelText,
        Action onConfirm,
        Action? onCancel,
        string confirmImage = "button_background.png",
        string cancelImage = "button_background_no.png")
    {
        var layout = new HorizontalStackLayout
        {
            Spacing = 20,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 15, 0, 0)
        };

        var confirmBtn = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupButton"]
        };
        var confirmGrid = new Grid();
        confirmGrid.Add(new Image { Source = confirmImage, Aspect = Aspect.Fill });
        confirmGrid.Add(new Label
        {
            Text = confirmText,
            Style = (Style)Application.Current.Resources["StockPopupButtonText"]
        });
        confirmBtn.Content = confirmGrid;
        confirmBtn.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await confirmBtn.ScaleTo(StockUIConstants.AnimationPressScale, 50);
                await confirmBtn.ScaleTo(1.0, 50);
                onConfirm();
            })
        });
        layout.Children.Add(confirmBtn);

        if (!string.IsNullOrEmpty(cancelText) && onCancel != null)
        {
            var cancelBtn = new Border
            {
                Style = (Style)Application.Current.Resources["StockPopupButton"]
            };
            var cancelGrid = new Grid();
            cancelGrid.Add(new Image { Source = cancelImage, Aspect = Aspect.Fill });
            cancelGrid.Add(new Label
            {
                Text = cancelText,
                Style = (Style)Application.Current.Resources["StockPopupButtonText"]
            });
            cancelBtn.Content = cancelGrid;
            cancelBtn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await cancelBtn.ScaleTo(StockUIConstants.AnimationPressScale, 50);
                    await cancelBtn.ScaleTo(1.0, 50);
                    onCancel();
                })
            });
            layout.Children.Add(cancelBtn);
        }

        return layout;
    }
    #endregion

    #region 🛒 1. نافذة بيع العنصر

    public static async Task<(bool confirmed, int amount)> ShowSellPopupAsync(StockItem item)
    {
        int selectedAmount = 1;

        var content = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center
        };

        var imgFrame = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupImageFrame"],
            WidthRequest = StockUIConstants.PopupImageSize,
            HeightRequest = StockUIConstants.PopupImageSize,
            HorizontalOptions = LayoutOptions.Center
        };
        imgFrame.Content = new Image { Source = item.ImageResource, Aspect = Aspect.AspectFill };
        content.Children.Add(imgFrame);

        var priceBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        var priceGrid = new Grid();
        priceGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });
        var priceLabel = new Label
        {
            Text = $"السعر: {item.GetSellPrice():N0}",
            Style = (Style)Application.Current.Resources["StockPopupMessage"],
            FontSize = StockUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -3
        };
        priceGrid.Add(priceLabel);
        priceBorder.Content = priceGrid;
        content.Children.Add(priceBorder);

        var availableBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        var availableGrid = new Grid();
        availableGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });
        var availableLabel = new Label
        {
            Text = $"العدد بالمخزن: {item.Count}",
            Style = (Style)Application.Current.Resources["StockPopupMessage"],
            FontSize = StockUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -3
        };
        availableGrid.Add(availableLabel);
        availableBorder.Content = availableGrid;
        content.Children.Add(availableBorder);

        var entryBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        var entryGrid = new Grid();
        entryGrid.Add(new Image { Source = "input_field_bg.png", Aspect = Aspect.Fill });

        var entry = new Entry
        {
            Text = "1",
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.Center,
            Style = (Style)Application.Current.Resources["StockPopupEntry"]
        };

        var slider = new CustomSlider
        {
            Minimum = 0,
            Maximum = item.Count,
            Value = 1,
            Style = (Style)Application.Current.Resources["StockPopupSlider"],
            HorizontalOptions = LayoutOptions.Center
        };

        entry.TextChanged += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.NewTextValue)) return;
            if (int.TryParse(e.NewTextValue, out int val))
            {
                if (val > item.Count) MainThread.BeginInvokeOnMainThread(() => entry.Text = item.Count.ToString());
                else if (val < 1) MainThread.BeginInvokeOnMainThread(() => entry.Text = "1");
                else { selectedAmount = val; slider.Value = val; priceLabel.Text = $"السعر: {(item.GetSellPrice() * val):N0}"; }
            }
            else MainThread.BeginInvokeOnMainThread(() => entry.Text = "1");
        };

        slider.ValueChanged += (s, e) =>
        {
            selectedAmount = (int)e.NewValue;
            if (selectedAmount == item.Count - 1 && Math.Abs(e.NewValue - slider.Maximum) < 0.01) selectedAmount = item.Count;
            if (selectedAmount < 1) selectedAmount = 1;
            entry.Text = selectedAmount.ToString();
            priceLabel.Text = $"السعر: {(item.GetSellPrice() * selectedAmount):N0}";
        };

        entryGrid.Add(entry);
        entryBorder.Content = entryGrid;
        content.Children.Add(entryBorder);
        content.Children.Add(slider);

        return await ShowPopupAsync<(bool, int)>("تأكيد البيع", content, async (tcs) =>
        {
            var buttons = CreatePopupButtons("نعم", "لا",
                onConfirm: () => tcs.TrySetResult((true, selectedAmount)),
                onCancel: () => tcs.TrySetResult((false, 0))
            );
            content.Children.Add(buttons);
            await Task.CompletedTask;
        });
    }
    #endregion

    #region 🎒 2. نافذة إضافة للحقيبة

    public static async Task<(bool confirmed, int amount)> ShowAddToBagPopupAsync(StockItem item, int bagFreeSpace)
    {
        int maxQuantity = Math.Min(item.Count, bagFreeSpace);
        if (maxQuantity <= 0)
            return (false, 0);

        int selectedAmount = 1;

        var content = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center
        };

        // صورة العنصر
        var imgFrame = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupImageFrame"],
            WidthRequest = StockUIConstants.PopupImageSize,
            HeightRequest = StockUIConstants.PopupImageSize,
            HorizontalOptions = LayoutOptions.Center
        };
        imgFrame.Content = new Image { Source = item.ImageResource, Aspect = Aspect.AspectFill };
        content.Children.Add(imgFrame);

        // ========== المتاح (مع صورة خلفية) ==========
        var availableBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        var availableGrid = new Grid();
        availableGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });
        var availableLabel = new Label
        {
            Text = $"المتاح: {item.Count}",
            Style = (Style)Application.Current.Resources["StockPopupMessage"],
            FontSize = StockUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -3
        };
        availableGrid.Add(availableLabel);
        availableBorder.Content = availableGrid;
        content.Children.Add(availableBorder);

        // ========== مساحة الحقيبة (مع صورة خلفية) ==========
        var bagSpaceBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        var bagSpaceGrid = new Grid();
        bagSpaceGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });
        var bagSpaceLabel = new Label
        {
            Text = $"مساحة الحقيبة: {bagFreeSpace}",
            Style = (Style)Application.Current.Resources["StockPopupMessage"],
            FontSize = StockUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -3
        };
        bagSpaceGrid.Add(bagSpaceLabel);
        bagSpaceBorder.Content = bagSpaceGrid;
        content.Children.Add(bagSpaceBorder);

        // حقل إدخال مع صورة خلفية
        var entryBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)

        };
        var entryGrid = new Grid();
        entryGrid.Add(new Image { Source = "input_field_bg.png", Aspect = Aspect.Fill });
        var entry = new Entry
        {
            Text = "1",
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.Center,
            Style = (Style)Application.Current.Resources["StockPopupEntry"]
        };
        entryGrid.Add(entry);
        entryBorder.Content = entryGrid;
        content.Children.Add(entryBorder);

        // السلايدر
        var slider = new CustomSlider
        {
            Minimum = 0,
            Maximum = maxQuantity,
            Value = 1,
            Style = (Style)Application.Current.Resources["StockPopupSlider"],
            HorizontalOptions = LayoutOptions.Center
        };
        content.Children.Add(slider);

        // ربط الإدخال بالسلايدر
        entry.TextChanged += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.NewTextValue)) return;
            if (int.TryParse(e.NewTextValue, out int val))
            {
                if (val > maxQuantity)
                    MainThread.BeginInvokeOnMainThread(() => entry.Text = maxQuantity.ToString());
                else if (val < 1)
                    MainThread.BeginInvokeOnMainThread(() => entry.Text = "1");
                else
                {
                    selectedAmount = val;
                    slider.Value = val;
                }
            }
            else
                MainThread.BeginInvokeOnMainThread(() => entry.Text = "1");
        };

        slider.ValueChanged += (s, e) =>
        {
            selectedAmount = (int)e.NewValue;
            if (selectedAmount == maxQuantity - 1 && Math.Abs(e.NewValue - slider.Maximum) < 0.01)
                selectedAmount = maxQuantity;
            if (selectedAmount < 1) selectedAmount = 1;
            entry.Text = selectedAmount.ToString();
        };

        return await ShowPopupAsync<(bool, int)>("إضافة للحقيبة", content, async (tcs) =>
        {
            var buttons = CreatePopupButtons("إضافة", "إلغاء",
                onConfirm: () => tcs.TrySetResult((true, selectedAmount)),
                onCancel: () => tcs.TrySetResult((false, 0))
            );
            content.Children.Add(buttons);
            await Task.CompletedTask;
        });
    }

    #endregion
    #region 🔍 3. نافذة تفاصيل العنصر

    public static async Task ShowItemDetailsPopupAsync(StockItem item)
    {
        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(15, 0, 15, 0)
        };

        var textStack = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        textStack.Children.Add(new Label
        {
            Text = item.Name,
            Style = (Style)Application.Current.Resources["StockCardTitle"],
            FontSize = StockUIConstants.FontSizeLarge,
            HorizontalOptions = LayoutOptions.Center
        });

        if (item.IsWeapon)
        {
            textStack.Children.Add(new Label
            {
                Text = $"ضرر: {item.Damage} | دقه: {item.Accuracy}",
                Style = (Style)Application.Current.Resources["StockPopupMessage"],
                FontSize = StockUIConstants.FontSizeSmall,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = StockUIConstants.TextRed
            });
        }
        else if (item.CategoryId == 1)
        {
            textStack.Children.Add(new Label
            {
                Text = $"دفاع: {item.Defense} | تفادي: {item.Evasion}",
                Style = (Style)Application.Current.Resources["StockPopupMessage"],
                FontSize = StockUIConstants.FontSizeSmall,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = StockUIConstants.BorderBlue
            });
        }

        if (!string.IsNullOrEmpty(item.Description))
        {
            textStack.Children.Add(new Label
            {
                Text = item.Description,
                Style = (Style)Application.Current.Resources["StockPopupMessage"],
                FontSize = StockUIConstants.FontSizeSmall,
                TextColor = Colors.LightGray,
                HorizontalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap
            });
        }

        contentGrid.Add(textStack, 0, 0);

        var imageBorder = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupImageFrame"],
            WidthRequest = StockUIConstants.PopupImageSize,
            HeightRequest = StockUIConstants.PopupImageSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };
        imageBorder.Content = new Image { Source = item.ImageResource, Aspect = Aspect.AspectFill };
        contentGrid.Add(imageBorder, 1, 0);

        var mainLayout = new VerticalStackLayout { Spacing = 10, Children = { contentGrid } };

        await ShowPopupAsync<object>("تفاصيل العنصر", mainLayout, async (tcs) =>
        {
            var buttons = CreatePopupButtons("إغلاق", null, () => tcs.TrySetResult(null), null);
            mainLayout.Children.Add(buttons);
            await Task.CompletedTask;
        });
    }
    #endregion

    #region 🏪 4. نافذة إضافة للمتجر (وتعديل أيضاً)

    public static async Task<(bool confirmed, int quantity, int price)> ShowShopAddPopupAsync(
        StockItem item,
        int availableQuantity,
        int currentQuantity = 1,
        int currentPrice = 0)
    {
        int maxQuantity = Math.Min(availableQuantity, 25);
        if (maxQuantity <= 0) return (false, 0, 0);

        int selectedQuantity = currentQuantity;
        int selectedPrice = currentPrice > 0 ? currentPrice : Math.Max(100, (int)item.OriginalPrice * 2);

        var content = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Center };

        // صورة العنصر
        var imgFrame = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupImageFrame"],
            WidthRequest = StockUIConstants.PopupImageSize,
            HeightRequest = StockUIConstants.PopupImageSize,
            HorizontalOptions = LayoutOptions.Center
        };
        imgFrame.Content = new Image { Source = item.ImageResource, Aspect = Aspect.AspectFill };
        content.Children.Add(imgFrame);

        // قسم السعر
        var priceTitleBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        var priceTitleGrid = new Grid();
        priceTitleGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });
        var priceTitleLabel = new Label
        {
            Text = "حدد سعر القطعه",
            Style = (Style)Application.Current.Resources["StockCardCount"],
            FontSize = StockUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -2
        };
        priceTitleGrid.Add(priceTitleLabel);
        priceTitleBorder.Content = priceTitleGrid;
        content.Children.Add(priceTitleBorder);

        var priceEntryBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center
        };
        var priceEntryGrid = new Grid();
        priceEntryGrid.Add(new Image { Source = "input_field_bg.png", Aspect = Aspect.Fill });
        var priceEntry = new Entry
        {
            Text = selectedPrice.ToString(),
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.Center,
            Style = (Style)Application.Current.Resources["StockPopupEntry"],
            MaxLength = 9
        };
        priceEntryGrid.Add(priceEntry);
        priceEntryBorder.Content = priceEntryGrid;
        content.Children.Add(priceEntryBorder);

        // قسم الكمية
        var qtyTitleBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 15, 0, 0)
        };
        var qtyTitleGrid = new Grid();
        qtyTitleGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });
        var qtyTitleLabel = new Label
        {
            Text = "حدد الكميه (ماكس 25)",
            Style = (Style)Application.Current.Resources["StockCardCount"],
            FontSize = StockUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -2
        };
        qtyTitleGrid.Add(qtyTitleLabel);
        qtyTitleBorder.Content = qtyTitleGrid;
        content.Children.Add(qtyTitleBorder);

        var qtyEntryBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center
        };
        var qtyEntryGrid = new Grid();
        qtyEntryGrid.Add(new Image { Source = "input_field_bg.png", Aspect = Aspect.Fill });
        var qtyEntry = new Entry
        {
            Text = selectedQuantity.ToString(),
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.Center,
            Style = (Style)Application.Current.Resources["StockPopupEntry"]
        };
        qtyEntryGrid.Add(qtyEntry);
        qtyEntryBorder.Content = qtyEntryGrid;
        content.Children.Add(qtyEntryBorder);

        var slider = new CustomSlider
        {
            Minimum = 0,
            Maximum = maxQuantity,
            Value = selectedQuantity,
            Style = (Style)Application.Current.Resources["StockPopupSlider"],
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        content.Children.Add(slider);

        // --- ربط الأحداث مع تجاهل أول تغيير ---
        int priceEntryChangeCount = 0;
        int qtyEntryChangeCount = 0;
        int sliderChangeCount = 0;

        priceEntry.TextChanged += (s, e) =>
        {
            priceEntryChangeCount++;
            if (priceEntryChangeCount == 1) return;
            if (string.IsNullOrEmpty(e.NewTextValue)) return;
            if (int.TryParse(e.NewTextValue, out int val))
            {
                if (val > 999999999) val = 999999999;
                if (val < 1) val = 1;
                selectedPrice = val;
            }
            else
                selectedPrice = 1;
            priceEntry.Text = selectedPrice.ToString();
        };

        qtyEntry.TextChanged += (s, e) =>
        {
            qtyEntryChangeCount++;
            if (qtyEntryChangeCount == 1) return;
            if (string.IsNullOrEmpty(e.NewTextValue)) return;
            if (int.TryParse(e.NewTextValue, out int val))
            {
                if (val > maxQuantity) val = maxQuantity;
                if (val < 1) val = 1;
                selectedQuantity = val;
                slider.Value = val;
            }
            else
                selectedQuantity = 1;
            qtyEntry.Text = selectedQuantity.ToString();
        };

        slider.ValueChanged += (s, e) =>
        {
            sliderChangeCount++;
            if (sliderChangeCount == 1) return;
            int val = (int)e.NewValue;
            if (val > maxQuantity) val = maxQuantity;
            if (val < 1) val = 1;
            selectedQuantity = val;
            qtyEntry.Text = val.ToString();
        };

        // ✅ إصلاح السلايدر: بعد إضافة جميع العناصر، نضمن مطابقة السلايدر للرقم
        Application.Current?.Dispatcher.Dispatch(() =>
        {
            slider.Value = selectedQuantity;
            qtyEntry.Text = selectedQuantity.ToString();
        });

        return await ShowPopupAsync<(bool, int, int)>("إضافة للمتجر", content, async (tcs) =>
        {
            var buttons = CreatePopupButtons("تأكيد", "إلغاء",
                onConfirm: () => tcs.TrySetResult((true, selectedQuantity, selectedPrice)),
                onCancel: () => tcs.TrySetResult((false, 0, 0))
            );
            content.Children.Add(buttons);
            await Task.CompletedTask;
        });
    }

    #endregion



    #region 🏛️ 5. نافذة إضافة للمتحف

    public static async Task<bool> ShowMuseumAddPopupAsync(StockItem item)
    {
        var content = new VerticalStackLayout { Spacing = 12, HorizontalOptions = LayoutOptions.Center };

        var imgFrame = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupImageFrame"],
            WidthRequest = StockUIConstants.PopupImageSize,
            HeightRequest = StockUIConstants.PopupImageSize,
            HorizontalOptions = LayoutOptions.Center
        };
        imgFrame.Content = new Image { Source = item.ImageResource, Aspect = Aspect.AspectFill };
        content.Children.Add(imgFrame);

        content.Children.Add(new Label
        {
            Text = item.Name,
            Style = (Style)Application.Current.Resources["StockPopupTitle"],
            FontSize = StockUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center
        });

        content.Children.Add(new Label
        {
            Text = "هل تريد عرض هذا العنصر في المتحف؟",
            Style = (Style)Application.Current.Resources["StockPopupMessage"],
            HorizontalOptions = LayoutOptions.Center
        });

        return await ShowPopupAsync<bool>("تأكيد العرض", content, async (tcs) =>
        {
            var buttons = CreatePopupButtons("عرض", "إلغاء",
                onConfirm: () => tcs.TrySetResult(true),
                onCancel: () => tcs.TrySetResult(false)
            );
            content.Children.Add(buttons);
            await Task.CompletedTask;
        });
    }

    #endregion

    #region ✏️ 6. نافذة تعديل كمية الحقيبة (إضافة/إزالة)

    /// <summary>
    /// نافذة لتعديل كمية عنصر في الحقيبة (إضافة كمية من المخزن أو إزالة كمية من الحقيبة)
    /// </summary>
    /// <param name="item">العنصر</param>
    /// <param name="maxAddable">أقصى كمية يمكن إضافتها (من المخزن) – في حالة الإزالة يمكن تمرير 0</param>
    /// <param name="currentInBag">الكمية الحالية في الحقيبة</param>
    /// <param name="isEditMode">true = إضافة كمية، false = إزالة كمية</param>
    /// <returns>(confirmed, amount) حيث amount هي الكمية المطلوب إضافتها أو إزالتها</returns>
    public static async Task<(bool confirmed, int amount)> ShowEditQuantityPopupAsync(StockItem item, int maxAddable, int currentInBag, bool isEditMode)
    {
        int maxQuantity = isEditMode ? maxAddable : currentInBag;
        if (maxQuantity <= 0) return (false, 0);

        int selectedAmount = 1;
        string actionText = isEditMode ? "إضافة" : "إزالة";
        string title = isEditMode ? "إضافة إلى للحقيبة" : "إزالة من الحقيبة";

        var content = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center
        };

        // صورة العنصر
        var imgFrame = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupImageFrame"],
            WidthRequest = StockUIConstants.PopupImageSize,
            HeightRequest = StockUIConstants.PopupImageSize,
            HorizontalOptions = LayoutOptions.Center
        };
        imgFrame.Content = new Image { Source = item.ImageResource, Aspect = Aspect.AspectFill };
        content.Children.Add(imgFrame);

        // ========== الكمية في الحقيبة (مع صورة خلفية) ==========
        var bagQuantityBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        var bagQuantityGrid = new Grid();
        bagQuantityGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });
        var bagQuantityLabel = new Label
        {
            Text = $"الكمية في الحقيبة: {currentInBag}",
            Style = (Style)Application.Current.Resources["StockPopupMessage"],
            FontSize = StockUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -3
        };
        bagQuantityGrid.Add(bagQuantityLabel);
        bagQuantityBorder.Content = bagQuantityGrid;
        content.Children.Add(bagQuantityBorder);

        // حقل إدخال مع صورة خلفية
        var entryBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        var entryGrid = new Grid();
        entryGrid.Add(new Image { Source = "input_field_bg.png", Aspect = Aspect.Fill });
        var entry = new Entry
        {
            Text = "1",
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.Center,
            Style = (Style)Application.Current.Resources["StockPopupEntry"]
        };
        entryGrid.Add(entry);
        entryBorder.Content = entryGrid;
        content.Children.Add(entryBorder);

        // السلايدر
        var slider = new CustomSlider
        {
            Minimum = 0,
            Maximum = maxQuantity,
            Value = 1,
            Style = (Style)Application.Current.Resources["StockPopupSlider"],
            HorizontalOptions = LayoutOptions.Center
        };
        content.Children.Add(slider);

        // ربط الإدخال بالسلايدر
        entry.TextChanged += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.NewTextValue)) return;
            if (int.TryParse(e.NewTextValue, out int val))
            {
                if (val > maxQuantity)
                    MainThread.BeginInvokeOnMainThread(() => entry.Text = maxQuantity.ToString());
                else if (val < 1)
                    MainThread.BeginInvokeOnMainThread(() => entry.Text = "1");
                else
                {
                    selectedAmount = val;
                    slider.Value = val;
                }
            }
            else
                MainThread.BeginInvokeOnMainThread(() => entry.Text = "1");
        };

        slider.ValueChanged += (s, e) =>
        {
            selectedAmount = (int)e.NewValue;
            if (selectedAmount == maxQuantity - 1 && Math.Abs(e.NewValue - slider.Maximum) < 0.01)
                selectedAmount = maxQuantity;
            if (selectedAmount < 1) selectedAmount = 1;
            entry.Text = selectedAmount.ToString();
        };

        return await ShowPopupAsync<(bool, int)>(title, content, async (tcs) =>
        {
            var buttons = CreatePopupButtons(actionText, "إلغاء",
                onConfirm: () => tcs.TrySetResult((true, selectedAmount)),
                onCancel: () => tcs.TrySetResult((false, 0))
            );
            content.Children.Add(buttons);
            await Task.CompletedTask;
        });
    }

    #endregion


    #region 🛍️ 7. نافذة شراء من السوق (Market Buy)

    public static async Task<(bool confirmed, int quantity)> ShowMarketBuyPopupAsync(
        MarketItem item,
        int playerGold)
    {
        int maxQuantity = item.CurrentStock;
        if (maxQuantity <= 0)
            return (false, 0);

        int selectedQuantity = 1;
        long pricePerItem = item.PriceGold;

        var content = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center
        };

        // صورة العنصر
        var imgFrame = new Border
        {
            Style = (Style)Application.Current.Resources["StockPopupImageFrame"],
            WidthRequest = StockUIConstants.PopupImageSize,
            HeightRequest = StockUIConstants.PopupImageSize,
            HorizontalOptions = LayoutOptions.Center
        };
        imgFrame.Content = new Image { Source = item.ImageResource, Aspect = Aspect.AspectFill };
        content.Children.Add(imgFrame);

        // اسم العنصر
        content.Children.Add(new Label
        {
            Text = item.Name,
            Style = (Style)Application.Current.Resources["StockCardTitle"],
            FontSize = StockUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 5, 0, 0)
        });


        // الإجمالي
        var totalBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 15, 0, 5)
        };
        var totalGrid = new Grid();
        totalGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill });
        var totalLabel = new Label
        {
            Text = $"الإجمالي: {NumberFormatter.FormatNumber(pricePerItem)} ذهب",
            Style = (Style)Application.Current.Resources["StockPopupMessage"],
            FontSize = StockUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -3
        };
        totalGrid.Add(totalLabel);
        totalBorder.Content = totalGrid;
        content.Children.Add(totalBorder);

        // حقل إدخال الكمية
        var entryBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = StockUIConstants.EntryWidth,
            HeightRequest = StockUIConstants.EntryHeight,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        var entryGrid = new Grid();
        entryGrid.Add(new Image { Source = "input_field_bg.png", Aspect = Aspect.Fill });
        var entry = new Entry
        {
            Text = "1",
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.Center,
            Style = (Style)Application.Current.Resources["StockPopupEntry"]
        };
        entryGrid.Add(entry);
        entryBorder.Content = entryGrid;
        content.Children.Add(entryBorder);

        // السلايدر
        var slider = new CustomSlider
        {
            Minimum = 0,
            Maximum = maxQuantity,
            Value = 1,
            Style = (Style)Application.Current.Resources["StockPopupSlider"],
            HorizontalOptions = LayoutOptions.Center
        };
        content.Children.Add(slider);

        // تحديث الإجمالي عند تغيير الكمية
        entry.TextChanged += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.NewTextValue)) return;
            if (int.TryParse(e.NewTextValue, out int val))
            {
                if (val > maxQuantity)
                    MainThread.BeginInvokeOnMainThread(() => entry.Text = maxQuantity.ToString());
                else if (val < 1)
                    MainThread.BeginInvokeOnMainThread(() => entry.Text = "1");
                else
                {
                    selectedQuantity = val;
                    slider.Value = val;
                    totalLabel.Text = $"الإجمالي: {NumberFormatter.FormatNumber(pricePerItem * val)} ذهب";
                }
            }
            else
                MainThread.BeginInvokeOnMainThread(() => entry.Text = "1");
        };

        slider.ValueChanged += (s, e) =>
        {
            selectedQuantity = (int)e.NewValue;
            if (selectedQuantity == maxQuantity - 1 && Math.Abs(e.NewValue - slider.Maximum) < 0.01)
                selectedQuantity = maxQuantity;
            if (selectedQuantity < 1) selectedQuantity = 1;
            entry.Text = selectedQuantity.ToString();
            totalLabel.Text = $"الإجمالي: {NumberFormatter.FormatNumber(pricePerItem * selectedQuantity)} ذهب";
        };

        // التحقق من كفاية الذهب
        bool canAfford = playerGold >= pricePerItem * selectedQuantity;

        return await ShowPopupAsync<(bool, int)>("شراء", content, async (tcs) =>
        {
            var buttons = CreatePopupButtons(
                "شراء", "إلغاء",
                onConfirm: () =>
                {
                    if (!canAfford)
                        return;
                    tcs.TrySetResult((true, selectedQuantity));
                },
                onCancel: () => tcs.TrySetResult((false, 0)),
                confirmImage: canAfford ? "button_background.png" : "card_background.png"
            );
            content.Children.Add(buttons);
            await Task.CompletedTask;
        });
    }

    #endregion

}