using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class EstateUpgradePage : ContentPage
{
    private PlayerAccount _player;
    private EstateObject _estate;
    private bool _isPrivateDomain = false;
    private const int MAX_IMAGE_SIZE_BYTES = 2 * 1024 * 1024;
    private List<EstateUpgradeItem> _allUpgrades = new();
    private List<EstateUpgradeItem> _allContracts = new();

    private double _screenWidth;
    private double _screenHeight;

    private const string ButtonBackground = "button_background.png";
    private const string ButtonBackgroundNo = "button_background_no.png";
    private readonly Color _goldTextColor = Color.FromArgb("#ffffff");
    private readonly Color _goldAccent = Colors.Goldenrod;

    public EstateUpgradePage(EstateObject estate)
    {
        try
        {
            InitializeComponent();
            _estate = estate;
            _player = AccountService.GetCurrentPlayer();
            _screenWidth = Application.Current.MainPage.Width;
            _screenHeight = Application.Current.MainPage.Height;
            _isPrivateDomain = (_estate.Id == 15);
            var imageChangeContainer = this.FindByName<Border>("ImageChangeContainer");
            if (imageChangeContainer != null)
                imageChangeContainer.IsVisible = _isPrivateDomain;
            ApplyFontSizes();
            LoadUpgradeData();
        }
        catch (Exception ex)
        {
            DisplayAlert("خطأ", $"فشل تحميل الصفحة: {ex.Message}", "موافق");
        }
    }

    private void ApplyFontSizes()
    {
        ImageChangeLabel.FontSize = EstateUIConstants.FontSizeSmall;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        _screenWidth = Application.Current.MainPage.Width;
        _screenHeight = Application.Current.MainPage.Height;
        ApplyFontSizes();
        LoadUpgradeData();
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

        var backButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnBackClicked,
            buttonImageSource: "footer_button_back.png"
        );

        grid.Add(backButton, 1, 0);
        PageFooter.SetContent(grid);
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

    private void LoadUpgradeData()
    {
        try
        {
            if (_player == null || _estate == null) return;
            var upgradesContainer = this.FindByName<VerticalStackLayout>("UpgradesContainer");
            if (upgradesContainer == null) return;
            upgradesContainer.Children.Clear();
            upgradesContainer.BackgroundColor = Colors.Transparent;

            if (_estate.Id != _player.PrimaryResidenceEstateId && _estate.Id != 0)
            {
                var warning = new Border
                {
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
                    BackgroundColor = Colors.Transparent,
                    Padding = new Thickness(_screenWidth * 0.04),
                    Margin = new Thickness(0, 0, 0, _screenWidth * 0.04)
                };
                var warningLabel = new Label
                {
                    Text = "⚠️ هذا العقار ليس مقر إقامتك الرئيسي!\nلا يمكن ترقيته حتى تنتقل للعيش فيه.",
                    Style = (Style)Application.Current.Resources["Upgrade_PrimaryWarning"],
                    FontSize = EstateUIConstants.FontSizeMedium
                };
                warning.Content = warningLabel;
                upgradesContainer.Children.Add(warning);
                return;
            }

            if (EstateUpgradesDatabase.EstateUpgrades.TryGetValue(_estate.Id, out var upgradesList))
            {
                foreach (var item in upgradesList)
                {
                    if (item.Type == "Upgrade") item.IsPurchased = _estate.PurchasedUpgrades.Contains(item.Name);
                    else if (item.Type == "Contract")
                    {
                        item.IsPurchased = _estate.ActiveContracts.Contains(item.Name);
                        if (_estate.ContractStartTimes.TryGetValue(item.Name, out var startTime))
                            item.ContractStartTime = startTime;
                    }
                }
                _allUpgrades = upgradesList.Where(u => u.Type == "Upgrade").ToList();
                _allContracts = upgradesList.Where(u => u.Type == "Contract").ToList();
            }
            else
            {
                upgradesContainer.Children.Add(new Label
                {
                    Text = "لا توجد ترقيات متاحة لهذا العقار",
                    Style = (Style)Application.Current.Resources["Upgrade_NoItems"],
                    FontSize = EstateUIConstants.FontSizeMedium
                });
                return;
            }

            if (_allUpgrades.Count > 0)
            {
                var upgradesGrid = new Grid();
                upgradesGrid.Add(new Image { Source = "card_background_upsell.png", Aspect = Aspect.Fill, Opacity = 1 });
                var upgradesHeader = new Border
                {
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 0 },
                    BackgroundColor = Colors.Transparent,
                    Padding = new Thickness(_screenWidth * 0.02),
                    Margin = new Thickness(0)
                };
                var headerLabel = new Label
                {
                    Text = "الترقيات (شراء دائم)",
                    Style = (Style)Application.Current.Resources["Upgrade_SectionHeader"],
                    FontSize = EstateUIConstants.FontSizeMedium
                };
                upgradesHeader.Content = headerLabel;
                upgradesGrid.Add(upgradesHeader);
                var upgradesBorder = new Border
                {
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
                    Padding = new Thickness(0),
                    Margin = new Thickness(50, 0, 50, 0),
                    HeightRequest = _screenWidth * 0.12,
                    HorizontalOptions = LayoutOptions.Fill,
                    Content = upgradesGrid
                };
                upgradesContainer.Children.Add(upgradesBorder);
                foreach (var upgrade in _allUpgrades) AddUpgradeCard(upgrade, "Upgrade");
            }

            var sectionSpacer = new ContentView
            {
                HeightRequest = 35,
                BackgroundColor = Colors.Transparent,
                IsVisible = true
            };
            upgradesContainer.Children.Add(sectionSpacer);

            if (_allContracts.Count > 0)
            {
                var contractsGrid = new Grid();
                contractsGrid.Add(new Image { Source = "card_background_upsell.png", Aspect = Aspect.Fill, Opacity = 1 });
                var contractsHeader = new Border
                {
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 0 },
                    BackgroundColor = Colors.Transparent,
                    Padding = new Thickness(_screenWidth * 0.02),
                    Margin = new Thickness(0)
                };
                var headerLabel = new Label
                {
                    Text = "التعاقدات (أجر أسبوعي)",
                    Style = (Style)Application.Current.Resources["Upgrade_SectionHeader"],
                    FontSize = EstateUIConstants.FontSizeMedium
                };
                contractsHeader.Content = headerLabel;
                contractsGrid.Add(contractsHeader);
                var contractsBorder = new Border
                {
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
                    Padding = new Thickness(0),
                    Margin = new Thickness(50, EstateUIConstants.CardMarginVertical * 2, 50, 0),
                    HeightRequest = _screenWidth * 0.12,
                    HorizontalOptions = LayoutOptions.Fill,
                    Content = contractsGrid
                };
                upgradesContainer.Children.Add(contractsBorder);
                foreach (var contract in _allContracts) AddUpgradeCard(contract, "Contract");
            }

            ShowHappinessSummary();
        }
        catch (Exception ex) { DisplayAlert("خطأ", $"فشل تحميل البيانات: {ex.Message}", "موافق"); }
    }

    private void AddUpgradeCard(EstateUpgradeItem item, string type)
    {
        try
        {
            var isContract = (type == "Contract");
            var costLabel = isContract ? $"السعر: {NumberFormatter.FormatNumber(item.Cost)} " : $"السعر: {NumberFormatter.FormatNumber(item.Cost)} ";

            var border = new Border
            {
                Stroke = Colors.Transparent,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 0 },
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(0),
                Margin = new Thickness(0, 0, 0, EstateUIConstants.CardMarginVertical),
                MinimumHeightRequest = EstateUIConstants.CardMinHeight,
                HorizontalOptions = LayoutOptions.Fill
            };

            var mainGrid = new Grid();
            mainGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 1 });

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(_screenWidth * 0.28) }
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                },
                RowSpacing = EstateUIConstants.RowSpacing,
                Padding = new Thickness(_screenWidth * 0.03)
            };

            var titleStack = new VerticalStackLayout
            {
                Spacing = EstateUIConstants.StackSpacing,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill,
                Padding = new Thickness(10, 10, 10, 10)
            };
            titleStack.Children.Add(new Label
            {
                Text = item.Name,
                Style = (Style)Application.Current.Resources["Upgrade_ItemName"],
                HorizontalOptions = LayoutOptions.Center,
                FontSize = EstateUIConstants.FontSizeMedium,
                Margin = new Thickness(0, 0, 0, 10)
            });
            titleStack.Children.Add(new Label
            {
                Text = item.Description,
                Style = (Style)Application.Current.Resources["Upgrade_ItemDescription"],
                HorizontalOptions = LayoutOptions.Fill,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.WordWrap,
                MaxLines = 2,
                Margin = new Thickness(5, 0, 5, 0)
            });
            var statsRow = new HorizontalStackLayout
            {
                Spacing = 15,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };
            statsRow.Children.Add(new Label
            {
                Text = costLabel,
                Style = (Style)Application.Current.Resources["Upgrade_GoldenPrice"],
                FontSize = EstateUIConstants.FontSizeSmall
            });
            statsRow.Children.Add(new Label
            {
                Text = $"السعادة: +{NumberFormatter.FormatNumber(item.Happiness)}",
                Style = (Style)Application.Current.Resources["Upgrade_GoldenValue"],
                FontSize = EstateUIConstants.FontSizeSmall
            });
            titleStack.Children.Add(statsRow);
            grid.Add(titleStack, 0, 0);

            var statusStack = new VerticalStackLayout
            {
                Spacing = EstateUIConstants.ButtonSpacing,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            if (item.IsPurchased)
            {
                var badge = new Border
                {
                    Stroke = Colors.Transparent,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    BackgroundColor = Colors.Transparent,
                    Padding = 0,
                    HeightRequest = _screenWidth * 0.08,
                    WidthRequest = _screenWidth * 0.20,
                    HorizontalOptions = LayoutOptions.Center
                };
                var badgeGrid = new Grid();
                badgeGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 1 });
                var badgeLabel = new Label
                {
                    Text = isContract ? "نشط" : "مضاف",
                    Style = (Style)Application.Current.Resources["Upgrade_StatusBadge"],
                    FontSize = EstateUIConstants.FontSizeButton
                };
                badgeGrid.Add(badgeLabel);
                badge.Content = badgeGrid;
                statusStack.Children.Add(badge);
                if (isContract && item.ContractStartTime.HasValue)
                {
                    var daysRemaining = GetContractDaysRemaining(item);
                    var statusText = daysRemaining > 0 ? $"باقي {daysRemaining} يوم" : "منتهي";
                    var color = daysRemaining > 0 ? Colors.Goldenrod : Color.FromArgb("#e74c3c");
                    statusStack.Children.Add(new Label
                    {
                        Text = statusText,
                        Style = (Style)Application.Current.Resources["Upgrade_StatusText"],
                        TextColor = color,
                        FontSize = EstateUIConstants.FontSizeSmall
                    });
                }
            }
            else
            {
                var buttonBorder = CreateButtonWithBackground(isContract ? "تعاقد" : "إضافة");
                buttonBorder.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () => { await AnimateBorder(buttonBorder); OnPurchaseClicked(item, isContract); })
                });
                statusStack.Children.Add(buttonBorder);
            }
            grid.Add(statusStack, 1, 0);

            if (isContract && item.IsPurchased && item.ContractStartTime.HasValue)
            {
                var daysRemaining = GetContractDaysRemaining(item);
                if (daysRemaining <= 0)
                {
                    var renewButtonBorder = CreateButtonWithBackground("تجديد");
                    renewButtonBorder.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(async () => { await AnimateBorder(renewButtonBorder); OnRenewContract(item); })
                    });
                    grid.Add(renewButtonBorder, 0, 1, 2, 1);
                }
            }

            mainGrid.Add(grid);
            border.Content = mainGrid;
            var upgradesContainer = this.FindByName<VerticalStackLayout>("UpgradesContainer");
            upgradesContainer?.Children.Add(border);
        }
        catch (Exception ex) { DisplayAlert("خطأ", $"فشل عرض الكارت: {ex.Message}", "موافق"); }
    }

    private Border CreateButtonWithBackground(string text)
    {
        var border = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ButtonCornerRadiusRound },
            Padding = 0,
            BackgroundColor = Colors.Transparent,
            HeightRequest = EstateUIConstants.ButtonHeight,
            WidthRequest = EstateUIConstants.ButtonWidth,
            HorizontalOptions = LayoutOptions.Center
        };
        var buttonGrid = new Grid();
        buttonGrid.Add(new Image { Source = ButtonBackground, Aspect = Aspect.Fill });
        buttonGrid.Add(new Label
        {
            Text = text,
            Style = (Style)Application.Current.Resources["Upgrade_ActionButton"],
            TextColor = _goldTextColor,
            FontSize = EstateUIConstants.FontSizeButton
        });
        border.Content = buttonGrid;
        return border;
    }

    private int GetContractDaysRemaining(EstateUpgradeItem contract)
    {
        if (!contract.ContractStartTime.HasValue) return 0;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var elapsedDays = (now - contract.ContractStartTime.Value) / (24 * 60 * 60 * 1000);
        return Math.Max(0, 7 - (int)elapsedDays);
    }

    private async void OnPurchaseClicked(EstateUpgradeItem item, bool isContract)
    {
        try
        {
            if (_player.Gold < item.Cost)
            {
                await ToastService.Show($"ليس لديك ذهب كافي!\nتحتاج {NumberFormatter.FormatNumber(item.Cost)} ", ToastType.Error);
                return;
            }
            var itemType = isContract ? "تعاقد" : "ترقية";

            var confirm = await PopupService.ShowConfirmAsync(
                title: $"تأكيد {itemType}",
                message: $"{item.Name}\nالسعر: {NumberFormatter.FormatNumber(item.Cost)}\nالسعادة: +{NumberFormatter.FormatNumber(item.Happiness)}",
                operationType: PopupOperationType.Confirm,
                positiveColor: _goldTextColor,
                negativeColor: _goldTextColor,
                positiveImage: ButtonBackground,
                negativeImage: ButtonBackgroundNo,
                onPositive: async () =>
                {
                    _player.Gold -= item.Cost;
                    item.IsPurchased = true;
                    if (isContract)
                    {
                        item.ContractStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        _estate.ActiveContracts.Add(item.Name);
                        _estate.ContractStartTimes[item.Name] = item.ContractStartTime.Value;
                    }
                    else
                    {
                        _estate.PurchasedUpgrades.Add(item.Name);
                    }
                    LoadUpgradeData();
                    await ToastService.Show($"{item.Name} {itemType} بنجاح!", ToastType.Success);
                });
        }
        catch (Exception ex) { await ToastService.Show($"فشل الشراء: {ex.Message}", ToastType.Error); }
    }

    private async void OnRenewContract(EstateUpgradeItem contract)
    {
        try
        {
            if (_player.Gold < contract.Cost)
            {
                await ToastService.Show($"ليس لديك ذهب كافي للتجديد!\nتحتاج {NumberFormatter.FormatNumber(contract.Cost)} ", ToastType.Error);
                return;
            }
            var confirm = await PopupService.ShowConfirmAsync(
                title: "تجديد التعاقد",
                message: $"تجديد {contract.Name} لمدة أسبوع آخر?\nالسعر: {NumberFormatter.FormatNumber(contract.Cost)} ",
                operationType: PopupOperationType.Confirm,
                positiveColor: _goldTextColor,
                negativeColor: _goldTextColor,
                positiveImage: ButtonBackground,
                negativeImage: ButtonBackgroundNo,
                onPositive: async () =>
                {
                    _player.Gold -= contract.Cost;
                    contract.ContractStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _estate.ContractStartTimes[contract.Name] = contract.ContractStartTime.Value;
                    LoadUpgradeData();
                    await ToastService.Show($"تم تجديد {contract.Name} لمدة أسبوع!", ToastType.Success);
                });
        }
        catch (Exception ex) { await ToastService.Show($"فشل التجديد: {ex.Message}", ToastType.Error); }
    }

    private void ShowHappinessSummary()
    {
        try
        {
            var baseHappiness = EstateObject.EstateTypes[_estate.Id].Happiness;
            var upgradeHappiness = _allUpgrades.Where(u => u.IsPurchased).Sum(u => u.Happiness);
            var contractHappiness = _allContracts.Where(c => c.IsPurchased && GetContractDaysRemaining(c) > 0).Sum(c => c.Happiness);
            var totalHappiness = baseHappiness + upgradeHappiness + contractHappiness;

            var summaryGrid = new Grid();
            summaryGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 1 });
            var summary = new Border
            {
                Stroke = Colors.Transparent,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(_screenWidth * 0.04),
                Margin = new Thickness(25, _screenWidth * 0.02, 25, 0)
            };
            var summaryStack = new VerticalStackLayout { Spacing = EstateUIConstants.StackSpacing, Padding = new Thickness(_screenWidth * 0.04) };
            summaryStack.Children.Add(new Label
            {
                Text = "ملخص السعادة",
                Style = (Style)Application.Current.Resources["Upgrade_SummaryTitle"],
                FontSize = EstateUIConstants.FontSizeMedium
            });
            summaryStack.Children.Add(new Label
            {
                Text = $"أساسية: {NumberFormatter.FormatNumber(baseHappiness)} + ترقيات: {NumberFormatter.FormatNumber(upgradeHappiness)} + تعاقدات: {NumberFormatter.FormatNumber(contractHappiness)}",
                Style = (Style)Application.Current.Resources["Upgrade_SummaryValue"],
                FontSize = EstateUIConstants.FontSizeSmall
            });
            summaryStack.Children.Add(new Label
            {
                Text = $"المجموع: {NumberFormatter.FormatNumber(totalHappiness)}",
                Style = (Style)Application.Current.Resources["Upgrade_SummaryTotal"],
                FontSize = EstateUIConstants.FontSizeMedium
            });
            summaryGrid.Add(summaryStack);
            summary.Content = summaryGrid;
            var upgradesContainer = this.FindByName<VerticalStackLayout>("UpgradesContainer");
            upgradesContainer?.Children.Add(summary);
        }
        catch (Exception ex) { DisplayAlert("خطأ", $"فشل عرض الملخص: {ex.Message}", "موافق"); }
    }

    private async void OnImageChangeClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        if (!_isPrivateDomain) return;
        try
        {
            var status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted) { await ToastService.Show("لا يمكن الوصول إلى التخزين لتغيير الصورة", ToastType.Error); return; }
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "اختر صورة للعقار" });
            if (result == null) return;
            using var stream = await result.OpenReadAsync();
            if (stream.Length > MAX_IMAGE_SIZE_BYTES) { await ToastService.Show($"الصورة تتجاوز الحد الأقصى (2 ميجابايت)!", ToastType.Error); return; }
            string localPath = await SaveImageToLocalStorage(result);
            _estate.EstateImageUrl = localPath;
            var playerEstate = _player.Estates.FirstOrDefault(est => est.InstanceId == _estate.InstanceId);
            if (playerEstate != null) playerEstate.EstateImageUrl = localPath;
            await ToastService.Show("تم تغيير صورة العقار بنجاح!", ToastType.Success);
        }
        catch (Exception ex) { await ToastService.Show($"فشل تغيير الصورة: {ex.Message}", ToastType.Error); }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopAsync(false);
    }

    private async Task<string> SaveImageToLocalStorage(FileResult imageFile)
    {
        string fileName = $"estate_{_estate.InstanceId}_{Guid.NewGuid():N}{System.IO.Path.GetExtension(imageFile.FileName)}";
        string localPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, fileName);
        using var sourceStream = await imageFile.OpenReadAsync();
        using var destStream = System.IO.File.Create(localPath);
        await sourceStream.CopyToAsync(destStream);
        return localPath;
    }
}