using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class ShopPage : ContentPage
{
    #region المتغيرات
    private PlayerAccount _player;
    private ObservableCollection<ShopSpaceItemViewModel> _shopSpaces;
    private Border _activeTab;
    private Label _spacesLabel;
    private double _screenWidth;
    private double _screenHeight;
    #endregion

    #region Constructor
    public ShopPage()
    {
        InitializeComponent();
        BindingContext = this;

        _screenWidth = Application.Current.MainPage.Width;
        _screenHeight = Application.Current.MainPage.Height;

        _shopSpaces = new ObservableCollection<ShopSpaceItemViewModel>();
        ShopSpacesGrid.ItemTemplate = CreateCardTemplate();
        LoadShopData();
    }
    #endregion

    #region دورة الحياة
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadShopData();
        SetActiveTab(ShopTabButton, ShopTabLabel);
        SetupFooter();
    }

    private void SetupFooter()
    {
        var footerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },    // زر HOME (يمين)
                new ColumnDefinition { Width = GridLength.Star },    // المساحات (وسط)
                new ColumnDefinition { Width = GridLength.Auto }     // زر شراء مساحات (يسار)
            },
            ColumnSpacing = 10,
            Padding = new Thickness(25, 0),
            VerticalOptions = LayoutOptions.Center
        };

        // زر HOME (يمين)
        var homeButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnHomeClicked,
            buttonImageSource: "footer_button_back.png",
            horizontalOptions: LayoutOptions.Start
        );

        // مؤشر مساحات العرض (وسط)
        var spaceStack = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        var spaceTitle = new Label
        {
            Text = "مساحة المتجر",
            TextColor = Color.FromArgb("#000000"),
            FontFamily = "Cairo-Black",
            FontSize = 12,
            HorizontalOptions = LayoutOptions.Center,
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#80000000")),
                Offset = new Point(0, 2),
                Radius = 2,
                Opacity = 1.5f
            }
        };
        var spaceValue = new Label
        {
            TextColor = Colors.WhiteSmoke,
            FontFamily = "Cairo-Black",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#80000000")),
                Offset = new Point(0, 2),
                Radius = 2,
                Opacity = 1.5f
            }
        };
        spaceStack.Children.Add(spaceTitle);
        spaceStack.Children.Add(spaceValue);
        footerGrid.Add(spaceStack, 1, 0);
        _spacesLabel = spaceValue;

        // زر شراء مساحات (يسار)
        var buySpacesButton = PageFooter.CreateFooterButton(
            text: "مساحات",
            tappedHandler: OnBuySpacesClicked,
            buttonImageSource: "footer_button_add.png", // استخدم صورة مناسبة لشراء المساحات
            horizontalOptions: LayoutOptions.End
        );

        footerGrid.Add(homeButton, 2, 0);
        footerGrid.Add(buySpacesButton, 0, 0);

        PageFooter.SetCustomContent(footerGrid);
        UpdateSpacesLabel();
    }

    private void UpdateSpacesLabel()
    {
        if (_spacesLabel != null && _player != null)
        {
            _spacesLabel.Text = $"{_player.StockObject.ShopSpaces}/{_player.StockObject.MaxShopSpaces}";
        }
    }
    #endregion

    #region قالب البطاقة (بدون تغيير)
    private DataTemplate CreateCardTemplate()
    {
        return new DataTemplate(() =>
        {
            var border = new Border();
            border.Style = Application.Current.Resources["StockCard"] as Style;

            border.BindingContextChanged += (s, e) =>
            {
                var vm = border.BindingContext as ShopSpaceItemViewModel;
                if (vm != null)
                {
                    border.Content = BuildCardContent(vm);
                }
            };
            return border;
        });
    }

    private View BuildCardContent(ShopSpaceItemViewModel vm)
    {
        double imageSize = StockUIConstants.CardImageSize;
        double fontSizeSmall = StockUIConstants.FontSizeSmall;
        double fontSizeButton = StockUIConstants.FontSizeButton;

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            RowSpacing = 0
        };

        var bgImage = new Image { Source = "stock_card_bg.png", Aspect = Aspect.Fill };
        mainGrid.Add(bgImage);
        Grid.SetRowSpan(bgImage, 3);

        var titleGrid = new Grid
        {
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(StockUIConstants.TitleMarginHorizontal, StockUIConstants.TitleMarginTop, StockUIConstants.TitleMarginHorizontal, 0),
            HeightRequest = StockUIConstants.TitleBarHeight
        };
        var titleBgImage = new Image { Source = "card_titel_name.png", Aspect = Aspect.Fill };
        var titleLabel = new Label
        {
            Text = vm.ItemName,
            Style = (Style)Application.Current.Resources["StockCardTitle"],
            FontSize = StockUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -3
        };
        titleGrid.Children.Add(titleBgImage);
        titleGrid.Children.Add(titleLabel);
        mainGrid.Add(titleGrid, 0, 0);

        var infoGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = StockUIConstants.ColumnSpacing,
            Padding = new Thickness(StockUIConstants.ContentPadding, 0, StockUIConstants.ContentPadding, 0)
        };

        var textStack = new VerticalStackLayout
        {
            Spacing = StockUIConstants.StackSpacing,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        };
        textStack.Children.Add(new Label
        {
            Text = vm.QuantityText,
            Style = (Style)Application.Current.Resources["StockCardCount"],
            FontSize = StockUIConstants.FontSizeSmall
        });
        textStack.Children.Add(new Label
        {
            Text = vm.PriceText,
            Style = (Style)Application.Current.Resources["StockCardCount"],
            FontSize = StockUIConstants.FontSizeSmall
        });
        infoGrid.Add(textStack, 0, 0);

        var image = new Image
        {
            Source = vm.ImageResource,
            Aspect = Aspect.AspectFill,
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            Margin = new Thickness(0, StockUIConstants.ContentPadding / 2, StockUIConstants.ContentPadding / 2, StockUIConstants.ContentPadding / 2)
        };
        infoGrid.Add(image, 1, 0);
        mainGrid.Add(infoGrid, 0, 1);

        Grid buttonsGrid;
        if (vm.IsEmpty)
        {
            buttonsGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition() },
                Padding = new Thickness(StockUIConstants.ButtonsPadding * 4, 0, StockUIConstants.ButtonsPadding * 4, StockUIConstants.ButtonsPadding)
            };
            var addBtn = CreateActionButton("إضافة", "button_background.png", vm.AddCommand,
                StockUIConstants.ShopButtonHeight, fontSizeButton);
            addBtn.WidthRequest = StockUIConstants.ShopButtonWidth;
            addBtn.HorizontalOptions = LayoutOptions.Center;
            buttonsGrid.Add(addBtn);
        }
        else
        {
            buttonsGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                },
                ColumnSpacing = StockUIConstants.ButtonSpacing,
                Padding = new Thickness(StockUIConstants.ButtonsPadding, 0, StockUIConstants.ButtonsPadding, StockUIConstants.ButtonsPadding)
            };
            var editBtn = CreateActionButton("تعديل", "button_background.png", vm.EditCommand,
                StockUIConstants.ButtonHeight, fontSizeButton);
            var removeBtn = CreateActionButton("إزالة", "button_background_no.png", vm.RemoveCommand,
                StockUIConstants.ButtonHeight, fontSizeButton);
            buttonsGrid.Add(editBtn, 0, 0);
            buttonsGrid.Add(removeBtn, 1, 0);
        }

        mainGrid.Add(buttonsGrid, 0, 2);

        return new Border
        {
            Style = (Style)Application.Current.Resources["StockCard"],
            Content = mainGrid
        };
    }

    private Border CreateActionButton(string text, string imageSource, Command command, double height, double fontSize)
    {
        var btn = new Border
        {
            Style = (Style)Application.Current.Resources["StockCardButton"],
            HeightRequest = height,
            HorizontalOptions = LayoutOptions.Fill,
            Content = new Grid
            {
                Children =
                {
                    new Image { Source = imageSource, Aspect = Aspect.Fill },
                    new Label
                    {
                        Text = text,
                        Style = (Style)Application.Current.Resources["StockCardButtonText"],
                        FontSize = fontSize,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                }
            }
        };
        btn.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await btn.ScaleTo(0.92, 50);
                await Task.Delay(50);
                await btn.ScaleTo(1.0, 50);
                command.Execute(btn);
            })
        });
        return btn;
    }
    #endregion

    #region تحميل البيانات
    private void LoadShopData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        UpdateSpacesLabel();

        _shopSpaces.Clear();

        for (int i = 0; i < _player.StockObject.ShopSpaces; i++)
        {
            var listing = i < _player.StockObject.ShopListings.Count
                ? _player.StockObject.ShopListings[i]
                : null;

            var space = new ShopSpaceItemViewModel(
                i,
                listing,
                _player,
                OnItemAdded,
                OnItemRemoved,
                OnItemEdited
            );

            _shopSpaces.Add(space);
        }

        ShopSpacesGrid.ItemsSource = null;
        ShopSpacesGrid.ItemsSource = _shopSpaces;
    }
    #endregion

    #region التبويبات
    private Color _defaultTabTextColor = Color.FromArgb("#1a1a1a");
    private Color _activeTabTextColor = Colors.WhiteSmoke;

    private void SetActiveTab(Border tabButton, Label tabLabel)
    {
        StockTabLabel.TextColor = _defaultTabTextColor;
        BagTabLabel.TextColor = _defaultTabTextColor;
        ShopTabLabel.TextColor = _defaultTabTextColor;
        MuseumTabLabel.TextColor = _defaultTabTextColor;

        tabLabel.TextColor = _activeTabTextColor;

        if (_activeTab != null && _activeTab != tabButton)
            _activeTab.Scale = 1.0;
        tabButton.Scale = 0.90;
        _activeTab = tabButton;
    }

    private async void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Border border && border.Content is Grid grid &&
            grid.Children.Count > 1 && grid.Children[1] is Label label)
        {
            string tabName = label.Text;
            if (_activeTab == border) return;

            await border.ScaleTo(0.92, 50, Easing.CubicOut);
            await Task.Delay(50);
            await border.ScaleTo(1.0, 50, Easing.CubicIn);

            if (tabName == "المخزن")
            {
                SetActiveTab(border, StockTabLabel);
                await Navigation.PushAsync(new StockPage());
            }
            else if (tabName == "الحقيبة")
            {
                SetActiveTab(border, BagTabLabel);
                await Navigation.PushAsync(new BagPage());
            }
            else if (tabName == "المتجر")
            {
                SetActiveTab(border, ShopTabLabel);
                LoadShopData();
            }
            else if (tabName == "المتحف")
            {
                SetActiveTab(border, MuseumTabLabel);
                await Navigation.PushAsync(new MuseumPage());
            }
        }
    }
    #endregion

    #region أزرار الشريط السفلي
    private async void OnBuySpacesClicked(object sender, EventArgs e)
    {
        if (sender is Border border)
        {
            await border.ScaleTo(0.92, 50, Easing.CubicOut);
            await Task.Delay(50);
            await border.ScaleTo(1.0, 50, Easing.CubicIn);
        }

        int currentSpaces = _player.StockObject.ShopSpaces;
        int cost = 1000 * (currentSpaces + 1);
        if (_player.Gold < cost)
        {
            await ToastService.Show($"لا يوجد ذهب كافٍ! تحتاج {NumberFormatter.FormatNumber(cost)} ذهب", ToastType.Error);
            return;
        }

        var confirm = await DisplayAlert("شراء مساحة إضافية",
            $"شراء مساحة عرض جديدة بتكلفة {NumberFormatter.FormatNumber(cost)} ذهب؟\n\nالمساحات الحالية: {currentSpaces}\nالمساحات بعد الشراء: {currentSpaces + 1}",
            "نعم", "لا");

        if (confirm)
        {
            _player.Gold -= cost;
            _player.StockObject.ShopSpaces++;
            while (_player.StockObject.ShopListings.Count < _player.StockObject.ShopSpaces)
                _player.StockObject.ShopListings.Add(null);
            LoadShopData();
            await ToastService.Show($"تم شراء مساحة جديدة! المساحات الإجمالية: {_player.StockObject.ShopSpaces}", ToastType.Success);
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        if (sender is Border border)
        {
            await border.ScaleTo(0.92, 50, Easing.CubicOut);
            await Task.Delay(50);
            await border.ScaleTo(1.0, 50, Easing.CubicIn);
        }
        await Navigation.PopToRootAsync();
    }
    #endregion

    #region معالجة إضافة / إزالة / تعديل العناصر (بدون تغيير)
    private async void OnItemAdded(int spaceIndex, StockItem item)
    {
        try
        {
            int available = Math.Min(item.Count, 25);
            if (available <= 0)
            {
                await ToastService.Show($"لا توجد كمية كافية من {item.Name}", ToastType.Error);
                return;
            }

            var (confirmed, quantity, price) = await StockPopupService.ShowShopAddPopupAsync(item, available);

            if (confirmed)
            {
                if (quantity > item.Count || quantity > 25)
                {
                    await ToastService.Show("الكمية غير متوفرة أو تتجاوز 25", ToastType.Error);
                    return;
                }

                var listing = new ShopListing
                {
                    ItemId = item.ItemId,
                    ItemName = item.Name,
                    ImageResource = item.ImageResource,
                    Quantity = quantity,
                    PricePerItem = price,
                    OriginalPrice = item.OriginalPrice,
                    IsActive = true
                };

                if (_player.StockObject.ShopListings.Count > spaceIndex)
                    _player.StockObject.ShopListings[spaceIndex] = listing;
                else
                    _player.StockObject.ShopListings.Add(listing);

                item.Count -= quantity;
                _player.StockObject.StockFreeSpace += quantity;

                LoadShopData();

                await ToastService.Show($"تمت إضافة {quantity} × {item.Name}", ToastType.Success);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Show($"خطأ: {ex.Message}", ToastType.Error);
        }
    }

    private async void OnItemRemoved(int spaceIndex)
    {
        try
        {
            if (spaceIndex >= _player.StockObject.ShopListings.Count ||
                _player.StockObject.ShopListings[spaceIndex] == null)
            {
                await ToastService.Show("لا يوجد صنف في هذه المساحة", ToastType.Error);
                return;
            }

            var listing = _player.StockObject.ShopListings[spaceIndex];

            if (_player.StockObject.ItemsInStock.TryGetValue(listing.ItemId, out var stockItem))
            {
                stockItem.Count += listing.Quantity;
                _player.StockObject.StockFreeSpace -= listing.Quantity;
            }
            else
            {
                _player.StockObject.ItemsInStock[listing.ItemId] = new StockItem
                {
                    ItemId = listing.ItemId,
                    Name = listing.ItemName,
                    ImageResource = listing.ImageResource,
                    Count = listing.Quantity,
                    OriginalPrice = listing.OriginalPrice,
                    CountInBag = 0,
                    IsLocked = false
                };
                _player.StockObject.StockFreeSpace -= listing.Quantity;
            }

            _player.StockObject.ShopListings[spaceIndex] = null;

            LoadShopData();

            await ToastService.Show($"تمت إزالة {listing.ItemName}", ToastType.Success);
        }
        catch (Exception ex)
        {
            await ToastService.Show($"خطأ: {ex.Message}", ToastType.Error);
        }
    }

    private async void OnItemEdited(int spaceIndex, ShopListing currentListing)
    {
        try
        {
            if (!_player.StockObject.ItemsInStock.TryGetValue(currentListing.ItemId, out var stockItem))
            {
                await ToastService.Show("العنصر غير موجود في المخزن!", ToastType.Error);
                return;
            }

            int oldQuantity = currentListing.Quantity;
            int maxAdditionalFromStock = stockItem.Count;
            int maxAllowedTotal = Math.Min(25, oldQuantity + maxAdditionalFromStock);
            int minTotal = 1;

            var (confirmed, newQuantity, newPrice) = await StockPopupService.ShowShopAddPopupAsync(
                new StockItem
                {
                    ItemId = currentListing.ItemId,
                    Name = currentListing.ItemName,
                    ImageResource = currentListing.ImageResource,
                    OriginalPrice = currentListing.OriginalPrice,
                    Count = oldQuantity
                },
                availableQuantity: maxAllowedTotal,
                currentQuantity: oldQuantity,
                currentPrice: currentListing.PricePerItem
            );

            if (!confirmed) return;

            int diff = newQuantity - oldQuantity;

            if (diff > 0)
            {
                if (stockItem.Count < diff)
                {
                    await ToastService.Show("لا توجد كمية كافية في المخزن", ToastType.Error);
                    return;
                }
                stockItem.Count -= diff;
                _player.StockObject.StockFreeSpace += diff;
            }
            else if (diff < 0)
            {
                int returnToStock = -diff;
                stockItem.Count += returnToStock;
                _player.StockObject.StockFreeSpace -= returnToStock;
            }

            currentListing.Quantity = newQuantity;
            currentListing.PricePerItem = newPrice;

            LoadShopData();
            await ToastService.Show($"تم تعديل {currentListing.ItemName} بنجاح", ToastType.Success);
        }
        catch (Exception ex)
        {
            await ToastService.Show($"خطأ: {ex.Message}", ToastType.Error);
        }
    }
    #endregion
}

// ViewModel بدون تغيير
public class ShopSpaceItemViewModel
{
    private readonly int _spaceIndex;
    private readonly PlayerAccount _player;
    private readonly Action<int, StockItem> _onItemAdded;
    private readonly Action<int> _onItemRemoved;
    private readonly Action<int, ShopListing> _onItemEdited;

    public ShopListing Listing { get; private set; }

    public string ItemName => Listing?.ItemName ?? "";
    public string QuantityText => Listing != null ? $"الكمية: {NumberFormatter.FormatNumber(Listing.Quantity)}" : string.Empty;
    public string PriceText => Listing != null ? $"السعر: {NumberFormatter.FormatNumber(Listing.PricePerItem)}" : string.Empty;
    public string ImageResource => Listing?.ImageResource ?? "item_unknown";

    public bool IsEmpty => Listing == null;
    public bool IsNotEmpty => Listing != null;

    public Command AddCommand { get; }
    public Command RemoveCommand { get; }
    public Command EditCommand { get; }

    public ShopSpaceItemViewModel(
        int spaceIndex,
        ShopListing listing,
        PlayerAccount player,
        Action<int, StockItem> onItemAdded,
        Action<int> onItemRemoved,
        Action<int, ShopListing> onItemEdited)
    {
        _spaceIndex = spaceIndex;
        _player = player;
        Listing = listing;
        _onItemAdded = onItemAdded;
        _onItemRemoved = onItemRemoved;
        _onItemEdited = onItemEdited;

        AddCommand = new Command(OnAddClicked);
        RemoveCommand = new Command(OnRemoveClicked);
        EditCommand = new Command(OnEditClicked);
    }

    private async void OnAddClicked(object parameter)
    {
        await Application.Current.MainPage.Navigation.PushAsync(
            new StockPage(isAddMode: true, isForShop: true, shopSpaceIndex: _spaceIndex));
    }

    private async void OnRemoveClicked(object parameter)
    {
        _onItemRemoved?.Invoke(_spaceIndex);
    }

    private async void OnEditClicked(object parameter)
    {
        if (Listing != null)
        {
            _onItemEdited?.Invoke(_spaceIndex, Listing);
        }
    }
}