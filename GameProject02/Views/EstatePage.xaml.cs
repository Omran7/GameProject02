using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;

namespace GameProject02.Views;

public partial class EstatePage : ContentPage
{
    private PlayerAccount _player;

    public EstatePage()
    {
        InitializeComponent();
        _player = AccountService.GetCurrentPlayer();
        ApplyDynamicSizes();
        ApplyFontSizes();
    }

    private void ApplyFontSizes()
    {
        // العناوين الرئيسية (CardTitle)
        MyEstatesTitle.FontSize = EstateUIConstants.FontSizeMedium;
        NewEstatesTitle.FontSize = EstateUIConstants.FontSizeMedium;
        UsedEstatesTitle.FontSize = EstateUIConstants.FontSizeMedium;
        RentEstatesTitle.FontSize = EstateUIConstants.FontSizeMedium;

        // النصوص الوصفية (CardDescription)
        MyEstatesDesc.FontSize = EstateUIConstants.FontSizeSmall;
        NewEstatesDesc.FontSize = EstateUIConstants.FontSizeSmall;
        UsedEstatesDesc.FontSize = EstateUIConstants.FontSizeSmall;
        RentEstatesDesc.FontSize = EstateUIConstants.FontSizeSmall;

        // أزرار "دخول"
        MyEstatesButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
        NewEstatesButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
        UsedEstatesButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
        RentEstatesButtonLabel.FontSize = EstateUIConstants.FontSizeButton;
    }

    private void ApplyDynamicSizes()
    {
        // ضبط أبعاد إطارات الصور
        var frames = new[] { MyEstatesImageFrame, NewEstatesImageFrame, UsedEstatesImageFrame, RentEstatesImageFrame };
        foreach (var frame in frames)
        {
            if (frame != null)
            {
                frame.WidthRequest = EstateUIConstants.ImageSize;
                frame.HeightRequest = EstateUIConstants.ImageSize;
            }
        }

        // ضبط أبعاد أزرار الإجراء (دخول)
        var buttons = new[] { MyEstatesButton, NewEstatesButton, UsedEstatesButton, RentEstatesButton };
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                btn.WidthRequest = EstateUIConstants.ButtonWidth;
                btn.HeightRequest = EstateUIConstants.ButtonHeight;
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        ApplyDynamicSizes();
        ApplyFontSizes();
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

        // وضع الزر في العمود الثاني (أقصى اليمين)
        grid.Add(homeButton, 1, 0);

        PageFooter.SetContent(grid);
    }

    private async void OnNewEstatesClicked(object sender, EventArgs e)
    {
        await AnimateBorder(sender as Border);
        await Navigation.PushAsync(new EstateNewPage());
    }

    private async void OnUsedEstatesClicked(object sender, EventArgs e)
    {
        await AnimateBorder(sender as Border);
        await Navigation.PushAsync(new UsedEstatePage());
    }

    private async void OnRentEstatesClicked(object sender, EventArgs e)
    {
        await AnimateBorder(sender as Border);
        await Navigation.PushAsync(new EstateRentPage());
    }

    private async void OnMyEstatesClicked(object sender, EventArgs e)
    {
        await AnimateBorder(sender as Border);
        if (_player.Estates != null && _player.Estates.Count > 0)
            await Navigation.PushAsync(new EstateOwnPage());
        else
            await ToastService.Show("لم تمتلك أي عقارات بعد!\nاشتري عقار جديد من قسم 'عقارات جديدة'", ToastType.Error);
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await AnimateBorder(sender as Border);
        await Navigation.PopToRootAsync();
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        // هذه الدالة لن تُستخدم حالياً بعد حذف زر Profile
        // يمكن الاحتفاظ بها أو حذفها
        await AnimateBorder(sender as Border);
        await Navigation.PushAsync(new ProfilePage());
    }

    private async Task AnimateBorder(Border border)
    {
        if (border == null) return;
        await border.ScaleTo(EstateUIConstants.AnimationPressScale, EstateUIConstants.AnimationPressDuration, Easing.CubicIn);
        await border.ScaleTo(1, EstateUIConstants.AnimationPressDuration, Easing.CubicOut);
    }
}