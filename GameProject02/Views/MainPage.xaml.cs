using GameProject02.Models;
using GameProject02.Services;
using GameProject02.Views;
using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using Microsoft.Maui.ApplicationModel;

namespace GameProject02;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
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

    protected override void OnAppearing()
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
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
    }

    private void LoadPlayerData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        XPText = $"{_player.CurrentXP}/{_player.MaxXP}";
        NobilityText = $"{_player.NobilityCurrent}/100";

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_player == null) return;
        // In the new layout, the TopHeaderView handles its own updates, 
        // but we can trigger it here if needed or just let its timer run.
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

    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PushAsync(new ProfilePage());
    private async void OnGymClicked(object sender, EventArgs e) => await Navigation.PushAsync(new GymPage());
    private async void OnSchoolClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SchoolPage());
    private async void OnStockClicked(object sender, EventArgs e) => await Navigation.PushAsync(new StockPage());
    private async void OnMarketClicked(object sender, EventArgs e) => await Navigation.PushAsync(new MarketCategoriesPage());
    private async void OnGangMarketClicked(object sender, EventArgs e) => await Navigation.PushAsync(new GangMarketCategoriesPage());
    private async void OnBlackMarketClicked(object sender, EventArgs e) => await Navigation.PushAsync(new BlackMarketMenuPage());
    private async void OnEstateClicked(object sender, EventArgs e) => await Navigation.PushAsync(new EstatePage());
    private async void OnWorkOfficeClicked(object sender, EventArgs e) => await Navigation.PushAsync(new WorkOfficePage());
    private async void OnCrimeClicked(object sender, EventArgs e) => await Navigation.PushAsync(new CrimePage());
    private async void OnHospitalClicked(object sender, EventArgs e) => await Navigation.PushAsync(new HospitalPage());
    private async void OnPrisonClicked(object sender, EventArgs e) => await Navigation.PushAsync(new PrisonPage());
    private async void OnFightClubClicked(object sender, EventArgs e) => await Navigation.PushAsync(new FightClubPage());
    private async void OnNotificationCenterClicked(object sender, EventArgs e) => await Navigation.PushAsync(new NotificationCenterPage());
    private async void OnNewsClicked(object sender, EventArgs e) => await Navigation.PushAsync(new NewsPage());
    private async void OnAirportClicked(object sender, EventArgs e) => await Navigation.PushAsync(new AirportPage());

    private async void OnGangClicked(object sender, EventArgs e)
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null) return;

        bool isInGang = player.GangObject != null && player.GangObject.IsMember(player.PlayerId);

        if (!isInGang)
        {
            bool createNew = await DisplayAlert(
                "العصابات",
                "أنت لست في عصابة. هل تريد إنشاء عصابة جديدة أو البحث عن واحدة؟",
                "إنشاء",
                "بحث");

            if (createNew)
                await Navigation.PushAsync(new Views.GangCreatePage());
            else
                await Navigation.PushAsync(new Views.GangSearchPage());
        }
        else
        {
            await Navigation.PushAsync(new Views.GangProfilePage());
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

    private async void OnCityMapClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CityMapPage());
    }
    private async void OnIdlibCityMapClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CityMapPage());
    }
}
