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

namespace GameProject02.Views;

public partial class HospitalPage : ContentPage, INotifyPropertyChanged
{
    private PlayerAccount _player;
    private Timer _countdownTimer;
    private ObservableCollection<PlayerAccount> _patients;

    // ═══════════════════════════════════════════════════
    //  أبعاد الشاشة
    // ═══════════════════════════════════════════════════
    private static double ScreenWidth => DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
    private static double ScreenHeight => DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;

    // ── أبعاد بطاقات قائمة المرضى ──────────────────
    private double CardHeight => ScreenHeight * 0.10;
    private double CardPadding => CardHeight * 0.15;
    private double CardMarginBottom => CardHeight * 0.05;
    private double CardSpacing => CardHeight * 0.06;

    private double AvatarContainerSize => CardHeight * 0.7;
    private double AvatarInnerSize => AvatarContainerSize * 0.85;

    private double LvlIconSize => CardHeight * 0.28;
    private double LvlDigitSize => CardHeight * 0.15;
    private double GenderIconSize => CardHeight * 0.20;

    // ── أبعاد بطاقة المريض ─────────────────────────
    private double InmateCardWidth => ScreenWidth * 0.55;
    private double InmateCardHeight => ScreenHeight * 0.22;
    private double InmateCardMarginTop => ScreenHeight * 0.145;
    private double InmateCardMarginRight => ScreenWidth * 0.00;

    // ── خطوط بطاقة المريض ──────────────────────────
    private double InfoFontTitle => ScreenHeight * 0.018;
    private double InfoFontValue => ScreenHeight * 0.018;
    private double InfoFontTime => ScreenHeight * 0.018;

    private bool _isInHospital;
    public bool IsInHospital
    {
        get => _isInHospital;
        set { _isInHospital = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsFreeMode)); }
    }
    public bool IsFreeMode => !IsInHospital;

    public ObservableCollection<PlayerAccount> Patients
    {
        get => _patients;
        set { _patients = value; OnPropertyChanged(); }
    }

    public HospitalPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
        SetupFooter();
        if (IsInHospital) StartCountdownTimer();
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

        var pharmacyButton = PageFooter.CreateFooterButton(
            text: "صيدلية",
            tappedHandler: OnPharmacyClicked,
            buttonImageSource: "button_background.png"
        );
        pharmacyButton.IsVisible = IsInHospital;

        var treatButton = PageFooter.CreateFooterButton(
            text: "علاج",
            tappedHandler: OnTreatmentClicked,
            buttonImageSource: "button_background.png"
        );
        treatButton.IsVisible = IsFreeMode;

#if DEBUG
        var addPatientButton = PageFooter.CreateFooterButton(
            text: "إضافة\nمريض",
            tappedHandler: OnAddFakePatientClicked,
            buttonImageSource: "button_background.png"
        );

        var enterHospitalButton = PageFooter.CreateFooterButton(
            text: "دخول\nمشفى",
            tappedHandler: OnEnterHospitalClicked,
            buttonImageSource: "button_background.png"
        );

        grid.Add(addPatientButton, 0, 0);
        grid.Add(enterHospitalButton, 1, 0);
#else
        grid.Add(new ContentView(), 0, 0);
        grid.Add(new ContentView(), 1, 0);
