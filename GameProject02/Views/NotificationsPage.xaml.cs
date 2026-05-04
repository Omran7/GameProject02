using GameProject02.Services;
using Microsoft.Maui.Controls;

namespace GameProject02.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage()
    {
        InitializeComponent();
        LoadNotifications();
    }

    private void LoadNotifications()
    {
        var history = NotificationService.GetHistory();
        NotificationList.ItemsSource = history;

        // As soon as we open the page, mark all as read like a digital pager
        NotificationService.MarkAllAsRead();
    }
}
