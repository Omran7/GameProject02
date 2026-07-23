using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class LessonPage : ContentPage
{
    private int _categoryIndex;
    private PlayerAccount _player;
    private SchoolObject _school;
    private bool _isCheckingStudy = false;

    private const string CompletedButtonImage = "card_background.png";
    private const string StudyButtonImage = "button_background.png";
    private const string CancelButtonImage = "button_background_no.png";
    private const string LockedButtonImage = "card_background.png";
    private const string DefaultButtonImage = "button_background.png";

    public LessonPage(int categoryIndex, PlayerAccount player)
    {
        InitializeComponent();
        _categoryIndex = categoryIndex;
        _player = player;
        _school = player.School;

        LoadLessons();
        StartStudyCheckTimer();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _player = AccountService.GetCurrentPlayer();
        if (_player != null)
        {
            _school = _player.School;
            LoadLessons();
        }
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isCheckingStudy = false;
    }

    private void LoadLessons()
    {
        LessonsContainer.Children.Clear();
        var lessons = _school.GetLessonsForCategory(_categoryIndex);

        for (int i = 0; i < lessons.Count; i++)
        {
            int lessonIndex = i;
            int status = lessons[i];

            if (status == 0 && i > 0 && lessons[i - 1] != 2)
                continue;

            bool isCurrentlyStudying = (_school.IsStudying &&
                                       _school.CurrentCategory == _categoryIndex &&
                                       _school.CurrentLesson == lessonIndex);

            var card = CreateLessonCard(lessonIndex, status, isCurrentlyStudying);
            LessonsContainer.Children.Add(card);
        }
    }

    private View CreateLessonCard(int lessonIndex, int status, bool isCurrentlyStudying)
    {
        var border = new Border
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
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = EstateUIConstants.ButtonWidth }
            },
            ColumnSpacing = EstateUIConstants.ColumnSpacing,
            Padding = new Thickness(EstateUIConstants.CardContentPadding, 0),
            VerticalOptions = LayoutOptions.Center
        };

        var infoStack = new VerticalStackLayout { Spacing = 5 };
        infoStack.Add(new Label
        {
            Text = _school.GetLessonName(_categoryIndex, lessonIndex),
            Style = (Style)Application.Current.Resources["CardTitle"],
            FontSize = EstateUIConstants.FontSizeMedium
        });
        infoStack.Add(new Label
        {
            Text = _school.GetLessonDescription(_categoryIndex, lessonIndex),
            Style = (Style)Application.Current.Resources["CardDescription"],
            FontSize = EstateUIConstants.FontSizeSmall
        });

        if (isCurrentlyStudying)
        {
            infoStack.Add(new Label
            {
                Text = $"المتبقي: {_school.GetRemainingStudyTime(_player)}",
                TextColor = (Color)Application.Current.Resources["ColorError"],
                FontSize = EstateUIConstants.FontSizeSmall,
                FontFamily = "Cairo-Black",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }
        else if (status != 2)
        {
            int cost = _school.GetStudyCost(_categoryIndex, lessonIndex);
            int days = _school.GetStudyTimeInDays(_categoryIndex, lessonIndex);
            infoStack.Add(new Label
            {
                Text = $"السعر: {NumberFormatter.FormatNumber(cost)} ذهب  |  المدة: {days} يوم",
                Style = (Style)Application.Current.Resources["CardDescription"],
                FontSize = EstateUIConstants.FontSizeSmall,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
        }

        contentGrid.Add(infoStack, 0);

        string buttonText = "";
        string buttonImage = DefaultButtonImage;
        Color textColor = Colors.Black;
        bool isDisabled = false;

        if (status == 2)
        {
            buttonText = "مكتمل";
            buttonImage = CompletedButtonImage;
            textColor = Colors.Goldenrod;
            isDisabled = true;
        }
        else if (isCurrentlyStudying)
        {
            buttonText = "إلغاء";
            buttonImage = CancelButtonImage;
            textColor = Colors.Black;
        }
        else if (status == 1 && !_school.IsStudying)
        {
            buttonText = "دراسة";
            buttonImage = StudyButtonImage;
        }
        else
        {
            buttonText = "مقفول";
            buttonImage = LockedButtonImage;
            textColor = (Color)Application.Current.Resources["ColorError"];
            isDisabled = true;
        }

        var btnBorder = new Border
        {
            Style = (Style)Application.Current.Resources["EstateActionButton"],
            WidthRequest = EstateUIConstants.ButtonWidth,
            HeightRequest = EstateUIConstants.ButtonHeight,
            IsEnabled = !isDisabled
        };
        var btnGrid = new Grid();
        btnGrid.Add(new Image { Source = buttonImage, Aspect = Aspect.Fill });
        btnGrid.Add(new Label
        {
            Text = buttonText,
            TextColor = textColor,
            Style = (Style)Application.Current.Resources["ActionButton"]
        });
        btnBorder.Content = btnGrid;

        if (!isDisabled)
        {
            btnBorder.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await AnimateBorder(btnBorder);
                    if (isCurrentlyStudying)
                        await CancelLesson(lessonIndex);
                    else
                        await StartLesson(lessonIndex);
                })
            });
        }

        contentGrid.Add(btnBorder, 1);
        mainGrid.Add(contentGrid);
        border.Content = mainGrid;
        return border;
    }

    private async Task StartLesson(int lessonIndex)
    {
        if (_school.IsStudying)
        {
            await ToastService.Show("لا يمكنك بدء درس جديد أثناء الدراسة الحالية", ToastType.Error);
            return;
        }

        string lessonName = _school.GetLessonName(_categoryIndex, lessonIndex);
        int studyCost = _school.GetStudyCost(_categoryIndex, lessonIndex);
        int studyDays = _school.GetStudyTimeInDays(_categoryIndex, lessonIndex);
        bool isVIP = _player.IsVIP;

        if (_player.Gold < studyCost)
        {
            await ToastService.Show("ليس لديك ذهب كافي!", ToastType.Error);
            return;
        }

        string message = $"الدرس: {lessonName}\n" +
                         $"السعر: {NumberFormatter.FormatNumber(studyCost)} ذهب\n" +
                         $"المدة: {studyDays} يوم";

        if (isVIP)
        {
            message += "\nخصم 20% للاعبين VIP";
        }

        var confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد بدء الدراسة",
            message: message,
            operationType: PopupOperationType.Confirm,
            overridePositiveText: "نعم",
            overrideNegativeText: "لا"
        );

        if (!confirm) return;

        if (_school.StartStudying(_player, _categoryIndex, lessonIndex))
        {
            LoadLessons();
            await ToastService.Show($"بدأت الدراسة!\nستكتمل خلال {studyDays} يوم", ToastType.Success);
        }
        else
        {
            await ToastService.Show("ليس لديك ذهب كافي!", ToastType.Error);
        }
    }

    private async Task CancelLesson(int lessonIndex)
    {
        int studyCost = _school.GetStudyCost(_categoryIndex, lessonIndex);
        string lessonName = _school.GetLessonName(_categoryIndex, lessonIndex);

        string message = $"الدرس: {lessonName}\n" +
                         $"سيتم فقدان {NumberFormatter.FormatNumber(studyCost)} ذهب نهائياً\n" +
                         $"لن يتم استرداد أي مبلغ";

        var confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد الإلغاء",
            message: message,
            operationType: PopupOperationType.Confirm,
            overridePositiveText: "نعم",
            overrideNegativeText: "لا"
        );

        if (!confirm) return;

        SchoolService.CancelStudying(_player);
        LoadLessons();
        await ToastService.Show($"تم الإلغاء\nتم فقدان {NumberFormatter.FormatNumber(studyCost)} ذهب", ToastType.Success);
    }

    private async void StartStudyCheckTimer()
    {
        _isCheckingStudy = true;
        while (_isCheckingStudy)
        {
            await Task.Delay(60000);
            if (_school.IsStudying && SchoolService.IsStudyComplete(_player))
            {
                _school.CompleteStudy(_player);
                await ToastService.Show("اكتملت الدراسة!\nتم تطبيق المكافآت", ToastType.Success);
                LoadLessons();
                break;
            }
            if (_school.IsStudying && _school.CurrentCategory == _categoryIndex)
            {
                LoadLessons();
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

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _isCheckingStudy = false;
        if (sender is Border border) await AnimateBorder(border);
        await Navigation.PopAsync(false);
    }
}