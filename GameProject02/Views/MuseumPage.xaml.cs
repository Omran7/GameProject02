using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class MuseumPage : ContentPage, INotifyPropertyChanged
{
    #region المتغيرات
    private PlayerAccount _player;
    private ObservableCollection<MuseumSpaceItemViewModel> _museumSpaces;
    private Border _activeTab;
    private Label _spaceLabel;
    private double _screenWidth;
    private double _screenHeight;
    private ObservableCollection<MuseumItem> _museumItems;
    private double _itemSize;
    #endregion

    #region خصائص الربط
    public ObservableCollection<MuseumItem> MuseumItems
    {
        get => _museumItems;
        set { _museumItems = value; OnPropertyChanged(); }
    }

    public double ItemSize
    {
        get => _itemSize;
        set { _itemSize = value; OnPropertyChanged(); }
    }
    #endregion

    #region Constructor
    public MuseumPage()
    {
        InitializeComponent();
        BindingContext = this;

        _screenWidth = Application.Current.MainPage.Width;
        _screenHeight = Application.Current.MainPage.Height;

        _museumSpaces = new ObservableCollection<MuseumSpaceItemViewModel>();
        MuseumItems = new ObservableCollection<MuseumItem>();
        MuseumSpacesGrid.ItemTemplate = CreateCardTemplate();
        LoadMuseumData();
    }
    #endregion

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region دورة الحياة
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadMuseumData();
        AdjustTopSectionSize();
        SetActiveTab(MuseumTabButton, MuseumTabLabel);
        SetupFooter();
    }

    private void SetupFooter()
    {
        var footerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },    // زر HOME (يمين)
                new ColumnDefinition { Width = GridLength.Star },    // المساحة (وسط)
                new ColumnDefinition { Width = GridLength.Auto }     // أزرار مساحات + خلفية (يسار)
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

        // مؤشر المساحة (وسط) مع ظل
        var spaceStack = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        var spaceTitle = new Label
        {
            Text = "مساحة المتحف",
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
        _spaceLabel = spaceValue;

        // حاوية للأزرار اليسرى (مساحات + خلفية)
        var leftButtonsStack = new HorizontalStackLayout
        {
            Spacing = 5,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };

        // زر شراء مساحات
        var buySpacesButton = PageFooter.CreateFooterButton(
            text: "مساحات",
            tappedHandler: OnBuySpacesClicked,
            buttonImageSource: "footer_button_add.png", // أو أي صورة مناسبة
            horizontalOptions: LayoutOptions.Center
        );

        // زر خلفية
        var backgroundButton = PageFooter.CreateFooterButton(
            text: "خلفية",
            tappedHandler: OnBackgroundClicked,
            buttonImageSource: "footer_button_museum.png", // صورة مخصصة للخلفية
            horizontalOptions: LayoutOptions.Center
        );

        leftButtonsStack.Children.Add(buySpacesButton);
        leftButtonsStack.Children.Add(backgroundButton);
        footerGrid.Add(leftButtonsStack, 0, 0);

        footerGrid.Add(homeButton, 2, 0);

        PageFooter.SetContent(footerGrid);
        UpdateSpaceLabel();
    }

    private void UpdateSpaceLabel()
    {
        if (_spaceLabel != null && _player != null)
        {
            _spaceLabel.Text = $"{_player.Museum.MuseumSpaces}/{_player.Museum.MaxMuseumSpaces}";
        }
    }
    #endregion

    #region ضبط القسم العلوي ديناميكياً
    private void AdjustTopSectionSize()
    {
        double topSectionHeight = _screenHeight * 0.16;
        TopSectionBorder.HeightRequest = topSectionHeight;

        double collectionViewHeight = topSectionHeight - 10;
        MuseumItemsScroll.HeightRequest = collectionViewHeight;

        ItemSize = collectionViewHeight * 0.60;
    }
    #endregion

    #region قالب البطاقة للمساحات السفلية
    private DataTemplate CreateCardTemplate()
    {
        return new DataTemplate(() =>
        {
            var border = new Border();
            border.Style = Application.Current.Resources["StockCard"] as Style;

            border.BindingContextChanged += (s, e) =>
            {
                var vm = border.BindingContext as MuseumSpaceItemViewModel;
                if (vm != null)
                {
                    border.Content = BuildCardContent(vm);
                }
            };
            return border;
        });
    }

    private View BuildCardContent(MuseumSpaceItemViewModel vm)
    {
        double imageSize = StockUIConstants.CardImageSize;
        double buttonHeight = StockUIConstants.ButtonHeight;
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

        var image = new Image
        {
            Source = vm.ImageResource,
            Aspect = Aspect.AspectFill,
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 5)
        };
        mainGrid.Add(image, 0, 0);

        var titleGrid = new Grid
        {
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(StockUIConstants.TitleMarginHorizontal, 0, StockUIConstants.TitleMarginHorizontal, 10),
            HeightRequest = StockUIConstants.TitleBarHeight
        };
        var titleBgImage = new Image { Source = "card_titel_name.png", Aspect = Aspect.Fill };
        var nameLabel = new Label
        {
            Text = vm.ItemName,
            Style = (Style)Application.Current.Resources["StockCardTitle"],
            FontSize = StockUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            TranslationY = -2
        };
        titleGrid.Children.Add(titleBgImage);
        titleGrid.Children.Add(nameLabel);
        mainGrid.Add(titleGrid, 0, 1);

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
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition() },
                Padding = new Thickness(StockUIConstants.ButtonsPadding * 4, 0, StockUIConstants.ButtonsPadding * 4, StockUIConstants.ButtonsPadding)
            };
            var removeBtn = CreateActionButton("إزالة", "button_background_no.png", vm.RemoveCommand,
                StockUIConstants.ShopButtonHeight, fontSizeButton);
            removeBtn.WidthRequest = StockUIConstants.ShopButtonWidth;
            removeBtn.HorizontalOptions = LayoutOptions.Center;
            buttonsGrid.Add(removeBtn);
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
    private void LoadMuseumData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        UpdateSpaceLabel();

        string backgroundResource = _player.Museum.GetBackgroundResource(_player.Museum.BackgroundId);
        if (string.IsNullOrEmpty(backgroundResource))
            backgroundResource = "museum_background_wood.png";
        MuseumBackgroundImage.Source = backgroundResource;

        var itemsInMuseum = _player.Museum.Items.Where(i => i != null).ToList();
        MuseumItems.Clear();
        foreach (var item in itemsInMuseum)
            MuseumItems.Add(item);
        MuseumItemsScroll.IsVisible = MuseumItems.Count > 0;

        for (int i = 0; i < _player.Museum.Items.Count; i++)
        {
            var museumItem = _player.Museum.Items[i];
            if (museumItem != null)
            {
                if (!_player.StockObject.ItemsInStock.TryGetValue(museumItem.ItemId, out var stockItem) ||
                    stockItem.Count <= 0)
                {
                    _player.Museum.Items[i] = null;
                }
            }
        }

        _museumSpaces.Clear();
        for (int i = 0; i < _player.Museum.MuseumSpaces; i++)
        {
            var item = i < _player.Museum.Items.Count ? _player.Museum.Items[i] : null;
            var space = new MuseumSpaceItemViewModel(i, item, _player, OnItemAdded, OnItemRemoved);
            _museumSpaces.Add(space);
        }

        MuseumSpacesGrid.ItemsSource = null;
        MuseumSpacesGrid.ItemsSource = _museumSpaces;
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
                await Navigation.PushAsync(new ShopPage());
            }
            else if (tabName == "المتحف")
            {
                SetActiveTab(border, MuseumTabLabel);
                LoadMuseumData();
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

        int currentSpaces = _player.Museum.MuseumSpaces;
        int cost = 1000 * (currentSpaces + 1);
        if (_player.Gold < cost)
        {
            await ToastService.Show($"لا يوجد ذهب كافٍ! تحتاج {NumberFormatter.FormatNumber(cost)} ذهب", ToastType.Error);
            return;
        }

        var confirm = await DisplayAlert("شراء مساحة إضافية",
            $"شراء مساحة متحف جديدة بتكلفة {NumberFormatter.FormatNumber(cost)} ذهب؟\n\nالمساحات الحالية: {currentSpaces}\nالمساحات بعد الشراء: {currentSpaces + 1}",
            "نعم", "لا");

        if (confirm)
        {
            _player.Gold -= cost;
            _player.Museum.MuseumSpaces++;
            while (_player.Museum.Items.Count < _player.Museum.MuseumSpaces)
                _player.Museum.Items.Add(null);
            LoadMuseumData();
            await ToastService.Show($"تم شراء مساحة جديدة! المساحات الإجمالية: {_player.Museum.MuseumSpaces}", ToastType.Success);
        }
    }

    private async void OnBackgroundClicked(object sender, EventArgs e)
    {
        if (sender is Border border)
        {
            await border.ScaleTo(0.92, 50, Easing.CubicOut);
            await Task.Delay(50);
            await border.ScaleTo(1.0, 50, Easing.CubicIn);
        }

        var backgrounds = new[]
        {
            "خلفية خشبية (افتراضية)",
            "خلفية وردية",
            "خلفية كلاسيكية",
            "خلفية ليزر",
            "خلفية صدف",
            "خلفية غابة",
            "خلفية كون"
        };

        var result = await DisplayActionSheet("اختر خلفية المتحف", "إلغاء", null, backgrounds);

        if (result != "إلغاء" && result != null)
        {
            int backgroundId = Array.IndexOf(backgrounds, result);

            if (_player.Museum.UnlockedBackgrounds.Contains(backgroundId))
            {
                _player.Museum.BackgroundId = backgroundId;
                MuseumBackgroundImage.Source = _player.Museum.GetBackgroundResource(backgroundId);
                await ToastService.Show($"تم تغيير الخلفية إلى {result}", ToastType.Success);
            }
            else
            {
                await ToastService.Show("هذه الخلفية غير متوفرة", ToastType.Error);
            }
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

    #region معالجة إضافة / إزالة العناصر
    private async void OnItemAdded(int spaceIndex, StockItem item)
    {
        var museumItem = new MuseumItem
        {
            ItemId = item.ItemId,
            ItemName = item.Name,
            ImageResource = item.ImageResource,
            Quantity = item.Count,
            OriginalPrice = item.OriginalPrice,
            Damage = item.Damage,
            Accuracy = item.Accuracy,
            Defense = item.Defense,
            Evasion = item.Evasion,
            IsWeapon = item.IsWeapon,
            IsGun = item.IsGun,
            GunType = item.GunType
        };

        if (spaceIndex < _player.Museum.Items.Count)
            _player.Museum.Items[spaceIndex] = museumItem;
        else
            _player.Museum.Items.Add(museumItem);

        LoadMuseumData();
        await ToastService.Show($"تم عرض {item.Name} في المتحف", ToastType.Success);
    }

    private async void OnItemRemoved(int spaceIndex)
    {
        if (spaceIndex >= _player.Museum.Items.Count || _player.Museum.Items[spaceIndex] == null)
        {
            await ToastService.Show("لا يوجد عنصر في هذه المساحة", ToastType.Error);
            return;
        }

        var item = _player.Museum.Items[spaceIndex];
        _player.Museum.Items[spaceIndex] = null;
        LoadMuseumData();
        await ToastService.Show($"تمت إزالة {item.ItemName}", ToastType.Success);
    }
    #endregion
}

// ============================================
// ViewModel لمساحة المتحف
// ============================================
public class MuseumSpaceItemViewModel
{
    private readonly int _spaceIndex;
    private readonly PlayerAccount _player;
    private readonly Action<int, StockItem> _onItemAdded;
    private readonly Action<int> _onItemRemoved;

    public MuseumItem Item { get; private set; }

    public string ItemName => Item?.ItemName ?? "";
    public string ImageResource => Item?.ImageResource ?? "item_unknown";

    public bool IsEmpty => Item == null;
    public bool IsNotEmpty => Item != null;

    public Command AddCommand { get; }
    public Command RemoveCommand { get; }

    public MuseumSpaceItemViewModel(
        int spaceIndex,
        MuseumItem item,
        PlayerAccount player,
        Action<int, StockItem> onItemAdded,
        Action<int> onItemRemoved)
    {
        _spaceIndex = spaceIndex;
        _player = player;
        Item = item;
        _onItemAdded = onItemAdded;
        _onItemRemoved = onItemRemoved;

        AddCommand = new Command(OnAddClicked);
        RemoveCommand = new Command(OnRemoveClicked);
    }

    private async void OnAddClicked(object parameter)
    {
        await Application.Current.MainPage.Navigation.PushAsync(
            new StockPage(isAddMode: true, isForMuseum: true, museumSpaceIndex: _spaceIndex));
    }

    private async void OnRemoveClicked(object parameter)
    {
        _onItemRemoved?.Invoke(_spaceIndex);
    }
}