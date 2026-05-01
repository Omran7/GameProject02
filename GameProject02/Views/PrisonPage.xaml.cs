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
using System.Windows.Input;

namespace GameProject02.Views;

public partial class PrisonPage : ContentPage, INotifyPropertyChanged
{
    private PlayerAccount _player;
    private Timer _countdownTimer;
    private ObservableCollection<PlayerAccount> _prisoners;

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
        PayBailForPlayerCommand = new Command<PlayerAccount>(OnPayBailForPlayer);
        SmugglePlayerCommand = new Command<PlayerAccount>(OnSmugglePlayer);
        BindingContext = this;
        LoadData();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
        if (IsInPrison) StartCountdownTimer();

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

        bool wasInPrison = _isInPrison;
        _player.CrimeObject.CheckConfinementStatus();
        IsInPrison = _player.CrimeObject.IsInPrison;

        if (IsInPrison)
        {
            ReasonLabel.Text = _player.CrimeObject.PrisonReason ?? "تم القبض عليك";
            BailAmountLabel.Text = $"{_player.CrimeObject.PrisonBailAmount:N0} ذهب";
            VisitCountLabel.Text = $"مرات السجن: {_player.CrimeObject.TotalPrisonVisits}";
            UpdateTimeDisplay();
            InmateModeLayout.IsVisible = true;
            VisitorModeLayout.IsVisible = false;
        }
        else
        {
            if (wasInPrison && !IsInPrison) { _ = GoToMainPage(); return; }
            Prisoners = new ObservableCollection<PlayerAccount>(PrisonService.GetPrisoners());
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
        if (_player == null || !_player.CrimeObject.IsInPrison) return;
        string remaining = PrisonService.GetRemainingTime(_player.CrimeObject.PrisonReleaseTime);
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
                if (_player?.CrimeObject?.IsInPrison == true) UpdateTimeDisplay();
                else { StopCountdownTimer(); await GoToMainPage(); }
            });
        }, null, 0, 1000);
    }

    private void StopCountdownTimer() { _countdownTimer?.Dispose(); _countdownTimer = null; }

    private async void OnJailbreakClicked(object sender, EventArgs e)
    {
        var result = PrisonService.AttemptJailbreak(_player);
        await DisplayAlert(result.success ? "✅ هروب ناجح!" : "❌ هروب فاشل!", result.message, "موافق");
        if (result.success) await GoToMainPage();
        else { LoadData(); UpdateTimeDisplay(); }
    }

    private async void OnStockClicked(object sender, EventArgs e) =>
        await Navigation.PushModalAsync(new NavigationPage(new StockPage()) { BarBackgroundColor = Color.FromArgb("#2c3e50"), BarTextColor = Colors.White });

    private async void OnMarketClicked(object sender, EventArgs e) =>
        await Navigation.PushModalAsync(new NavigationPage(new MarketCategoriesPage()) { BarBackgroundColor = Color.FromArgb("#2c3e50"), BarTextColor = Colors.White });

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (IsInPrison) { await DisplayAlert("🔒 السجن", "لا يمكنك الخروج قبل انتهاء المدة!", "موافق"); return; }
        await GoToMainPage();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadData();
        await DisplayAlert("🔄 تحديث", "تم تحديث قائمة السجناء", "موافق");
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirmed = await DisplayAlert("تسجيل الخروج", "هل أنت متأكد؟", "نعم", "لا");
        if (confirmed) { AccountService.Logout(); Application.Current.MainPage = new NavigationPage(new LoginPage()); }
    }

    private async void OnPayBailForPlayer(PlayerAccount target)
    {
        if (target == null) return;
        bool confirm = await DisplayAlert("تأكيد", $"هل تريد دفع كفالة {target.Username}؟", "نعم", "لا");
        if (!confirm) return;
        var result = PrisonService.PayBailForPlayer(_player, target);
        await DisplayAlert(result.success ? "✅ نجاح!" : "❌ فشل!", result.message, "موافق");
        if (result.success) LoadData();
    }

    private async void OnSmugglePlayer(PlayerAccount target)
    {
        if (target == null) return;
        bool confirm = await DisplayAlert("تأكيد", $"هل تريد تهريب {target.Username}؟", "نعم", "لا");
        if (!confirm) return;
        var result = PrisonService.AttemptJailbreakForPlayer(_player, target);
        await DisplayAlert(result.success ? "✅ تهريب ناجح!" : "❌ فشل!", result.message, "موافق");
        if (result.success) LoadData();
        else if (_player.CrimeObject.IsInPrison) { await DisplayAlert("🚨 تم القبض عليك!", "تم القبض عليك أثناء محاولة التهريب!", "موافق"); LoadData(); StartCountdownTimer(); }
    }

    private async void OnPhoneClicked(object sender, EventArgs e) => await DisplayAlert("📞 اتصال", "سيتم إضافته قريباً", "موافق");
    private async void OnChatClicked(object sender, EventArgs e) => await DisplayAlert("💬 دردشة", "غير متوفر حالياً", "موافق");
    private async void OnNewsClicked(object sender, EventArgs e) => await DisplayAlert("📰 أخبار", "غير متوفر حالياً", "موافق");

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}