using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameProject02.Views;

public partial class PrisonPage : ContentPage, INotifyPropertyChanged
{
    private PlayerAccount _player;
    private Timer _countdownTimer;
    private ObservableCollection<PlayerAccount> _prisoners;

    // ═══════════════════════════════════════════════════
    //  أبعاد الشاشة
    // ═══════════════════════════════════════════════════
    private static double ScreenWidth => DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
    private static double ScreenHeight => DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;

    private double CardHeight => ScreenHeight * 0.10;
    private double CardPadding => CardHeight * 0.15;
    private double CardMarginBottom => CardHeight * 0.05;
    private double CardSpacing => CardHeight * 0.06;

    private double AvatarContainerSize => CardHeight * 0.7;
    private double AvatarInnerSize => AvatarContainerSize * 0.85;

    private double LvlIconSize => CardHeight * 0.28;
    private double LvlDigitSize => CardHeight * 0.15;
    private double GenderIconSize => CardHeight * 0.20;

    private double InmateCardWidth => ScreenWidth * 0.55;
    private double InmateCardHeight => ScreenHeight * 0.22;
    private double InmateCardMarginTop => ScreenHeight * 0.145;
    private double InmateCardMarginRight => ScreenWidth * 0.00;

    private double InfoFontTitle => ScreenHeight * 0.018;
    private double InfoFontValue => ScreenHeight * 0.018;
    private double InfoFontTime => ScreenHeight * 0.018;

    private bool _isInPrison;
    public bool IsInPrison
    {
        get => _isInPrison;
        set { _isInPrison = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsFreeMode)); }
    }
    public bool IsFreeMode => !IsInPrison;

    public ObservableCollection<PlayerAccount> Prisoners
    {
        get => _prisoners;
        set { _prisoners = value; OnPropertyChanged(); }
    }

    public ICommand PayBailForPlayerCommand { get; }
    public ICommand SmugglePlayerCommand { get; }

    public PrisonPage()
    {
        InitializeComponent();
        PayBailForPlayerCommand = new Command<PlayerAccount>(async p => await BailAsync(p, null));
        SmugglePlayerCommand = new Command<PlayerAccount>(async p => await SmuggleAsync(p, null));
        BindingContext = this;
        LoadData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
        SetupFooter();
        if (IsInPrison) StartCountdownTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopCountdownTimer();
    }

    // ═══════════════════════════════════════════════════
    //  إعداد الفوتر
    // ═══════════════════════════════════════════════════
    private void SetupFooter()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(ScreenWidth * 0.02, 0),
            HorizontalOptions = LayoutOptions.Fill
        };

        var backButton = PageFooter.CreateFooterButton(
            text: "رجوع",
            tappedHandler: OnBackClicked,
            buttonImageSource: "footer_button_back.png"
        );

        var escapeButton = PageFooter.CreateFooterButton(
            text: "هروب",
            tappedHandler: OnJailbreakClicked,
            buttonImageSource: "button_background.png"
        );
        escapeButton.IsVisible = IsInPrison;

        var refreshButton = PageFooter.CreateFooterButton(
            text: "تحديث",
            tappedHandler: OnRefreshClicked,
            buttonImageSource: "button_background.png"
        );
        refreshButton.IsVisible = IsFreeMode;

#if DEBUG
        var addPrisonerButton = PageFooter.CreateFooterButton(
            text: "إضافة\nسجين",
            tappedHandler: OnAddFakePrisonerClicked,
            buttonImageSource: "button_background.png"
        );

        var enterPrisonButton = PageFooter.CreateFooterButton(
            text: "دخول\nسجن",
            tappedHandler: OnEnterPrisonClicked,
            buttonImageSource: "button_background.png"
        );

        grid.Add(addPrisonerButton, 0, 0);
        grid.Add(enterPrisonButton, 1, 0);
#else
        grid.Add(new ContentView(), 0, 0);
        grid.Add(new ContentView(), 1, 0);
