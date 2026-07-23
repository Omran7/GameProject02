using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class SchoolPage : ContentPage
{
    private PlayerAccount _player;
    private SchoolObject _school;
    private bool _isUpdatingStudyStatus = false;

    public SchoolPage()
    {
        InitializeComponent();
        _player = AccountService.GetCurrentPlayer();
        _school = _player?.School ?? new SchoolObject();

        ApplyFontSizes();
        ApplyDynamicSizes();
        StartStudyStatusTimer();
    }

    private void ApplyFontSizes()
    {
        LawTitle.FontSize = EstateUIConstants.FontSizeMedium;
        MilitaryTitle.FontSize = EstateUIConstants.FontSizeMedium;
        HistoryTitle.FontSize = EstateUIConstants.FontSizeMedium;
        ScienceTitle.FontSize = EstateUIConstants.FontSizeMedium;
        GymTitle.FontSize = EstateUIConstants.FontSizeMedium;

        LawDesc.FontSize = EstateUIConstants.FontSizeSmall;
        MilitaryDesc.FontSize = EstateUIConstants.FontSizeSmall;
        HistoryDesc.FontSize = EstateUIConstants.FontSizeSmall;
        ScienceDesc.FontSize = EstateUIConstants.FontSizeSmall;
        GymDesc.FontSize = EstateUIConstants.FontSizeSmall;

        LawButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
        MilitaryButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
        HistoryButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
        ScienceButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
        GymButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
    }

    private void ApplyDynamicSizes()
    {
        // إطارات الصور
        LawImageFrame.WidthRequest = EstateUIConstants.ImageSize;
        LawImageFrame.HeightRequest = EstateUIConstants.ImageSize;
        MilitaryImageFrame.WidthRequest = EstateUIConstants.ImageSize;
        MilitaryImageFrame.HeightRequest = EstateUIConstants.ImageSize;
        HistoryImageFrame.WidthRequest = EstateUIConstants.ImageSize;
        HistoryImageFrame.HeightRequest = EstateUIConstants.ImageSize;
        ScienceImageFrame.WidthRequest = EstateUIConstants.ImageSize;
        ScienceImageFrame.HeightRequest = EstateUIConstants.ImageSize;
        GymImageFrame.WidthRequest = EstateUIConstants.ImageSize;
        GymImageFrame.HeightRequest = EstateUIConstants.ImageSize;

        // أزرار الإجراء
        LawActionButton.WidthRequest = EstateUIConstants.ButtonWidth;
        LawActionButton.HeightRequest = EstateUIConstants.ButtonHeight;
        MilitaryActionButton.WidthRequest = EstateUIConstants.ButtonWidth;
        MilitaryActionButton.HeightRequest = EstateUIConstants.ButtonHeight;
        HistoryActionButton.WidthRequest = EstateUIConstants.ButtonWidth;
        HistoryActionButton.HeightRequest = EstateUIConstants.ButtonHeight;
        ScienceActionButton.WidthRequest = EstateUIConstants.ButtonWidth;
        ScienceActionButton.HeightRequest = EstateUIConstants.ButtonHeight;
        GymActionButton.WidthRequest = EstateUIConstants.ButtonWidth;
        GymActionButton.HeightRequest = EstateUIConstants.ButtonHeight;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        if (_player != null)
        {
            _school = _player.School;
            UpdateAllCards();
        }
        ApplyDynamicSizes();
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

        var homeButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnHomeClicked,
            buttonImageSource: "footer_button_back.png" 
        );

        grid.Add(homeButton, 1, 0);
        PageFooter.SetContent(grid);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isUpdatingStudyStatus = false;
    }

    private void UpdateAllCards()
    {
        UpdateCard(0, LawDesc, LawStarsContainer);
        UpdateCard(1, MilitaryDesc, MilitaryStarsContainer);
        UpdateCard(2, HistoryDesc, HistoryStarsContainer);
        UpdateCard(3, ScienceDesc, ScienceStarsContainer);
        UpdateCard(4, GymDesc, GymStarsContainer);
    }

    private void UpdateCard(int category, Label descLabel, HorizontalStackLayout starsContainer)
    {
        if (_school == null) return;

        // تحديث الوصف (حالة الدراسة)
        if (_school.IsStudying && _school.CurrentCategory == category)
        {
            string lessonName = _school.GetLessonName(category, _school.CurrentLesson);
            string remaining = _school.GetRemainingStudyTime(_player);
            descLabel.Text = $"يدرس: {lessonName}\nالمتبقي: {remaining}";
            descLabel.TextColor = (Color)Application.Current.Resources["ColorError"];
        }
        else
        {
            descLabel.Text = "";
        }

        // تحديث النجوم
        starsContainer.Children.Clear();
        var lessons = category switch
        {
            0 => _school.LawLessons,
            1 => _school.MilitaryLessons,
            2 => _school.HistoryLessons,
            3 => _school.ScienceLessons,
            4 => _school.GymLessons,
            _ => null
        };

        if (lessons != null)
        {
            foreach (var status in lessons)
            {
                starsContainer.Children.Add(new Label
                {
                    Text = "★",
                    FontSize = EstateUIConstants.FontSizeLarge,
                    TextColor = status == 2 ? Colors.Gold : Color.FromArgb("#000000").WithAlpha(0.3f),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                });
            }
        }
    }

    private async void StartStudyStatusTimer()
    {
        _isUpdatingStudyStatus = true;
        while (_isUpdatingStudyStatus)
        {
            await Task.Delay(1000);
            if (_player != null && _school != null && _school.IsStudying)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateAllCards();
                });

                if (SchoolService.IsStudyComplete(_player))
                {
                    _school.CompleteStudy(_player);
                    UpdateAllCards();
                }
            }
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

    private async void OnLawClicked(object sender, EventArgs e) => await NavigateToCategory(0, sender);
    private async void OnMilitaryClicked(object sender, EventArgs e) => await NavigateToCategory(1, sender);
    private async void OnHistoryClicked(object sender, EventArgs e) => await NavigateToCategory(2, sender);
    private async void OnScienceClicked(object sender, EventArgs e) => await NavigateToCategory(3, sender);
    private async void OnGymClicked(object sender, EventArgs e) => await NavigateToCategory(4, sender);

    private async Task NavigateToCategory(int category, object sender)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PushAsync(new LessonPage(category, _player), false);
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopToRootAsync(false);
    }
}