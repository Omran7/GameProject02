using GameProject02.Models;
using GameProject02.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GameProject02.Views;

public partial class NotificationCenterPage : ContentPage
{
    public ObservableCollection<NotificationItem> Notifications { get; } = new();

    // ✅ FIX: Make command public so XAML can bind to it
    public Command<NotificationItem> OnNotificationTappedCommand { get; }

    public NotificationCenterPage()
    {
        InitializeComponent();
        BindingContext = this;

        // ✅ FIX: Initialize command
        OnNotificationTappedCommand = new Command<NotificationItem>(OnNotificationTapped);

        LoadNotifications();
        NotificationService.OnNotificationsUpdated += OnNotificationsUpdated;
    }

    private void LoadNotifications()
    {
        Notifications.Clear();
        var gameNotifications = NotificationService.GetGameNotifications();
        foreach (var notification in gameNotifications)
        {
            Notifications.Add(notification);
        }
    }

    private void OnNotificationsUpdated()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadNotifications();
        });
    }

    private async void OnNotificationTapped(NotificationItem notification)
    {
        // Mark as read
        NotificationService.MarkAsRead(notification.Id);

        // Navigate if action target exists
        if (!string.IsNullOrEmpty(notification.ActionTarget))
        {
            await NavigateToTarget(notification.ActionTarget);
        }
    }

    private async Task NavigateToTarget(string target)
    {
        // Simple navigation mapper - expand as needed
        switch (target)
        {
            case "CrimePage":
                await Navigation.PushAsync(new CrimePage());
                break;
            case "ProfilePage":
                await Navigation.PushAsync(new ProfilePage());
                break;
            case "GymPage":
                await Navigation.PushAsync(new GymPage());
                break;
            case "HospitalPage":
                await Navigation.PushAsync(new HospitalPage());
                break;
            case "PrisonPage":
                await Navigation.PushAsync(new PrisonPage());
                break;
            default:
                await DisplayAlert("Info", "Feature coming soon!", "OK");
                break;
        }
    }

    private async void OnMarkAllReadClicked(object sender, EventArgs e)
    {
        NotificationService.MarkAllAsRead();
        await DisplayAlert("Done", "All notifications marked as read", "OK");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        NotificationService.OnNotificationsUpdated -= OnNotificationsUpdated;
    }
}