#endif

        grid.Add(new ContentView(), 2, 0);

        if (IsInPrison)
            grid.Add(escapeButton, 3, 0);
        else
            grid.Add(refreshButton, 3, 0);

        grid.Add(backButton, 4, 0);

        PageFooter.SetContent(grid);
    }

    // ═══════════════════════════════════════════════════
    //  بناء عرض الليفل بالصور
    // ═══════════════════════════════════════════════════
    private View BuildLevelView(int level, double iconSize, double digitSize)
    {
        var container = new HorizontalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            FlowDirection = FlowDirection.LeftToRight
        };

        container.Children.Add(new Image
        {
            Source = "icon_lvl.png",
            WidthRequest = iconSize,
            HeightRequest = iconSize,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 5, 0)
        });

        foreach (char c in level.ToString())
        {
            if (c >= '0' && c <= '9')
            {
                container.Children.Add(new Image
                {
                    Source = ImageSource.FromFile($"digit_{c}.png"),
                    WidthRequest = digitSize,
                    HeightRequest = digitSize,
                    Aspect = Aspect.AspectFit,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(-2, 0, 0, 0)
                });
            }
        }

        return container;
    }

    // ═══════════════════════════════════════════════════
    //  بناء بطاقة السجين ديناميكياً
    // ═══════════════════════════════════════════════════
    private View BuildPrisonerCard(PlayerAccount prisoner)
    {
        var cardGrid = new Grid
        {
            Margin = new Thickness(0, 0, 0, CardMarginBottom),
            HeightRequest = CardHeight
        };

        cardGrid.Add(new Image
        {
            Source = "card_background_upsell",
            Aspect = Aspect.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        });

        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = AvatarContainerSize },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = CardSpacing,
            Padding = new Thickness(CardPadding)
        };

        // ── صورة اللاعب مع الكنار ──
        var avatarGrid = new Grid
        {
            WidthRequest = AvatarContainerSize,
            HeightRequest = AvatarContainerSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        avatarGrid.Add(new Image
        {
            Source = "avatar_frame.png",
            Aspect = Aspect.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        });

        ImageSource avatarSource;
        if (!string.IsNullOrEmpty(prisoner.AvatarPath)
            && System.IO.Path.IsPathRooted(prisoner.AvatarPath)
            && System.IO.File.Exists(prisoner.AvatarPath))
            avatarSource = ImageSource.FromFile(prisoner.AvatarPath);
        else
            avatarSource = "avatar_player.png";

        avatarGrid.Add(new Frame
        {
            WidthRequest = AvatarInnerSize,
            HeightRequest = AvatarInnerSize,
            CornerRadius = 0,
            Padding = 0,
            HasShadow = false,
            IsClippedToBounds = true,
            BackgroundColor = Colors.Transparent,
            BorderColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = new Image
            {
                Source = avatarSource,
                Aspect = Aspect.AspectFill,
                WidthRequest = AvatarInnerSize,
                HeightRequest = AvatarInnerSize
            }
        });

        contentGrid.Add(avatarGrid, 0, 0);

        // ── العمود الأوسط: معلومات السجين ──
        var infoStack = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Start,
            Spacing = CardHeight * 0.00,
            Padding = new Thickness(0, CardHeight * -0.02, 0, 0)
        };

        // 1 — الجنس + الاسم + الليفل
        var nameRow = new HorizontalStackLayout
        {
            Spacing = CardHeight * 0.06,
            VerticalOptions = LayoutOptions.Center,
            FlowDirection = FlowDirection.RightToLeft
        };

        nameRow.Children.Add(new Image
        {
            Source = (prisoner.Gender == "أنثى") ? "icon_female.png" : "icon_male.png",
            WidthRequest = GenderIconSize,
            HeightRequest = GenderIconSize,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        });

        nameRow.Children.Add(new Label
        {
            Text = prisoner.Username,
            TextColor = Colors.WhiteSmoke,
            FontSize = CardHeight * 0.19,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        });

        nameRow.Children.Add(
            BuildLevelView(prisoner.Level, LvlIconSize, LvlDigitSize));

        infoStack.Children.Add(nameRow);

        // 2 — مبلغ الكفالة
        infoStack.Children.Add(new Label
        {
            Text = $"الكفالة: {prisoner.CrimeObject?.PrisonBailAmount:N0} ذهب",
            TextColor = Color.FromArgb("#f39c12"),
            FontSize = CardHeight * 0.15,
            FontAttributes = FontAttributes.Bold
        });

        // 3 — سبب السجن
        infoStack.Children.Add(new Label
        {
            Text = prisoner.CrimeObject?.PrisonReason ?? "سبب غير محدد",
            TextColor = Colors.WhiteSmoke,
            FontSize = CardHeight * 0.15,
            LineBreakMode = LineBreakMode.TailTruncation
        });

        contentGrid.Add(infoStack, 1, 0);

        // ── العمود الثالث: أزرار التهريب والكفالة ──
        double btnWidth = CardHeight * 0.8;
        double btnHeight = CardHeight * 0.35;
        double btnFont = CardHeight * 0.15;

        var buttonsStack = new VerticalStackLayout
        {
            Spacing = CardHeight * 0.04,
            VerticalOptions = LayoutOptions.Center
        };

        // زر تهريب
        var smuggleBtn = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = btnWidth,
            HeightRequest = btnHeight,
            Padding = 0
        };
        var smuggleGrid = new Grid();
        smuggleGrid.Add(new Image { Source = "button_background.png", Aspect = Aspect.Fill });
        smuggleGrid.Add(new Label
        {
            Text = "تهريب",
            TextColor = Colors.White,
            FontSize = btnFont,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        });
        smuggleBtn.Content = smuggleGrid;
        smuggleBtn.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await SmuggleAsync(prisoner, cardGrid))
        });

        // زر كفالة
        var bailBtn = new Border
        {
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            BackgroundColor = Colors.Transparent,
            WidthRequest = btnWidth,
            HeightRequest = btnHeight,
            Padding = 0
        };
        var bailGrid = new Grid();
        bailGrid.Add(new Image { Source = "button_background_no.png", Aspect = Aspect.Fill });
        bailGrid.Add(new Label
        {
            Text = "كفالة",
            TextColor = Colors.White,
            FontSize = btnFont,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        });
        bailBtn.Content = bailGrid;
        bailBtn.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await BailAsync(prisoner, cardGrid))
        });

        buttonsStack.Children.Add(smuggleBtn);
        buttonsStack.Children.Add(bailBtn);

        contentGrid.Add(buttonsStack, 2, 0);
        cardGrid.Add(contentGrid);

        return cardGrid;
    }

    // ═══════════════════════════════════════════════════
    //  دوال الكفالة والتهريب
    // ═══════════════════════════════════════════════════
    private async Task SmuggleAsync(PlayerAccount target, View? cardToRemove)
    {
        if (target == null) return;

        int cost = PrisonService.GetSmugglerCourageCost();

        bool confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد التهريب",
            message: $"هل تريد تهريب {target.Username}؟\n" +
                     $"التكلفة: {cost} شجاعة",
            operationType: PopupOperationType.Confirm,
            overridePositiveText: "تهريب",
            overrideNegativeText: "إلغاء"
        );

        if (!confirm) return;

        var result = PrisonService.AttemptJailbreakForPlayer(_player, target);

        if (result.success)
        {
            // نجح: أخرج السجين فوراً ثم اشعار
            Prisoners?.Remove(target);
            if (cardToRemove != null)
                PrisonersContainer.Children.Remove(cardToRemove);
            await ToastService.Show($"تم تهريب {target.Username} بنجاح!", ToastType.Success);
        }
        else if (_player.CrimeObject.IsInPrison)
        {
            // فشل وتم القبض: انتقل للسجن فوراً ثم اشعار
            LoadData();
            SetupFooter();
            StartCountdownTimer();
            await ToastService.Show("تم القبض عليك أثناء محاولة التهريب!", ToastType.Error);
        }
        else
        {
            await ToastService.Show("فشل التهريب!", ToastType.Error);
        }
    }

    private async Task BailAsync(PlayerAccount target, View? cardToRemove)
    {
        if (target == null) return;

        bool confirm = await PopupService.ShowConfirmAsync(
            title: "تأكيد الكفالة",
            message: $"هل تريد دفع كفالة {target.Username}؟\n" +
                     $"المبلغ: {target.CrimeObject?.PrisonBailAmount:N0} ذهب",
            operationType: PopupOperationType.Confirm,
            overridePositiveText: "دفع",
            overrideNegativeText: "إلغاء"
        );

        if (!confirm) return;

        var result = PrisonService.PayBailForPlayer(_player, target);

        if (result.success)
        {
            // نجح: أخرج السجين فوراً ثم اشعار
            Prisoners?.Remove(target);
            if (cardToRemove != null)
                PrisonersContainer.Children.Remove(cardToRemove);
            await ToastService.Show($"تم دفع كفالة {target.Username}!", ToastType.Success);
        }
        else
        {
            await ToastService.Show($" {result.message}", ToastType.Error);
        }
    }

    // ═══════════════════════════════════════════════════
    //  بناء قائمة السجناء ديناميكياً
    // ═══════════════════════════════════════════════════
    private void BuildPrisonersList()
    {
        PrisonersContainer.Children.Clear();

        if (Prisoners == null || Prisoners.Count == 0)
        {
            double emptySize = ScreenWidth * 0.75;

            var centerGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                MinimumHeightRequest = ScreenHeight * 0.5
            };

            var emptyGrid = new Grid
            {
                WidthRequest = emptySize,
                HeightRequest = emptySize,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            emptyGrid.Add(new Image
            {
                Source = "card_background_empty.png",
                Aspect = Aspect.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            });

            emptyGrid.Add(new Label
            {
                Text = "السجن فارغ حاليا لا يوجد سجناء",
                TextColor = Colors.White,
                FontSize = ScreenHeight * 0.022,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(emptySize * 0.15, 0)
            });

            centerGrid.Add(emptyGrid);
            PrisonersContainer.Children.Add(centerGrid);
            return;
        }

        foreach (var prisoner in Prisoners)
            PrisonersContainer.Children.Add(BuildPrisonerCard(prisoner));
    }

    // ═══════════════════════════════════════════════════
    //  تحميل البيانات
    // ═══════════════════════════════════════════════════
    private void LoadData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        bool wasInPrison = _isInPrison;
        _player.CrimeObject.CheckConfinementStatus();
        IsInPrison = _player.CrimeObject.IsInPrison;

        if (IsInPrison)
        {
            PageHeader.IsVisible = false;

            InmateInfoCard.WidthRequest = InmateCardWidth;
            InmateInfoCard.HeightRequest = InmateCardHeight;
            InmateInfoCard.Margin = new Thickness(
                0, InmateCardMarginTop, InmateCardMarginRight, 0);

            ReasonLabel.Text = _player.CrimeObject.PrisonReason ?? "تم القبض عليك";
            BailAmountLabel.Text = $"{_player.CrimeObject.PrisonBailAmount:N0} ذهب";
            VisitCountLabel.Text = $"مرات السجن: {_player.CrimeObject.TotalPrisonVisits}";
            ReasonLabel.FontSize = InfoFontValue;
            TimeRemainingLabel.FontSize = InfoFontTime;
            BailAmountLabel.FontSize = InfoFontValue;
            VisitCountLabel.FontSize = InfoFontTitle;
            UpdateTimeDisplay();
            InmateModeLayout.IsVisible = true;
            VisitorModeLayout.IsVisible = false;
        }
        else
        {
            if (wasInPrison && !IsInPrison) { _ = GoToMainPage(); return; }
            PageHeader.IsVisible = true;
            PageHeader.HeaderTitle = "السجن";
            Prisoners = new ObservableCollection<PlayerAccount>(
                                              PrisonService.GetPrisoners());
            InmateModeLayout.IsVisible = false;
            VisitorModeLayout.IsVisible = true;
            BuildPrisonersList();
        }
    }

    // ═══════════════════════════════════════════════════
    //  أزرار التجربة
    // ═══════════════════════════════════════════════════
