using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameProject02.Views;

public partial class GangProfilePage : ContentPage, INotifyPropertyChanged
{
    private PlayerAccount _player;
    private GangObject _gang;
    private bool _isRefreshing = false;
    public bool IsRefreshing { get => _isRefreshing; set { _isRefreshing = value; OnPropertyChanged(); } }

    public ICommand RefreshCommand { get; }

    public GangProfilePage()
    {
        InitializeComponent();
        RefreshCommand = new Command(async () => await RefreshData());
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshData();
        MessagingCenter.Subscribe<string>(this, "RefreshGangProfile", async (_) => await RefreshData());
        MessagingCenter.Subscribe<GangObject>(this, "GangDataUpdated", async (gang) => { if (_gang != null && _gang.GangId == gang.GangId) await RefreshData(); });
        MessagingCenter.Subscribe<object, string>(this, "GangStatusChanged", async (_, gangId) => { if (gangId == _gang?.GangId) await RefreshData(); });

        // ✅ New: Handle militia join without full Firestore reload
        MessagingCenter.Subscribe<string>(this, "MilitiaJoined", _ =>
        {
            // Just update the UI using the already updated _gang object
            if (_gang != null)
            {
                BindingContext = _gang;
                LoadMembers();
                SettingsButton.IsVisible = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.AcceptJoinRequest);
            }
        });
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<string>(this, "RefreshGangProfile");
        MessagingCenter.Unsubscribe<GangObject>(this, "GangDataUpdated");
        MessagingCenter.Unsubscribe<object, string>(this, "GangStatusChanged");
        MessagingCenter.Unsubscribe<string>(this, "MilitiaJoined");
    }
    private async Task RefreshData()
    {
        IsRefreshing = true;
        try
        {
            var currentPlayer = AccountService.CurrentPlayer;
            if (currentPlayer == null)
            {
                await Navigation.PopToRootAsync(false);
                return;
            }

            // Reload player from Firestore
            var freshPlayer = await FirebaseService.LoadPlayerAsync(currentPlayer.PlayerId);
            if (freshPlayer == null)
            {
                await Navigation.PopToRootAsync(false);
                return;
            }

            // Reload gang
            GangObject freshGang = null;
            if (!string.IsNullOrEmpty(freshPlayer.GangId))
            {
                freshGang = await GangDatabaseService.GetGangAsync(freshPlayer.GangId);
            }

            if (freshGang == null)
            {
                // No gang – go back
                await Navigation.PopToRootAsync(false);
                return;
            }

            freshPlayer.GangObject = freshGang;
            AccountService.CurrentPlayer = freshPlayer;
            _player = freshPlayer;
            _gang = freshGang;

            BindingContext = _gang;
            LoadMembers();
            SettingsButton.IsVisible = GangService.CanPerformAction(_gang, _player.PlayerId, GangAction.AcceptJoinRequest);
        }
        finally
        {
            IsRefreshing = false;
        }
    }
    private void LoadMembers()
    {
        if (_gang == null) return;
        var members = GangService.GetGangMembers(_gang);
        MembersList.ItemsSource = new ObservableCollection<GangMemberInfo>(members);
    }

    private async void OnGangMarketClicked(object sender, EventArgs e) => await Navigation.PushAsync(new GangMarketCategoriesPage(), false);
    private async void OnDonateClicked(object sender, EventArgs e)
    {
        if (_gang == null || _player == null) return;
        string amountStr = await DisplayPromptAsync("تبرع للعصابة", "أدخل المبلغ الذي تريد التبرع به (ذهب):", keyboard: Microsoft.Maui.Keyboard.Numeric);
        if (!int.TryParse(amountStr, out int amount) || amount <= 0) return;

        var result = await GangService.DonateToGangAsync(_player, _gang, amount);
        await DisplayAlert(result.success ? "✅ نجاح" : "❌ فشل", result.message, "موافق");
        if (result.success)
        {
            await RefreshData(); // Reload the gang profile page to show updated loyalty
        }
    }
    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync(false);
    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync(false);
    private async void OnLevelUpgradeClicked(object sender, EventArgs e) => await Navigation.PushAsync(new GangLevelUpgradePage(), false);
    private async void OnSettingsClicked(object sender, EventArgs e) => await Navigation.PushAsync(new GangManagementPage(), false);
    private async void OnOpenMilitiaPageClicked(object sender, EventArgs e)
    {
        if (_gang != null) await Navigation.PushAsync(new GangMilitiaPage(_gang), false);
        else await DisplayAlert("خطأ", "بيانات العصابة غير متوفرة", "موافق");
    }
    private async void OnSkillsClicked(object sender, EventArgs e) => await Navigation.PushAsync(new GangSkillsPage(), false);

    public new event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}