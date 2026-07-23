using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views
{
    public partial class BagPage : ContentPage
    {
        #region المتغيرات
        private PlayerAccount _player;
        private ObservableCollection<StockItem> _bagItems;
        private Border _activeTab;
        private Label _spaceLabel;
        private double _screenWidth;
        private double _screenHeight;
        #endregion

        #region خصائص الربط
        public ObservableCollection<StockItem> BagItems
        {
            get => _bagItems;
            set { _bagItems = value; OnPropertyChanged(nameof(BagItems)); }
        }
        #endregion

        #region Constructor
        public BagPage()
        {
            InitializeComponent();
            BindingContext = this;

            _screenWidth = Application.Current.MainPage.Width;
            _screenHeight = Application.Current.MainPage.Height;

            BagItems = new ObservableCollection<StockItem>();
            BagItemsGrid.ItemTemplate = CreateCardTemplate();
            LoadBagData();
        }
        #endregion

        #region دورة الحياة
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadBagData();
            SetActiveTab(BagTabButton, BagTabLabel);
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
                    new ColumnDefinition { Width = GridLength.Auto }     // زر إضافة (يسار)
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

            // مؤشر المساحة (وسط)
            var spaceStack = new VerticalStackLayout
            {
                Spacing = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            var spaceTitle = new Label
            {
                Text = "مساحة الحقيبة",
                TextColor = Color.FromArgb("#000000"),
                FontFamily = "Cairo-Black",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Color.FromArgb("#80000000")), // لون الظل (أسود شفاف)
                    Offset = new Point(0, 2),                                 // إزاحة (يمين، أسفل)
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
                    Offset = new Point(0, 2),                                 // إزاحة (يمين، أسفل)
                    Radius = 2,                                               // نصف قطر التمويه
                    Opacity = 1.5f                                            // شفافية الظل
                }
            };
            spaceStack.Children.Add(spaceTitle);
            spaceStack.Children.Add(spaceValue);
            footerGrid.Add(spaceStack, 1, 0);
            _spaceLabel = spaceValue;

            // زر إضافة (يسار)
            var addButton = PageFooter.CreateFooterButton(
                text: "إضافة",
                tappedHandler: OnAddClicked,
                buttonImageSource: "footer_button_add.png", // صورة مخصصة للإضافة
                horizontalOptions: LayoutOptions.End
            );

            footerGrid.Add(homeButton, 2, 0);
            footerGrid.Add(addButton, 0, 0);

            PageFooter.SetCustomContent(footerGrid);
            UpdateSpaceLabel();
        }

        private void UpdateSpaceLabel()
        {
            if (_spaceLabel != null && _player != null)
            {
                int usedSpace = _player.StockObject.BagSpace - _player.StockObject.BagFreeSpace;
                int totalSpace = _player.StockObject.BagSpace;
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
                Text = $"العدد: x{item.CountInBag}",
                Style = (Style)Application.Current.Resources["StockCardCount"],
                FontSize = StockUIConstants.FontSizeMedium
            });
            textStack.Children.Add(new Label
            {
                Text = $"المتبقي في المخزن: {item.Count}",
                Style = (Style)Application.Current.Resources["StockCardCount"],
                FontSize = StockUIConstants.FontSizeTiny,
                TextColor = Colors.LightGray
            });
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

            var buttonsGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition() },
                Padding = new Thickness(StockUIConstants.ButtonsPadding * 4, 0, StockUIConstants.ButtonsPadding * 4, StockUIConstants.ButtonsPadding)
            };
            var removeBtn = CreateActionButton("إزالة", "button_background_no.png", () => OnRemoveClicked(item),
                StockUIConstants.ShopButtonHeight, fontSizeButton);
            removeBtn.WidthRequest = StockUIConstants.ShopButtonWidth;
            removeBtn.HorizontalOptions = LayoutOptions.Center;
            buttonsGrid.Add(removeBtn);
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
        private void LoadBagData()
        {
            _player = AccountService.GetCurrentPlayer();
            if (_player == null) return;

            UpdateSpaceLabel();

            BagItems.Clear();
            foreach (var item in _player.StockObject.ItemsInStock.Values
                         .Where(i => i.CountInBag > 0)
                         .OrderBy(i => i.Name))
            {
                BagItems.Add(item);
            }

            BagItemsGrid.ItemsSource = null;
            BagItemsGrid.ItemsSource = BagItems;
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
                    await Navigation.PushAsync(new StockPage(), false);
                }
                else if (tabName == "الحقيبة")
                {
                    SetActiveTab(border, BagTabLabel);
                    LoadBagData();
                }
                else if (tabName == "المتجر")
                {
                    SetActiveTab(border, ShopTabLabel);
                    await Navigation.PushAsync(new ShopPage(), false);
                }
                else if (tabName == "المتحف")
                {
                    SetActiveTab(border, MuseumTabLabel);
                    await Navigation.PushAsync(new MuseumPage(), false);
                }
            }
        }
        #endregion

        #region أحداث الأزرار
        private async void OnAddClicked(object sender, EventArgs e)
        {
            if (sender is Border border)
            {
                await border.ScaleTo(0.92, 50, Easing.CubicOut);
                await Task.Delay(50);
                await border.ScaleTo(1.0, 50, Easing.CubicIn);
            }

            _player = AccountService.GetCurrentPlayer();
            if (_player == null || _player.StockObject.BagFreeSpace <= 0)
            {
                await ToastService.Show("الحقيبة ممتلئة!", ToastType.Error);
                return;
            }
            await Navigation.PushAsync(new StockPage(isAddMode: true), false);
        }

        private async void OnRemoveClicked(StockItem item)
        {
            var (confirmed, amount) = await StockPopupService.ShowEditQuantityPopupAsync(item, 0, item.CountInBag, isEditMode: false);
            if (confirmed && amount > 0)
            {
                if (_player.StockObject.RemoveFromBag(item.ItemId, amount))
                {
                    LoadBagData();
                    await ToastService.Show($"تمت إزالة {amount} × {item.Name}", ToastType.Success);
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
            await Navigation.PopToRootAsync(false);
        }
        #endregion
    }
}