using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameProject02.Views;

public partial class AdminPanelPage : ContentPage
{
    public ObservableCollection<AdminMenuItem> MenuItems { get; } = new();
    public string RoleDisplay { get; set; }
    public ICommand MenuItemTappedCommand { get; }
    public ICommand GoHomeCommand { get; }

    public AdminPanelPage()
    {
        InitializeComponent();
        BindingContext = this;
        MenuItemTappedCommand = new Command<AdminMenuItem>(OnMenuItemTapped);
        GoHomeCommand = new Command(OnGoHome);
        LoadMenu();
    }

    private void LoadMenu()
    {
        var player = AccountService.GetCurrentPlayer();
        if (player == null || !AdminService.IsPlayerAdmin(player) && !AdminService.IsPlayerManager(player))
        {
            DisplayAlert("خطأ", "ليس لديك صلاحية", "موافق");
            Navigation.PopAsync();
            return;
        }

        // Set role display
        if (AdminService.IsPlayerAdmin(player))
            RoleDisplay = "مسؤول كامل";
        else if (AdminService.IsPlayerManager(player))
            RoleDisplay = "مدير";
        OnPropertyChanged(nameof(RoleDisplay));

        MenuItems.Clear();

        // Admin-only features
        if (AdminService.IsPlayerAdmin(player))
        {
            MenuItems.Add(new AdminMenuItem { Title = "📋 مراجعة الطلبات", Icon = "📋", Action = "requests" });
            MenuItems.Add(new AdminMenuItem { Title = "👥 إدارة المسؤولين", Icon = "👥", Action = "manage" });
            MenuItems.Add(new AdminMenuItem { Title = "🚫 حظر اللاعبين", Icon = "🚫", Action = "ban" });
            MenuItems.Add(new AdminMenuItem { Title = "📢 إرسال إعلان", Icon = "📢", Action = "announce" });
        }
        else // Manager
        {
            MenuItems.Add(new AdminMenuItem { Title = "📋 تقديم بلاغ حظر", Icon = "📋", Action = "report" });
        }
    }

    private async void OnMenuItemTapped(AdminMenuItem item)
    {
        switch (item.Action)
        {
            case "requests":
                await Navigation.PushAsync(new AdminRequestsPage());
                break;
            case "ban":
                await Navigation.PushAsync(new AdminBanPlayerPage());
                break;
            case "announce":
                await Navigation.PushAsync(new AdminAnnouncementPage());
                break;
            case "manage":
                await Navigation.PushAsync(new AdminManagementPage());
                break;
            case "report":
                await Navigation.PushAsync(new ManagerReportPage());
                break;
            default:
                await DisplayAlert("قريباً", "هذه الميزة قيد التطوير", "موافق");
                break;
        }
    }

    private async void OnGoHome() => await Navigation.PopAsync();
}