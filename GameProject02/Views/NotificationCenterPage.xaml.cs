using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class NotificationCenterPage : ContentPage
{
    public ObservableCollection<NotificationItem> Notifications { get; } = new();

    public NotificationCenterPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadNotifications();
        NotificationService.OnNotificationsUpdated += OnNotificationsUpdated;
    }

    private void LoadNotifications()
    {
        Notifications.Clear();
        var gameNotifications = NotificationService.GetGameNotifications();
        foreach (var n in gameNotifications)
            Notifications.Add(n);
    }

    private void OnNotificationsUpdated()
    {
        MainThread.BeginInvokeOnMainThread(LoadNotifications);
    }

    private async void OnNotificationTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is NotificationItem notif)
        {
            NotificationService.MarkAsRead(notif.Id);
            if (!string.IsNullOrEmpty(notif.ActionTarget))
                await NavigateToTarget(notif.ActionTarget);
        }
    }

    private async Task NavigateToTarget(string target)
    {
        switch (target)
        {
            case "CrimePage": await Navigation.PushAsync(new CrimePage()); break;
            case "ProfilePage": await Navigation.PushAsync(new ProfilePage()); break;
            case "GymPage": await Navigation.PushAsync(new GymPage()); break;
            case "HospitalPage": await Navigation.PushAsync(new HospitalPage()); break;
            case "PrisonPage": await Navigation.PushAsync(new PrisonPage()); break;
            case "MainPage": await Navigation.PushAsync(new MainPage()); break;
            default: await DisplayAlert("Info", "Feature coming soon!", "OK"); break;
        }
    }

    private async void OnMarkAllReadClicked(object sender, EventArgs e)
    {
        NotificationService.MarkAllAsRead();
        await DisplayAlert("✅", "تم تعليم كل الإشعارات كمقروءة", "OK");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        NotificationService.OnNotificationsUpdated -= OnNotificationsUpdated;
    }
}