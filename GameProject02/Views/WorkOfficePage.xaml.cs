using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class WorkOfficePage : ContentPage
{
    private PlayerAccount _player;
    private Dictionary<int, Label> _timerLabels = new();
    private bool _isTimerRunning;

    public WorkOfficePage()
    {
        InitializeComponent();
        _player = AccountService.GetCurrentPlayer();
        LoadCategories();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        LoadCategories();
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

        var homeButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnHomeClicked,
            buttonImageSource: "footer_button_back.png"
        );

        grid.Add(homeButton, 1, 0);
        PageFooter.SetContent(grid);
    }

    private void LoadCategories()
    {
        if (_player == null) return;

        var categories = WorkOfficeService.GetAllCategories();
        CategoryCardsGrid.Children.Clear();
        CategoryCardsGrid.RowDefinitions.Clear();
        CategoryCardsGrid.ColumnDefinitions.Clear();
        _timerLabels.Clear();

        CategoryCardsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int i = 0; i < categories.Count; i++)
        {
            CategoryCardsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var card = CreateCategoryCard(categories[i]);
            CategoryCardsGrid.Add(card, 0, i);
        }
    }

    private Border CreateCategoryCard(WorkCategory category)
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
            Margin = new Thickness(10, EstateUIConstants.CardMarginVertical),
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
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = EstateUIConstants.ColumnSpacing,
            Padding = new Thickness(EstateUIConstants.CardContentPadding, 0),
            VerticalOptions = LayoutOptions.Center
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
        imageBorder.Content = new Image { Source = category.ImageResource, Aspect = Aspect.AspectFill, WidthRequest = imageSize, HeightRequest = imageSize };
        contentGrid.Add(imageBorder, 0);

        var detailsStack = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        detailsStack.Children.Add(new Label
        {
            Text = category.Name,
            Style = (Style)Application.Current.Resources["CardTitle"],
            FontSize = EstateUIConstants.FontSizeMedium,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        });

        bool isWorkingHere = (_player.WorkObject.WorkType == category.Id && _player.WorkObject.JobLevel >= 0);
        int currentLevel = isWorkingHere ? _player.WorkObject.JobLevel : -1;
        object currentJob = isWorkingHere ? WorkOfficeService.GetJob(category.Id, currentLevel) : null;

        if (isWorkingHere && currentJob != null)
        {
            string jobName = GetPropertyValue(currentJob, "Name") ?? "";
            detailsStack.Children.Add(new Label
            {
                Text = jobName,
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeSmall,
                TextColor = Colors.Goldenrod,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }

        if (isWorkingHere && _player.WorkObject.JobStartTimeMilli > 0)
        {
            double daysWorked = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _player.WorkObject.JobStartTimeMilli) / (86400.0 * 1000.0);
            detailsStack.Children.Add(new Label
            {
                Text = $"أيام العمل : {daysWorked:F0} يوم",
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeTiny,
                TextColor = Colors.WhiteSmoke,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }

        if (isWorkingHere && _player.WorkObject.JobStartTimeMilli > 0)
        {
            var timerLabel = new Label
            {
                Text = "",
                FontFamily = "Cairo-Black",
                FontSize = EstateUIConstants.FontSizeTiny,
                TextColor = Colors.WhiteSmoke,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            detailsStack.Children.Add(timerLabel);
            _timerLabels[category.Id] = timerLabel;
        }

        int totalLevels = category.Jobs.Count;
        var starsContainer = new HorizontalStackLayout { Spacing = 3, HorizontalOptions = LayoutOptions.Center };
        for (int i = 0; i < totalLevels; i++)
        {
            bool isFilled = (isWorkingHere && i <= currentLevel);
            starsContainer.Children.Add(new Label
            {
                Text = "★",
                FontSize = 16,
                TextColor = isFilled ? Colors.Gold : Colors.Gray,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            });
        }
        detailsStack.Children.Add(starsContainer);

        contentGrid.Add(detailsStack, 1);

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
            BackgroundColor = Colors.Transparent
        };
        var buttonGrid = new Grid();
        buttonGrid.Add(new Image { Source = "button_background.png", Aspect = Aspect.Fill });
        buttonGrid.Add(new Label
        {
            Text = "دخول",
            Style = (Style)Application.Current.Resources["ActionButton"],
            FontSize = EstateUIConstants.FontSizeButton
        });
        buttonBorder.Content = buttonGrid;

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await AnimateBorder(buttonBorder);
            await Navigation.PushAsync(new WorkDetailsPage(category.Id, category.Name));
        };
        buttonBorder.GestureRecognizers.Add(tap);

        contentGrid.Add(buttonBorder, 2);

        mainGrid.Add(contentGrid);
        mainBorder.Content = mainGrid;
        return mainBorder;
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
                    if (_player.WorkObject.WorkType == kvp.Key)
                        UpdateTimerLabel(kvp.Value);
                }
            });
            return true;
        });
    }

    private void UpdateTimerLabel(Label timerLabel)
    {
        if (!WorkOfficeService.CanCollectSalary(_player))
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var startTime = _player.WorkObject.JobStartTimeMilli;
            var timePassed = now - startTime;
            var cooldown = 24 * 60 * 60 * 1000;
            var timeRemaining = cooldown - (timePassed % cooldown);
            if (timeRemaining < 0) timeRemaining = 0;
            var hours = (int)(timeRemaining / (60 * 60 * 1000));
            var minutes = (int)((timeRemaining % (60 * 60 * 1000)) / (60 * 1000));
            var seconds = (int)((timeRemaining % (60 * 1000)) / 1000);
            timerLabel.Text = $"القبض بعد : {hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        else
        {
            timerLabel.Text = "يمكنك استلام الراتب الآن!";
        }
    }

    private string GetPropertyValue(object obj, string propName)
    {
        if (obj == null) return null;
        var prop = obj.GetType().GetProperty(propName);
        return prop?.GetValue(obj)?.ToString();
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

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopToRootAsync();
    }
}