#if DEBUG
    private void OnAddFakePrisonerClicked(object sender, EventArgs e)
    {
        if (Prisoners == null)
            Prisoners = new ObservableCollection<PlayerAccount>();

        var random = new Random();
        string[] genders = { "ذكر", "أنثى" };

        var fakePrisoner = new PlayerAccount
        {
            Username = $"سجين_وهمي_{Prisoners.Count + 1}",
            AvatarPath = "avatar_player.png",
            Gender = genders[random.Next(0, 2)],
            CrimeObject = new CrimeObject
            {
                IsInPrison = true,
                PrisonReason = "تم القبض عليه أثناء جريمة",
                PrisonReleaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                    + (10 * 60 * 1000),
                PrisonBailAmount = random.Next(1000, 50000),
                TotalPrisonVisits = random.Next(1, 15)
            }
        };

        fakePrisoner.MainStatesObject.Level = random.Next(1, 100);
        Prisoners.Add(fakePrisoner);
        BuildPrisonersList();
    }

    private void OnEnterPrisonClicked(object sender, EventArgs e)
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        _player.CrimeObject.IsInPrison = true;
        _player.CrimeObject.PrisonReason = "اختبار تجريبي — دخل السجن يدوياً";
        _player.CrimeObject.PrisonReleaseTime =
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (2 * 60 * 1000);
        _player.CrimeObject.PrisonBailAmount = 5000;
        _player.CrimeObject.TotalPrisonVisits++;

        LoadData();
        SetupFooter();
        StartCountdownTimer();
    }
