using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameProject02.Views;

public class UserRoleViewModel
{
    public PlayerAccount Player { get; }
    public string Username => Player.Username;
    public int Level => Player.Level;

    public string DisplayRole => AdminService.GetPlayerRole(Player);

    public Color RoleColor => Player.IsAdmin ? Colors.Gold :
                              Player.IsTemporaryAdmin ? Colors.Orange :
                              Player.IsManager ? Colors.LightBlue :
                              Colors.Gray;

    public bool CanPromote => !Player.IsAdmin && !Player.IsTemporaryAdmin && !Player.IsManager;
    public bool CanDemote => Player.IsAdmin || Player.IsTemporaryAdmin || Player.IsManager;

    public UserRoleViewModel(PlayerAccount player)
    {
        Player = player;
    }
}

public partial class AdminManagementPage : ContentPage
{
    private ObservableCollection<UserRoleViewModel> _allPlayers = new();
    public ObservableCollection<UserRoleViewModel> FilteredPlayers { get; } = new();
    public ICommand PromoteCommand { get; }
    public ICommand DemoteCommand { get; }

    public AdminManagementPage()
    {
        InitializeComponent();
        BindingContext = this;
        PromoteCommand = new Command<UserRoleViewModel>(OnPromote);
        DemoteCommand = new Command<UserRoleViewModel>(OnDemote);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllPlayers();
    }

    private async Task LoadAllPlayers()
    {
        var currentAdmin = AccountService.GetCurrentPlayer();
        if (currentAdmin == null || !AdminService.IsPlayerAdmin(currentAdmin))
        {
            await DisplayAlert("خطأ", "ليس لديك صلاحية", "موافق");
            await Navigation.PopAsync(false);
            return;
        }

        // 🔥 Force fresh load from Firestore (no cache)
        var all = await AdminService.GetAllPlayersFreshAsync();
        _allPlayers.Clear();
        foreach (var p in all)
        {
            if (p.PlayerId == currentAdmin.PlayerId)
                continue;

            var vm = new UserRoleViewModel(p);
            Debug.WriteLine($"[ADMIN] Loaded: {p.Username}, IsAdmin={p.IsAdmin}, IsManager={p.IsManager}, Temp={p.IsTemporaryAdmin}, Role={vm.DisplayRole}");
            _allPlayers.Add(vm);
        }
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchEntry.Text?.Trim()?.ToLower() ?? "";
        var filtered = string.IsNullOrEmpty(query)
            ? _allPlayers
            : _allPlayers.Where(vm => vm.Username.ToLower().Contains(query));
        FilteredPlayers.Clear();
        foreach (var vm in filtered.OrderBy(vm => vm.Username))
            FilteredPlayers.Add(vm);
        UsersList.ItemsSource = null;
        UsersList.ItemsSource = FilteredPlayers;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();
    private void OnSearchClicked(object sender, EventArgs e) => ApplyFilter();

    private async void OnPromote(UserRoleViewModel vm)
    {
        var admin = AccountService.GetCurrentPlayer();
        if (admin == null || !AdminService.IsPlayerAdmin(admin))
        {
            await DisplayAlert("خطأ", "ليس لديك صلاحية", "موافق");
            return;
        }

        var player = vm.Player;
        if (!vm.CanPromote)
        {
            await DisplayAlert("تنبيه", "هذا اللاعب بالفعل لديه دور إداري", "موافق");
            return;
        }

        string action = await DisplayActionSheet("اختر الإجراء", "إلغاء", null, "ترقية إلى مدير");
        if (action == "إلغاء") return;

        bool success = await AdminService.PromoteToManagerAsync(player.PlayerId);
        await DisplayAlert(success ? "نجاح" : "فشل", success ? "تمت الترقية إلى مدير" : "حدث خطأ", "موافق");

        if (success)
        {
            // Retry loading until the change is visible (up to 4 attempts)
            for (int i = 0; i < 4; i++)
            {
                await Task.Delay(500);
                await LoadAllPlayers();
                var updated = _allPlayers.FirstOrDefault(v => v.Player.PlayerId == player.PlayerId);
                if (updated != null && updated.DisplayRole == "مدير")
                    break;
            }
        }
    }

    private async void OnDemote(UserRoleViewModel vm)
    {
        var admin = AccountService.GetCurrentPlayer();
        if (admin == null || !AdminService.IsPlayerAdmin(admin))
        {
            await DisplayAlert("خطأ", "ليس لديك صلاحية", "موافق");
            return;
        }

        var player = vm.Player;
        if (player.PlayerId == admin.PlayerId)
        {
            await DisplayAlert("تنبيه", "لا يمكنك تنزيل نفسك", "موافق");
            return;
        }

        if (!vm.CanDemote)
        {
            await DisplayAlert("تنبيه", "هذا اللاعب ليس لديه أي دور للإدارة", "موافق");
            return;
        }

        string role = AdminService.GetPlayerRole(player);
        bool confirm = await DisplayAlert("تأكيد التنزيل",
            $"سيتم إزالة دور {role} من {player.Username}. هل أنت متأكد؟",
            "نعم", "إلغاء");
        if (!confirm) return;

        bool success = await AdminService.DemotePlayerAsync(player.PlayerId);
        await DisplayAlert(success ? "نجاح" : "فشل", success ? "تم التنزيل" : "حدث خطأ", "موافق");

        if (success)
        {
            for (int i = 0; i < 4; i++)
            {
                await Task.Delay(500);
                await LoadAllPlayers();
                var updated = _allPlayers.FirstOrDefault(v => v.Player.PlayerId == player.PlayerId);
                if (updated != null && updated.DisplayRole == "لاعب عادي")
                    break;
            }
        }
    }
}