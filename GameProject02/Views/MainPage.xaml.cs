using GameProject02.Models;
using GameProject02.Services;
using GameProject02.Views;
using GameProject02.Helpers;
using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace GameProject02;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private bool _banAlertShown = false; // ✅ Track if ban alert was shown
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _nobilityText = "100/100";
    private string _xpText = "0/100";

    public string NobilityText
    {
        get => _nobilityText;
        set { if (_nobilityText != value) { _nobilityText = value; OnPropertyChanged(); } }
    }

    public string XPText
    {
        get => _xpText;
        set { if (_xpText != value) { _xpText = value; OnPropertyChanged(); } }
    }

    private PlayerAccount _player;
    private System.Timers.Timer _refreshTimer;
    private bool _isRefreshing = false;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadPlayerData();
        ApplyLanguage();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();

        LoadPlayerData();
        ApplyLanguage();

        _refreshTimer = new System.Timers.Timer(3000);
        _refreshTimer.Elapsed += OnRefreshTimerElapsed;
        _refreshTimer.Start();

        CheckPrisonStateAndNavigate();

        // ✅ Show ban alert ONLY ONCE after page loads
        if (!_banAlertShown && _player != null)
        {
            _banAlertShown = true; // Mark as shown
            await Task.Delay(500); // Wait for page to fully render
            //await BanHelper.ShowBansOnLogin(_player);
        }

        // ✅ Subscribe to gang status changes
        MessagingCenter.Subscribe<object, string>(this, "GangStatusChanged", (_, gangId) =>
        {
            LoadPlayerData();
        });
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        MessagingCenter.Unsubscribe<object, string>(this, "GangStatusChanged");
    }

    private void LoadPlayerData()
    {
        _player = AccountService.GetCurrentPlayer();
        var player = AccountService.GetCurrentPlayer();
        if (player != null) MedalService.CheckAndAwardAll(player);
        if (_player == null) return;

        // Ensure gang object is loaded if GangId exists but GangObject is null
        if (!string.IsNullOrEmpty(_player.GangId) && _player.GangObject == null)
        {
            // Try to load the gang object (will be loaded by the poller anyway, but this is a fallback)
            Task.Run(async () =>
            {
                var gang = await GangDatabaseService.GetGangAsync(_player.GangId);
                if (gang != null)
                {
                    _player.GangObject = gang;
                    AccountService.CurrentPlayer = _player;
                }
            });
        }

        XPText = $"{_player.CurrentXP}/{_player.MaxXP}";
        NobilityText = $"{_player.NobilityCurrent}/100";

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_player == null) return;
        // The TopHeaderView handles its own updates; we can leave this as is
    }

    private void ApplyLanguage()
    {
        if (GymButton != null) GymButton.Text = LanguageManager.TrainGym;
        if (SchoolButton != null) SchoolButton.Text = LanguageManager.StudySchool;
        if (StockButton != null) StockButton.Text = LanguageManager.Stock;

        FlowDirection = LanguageManager.CurrentLanguage == LanguageManager.Language.Arabic
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }

    private void OnRefreshTimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (_isRefreshing || _player == null) return;

        _isRefreshing = true;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                NobilityService.UpdateNobility(_player);
                XPText = $"{_player.CurrentXP}/{_player.MaxXP}";
                NobilityText = $"{_player.NobilityCurrent}/100";

                // Refresh gang status in case it changed without a message
                if (_player.GangObject == null && !string.IsNullOrEmpty(_player.GangId))
                {
                    LoadPlayerData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Timer Error] {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        });
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirmed = await DisplayAlert("تسجيل الخروج", "هل أنت متأكد من تسجيل الخروج؟", "نعم", "لا");
        if (confirmed)
        {
            AccountService.Logout();
            // Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }

    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PushAsync(new ProfilePage(), false);
    private async void OnGymClicked(object sender, EventArgs e) => await Navigation.PushAsync(new GymPage(), false);
    private async void OnSchoolClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SchoolPage(), false);
    private async void OnStockClicked(object sender, EventArgs e) => await Navigation.PushAsync(new StockPage(), false);
    private async void OnMarketClicked(object sender, EventArgs e) => await Navigation.PushAsync(new MarketCategoriesPage(), false);
    private async void OnGangMarketClicked(object sender, EventArgs e) => await Navigation.PushAsync(new GangMarketCategoriesPage(), false);
    private async void OnBlackMarketClicked(object sender, EventArgs e) => await Navigation.PushAsync(new BlackMarketMenuPage(), false);
    private async void OnEstateClicked(object sender, EventArgs e) => await Navigation.PushAsync(new EstatePage(), false);
    private async void OnWorkOfficeClicked(object sender, EventArgs e) => await Navigation.PushAsync(new WorkOfficePage(), false);
    private async void OnCrimeClicked(object sender, EventArgs e) => await Navigation.PushAsync(new CrimePage(), false);
    private async void OnHospitalClicked(object sender, EventArgs e) => await Navigation.PushAsync(new HospitalPage(), false);
    private async void OnPrisonClicked(object sender, EventArgs e) => await Navigation.PushAsync(new PrisonPage(), false);
    private async void OnFightClubClicked(object sender, EventArgs e) => await Navigation.PushAsync(new FightClubPage(), false);
    private async void OnNotificationCenterClicked(object sender, EventArgs e) => await Navigation.PushAsync(new NotificationCenterPage(), false);
    private async void OnNewsClicked(object sender, EventArgs e) => await Navigation.PushAsync(new NewsPage(), false);
    private async void OnAirportClicked(object sender, EventArgs e) => await Navigation.PushAsync(new AirportPage(), false);
    private async void OnChatClicked(object sender, EventArgs e) => await Navigation.PushAsync(new ChatPage(), false);
    private async void OnSkillsClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SkillsPage(), false);
    private async void OnTheShopClicked(object sender, EventArgs e) => await Navigation.PushAsync(new TheShopPage(), false);
    private async void OnLuckyWheelClicked(object sender, EventArgs e) => await Navigation.PushAsync(new LuckyWheelPage(), false);
    private async void OnPrivateChatClicked(object sender, EventArgs e) => await Navigation.PushAsync(new PrivateChatListPage(), false);
    private async void OnMedalsClicked(object sender, EventArgs e) => await Navigation.PushAsync(new MedalsPage(), false);
    private async void OnAdminPanelClicked(object sender, EventArgs e) => await Navigation.PushAsync(new AdminPanelPage(), false);

    private async void OnGangClicked(object sender, EventArgs e)
    {
        // Always use the latest player data from AccountService
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        // If player has GangId but GangObject is null, try to load it now
        if (!string.IsNullOrEmpty(player.GangId) && player.GangObject == null)
        {
            var gang = await GangDatabaseService.GetGangAsync(player.GangId);
            if (gang != null)
            {
                player.GangObject = gang;
                AccountService.CurrentPlayer = player;
            }
        }

        bool isInGang = player.GangObject != null && player.GangObject.IsMember(player.PlayerId);

        if (!isInGang)
        {
            bool createNew = await DisplayAlert(
                "العصابات",
                "أنت لست في عصابة. هل تريد إنشاء عصابة جديدة أو البحث عن واحدة؟",
                "إنشاء",
                "بحث");

            if (createNew)
                await Navigation.PushAsync(new Views.GangCreatePage(), false);
            else
                await Navigation.PushAsync(new Views.GangSearchPage(), false);
        }
        else
        {
            await Navigation.PushAsync(new Views.GangProfilePage(), false);
        }
    }

    private void OnSwitchLanguageClicked(object sender, EventArgs e)
    {
        LanguageManager.CurrentLanguage = LanguageManager.CurrentLanguage == LanguageManager.Language.Arabic
            ? LanguageManager.Language.English
            : LanguageManager.Language.Arabic;
        ApplyLanguage();
        UpdateUI();
    }

    private async void CheckPrisonStateAndNavigate()
    {
        if (_player == null) return;
        _player.CrimeObject.CheckConfinementStatus();
        if (_player.CrimeObject.IsInPrison)
        {
            await Navigation.PushModalAsync(new PrisonPage());
        }
    }

    private async void OnCityMapClicked(object sender, EventArgs e) => await Navigation.PushAsync(new CityMapPage(), false);
    private async void OnIdlibCityMapClicked(object sender, EventArgs e) => await Navigation.PushAsync(new CityMapPage(), false);
}