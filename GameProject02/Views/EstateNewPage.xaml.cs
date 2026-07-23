using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class EstateNewPage : ContentPage
{
    private PlayerAccount _player;
    private List<EstateItem> _estates;

    public EstateNewPage()
    {
        InitializeComponent();
        LoadEstateData();
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
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        EstatesContainer.Children.Clear();
        _estates = new List<EstateItem>
        {
            new EstateItem { Id = 1, Name = "كوخ قش", Happiness = 120, Cost = 1000, CostDisplay = "1 الف", Description = "1", Image = "estate_1.png" },
            new EstateItem { Id = 2, Name = "كوخ خشبي", Happiness = 150, Cost = 5000, CostDisplay = "5 الاف", Description = "5", Image = "estate_2.png" },
            new EstateItem { Id = 3, Name = "بيت صغير", Happiness = 180, Cost = 10000, CostDisplay = "10 الاف", Description = "10", Image = "estate_3.png" },
            new EstateItem { Id = 4, Name = "بيت ريفي", Happiness = 200, Cost = 50000, CostDisplay = "50 الف", Description = "50", Image = "estate_4.png" },
            new EstateItem { Id = 5, Name = "بيت الشجرة", Happiness = 450, Cost = 1000000, CostDisplay = "1 مليون", Description = "1K", Image = "estate_5.png" },
            new EstateItem { Id = 6, Name = "منزل عائلي", Happiness = 700, Cost = 3000000, CostDisplay = "3 مليون", Description = "3K", Image = "estate_6.png" },
            new EstateItem { Id = 7, Name = "فيلا خشبيه", Happiness = 810, Cost = 5000000, CostDisplay = "5 مليون", Description = "5K", Image = "estate_7.png" },
            new EstateItem { Id = 8, Name = "بيت السعادة", Happiness = 850, Cost = 6000000, CostDisplay = "6 مليون", Description = "6K", Image = "estate_8.png" },
            new EstateItem { Id = 9, Name = "فيلا مرفهة", Happiness = 900, Cost = 8000000, CostDisplay = "8 مليون", Description = "8K", Image = "estate_9.png" },
            new EstateItem { Id = 10, Name = "قلعة خشبيه", Happiness = 1350, Cost = 10000000, CostDisplay = "10 مليون", Description = "10K", Image = "estate_10.png" },
            new EstateItem { Id = 11, Name = "فيلا بطوابق", Happiness = 2100, Cost = 20000000, CostDisplay = "20 مليون", Description = "20K", Image = "estate_11.png" },
            new EstateItem { Id = 12, Name = "قصر حجري", Happiness = 3220, Cost = 50000000, CostDisplay = "50 مليون", Description = "50K", Image = "estate_12.png" },
            new EstateItem { Id = 13, Name = "قلعة ساحرة", Happiness = 4100, Cost = 100000000, CostDisplay = "100 مليون", Description = "100K", Image = "estate_13.png" },
            new EstateItem { Id = 14, Name = "قصر ملكي", Happiness = 4500, Cost = 400000000, CostDisplay = "400 مليون", Description = "400K", Image = "estate_14.png" },
            new EstateItem { Id = 15, Name = "هرم فرعوني", Happiness = 5000, Cost = 600000000, CostDisplay = "600 مليون", Description = "600K", Image = "estate_15.png", Note = "يمكنك تغيير صورة العقار" },
        };

        _estates = _estates.OrderBy(e => e.Cost).ToList();

        foreach (var estate in _estates)
        {
            var card = CreateEstateCard(estate);
            EstatesContainer.Children.Add(card);
        }
    }

    private Border CreateEstateCard(EstateItem estate)
    {
        double imageSize = EstateUIConstants.ImageSize;
        double buttonWidth = EstateUIConstants.ButtonWidth;
        double buttonHeight = EstateUIConstants.ButtonHeight;

        var mainBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
            Padding = new Thickness(0),
            Margin = new Thickness(0, EstateUIConstants.CardMarginVertical),
            MinimumHeightRequest = EstateUIConstants.CardMinHeight,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill
        };

        var mainGrid = new Grid();
        mainGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 1 });

        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = EstateUIConstants.ColumnSpacing,
            Padding = new Thickness(EstateUIConstants.CardContentPadding, 0),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };

        var imageBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ImageCornerRadius },
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };

        if (estate.Id == 15)
        {
            double frameSize = imageSize * 1.15;
            imageBorder.WidthRequest = frameSize;
            imageBorder.HeightRequest = frameSize;

            var innerImage = new Image
            {
                Source = "estate_15.png",
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
                WidthRequest = frameSize,
                HeightRequest = frameSize,
                InputTransparent = true
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
            var estateImage = new Image
            {
                Source = estate.Image,
                Aspect = Aspect.Fill,
                WidthRequest = imageSize,
                HeightRequest = imageSize,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            imageBorder.Content = estateImage;
        }

        contentGrid.Add(imageBorder, 0);

        var detailsStack = new VerticalStackLayout
        {
            Spacing = EstateUIConstants.StackSpacing,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 0)
        };

        detailsStack.Children.Add(new Label
        {
            Text = estate.Name,
            Style = (Style)Application.Current.Resources["EstateNew_CardTitle"],
            FontSize = EstateUIConstants.FontSizeMedium,
            Margin = new Thickness(0, 0, 0, 0)
        });
        detailsStack.Children.Add(new Label
        {
            Text = $"السعر: {estate.CostDisplay} 💵",
            Style = (Style)Application.Current.Resources["EstateNew_Statistic"],
            FontSize = EstateUIConstants.FontSizeSmall
        });
        detailsStack.Children.Add(new Label
        {
            Text = $"السعادة: {NumberFormatter.FormatNumber(estate.Happiness)} 😊",
            Style = (Style)Application.Current.Resources["EstateNew_Statistic"],
            FontSize = EstateUIConstants.FontSizeSmall
        });
        detailsStack.Children.Add(new Label
        {
            Text = $"الضريبة: {estate.Description:N0} 💸",
            Style = (Style)Application.Current.Resources["EstateNew_Statistic"],
            FontSize = EstateUIConstants.FontSizeSmall
        });
        contentGrid.Add(detailsStack, 1);

        var buttonBorder = new Border
        {
            Stroke = EstateUIConstants.TextDark,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ButtonCornerRadius },
            WidthRequest = buttonWidth,
            HeightRequest = buttonHeight,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            Padding = new Thickness(0),
            BackgroundColor = Colors.Transparent
        };
        var buttonGrid = new Grid();
        buttonGrid.Add(new Image { Source = "button_background.png", Aspect = Aspect.Fill });
        buttonGrid.Add(new Label
        {
            Text = "شراء",
            Style = (Style)Application.Current.Resources["EstateNew_BuyButton"],
            FontSize = EstateUIConstants.FontSizeButton
        });
        buttonBorder.Content = buttonGrid;
        buttonBorder.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await OnBuyWithAnimation(estate.Id, buttonBorder))
        });

        contentGrid.Add(buttonBorder, 2);
        mainGrid.Add(contentGrid);
        mainBorder.Content = mainGrid;
        return mainBorder;
    }

    private async Task OnBuyWithAnimation(int estateId, Border buttonBorder)
    {
        if (buttonBorder != null)
        {
            await buttonBorder.ScaleTo(EstateUIConstants.AnimationPressScale, EstateUIConstants.AnimationPressDuration, Easing.CubicOut);
            await buttonBorder.ScaleTo(1, EstateUIConstants.AnimationPressDuration, Easing.CubicIn);
        }
        BuyEstate(estateId);
    }

    private async void BuyEstate(int estateId)
    {
        if (_player == null) return;
        var estate = _estates.Find(e => e.Id == estateId);
        if (estate == null) return;

        if (_player.Gold < estate.Cost)
        {
            await ToastService.Show("للأسف ليس لديك المال الكافي", ToastType.Error);
            return;
        }

        var monthlyTax = estate.Cost * 0.001;

        var confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد الشراء",
            message: $"{estate.Name}\n💵 السعر: {NumberFormatter.FormatNumber(estate.Cost)}\n😊 السعادة: {NumberFormatter.FormatNumber(estate.Happiness)}\n💸 الضريبة: {NumberFormatter.FormatNumber((int)monthlyTax)}",
            operationType: PopupOperationType.Confirm,
            positiveColor: EstateUIConstants.TextDark,
            negativeColor: EstateUIConstants.TextDark,
            positiveImage: "button_background.png",
            negativeImage: "button_background_no.png"
        );

        if (!confirm) return;

        _player.Gold -= estate.Cost;
        var newEstate = new EstateObject
        {
            Id = estate.Id,
            InstanceId = Guid.NewGuid().ToString().Substring(0, 8),
            EstateOwnerId = _player.PlayerId,
            IsUsed = false,
            LastTaxPaidTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FixedModifications = new List<bool>(),
            ServantContractStartTimes = new List<long>(),
            PurchasedUpgrades = new List<string>(),
            ActiveContracts = new List<string>(),
            ContractStartTimes = new Dictionary<string, long>()
        };
        if (_player.Estates == null) _player.Estates = new List<EstateObject>();
        _player.Estates.Add(newEstate);
        if (_player.Estates.Count == 2 && _player.PrimaryResidenceEstateId == 0)
        {
            _player.PrimaryResidenceEstateId = estate.Id;
            _player.PrimaryResidenceEstateInstanceId = newEstate.InstanceId;
        }
        await ToastService.Show("مبروك تم شراء العقار", ToastType.Success);
        LoadEstateData();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border b)
            await b.ScaleTo(EstateUIConstants.AnimationPressScale, EstateUIConstants.AnimationPressDuration, Easing.CubicIn);
        await Navigation.PopAsync(false);
    }

    public class EstateItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Happiness { get; set; }
        public int Cost { get; set; }
        public string CostDisplay { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}