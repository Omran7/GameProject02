using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class EstateOwnPage : ContentPage
{
    private PlayerAccount _player;

    public EstateOwnPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEstateData();
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

    private void LoadEstateData()
    {
        try
        {
            _player = AccountService.GetCurrentPlayer();
            var container = this.FindByName<VerticalStackLayout>("EstatesContainer");
            if (container == null) return;
            container.Children.Clear();

            if (_player == null)
            {
                AddErrorMessage("خطأ: لم يتم تحميل بيانات اللاعب!\nالرجاء إعادة تشغيل اللعبة.");
                return;
            }

            if (_player.Estates == null || _player.Estates.Count == 0)
            {
                AddEmptyMessage("لم تشتري عقار بعد!\nاشتري عقار جديد من قسم 'عقارات جديدة'");
                return;
            }

            foreach (var estate in _player.Estates)
            {
                if (string.IsNullOrEmpty(estate.InstanceId))
                    estate.InstanceId = Guid.NewGuid().ToString().Substring(0, 8);
            }

            if (!string.IsNullOrEmpty(_player.PrimaryResidenceEstateInstanceId))
            {
                var primaryExists = _player.Estates.Any(e => e.InstanceId == _player.PrimaryResidenceEstateInstanceId);
                if (!primaryExists)
                {
                    var fallback = _player.Estates.FirstOrDefault(e => e.Id == _player.PrimaryResidenceEstateId);
                    _player.PrimaryResidenceEstateInstanceId = fallback?.InstanceId ?? string.Empty;
                }
            }
            else
            {
                var primaryEstate = _player.Estates.FirstOrDefault(e => e.Id == _player.PrimaryResidenceEstateId);
                _player.PrimaryResidenceEstateInstanceId = primaryEstate?.InstanceId ?? string.Empty;
            }

            var estatesByType = _player.Estates.GroupBy(e => e.Id).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var group in estatesByType.Values)
            {
                for (int i = 0; i < group.Count; i++)
                    group[i].InstanceNumber = i + 1;
            }

            var primaryResidenceEstate = _player.Estates.FirstOrDefault(e =>
                !string.IsNullOrEmpty(_player.PrimaryResidenceEstateInstanceId) &&
                e.InstanceId == _player.PrimaryResidenceEstateInstanceId);

            var otherEstates = _player.Estates
                .Where(e => string.IsNullOrEmpty(_player.PrimaryResidenceEstateInstanceId) ||
                            e.InstanceId != _player.PrimaryResidenceEstateInstanceId)
                .OrderByDescending(e => EstateObject.EstateTypes.TryGetValue(e.Id, out var type) ? type.Cost : 0)
                .ToList();

            var sortedEstates = new List<EstateObject>();
            if (primaryResidenceEstate != null) sortedEstates.Add(primaryResidenceEstate);
            sortedEstates.AddRange(otherEstates);

            var rentedOutEstates = sortedEstates.Where(e => e.IsRentedOut).OrderBy(e => e.RentEndTime).ToList();
            var normalEstates = sortedEstates.Where(e => !e.IsRentedOut).ToList();

            foreach (var estate in rentedOutEstates)
                container.Children.Add(CreateEstateCard(estate, isRentedOut: true));
            foreach (var estate in normalEstates)
                container.Children.Add(CreateEstateCard(estate, isRentedOut: false));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LoadEstateData: {ex.Message}");
        }
    }

    private void AddErrorMessage(string msg)
    {
        var container = this.FindByName<VerticalStackLayout>("EstatesContainer");
        if (container == null) return;
        container.Children.Add(new Label
        {
            Text = msg,
            Style = (Style)Application.Current.Resources["ErrorMessage"]
        });
    }

    private void AddEmptyMessage(string msg)
    {
        var container = this.FindByName<VerticalStackLayout>("EstatesContainer");
        if (container == null) return;
        container.Children.Add(new Label
        {
            Text = msg,
            Style = (Style)Application.Current.Resources["EmptyMessage"]
        });
    }

    private Border CreateEstateCard(EstateObject estate, bool isRentedOut = false)
    {
        try
        {
            bool isRented = estate.IsRentedEstate;
            bool isListedForRent = !string.IsNullOrEmpty(estate.InstanceId) &&
                                   RentalService.GetListedEstateInstanceIds(_player.PlayerId).Contains(estate.InstanceId);
            bool isListedForSale = !string.IsNullOrEmpty(estate.InstanceId) &&
                                   UsedEstateService.GetListedEstateInstanceIds(_player.PlayerId).Contains(estate.InstanceId);
            bool isPrimaryResidence = !string.IsNullOrEmpty(_player.PrimaryResidenceEstateInstanceId) &&
                                     estate.InstanceId == _player.PrimaryResidenceEstateInstanceId;
            bool isShack = estate.Id == 0;

            var border = new Border
            {
                Stroke = Colors.Transparent,
                BackgroundColor = Colors.Transparent,
                Padding = 0,
                Margin = new Thickness(0, EstateUIConstants.CardMarginVertical),
                MinimumHeightRequest = EstateUIConstants.CardMinHeight,
                StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius }
            };

            var mainGrid = new Grid();
            var backgroundImage = new Image
            {
                Source = "card_background.png",
                Aspect = Aspect.Fill,
                Opacity = 1
            };
            mainGrid.Add(backgroundImage);

            var grid = new Grid
            {
                ColumnSpacing = EstateUIConstants.ColumnSpacing,
                Padding = new Thickness(EstateUIConstants.CardContentPadding, 0),
                VerticalOptions = LayoutOptions.Center,
                FlowDirection = FlowDirection.LeftToRight,
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var buttonsContainer = CreateButtonsForEstate(estate, isRented, isListedForRent, isListedForSale, isPrimaryResidence, isShack, isRentedOut);
            if (buttonsContainer.Children.Count == 0)
            {
                var dummy = new BoxView { WidthRequest = EstateUIConstants.ButtonWidth, HeightRequest = 0, Opacity = 0 };
                buttonsContainer.Children.Add(dummy);
            }

            grid.Add(buttonsContainer, 0, 0);
            grid.Add(CreateDetailsStack(estate, isPrimaryResidence, isListedForSale, isListedForRent, isShack, isRented, isRentedOut), 1, 0);
            grid.Add(CreateImageContainer(estate), 2, 0);

            mainGrid.Add(grid);
            border.Content = mainGrid;
            return border;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CreateEstateCard: {ex.Message}");
            return new Border();
        }
    }

    private VerticalStackLayout CreateButtonsForEstate(EstateObject estate, bool isRented, bool isListedForRent, bool isListedForSale, bool isPrimaryResidence, bool isShack, bool isRentedOut)
    {
        var stack = new VerticalStackLayout
        {
            Spacing = EstateUIConstants.ButtonSpacing,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 0)
        };

        if (isRentedOut)
        {
            stack.Children.Add(CreateBadge("مؤجر", "#c0392b"));
            return stack;
        }

        if (isRented)
        {
            if (isPrimaryResidence)
            {
                stack.Children.Add(CreateBadge("مستأجر", "#c0392b"));
            }
            else
            {
                var btnMove = CreateButton("انتقل", "#000000", "button_background.png");
                btnMove.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () => { await AnimateBorder(btnMove); await OnMoveToRentedEstateClicked(estate); })
                });
                stack.Children.Add(btnMove);
            }
        }
        else if (isListedForSale)
        {
            var btn = CreateButton("إزالة", "#FFFFFF", "button_background_no.png");
            btn.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => { await AnimateBorder(btn); OnRemoveFromSaleClicked(estate); }) });
            stack.Children.Add(btn);
        }
        else if (isListedForRent)
        {
            var btn = CreateButton("إزالة", "#FFFFFF", "button_background_no.png");
            btn.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => { await AnimateBorder(btn); OnRemoveFromRentClicked(estate); }) });
            stack.Children.Add(btn);
        }
        else if (isPrimaryResidence)
        {
            if (isShack)
            {
                stack.Children.Add(CreateBadge("لا يباع", "#c0392b"));
            }
            else
            {
                var btn = CreateButton("ترقيات", "#FFFFFF", "button_background.png");
                btn.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => { await AnimateBorder(btn); OnUpgradeClicked(estate); }) });
                stack.Children.Add(btn);
            }
        }
        else
        {
            if (isShack)
            {
                var btnLive = CreateButton("انتقل", "#FFFFFF", "button_background.png");
                btnLive.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => { await AnimateBorder(btnLive); OnLiveHereClicked(estate); }) });
                stack.Children.Add(btnLive);
            }
            else
            {
                var btnLive = CreateButton("انتقل", "#FFFFFF", "button_background.png");
                btnLive.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => { await AnimateBorder(btnLive); OnLiveHereClicked(estate); }) });
                stack.Children.Add(btnLive);
                var btnSell = CreateButton("بيع", "#FFFFFF", "button_background_no.png");
                btnSell.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => { await AnimateBorder(btnSell); OnSellClicked(estate); }) });
                stack.Children.Add(btnSell);
            }
        }
        return stack;
    }

    private async Task OnMoveToRentedEstateClicked(EstateObject estate)
    {
        var confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد الانتقال",
            message: $"هل تريد الانتقال للعيش في {estate.GetEstateTypeName()} (مستأجر)؟\nلا يمكنك ترقية او بيع العقار",
            operationType: PopupOperationType.Move,
            positiveColor: EstateUIConstants.TextDark,
            onPositive: async () =>
            {
                _player.PrimaryResidenceEstateInstanceId = estate.InstanceId;
                _player.PrimaryResidenceEstateId = estate.Id;
                LoadEstateData();
                await ToastService.Show($"انتقلت إلى {estate.GetEstateTypeName()} بنجاح!", ToastType.Success);
            }
        );
    }

    private Grid CreateDetailsStack(EstateObject estate, bool isPrimaryResidence, bool isListedForSale, bool isListedForRent, bool isShack, bool isRented, bool isRentedOut)
    {
        var grid = new Grid
        {
            RowSpacing = EstateUIConstants.RowSpacing,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0),
            Padding = new Thickness(0)
        };

        string displayName = estate.GetEstateTypeName();
        if (estate.InstanceNumber > 1) displayName += $" #{estate.InstanceNumber}";

        var nameLabel = new Label
        {
            Text = displayName,
            Style = (Style)Application.Current.Resources["EstateName"],
            FontSize = EstateUIConstants.FontSizeMedium,
            Margin = new Thickness(0, 0, 0, 0)
        };
        grid.Add(nameLabel, 0, 0);

        if (isRentedOut)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var timeLeftMs = estate.RentEndTime - now;
            var daysLeft = Math.Max(0, (int)Math.Ceiling(timeLeftMs / (24.0 * 60 * 60 * 1000)));
            var hoursLeft = (int)Math.Floor((timeLeftMs % (24 * 60 * 60 * 1000)) / (60 * 60 * 1000.0));
            var timeText = daysLeft > 0 ? $"{daysLeft} يوم" : (hoursLeft > 0 ? $"{hoursLeft} ساعة" : "أقل من ساعة");

            var rentInfoLabel = new Label
            {
                Text = $"مؤجر إلى: {estate.RentedToPlayerName}\nيعود بعد: {timeText}",
                Style = (Style)Application.Current.Resources["EstateStatistic"],
                FontSize = EstateUIConstants.FontSizeSmall,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            grid.Add(rentInfoLabel, 0, 1);
        }
        else
        {
            var happinessLabel = new Label
            {
                Text = $"{NumberFormatter.FormatNumber(estate.GetHappiness(_player))} السعادة",
                Style = (Style)Application.Current.Resources["EstateStatistic"],
                FontSize = EstateUIConstants.FontSizeSmall
            };
            grid.Add(happinessLabel, 0, 1);

            var taxLabel = new Label
            {
                Text = $"{NumberFormatter.FormatNumber(estate.GetDailyTax())} ذهب/يوم",
                Style = (Style)Application.Current.Resources["EstateStatistic"],
                FontSize = EstateUIConstants.FontSizeSmall
            };
            grid.Add(taxLabel, 0, 2);
        }

        string statusText = "";
        bool isBold = false;
        if (isPrimaryResidence)
        {
            statusText = estate.IsRentedEstate ? "مقر اقامتك الحالي (مستأجر)" : "مقر اقامتك الحالي";
            isBold = true;
        }
        else if (isRentedOut)
        {
            statusText = " مؤجر (هذا العقار مؤجر حاليا )";
            isBold = false;
        }
        else if (isListedForSale)
        {
            statusText = "هذا العقار معروض للبيع";
            isBold = true;
        }
        else if (isListedForRent)
        {
            statusText = "هذا العقار معروض للايجار";
            isBold = true;
        }
        else if (isRented)
        {
            statusText = "مستأجر (لا يمكن ترقية او بيع العقار)";
            isBold = false;
        }

        if (!string.IsNullOrEmpty(statusText))
        {
            int statusRow = isRentedOut ? 2 : 3;
            var statusLabel = new Label
            {
                Text = statusText,
                Style = (Style)Application.Current.Resources["StatusText"],
                FontAttributes = isBold ? FontAttributes.Bold : FontAttributes.None,
                FontSize = EstateUIConstants.FontSizeTiny
            };
            grid.Add(statusLabel, 0, statusRow);
        }

        return grid;
    }

    private Border CreateImageContainer(EstateObject estate)
    {
        double imageSize = EstateUIConstants.ImageSize;
        double frameSize = imageSize * 1.15;

        var imageBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ImageCornerRadius },
            Padding = new Thickness(0),
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0),
            BackgroundColor = Colors.Transparent
        };

        if (estate.Id == 15)
        {
            imageBorder.WidthRequest = frameSize;
            imageBorder.HeightRequest = frameSize;
            imageBorder.StrokeShape = new RoundRectangle { CornerRadius = 0 };

            string innerImageSource = string.IsNullOrWhiteSpace(estate.EstateImageUrl) ? "estate_15.png" : estate.EstateImageUrl;
            var innerImage = new Image
            {
                Source = innerImageSource,
                Aspect = Aspect.Fill,
                WidthRequest = imageSize,
                HeightRequest = imageSize,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            var frame = new Image
            {
                Source = "estate_hrm",
                Aspect = Aspect.Fill,
                InputTransparent = true,
                WidthRequest = frameSize,
                HeightRequest = frameSize
            };
            var grid = new Grid();
            grid.Children.Add(innerImage);
            grid.Children.Add(frame);
            imageBorder.Content = grid;
        }
        else
        {
            imageBorder.WidthRequest = imageSize;
            imageBorder.HeightRequest = imageSize;
            var image = new Image
            {
                Source = estate.GetImageSource(),
                Aspect = Aspect.AspectFill,
                WidthRequest = imageSize,
                HeightRequest = imageSize,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            imageBorder.Content = image;
        }

        return imageBorder;
    }

    private Border CreateBadge(string text, string colorHex)
    {
        var badge = new Border
        {
            Stroke = Colors.Transparent,
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(0, 0),
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = EstateUIConstants.ButtonWidth,
            HeightRequest = EstateUIConstants.ButtonHeight,
            StrokeShape = new RoundRectangle { CornerRadius = 8 }
        };
        var grid = new Grid();
        grid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 0.8 });
        grid.Add(new Label
        {
            Text = text,
            Style = (Style)Application.Current.Resources["BadgeText"],
            TextColor = Color.FromArgb(colorHex)
        });
        badge.Content = grid;
        return badge;
    }

    private Border CreateButton(string text, string textColorHex, string imageSource = "button_background.png")
    {
        var border = new Border
        {
            Stroke = Colors.Transparent,
            Padding = 0,
            BackgroundColor = Colors.Transparent,
            HeightRequest = EstateUIConstants.ButtonHeight,
            WidthRequest = EstateUIConstants.ButtonWidth,
            HorizontalOptions = LayoutOptions.Fill,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ButtonCornerRadius }
        };
        var borderGrid = new Grid();
        borderGrid.Add(new Image { Source = imageSource, Aspect = Aspect.Fill });
        borderGrid.Add(new Label
        {
            Text = text,
            Style = (Style)Application.Current.Resources["CardButton"],
            TextColor = Color.FromArgb(textColorHex),
            FontSize = EstateUIConstants.FontSizeButton
        });
        border.Content = borderGrid;
        return border;
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

    private async void OnLiveHereClicked(EstateObject estate)
    {
        if (estate.IsRentedOut)
        {
            await ToastService.Show("لا يمكن الانتقال إلى عقار مؤجر!", ToastType.Error);
            return;
        }
        var confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد الانتقال",
            message: $"هل تريد الانتقال للعيش في {estate.GetEstateTypeName()}؟",
            operationType: PopupOperationType.Move,
            positiveColor: Colors.Black,
            onPositive: async () =>
            {
                _player.PrimaryResidenceEstateInstanceId = estate.InstanceId;
                _player.PrimaryResidenceEstateId = estate.Id;
                LoadEstateData();
                await ToastService.Show($"أصبح {estate.GetEstateTypeName()} مقر إقامتك الرئيسي!", ToastType.Success);
            }
        );
    }

    private async void OnUpgradeClicked(EstateObject estate)
    {
        if (estate.IsRentedOut)
        {
            await ToastService.Show("لا يمكن ترقية عقار مؤجر!", ToastType.Error);
            return;
        }
        if (string.IsNullOrEmpty(_player.PrimaryResidenceEstateInstanceId) || estate.InstanceId != _player.PrimaryResidenceEstateInstanceId)
        {
            await PopupService.ShowAlertAsync("تنبيه", "يمكنك ترقية مقر إقامتك الرئيسي فقط!");
            return;
        }
        await Navigation.PushAsync(new EstateUpgradePage(estate), false);
    }

    private async void OnRemoveFromSaleClicked(EstateObject estate)
    {
        if (estate.IsRentedOut)
        {
            await ToastService.Show("لا يمكن إزالة عقار مؤجر من البيع!", ToastType.Error);
            return;
        }
        var listing = UsedEstateService.GetAvailableListingsGrouped()
            .SelectMany(g => g.Value)
            .FirstOrDefault(l => l.EstateInstanceId == estate.InstanceId && l.SellerId == _player.PlayerId);
        if (listing == null)
        {
            await PopupService.ShowAlertAsync("تنبيه", "هذا العقار غير معروض للبيع حالياً!");
            return;
        }
        var confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد الإزالة",
            message: $"هل تريد إزالة {estate.GetEstateTypeName()} من قائمة البيع؟",
            operationType: PopupOperationType.Remove,
            positiveColor: Color.FromArgb("#0a0a0a"),
            onPositive: async () =>
            {
                var (success, message) = UsedEstateService.RemoveListing(listing.ListingId);
                if (success) LoadEstateData();
                await ToastService.Show(success ? "تم الإزالة بنجاح!" : message, success ? ToastType.Success : ToastType.Error);
            }
        );
    }

    private async void OnRemoveFromRentClicked(EstateObject estate)
    {
        if (estate.IsRentedOut)
        {
            await ToastService.Show("لا يمكن إزالة عقار مؤجر من الإيجار!", ToastType.Error);
            return;
        }
        var listing = RentalService.GetAvailableListingsGrouped()
            .SelectMany(g => g.Value)
            .FirstOrDefault(l => l.EstateInstanceId == estate.InstanceId && l.OwnerId == _player.PlayerId);
        if (listing == null)
        {
            await PopupService.ShowAlertAsync("تنبيه", "هذا العقار غير معروض للإيجار حالياً!");
            return;
        }
        var confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد الإزالة",
            message: $"هل تريد إزالة {estate.GetEstateTypeName()} من قائمة الإيجار؟",
            operationType: PopupOperationType.Remove,
            positiveColor: Color.FromArgb("#0a0a0a"),
            onPositive: async () =>
            {
                var (success, message) = RentalService.RemoveListing(listing.ListingId);
                if (success) LoadEstateData();
                await ToastService.Show(success ? "تم الإزالة بنجاح!" : message, success ? ToastType.Success : ToastType.Error);
            }
        );
    }

    private async void OnSellClicked(EstateObject estate)
    {
        if (estate.IsRentedOut)
        {
            await ToastService.Show("لا يمكن بيع عقار مؤجر!", ToastType.Error);
            return;
        }
        if (estate.Id == 0)
        {
            await PopupService.ShowAlertAsync("تنبيه", "لا يمكن بيع العشة أبداً!");
            return;
        }
        if (RentalService.GetListedEstateInstanceIds(_player.PlayerId).Contains(estate.InstanceId))
        {
            await PopupService.ShowAlertAsync("تنبيه", "هذا العقار معروض للإيجار حالياً!");
            return;
        }
        if (UsedEstateService.GetListedEstateInstanceIds(_player.PlayerId).Contains(estate.InstanceId))
        {
            await PopupService.ShowAlertAsync("تنبيه", "هذا العقار معروض للبيع حالياً!");
            return;
        }
        bool wasPrimaryResidence = !string.IsNullOrEmpty(_player.PrimaryResidenceEstateInstanceId) && estate.InstanceId == _player.PrimaryResidenceEstateInstanceId;
        string moveWarning = wasPrimaryResidence ? "\nسيتم نقل مقر إقامتك تلقائياً إلى العشة." : "";
        var originalCost = EstateObject.EstateTypes[estate.Id].Cost;
        int sellPrice = (int)(originalCost * 0.5);
        var confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد البيع",
            message: $"هل تريد بيع {estate.GetEstateTypeName()} للنظام؟\nالسعر: {NumberFormatter.FormatNumber(sellPrice)} ذهب (50%){moveWarning}",
            operationType: PopupOperationType.Sell,
            positiveColor: Color.FromArgb("#0a0a0a"),
            negativeColor: Color.FromArgb("#1a1a1a"),
            onPositive: async () =>
            {
                if (wasPrimaryResidence)
                {
                    _player.PrimaryResidenceEstateInstanceId = string.Empty;
                    _player.PrimaryResidenceEstateId = 0;
                }
                _player.Estates.Remove(estate);
                _player.Gold += sellPrice;
                LoadEstateData();
                await ToastService.Show($"تم البيع! حصلت على {NumberFormatter.FormatNumber(sellPrice)} ذهب", ToastType.Success);
            }
        );
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopAsync(false);
    }
}