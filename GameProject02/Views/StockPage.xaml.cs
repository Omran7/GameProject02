using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class StockPage : ContentPage
    {
        #region المتغيرات
        private PlayerAccount _player;
        private bool _isAddMode = false;
        private bool _isForShop = false;
        private bool _isForMuseum = false;
        private int _shopSpaceIndex = -1;
        private int _museumSpaceIndex = -1;
        private ObservableCollection<StockItem> _stockItems;
        private Border _activeTab;
        private Label _spaceLabel;
        private double _screenWidth;
        private double _screenHeight;
        #endregion

        #region خصائص الربط
        public ObservableCollection<StockItem> StockItems
        {
            get => _stockItems;
            set { _stockItems = value; OnPropertyChanged(nameof(StockItems)); }
        }

        public bool IsAddMode
        {
            get => _isAddMode;
            set { _isAddMode = value; OnPropertyChanged(nameof(IsAddMode)); }
        }
        #endregion

        #region Constructor
        public StockPage(bool isAddMode = false, bool isForShop = false, bool isForMuseum = false,
                         int shopSpaceIndex = -1, int museumSpaceIndex = -1)
        {
            InitializeComponent();
            BindingContext = this;

            IsAddMode = isAddMode;
            _isForShop = isForShop;
            _isForMuseum = isForMuseum;
            _shopSpaceIndex = shopSpaceIndex;
            _museumSpaceIndex = museumSpaceIndex;

            _screenWidth = Application.Current.MainPage.Width;
            _screenHeight = Application.Current.MainPage.Height;

            StockItems = new ObservableCollection<StockItem>();
            StockItemsGrid.ItemTemplate = CreateCardTemplate();
            LoadStockData();
        }
        #endregion

        #region دورة الحياة
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadStockData();
            SetActiveTab(StockTabButton, StockTabLabel);
            SetupFooter();
        }

        private void SetupFooter()
        {
            // إنشاء Grid للفوتر (3 أعمدة: HOME | المساحة | تصفية)
            var footerGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },    // زر HOME (يمين)
                    new ColumnDefinition { Width = GridLength.Star },    // المساحة (وسط)
                    new ColumnDefinition { Width = GridLength.Auto }     // زر تصفية (يسار)
                },
                ColumnSpacing = 10,
                Padding = new Thickness(25, 0),
                VerticalOptions = LayoutOptions.Center
            };

            // زر HOME (باستخدام الدالة الموحدة من FooterView)
            var homeButton = PageFooter.CreateFooterButton(
                text: "رجوع",                                   // بدون نص
                tappedHandler: OnHomeClicked,
                buttonImageSource: "footer_button_back.png", // نفس صورة باقي الصفحات
                horizontalOptions: LayoutOptions.Start
            );

            // مؤشر المساحة (وسط)
            var spaceStack = new VerticalStackLayout
            {
                Spacing = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            var spaceTitle = new Label
            {
                Text = "مساحة المخزن",
                TextColor = Color.FromArgb("#000000"),
                FontFamily = "Cairo-Black",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Color.FromArgb("#80000000")), // لون الظل (أسود شفاف)
                    Offset = new Point(3, 0),                                 // إزاحة (يمين، أسفل)
                    Radius = 2,                                               // نصف قطر التمويه
                    Opacity = 1.5f                                            // شفافية الظل
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
                    Brush = new SolidColorBrush(Color.FromArgb("#80000000")), // لون الظل (أسود شفاف)
                    Offset = new Point(3, 0),                                 // إزاحة (يمين، أسفل)
                    Radius = 2,                                               // نصف قطر التمويه
                    Opacity = 1.5f                                            // شفافية الظل
                }
            };

            spaceStack.Children.Add(spaceTitle);
            spaceStack.Children.Add(spaceValue);
            footerGrid.Add(spaceStack, 1, 0);
            _spaceLabel = spaceValue;

            // زر تصفية (باستخدام الدالة الموحدة من FooterView)
            var filterButton = PageFooter.CreateFooterButton(
                text: "فلتر",                                  
                tappedHandler: OnFilterClicked,
                buttonImageSource: "footer_button_filter.png", // أو صورة مخصصة مثل "filter_icon.png"
                horizontalOptions: LayoutOptions.End
            );

            footerGrid.Add(homeButton, 2, 0);
            footerGrid.Add(filterButton, 0, 0);

            // وضع المحتوى المخصص في الفوتر
            PageFooter.SetCustomContent(footerGrid);
            UpdateSpaceLabel();
        }

        private void UpdateSpaceLabel()
        {
            if (_spaceLabel != null && _player != null)
            {
                int usedSpace = _player.StockObject.StockSpace - _player.StockObject.StockFreeSpace;
                int totalSpace = _player.StockObject.StockSpace;
                _spaceLabel.Text = $"{usedSpace}/{totalSpace}";
            }
        }
        #endregion

        #region قالب البطاقة
        private DataTemplate CreateCardTemplate()
        {
            return new DataTemplate(() =>
            {
                var border = new Border();
                border.Style = Application.Current.Resources["StockCard"] as Style;

                border.BindingContextChanged += (s, e) =>
                {
                    var item = border.BindingContext as StockItem;
                    if (item != null)
                    {
                        border.Content = BuildCardContent(item);
                    }
                };
                return border;
            });
        }

        private View BuildCardContent(StockItem item)
        {
            double imageSize = StockUIConstants.CardImageSize;
            double buttonHeight = StockUIConstants.ButtonHeight;
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
                Text = item.Name,
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
                Text = $"السعر: {NumberFormatter.FormatNumber(item.OriginalPrice)}",
                Style = (Style)Application.Current.Resources["StockCardCount"],
                FontSize = StockUIConstants.FontSizeSmall
            });
            textStack.Children.Add(new Label
            {
                Text = $"الكمية: {item.Count}",
                Style = (Style)Application.Current.Resources["StockCardCount"],
                FontSize = StockUIConstants.FontSizeSmall
            });

            if (item.IsWeapon)
            {
                textStack.Children.Add(new Label
                {
                    Text = $"⚔️ ضرر: {item.Damage}",
                    Style = (Style)Application.Current.Resources["StockCardCount"],
                    FontSize = StockUIConstants.FontSizeTiny,
                    TextColor = StockUIConstants.TextRed
                });
                textStack.Children.Add(new Label
                {
                    Text = $"🎯 دقة: {item.Accuracy}",
                    Style = (Style)Application.Current.Resources["StockCardCount"],
                    FontSize = StockUIConstants.FontSizeTiny,
                    TextColor = StockUIConstants.TextRed
                });
            }
            else if (item.CategoryId == 1)
            {
                textStack.Children.Add(new Label
                {
                    Text = $"🛡️ دفاع: {item.Defense}",
                    Style = (Style)Application.Current.Resources["StockCardCount"],
                    FontSize = StockUIConstants.FontSizeTiny,
                    TextColor = StockUIConstants.BorderBlue
                });
                textStack.Children.Add(new Label
                {
                    Text = $"💨 تفادي: {item.Evasion}",
                    Style = (Style)Application.Current.Resources["StockCardCount"],
                    FontSize = StockUIConstants.FontSizeTiny,
                    TextColor = StockUIConstants.BorderBlue
                });
            }

            infoGrid.Add(textStack, 0, 0);

            var image = new Image
            {
                Source = item.ImageResource,
                Aspect = Aspect.AspectFill,
                WidthRequest = imageSize,
                HeightRequest = imageSize,
                Margin = new Thickness(0, StockUIConstants.ContentPadding / 2, StockUIConstants.ContentPadding / 2, StockUIConstants.ContentPadding / 2)
            };
            var imageTap = new TapGestureRecognizer();
            imageTap.Tapped += async (s, e) => await StockPopupService.ShowItemDetailsPopupAsync(item);
            image.GestureRecognizers.Add(imageTap);
            infoGrid.Add(image, 1, 0);
            mainGrid.Add(infoGrid, 0, 1);

            Grid buttonsGrid;
            if (IsAddMode)
            {
                buttonsGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition() },
                    Padding = new Thickness(StockUIConstants.ButtonsPadding * 4, 0, StockUIConstants.ButtonsPadding * 4, StockUIConstants.ButtonsPadding)
                };
                var addBtn = CreateActionButton("إضافة", "button_background.png", () => OnAddToBagClicked(item), buttonHeight, fontSizeButton);
                buttonsGrid.Add(addBtn); addBtn.WidthRequest = StockUIConstants.ShopButtonWidth;
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
                var useBtn = CreateActionButton("استخدام", "button_background.png", () => OnUseItemClicked(item), buttonHeight, fontSizeButton);
                var sellBtn = CreateActionButton("بيع", "button_background_no.png", () => OnSellItemClicked(item), buttonHeight, fontSizeButton);
                buttonsGrid.Add(useBtn, 0, 0);
                buttonsGrid.Add(sellBtn, 1, 0);
            }

            mainGrid.Add(buttonsGrid, 0, 2);

            return new Border
            {
                Style = (Style)Application.Current.Resources["StockCard"],
                HeightRequest = StockUIConstants.CardHeight,
                Content = mainGrid
            };
        }

        private Border CreateActionButton(string text, string imageSource, Action action, double height, double fontSize)
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
                    action();
                })
            });
            return btn;
        }
        #endregion

        #region تحميل البيانات
        private void LoadStockData()
        {
            _player = AccountService.GetCurrentPlayer();
            if (_player == null) return;

            UpdateSpaceLabel();

            StockItems.Clear();
            foreach (var item in _player.StockObject.ItemsInStock.Values
                         .Where(i => i.Count > 0)
                         .OrderBy(i => i.Name))
            {
                item.IsLocked = _player.StockObject.LockedItemIds.Contains(item.ItemId);
                StockItems.Add(item);
            }

            StockItemsGrid.ItemsSource = null;
            StockItemsGrid.ItemsSource = StockItems;
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
            if (IsAddMode)
            {
                await ToastService.Show("أضف العناصر ثم عد", ToastType.Error);
                return;
            }

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
                    LoadStockData();
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
                    await Navigation.PushAsync(new MuseumPage());
                }
            }
        }
        #endregion

        #region أحداث الأزرار
        private async void OnUseItemClicked(StockItem item)
        {
            if (IsAddMode)
            {
                await ToastService.Show("في وضع الإضافة فقط", ToastType.Error);
                return;
            }
            if (item.CategoryId != 0)
            {
                await ToastService.Show($"البند '{item.Name}' لا يمكن استخدامه حالياً.", ToastType.Error);
                return;
            }
            int restoreAmount = item.OriginalPrice / 5;
            int newHealth = Math.Min(_player.MaxHealth, _player.Health + restoreAmount);
            int healthGain = newHealth - _player.Health;
            bool confirm = await DisplayAlert("تأكيد الاستخدام",
                $"هل تريد استخدام {item.Name}؟\n\n💚 الصحة الحالية: {_player.Health}/{_player.MaxHealth}\n💚 الصحة بعد الاستخدام: {newHealth}/{_player.MaxHealth}\n💚 ستستعيد: {healthGain} نقطة صحة",
                "نعم", "لا");
            if (confirm)
            {
                _player.Health = newHealth;
                _player.StockObject.RemoveItem(item.ItemId, 1);
                await ToastService.Show($"استعدت {healthGain} نقطة صحة!", ToastType.Success);
                LoadStockData();
            }
        }

        private async void OnSellItemClicked(StockItem item)
        {
            if (IsAddMode)
            {
                await ToastService.Show("في وضع الإضافة فقط", ToastType.Error);
                return;
            }
            if (item.IsLocked)
            {
                await ToastService.Show("هذا الصنف مقفول!", ToastType.Error);
                return;
            }
            var (confirmed, amount) = await StockPopupService.ShowSellPopupAsync(item);
            if (confirmed)
            {
                _player.Gold += item.GetSellPrice() * amount;
                _player.StockObject.RemoveItem(item.ItemId, amount);
                LoadStockData();
                await ToastService.Show($"تم بيع {amount} × {item.Name}!", ToastType.Success);
            }
        }

        private async void OnAddToBagClicked(StockItem item)
        {
            if (!IsAddMode)
            {
                await ToastService.Show("اضغط من الحقيبة للإضافة", ToastType.Error);
                return;
            }
            if (item.Count <= 0)
            {
                await ToastService.Show("لا توجد كمية متاحة", ToastType.Error);
                return;
            }
            if (_isForShop)
            {
                int available = Math.Min(item.Count, 25);
                if (available <= 0)
                {
                    await ToastService.Show("لا توجد كمية متاحة", ToastType.Error);
                    return;
                }
                var (confirmed, quantity, price) = await StockPopupService.ShowShopAddPopupAsync(item, available);
                if (confirmed)
                {
                    var listing = new ShopListing
                    {
                        ItemId = item.ItemId,
                        ItemName = item.Name,
                        ImageResource = item.ImageResource,
                        Quantity = quantity,
                        PricePerItem = price,
                        OriginalPrice = (int)item.OriginalPrice,
                        IsActive = true
                    };
                    if (_shopSpaceIndex >= 0)
                    {
                        if (_player.StockObject.ShopListings.Count > _shopSpaceIndex)
                            _player.StockObject.ShopListings[_shopSpaceIndex] = listing;
                        else
                            _player.StockObject.ShopListings.Add(listing);
                        item.Count -= quantity;
                        _player.StockObject.StockFreeSpace += quantity;
                        await Navigation.PopAsync();
                        await ToastService.Show($"تمت إضافة {quantity} × {item.Name}", ToastType.Success);
                    }
                }
                return;
            }
            if (_isForMuseum)
            {
                bool confirmed = await StockPopupService.ShowMuseumAddPopupAsync(item);
                if (confirmed)
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
                    if (_museumSpaceIndex >= 0)
                    {
                        if (_player.Museum.Items.Count > _museumSpaceIndex)
                            _player.Museum.Items[_museumSpaceIndex] = museumItem;
                        else
                            _player.Museum.Items.Add(museumItem);
                        await Navigation.PopAsync();
                        await ToastService.Show($"تم عرض {item.Name} في المتحف", ToastType.Success);
                    }
                }
                return;
            }
            var (bagConfirmed, amount) = await StockPopupService.ShowAddToBagPopupAsync(item, _player.StockObject.BagFreeSpace);
            if (bagConfirmed)
            {
                if (_player.StockObject.AddToBag(item.ItemId, amount))
                {
                    await Navigation.PopAsync();
                    await ToastService.Show($"تمت إضافة {amount} × {item.Name}", ToastType.Success);
                    LoadStockData();
                }
            }
        }

        private async void OnFilterClicked(object sender, EventArgs e)
        {
            if (sender is Border border)
            {
                await border.ScaleTo(0.92, 50, Easing.CubicOut);
                await Task.Delay(50);
                await border.ScaleTo(1.0, 50, Easing.CubicIn);
            }
            await ToastService.Show("التصنيفات قريباً", ToastType.Error);
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            if (sender is Border border)
            {
                await border.ScaleTo(0.92, 50, Easing.CubicOut);
                await Task.Delay(50);
                await border.ScaleTo(1.0, 50, Easing.CubicIn);
            }
            if (IsAddMode)
                await Navigation.PopAsync();
            else
                await Navigation.PopToRootAsync();
        }
        #endregion
    }

    #region محول
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue) return !boolValue;
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue) return !boolValue;
            return false;
        }
    }
    #endregion
}