#endif

        grid.Add(new ContentView(), 2, 0);

        if (IsInHospital)
            grid.Add(pharmacyButton, 3, 0);
        else
            grid.Add(treatButton, 3, 0);

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
    //  بناء بطاقة المريض ديناميكياً
    // ═══════════════════════════════════════════════════
    private View BuildPatientCard(PlayerAccount patient)
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
                new ColumnDefinition { Width = GridLength.Star }
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
        if (!string.IsNullOrEmpty(patient.AvatarPath)
            && System.IO.Path.IsPathRooted(patient.AvatarPath)
            && System.IO.File.Exists(patient.AvatarPath))
            avatarSource = ImageSource.FromFile(patient.AvatarPath);
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

        // ── معلومات اللاعب ──
        var infoStack = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = CardHeight * 0.05
        };

        var nameRow = new HorizontalStackLayout
        {
            Spacing = CardHeight * 0.08,
            VerticalOptions = LayoutOptions.Center,
            FlowDirection = FlowDirection.RightToLeft
        };

        nameRow.Children.Add(new Image
        {
            Source = (patient.Gender == "أنثى") ? "icon_female.png" : "icon_male.png",
            WidthRequest = GenderIconSize,
            HeightRequest = GenderIconSize,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        });

        nameRow.Children.Add(new Label
        {
            Text = patient.Username,
            TextColor = Colors.WhiteSmoke,
            FontSize = CardHeight * 0.19,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        });

        nameRow.Children.Add(
            BuildLevelView(patient.Level, LvlIconSize, LvlDigitSize));

        infoStack.Children.Add(nameRow);

        infoStack.Children.Add(new Label
        {
            Text = patient.CrimeObject?.HospitalReason ?? "سبب غير محدد",
            TextColor = Colors.WhiteSmoke,
            FontSize = CardHeight * 0.15,
            LineBreakMode = LineBreakMode.TailTruncation
        });

        contentGrid.Add(infoStack, 1, 0);
        cardGrid.Add(contentGrid);

        return cardGrid;
    }

    // ═══════════════════════════════════════════════════
    //  بناء قائمة المرضى ديناميكياً
    // ═══════════════════════════════════════════════════
    private void BuildPatientsList()
    {
        PatientsContainer.Children.Clear();

        if (Patients == null || Patients.Count == 0)
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
                Text = "المشفى خالي حاليا لا يوجد مرضى يمكنك زيارتهم",
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
            PatientsContainer.Children.Add(centerGrid);
            return;
        }

        foreach (var patient in Patients)
            PatientsContainer.Children.Add(BuildPatientCard(patient));
    }

    // ═══════════════════════════════════════════════════
    //  تحميل البيانات
    // ═══════════════════════════════════════════════════
    private void LoadData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        bool wasInHospital = _isInHospital;
        _player.CrimeObject.CheckConfinementStatus();
        IsInHospital = _player.CrimeObject.IsInHospital;

        if (IsInHospital)
        {
            PageHeader.IsVisible = false;

            // ── ضبط أبعاد البطاقة ديناميكياً ──
            InmateInfoCard.WidthRequest = InmateCardWidth;
            InmateInfoCard.HeightRequest = InmateCardHeight;
            InmateInfoCard.Margin = new Thickness(
                0,
                InmateCardMarginTop,
                InmateCardMarginRight,
                0);

            ReasonLabel.Text = _player.CrimeObject.HospitalReason
                                          ?? "جرحت نفسك أثناء جريمة فاشلة";
            VisitCountLabel.Text = $"{_player.CrimeObject.TotalHospitalVisits}";
            ReasonLabel.FontSize = InfoFontValue;
            TimeRemainingLabel.FontSize = InfoFontTime;
            VisitCountLabel.FontSize = InfoFontValue;
            UpdateTimeDisplay();
            InmateModeLayout.IsVisible = true;
            VisitorModeLayout.IsVisible = false;
        }
        else
        {
            if (wasInHospital && !IsInHospital) { _ = GoToMainPage(); return; }
            PageHeader.IsVisible = true;
            PageHeader.HeaderTitle = "المستشفى";
            Patients = new ObservableCollection<PlayerAccount>(
                                              HospitalService.GetPatients());
            InmateModeLayout.IsVisible = false;
            VisitorModeLayout.IsVisible = true;
            BuildPatientsList();
        }
    }

    // ═══════════════════════════════════════════════════
    //  أزرار التجربة
    // ═══════════════════════════════════════════════════
#if DEBUG
    private void OnAddFakePatientClicked(object sender, EventArgs e)
    {
        if (Patients == null)
            Patients = new ObservableCollection<PlayerAccount>();

        var random = new Random();
        string[] genders = { "ذكر", "أنثى" };

        var fakePatient = new PlayerAccount
        {
            Username = $"ALQANNAS {Patients.Count + 1}",
            AvatarPath = "avatar_player.png",
            Gender = genders[random.Next(0, 2)],
            CrimeObject = new CrimeObject
            {
                IsInHospital = true,
                HospitalReason = "تلقى ضربة قاضية في المعركة",
                HospitalReleaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                      + (5 * 60 * 1000),
                TotalHospitalVisits = random.Next(1, 20)
            }
        };

        fakePatient.MainStatesObject.Level = random.Next(1, 100);
        Patients.Add(fakePatient);
        BuildPatientsList();
    }

    private void OnEnterHospitalClicked(object sender, EventArgs e)
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        _player.CrimeObject.IsInHospital = true;
        _player.CrimeObject.HospitalReason = "اختبار تجريبي — دخل المستشفى يدوياً";
        _player.CrimeObject.HospitalReleaseTime =
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (2 * 60 * 1000);
        _player.CrimeObject.TotalHospitalVisits++;

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
        if (_player == null || !_player.CrimeObject.IsInHospital) return;
        TimeRemainingLabel.Text = HospitalService.GetRemainingTime(
                                      _player.CrimeObject.HospitalReleaseTime);
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

                if (!_player.CrimeObject.IsInHospital)
                {
                    // انتهى الوقت — عودة فورية بدون await
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
    private async void OnTreatmentClicked(object sender, EventArgs e)
        => await DisplayAlert("💊 علاج", "سيتم إضافته قريباً", "موافق");

    private async void OnPharmacyClicked(object sender, EventArgs e)
        => await DisplayAlert("💊 الصيدلية", "سيتم إضافتها قريباً", "موافق");

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirmed = await DisplayAlert("تسجيل الخروج", "هل أنت متأكد؟", "نعم", "لا");
        if (!confirmed) return;

        AccountService.Logout();
        if (Navigation.ModalStack.Contains(this))
            await Navigation.PopModalAsync();
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await GoToMainPage();

    public new event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}