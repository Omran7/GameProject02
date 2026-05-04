using GameProject02.Models;
using GameProject02.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class NotificationService
{
    private static NotificationState _state = new NotificationState();
    private static List<NotificationItem> _history = new List<NotificationItem>
    {
        new NotificationItem { Title = "Welcome", Message = "Welcome to Crime City. Keep your head down.", Timestamp = DateTime.Now.AddDays(-1), Type = NotificationType.System, Icon = "🏙️", IsRead = true },
        new NotificationItem { Title = "Daily Reward", Message = "Your daily login bonus is available!", Timestamp = DateTime.Now.AddMinutes(-10), Type = NotificationType.Reward, Icon = "🎁", IsRead = false }
    };

    public static NotificationState GetState()
    {
        _state.HasUnreadNews = _history.Any(n => !n.IsRead);
        return _state;
    }

    public static List<NotificationItem> GetHistory() => _history.OrderByDescending(n => n.Timestamp).ToList();

    public static void MarkAllAsRead()
    {
        foreach (var item in _history) item.IsRead = true;
        _state.HasUnreadNews = false;
    }

    public static void SetNotification(string type, bool value)
    {
        switch (type.ToLower())
        {
            case "news": _state.HasUnreadNews = value; break;
            case "chat": _state.HasUnreadMessages = value; break;
            case "profile": _state.HasUnseenProfileUpdate = value; break;
            case "reward": _state.HasUncollectedRewards = value; break;
        }
    }

    public static void AddNotification(string title, string message, NotificationType type, string icon)
    {
        _history.Add(new NotificationItem
        {
            Title = title,
            Message = message,
            Timestamp = DateTime.Now,
            Type = type,
            Icon = icon,
            IsRead = false
        });
    }
}
