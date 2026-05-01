using GameProject02.Helpers;
using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
        if (IsInHospital) StartCountdownTimer();

        // ✅ عرض الرسالة بعد أن تصبح الصفحة نشطة
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopCountdownTimer();
    }

    private void LoadData()
    {
        _player = AccountService.GetCurrentPlayer();
        if (_player == null) return;

        bool wasInHospital = _isInHospital;
        _player.CrimeObject.CheckConfinementStatus();
        IsInHospital = _player.CrimeObject.IsInHospital;

        if (IsInHospital)
        {
            ReasonLabel.Text = _player.CrimeObject.HospitalReason ?? "جرحت نفسك أثناء جريمة فاشلة";
            VisitCountLabel.Text = $"{_player.CrimeObject.TotalHospitalVisits}";
            UpdateTimeDisplay();
            InmateModeLayout.IsVisible = true;
            VisitorModeLayout.IsVisible = false;
        }
        else
        {
            if (wasInHospital && !IsInHospital) { _ = GoToMainPage(); return; }
            Patients = new ObservableCollection<PlayerAccount>(HospitalService.GetPatients());
            InmateModeLayout.IsVisible = false;
            VisitorModeLayout.IsVisible = true;
        }
    }

    private async Task GoToMainPage()
    {
        try
        {
            while (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
        catch { Application.Current.MainPage = new NavigationPage(new MainPage()); }
    }

    private void UpdateTimeDisplay()
    {
        if (_player == null || !_player.CrimeObject.IsInHospital) return;
        string remaining = HospitalService.GetRemainingTime(_player.CrimeObject.HospitalReleaseTime);
        TimeRemainingLabel.Text = remaining;
        BottomTimeLabel.Text = remaining;
    }

    private void StartCountdownTimer()
    {
        StopCountdownTimer();
        _countdownTimer = new Timer(_ =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (_player?.CrimeObject?.IsInHospital == true) UpdateTimeDisplay();
                else { StopCountdownTimer(); await GoToMainPage(); }
            });
        }, null, 0, 1000);
    }

    private void StopCountdownTimer() { _countdownTimer?.Dispose(); _countdownTimer = null; }

    private async void OnTreatmentClicked(object sender, EventArgs e) => await DisplayAlert("💊 علاج", "سيتم إضافته قريباً", "موافق");
    private async void OnPharmacyClicked(object sender, EventArgs e) => await DisplayAlert("💊 الصيدلية", "سيتم إضافتها قريباً", "موافق");

    private async void OnMarketClicked(object sender, EventArgs e) =>
        await Navigation.PushModalAsync(new NavigationPage(new MarketCategoriesPage()) { BarBackgroundColor = Color.FromArgb("#2c3e50"), BarTextColor = Colors.White });

    private async void OnStockClicked(object sender, EventArgs e) =>
        await Navigation.PushModalAsync(new NavigationPage(new StockPage()) { BarBackgroundColor = Color.FromArgb("#2c3e50"), BarTextColor = Colors.White });

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirmed = await DisplayAlert("تسجيل الخروج", "هل أنت متأكد؟", "نعم", "لا");
        if (confirmed)
        {
            AccountService.Logout();
            if (Navigation.ModalStack.Contains(this)) await Navigation.PopModalAsync();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e) => await GoToMainPage();
    private async void OnBackClicked(object sender, EventArgs e) => await GoToMainPage();

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadData();
        await DisplayAlert("🔄 تحديث", "تم تحديث قائمة المرضى", "موافق");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}