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

public partial class EstateRentPage : ContentPage
{
    private PlayerAccount _player;

    public EstateRentPage()
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
                new ColumnDefinition { Width = GridLength.Auto },
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

        var rentOutButton = PageFooter.CreateFooterButton(
            text: "تأجير",
            tappedHandler: OnRentOutClicked,
            buttonImageSource: "footer_button_rent.png"
        );

        grid.Add(backButton, 2, 0);
        grid.Add(rentOutButton, 0, 0);

        PageFooter.SetContent(grid);
    }

    private void LoadEstateData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        var container = this.FindByName<VerticalStackLayout>("EstatesContainer");
        if (container == null) return;
        container.Children.Clear();

        var listingsByType = RentalService.GetAvailableListingsGrouped()
            .OrderBy(g => EstateObject.EstateTypes.TryGetValue(g.Key, out var type) ? type.Name : string.Empty)
            .ToList();

        if (listingsByType.Count == 0)
        {
            double screenWidth = Application.Current.MainPage.Width;
            double screenHeight = Application.Current.MainPage.Height;
            container.Children.Add(new BoxView { HeightRequest = screenHeight * 0.15, BackgroundColor = Colors.Transparent });
            var emptyBorder = new Border
            {
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
                BackgroundColor = Colors.Transparent,
                WidthRequest = screenWidth * 0.75,
                HeightRequest = screenHeight * 0.40,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            var emptyGrid = new Grid();
            emptyGrid.Add(new Image { Source = "card_background_empty.png", Aspect = Aspect.Fill, Opacity = 1 });
            var emptyStack = new VerticalStackLayout { Spacing = 15, Padding = 20, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            emptyStack.Children.Add(new Label
            {
                Text = "لا توجد عقارات متاحة للإيجار حالياً\n(اعرض عقارك للإيجار)\n(انتظار اللاعبين لعرض عقاراتهم)",
                Style = (Style)Application.Current.Resources["CardTitle"],
                FontSize = EstateUIConstants.FontSizeLarge,
                LineBreakMode = LineBreakMode.WordWrap,
                LineHeight = 1.2,
                HorizontalOptions = LayoutOptions.Center
            });
            emptyGrid.Add(emptyStack);
            emptyBorder.Content = emptyGrid;
            container.Children.Add(emptyBorder);
            return;
        }

        foreach (var group in listingsByType)
        {
            var estateId = group.Key;
            var listings = group.Value;
            if (!EstateObject.EstateTypes.TryGetValue(estateId, out var estateType)) continue;
            container.Children.Add(CreateEstateCard(estateId, estateType, listings));
        }
    }

    private Border CreateEstateCard(int estateId, EstateObject.EstateType estateType, List<RentalListing> listings)
    {
        double imageSize = EstateUIConstants.ImageSize;
        double buttonWidth = EstateUIConstants.ButtonWidth;

        var mainBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.CardCornerRadius },
            Padding = 0,
            Margin = new Thickness(10, EstateUIConstants.CardMarginVertical),
            MinimumHeightRequest = EstateUIConstants.CardMinHeight,
            BackgroundColor = Colors.Transparent
        };
        var mainGrid = new Grid();
        mainGrid.Add(new Image { Source = "card_background.png", Aspect = Aspect.Fill, Opacity = 1 });

        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = imageSize },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = buttonWidth }
            },
            ColumnSpacing = EstateUIConstants.ColumnSpacing,
            Padding = new Thickness(EstateUIConstants.CardContentPadding),
            VerticalOptions = LayoutOptions.Center
        };

        var imageBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ImageCornerRadius },
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        };
        imageBorder.Content = new Image { Source = estateType.ImageResource, Aspect = Aspect.Fill };
        contentGrid.Add(imageBorder, 0);

        var detailsStack = new StackLayout
        {
            Spacing = EstateUIConstants.StackSpacing,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 0)
        };

        detailsStack.Children.Add(new Label
        {
            Text = estateType.Name,
            Style = (Style)Application.Current.Resources["CardTitle"],
            FontSize = EstateUIConstants.FontSizeMedium,
            Margin = new Thickness(0, 0, 0, 10)
        });

        detailsStack.Children.Add(new Label
        {
            Text = $"السعادة: {NumberFormatter.FormatNumber(estateType.Happiness)}",
            Style = (Style)Application.Current.Resources["CardDescription"],
            FontSize = EstateUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        });

        detailsStack.Children.Add(new Label
        {
            Text = $"العدد المتوفر: {listings.Count}",
            Style = (Style)Application.Current.Resources["CardDescription"],
            FontSize = EstateUIConstants.FontSizeSmall,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        });
        contentGrid.Add(detailsStack, 1);

        var buttonBorder = new Border
        {
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = EstateUIConstants.ButtonCornerRadius },
            WidthRequest = buttonWidth,
            HeightRequest = EstateUIConstants.ButtonHeight,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            Padding = 0,
            BackgroundColor = Colors.Transparent
        };
        var buttonGrid = new Grid();
        buttonGrid.Add(new Image { Source = "button_background.png", Aspect = Aspect.Fill });
        buttonGrid.Add(new Label
        {
            Text = "عرض",
            TextColor = EstateUIConstants.TextDark,
            FontFamily = "Cairo-Black",
            FontSize = EstateUIConstants.FontSizeButton,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        });
        buttonBorder.Content = buttonGrid;
        buttonBorder.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await AnimateBorder(buttonBorder);
                await ShowListingsPopup(listings);
            })
        });
        contentGrid.Add(buttonBorder, 2);
        mainGrid.Add(contentGrid);
        mainBorder.Content = mainGrid;
        return mainBorder;
    }

    private async Task ShowListingsPopup(List<RentalListing> listings)
    {
        EstatesContainer.IsEnabled = false;
        PageFooter.InputTransparent = true;

        var sortedListings = listings.OrderBy(l => l.TotalPriceFor30Days).ToList();

        async Task DummyOnItemSelected(RentalListing _) => await Task.CompletedTask;

        View CreateItemView(RentalListing listing)
        {
            var owner = AccountService.GetAllPlayers().FirstOrDefault(p => p.PlayerId == listing.OwnerId);
            var ownerName = owner?.Username ?? "لاعب مجهول";

            var border = new Border
            {
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = 0,
                BackgroundColor = Colors.Transparent,
                HeightRequest = -1,
                Margin = new Thickness(0, 3, 0, 3),
                HorizontalOptions = LayoutOptions.Fill
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 10,
                Padding = 0
            };

            var backgroundImage = new Image
            {
                Source = "card_background.png",
                Aspect = Aspect.Fill,
                Opacity = 1,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            grid.Add(backgroundImage, 0, 0);
            Grid.SetColumnSpan(backgroundImage, 2);

            var contentStack = new VerticalStackLayout
            {
                Spacing = 0,
                Padding = new Thickness(15, 10, 5, 10),
                VerticalOptions = LayoutOptions.Center
            };

            var listItemStyle = (Style)Application.Current.Resources["PopupListItem"];

            contentStack.Children.Add(new Label
            {
                Text = $"المالك: {ownerName}",
                Style = listItemStyle,
                FontSize = EstateUIConstants.FontSizeSmall
            });

            contentStack.Children.Add(new BoxView
            {
                BackgroundColor = Color.FromArgb("#0a0a0a"),
                HeightRequest = 1,
                Margin = new Thickness(0, 2, 0, 2)
            });

            contentStack.Children.Add(new Label
            {
                Text = $"السعادة: {NumberFormatter.FormatNumber(listing.CurrentHappiness)}",
                Style = listItemStyle,
                FontSize = EstateUIConstants.FontSizeSmall
            });

            contentStack.Children.Add(new BoxView
            {
                BackgroundColor = Color.FromArgb("#0a0a0a"),
                HeightRequest = 1,
                Margin = new Thickness(0, 2, 0, 2)
            });

            contentStack.Children.Add(new Label
            {
                Text = $"السعر(30 يوم): {NumberFormatter.FormatNumber(listing.TotalPriceFor30Days)}",
                Style = listItemStyle,
                FontSize = EstateUIConstants.FontSizeSmall,
                FontAttributes = FontAttributes.Bold
            });

            grid.Add(contentStack, 0, 0);

            var rentButton = new Border
            {
                Style = (Style)Application.Current.Resources["PopupActionButton"],
                Margin = new Thickness(0, 0, 10, 0)
            };
            var rentGrid = new Grid();
            rentGrid.Add(new Image { Source = "button_background.png", Aspect = Aspect.Fill });
            rentGrid.Add(new Label
            {
                Text = "استئجار",
                Style = (Style)Application.Current.Resources["PopupActionButtonText"]
            });

            rentButton.Content = rentGrid;

            rentButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await AnimateBorder(rentButton);

                    if (listing.OwnerId == _player.PlayerId)
                    {
                        ToastService.Show("لا يمكنك استئجار عقارك الخاص!", ToastType.Error);
                        return;
                    }

                    await PopupService.CompleteSelection(listing);
                })
            });

            grid.Add(rentButton, 1, 0);
            border.Content = grid;
            return border;
        }

        var selectedListing = await PopupService.ShowSelectionPopupWithCustomView(
            title: "اختر عقار للإيجار",
            items: sortedListings,
            createItemView: CreateItemView,
            onItemSelected: DummyOnItemSelected
        );

        EstatesContainer.IsEnabled = true;
        PageFooter.InputTransparent = false;

        if (selectedListing != null)
        {
            if (selectedListing.OwnerId == _player.PlayerId)
            {
                ToastService.Show("لا يمكنك استئجار عقارك الخاص!", ToastType.Error);
                return;
            }
            await Navigation.PushAsync(new EstateRentConfirmPage(selectedListing));
        }
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

    private async void OnRentOutClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PushAsync(new EstateRentOutPage());
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopAsync();
    }
}