#endif

    // ═══════════════════════════════════════════════════
    //  العودة للصفحة الرئيسية
    // ═══════════════════════════════════════════════════
    private async Task GoToMainPage()
    {
        try
        {
            while (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
        catch
        {
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
    }

    // ═══════════════════════════════════════════════════
    //  تحديث الوقت
    // ═══════════════════════════════════════════════════
    private void UpdateTimeDisplay()
    {
        if (_player == null || !_player.CrimeObject.IsInPrison) return;
        TimeRemainingLabel.Text = PrisonService.GetRemainingTime(
                                      _player.CrimeObject.PrisonReleaseTime);
    }

    private void StartCountdownTimer()
    {
        StopCountdownTimer();
        _countdownTimer = new Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_player?.CrimeObject == null) return;

                _player.CrimeObject.CheckConfinementStatus();

                if (!_player.CrimeObject.IsInPrison)
                {
                    StopCountdownTimer();
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                    return;
                }

                UpdateTimeDisplay();
            });
        }, null, 0, 1000);
    }

    private void StopCountdownTimer()
    {
        _countdownTimer?.Dispose();
        _countdownTimer = null;
    }

    // ═══════════════════════════════════════════════════
    //  الأحداث
    // ═══════════════════════════════════════════════════
    private async void OnJailbreakClicked(object sender, EventArgs e)
    {
        int cost = PrisonService.GetEscapeCourageCost();

        bool confirm = await PopupService.ShowConfirmAsync(
            title: "محاولة الهروب",
            message: $"هل تريد محاولة الهروب من السجن؟\n" +
                     $"التكلفة: {cost} شجاعة",
            operationType: PopupOperationType.Confirm,
            overridePositiveText: "هروب",
            overrideNegativeText: "إلغاء"
        );
        
        if (!confirm) return;

        var result = PrisonService.AttemptJailbreak(_player);

        if (result.success)
        {
            // نجح: انتقل للرئيسية فوراً ثم اشعار
            Application.Current.MainPage = new NavigationPage(new MainPage());
            await ToastService.Show("هروب ناجح!", ToastType.Success);
        }
        else
        {
            // فشل: حدّث الوقت فوراً ثم اشعار
            LoadData();
            UpdateTimeDisplay();
            await ToastService.Show("فشل الهروب! أضيف وقت إضافي.", ToastType.Error);
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (IsInPrison)
        {
            await ToastService.Show("لا يمكنك الخروج قبل انتهاء المدة!", ToastType.Error);
            return;
        }
        await GoToMainPage();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        Prisoners = new ObservableCollection<PlayerAccount>(PrisonService.GetPrisoners());
        BuildPrisonersList();
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirmed = await PopupService.ShowConfirmAsync(
            title: "تسجيل الخروج",
            message: "هل أنت متأكد؟",
            operationType: PopupOperationType.Confirm,
            overridePositiveText: "نعم",
            overrideNegativeText: "لا"
        );
        if (!confirmed) return;
        AccountService.Logout();
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }

    private async void OnPayBailForPlayer(PlayerAccount target)
        => await BailAsync(target, null);

    private async void OnSmugglePlayer(PlayerAccount target)
        => await SmuggleAsync(target, null);

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}