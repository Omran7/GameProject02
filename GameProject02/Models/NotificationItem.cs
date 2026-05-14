using System;

namespace GameProject02.Models
{
    public enum GameNotificationPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public enum NotificationCategory
    {
        General = 0,
        Gang = 1,
        Game = 0,
        Social = 1,
        System = 2
    }

    public class NotificationItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public NotificationCategory Category { get; set; } = NotificationCategory.Game;
        public GameNotificationPriority Priority { get; set; } = GameNotificationPriority.Normal;
        public string Icon { get; set; } = "🔔";
        public bool IsRead { get; set; } = false;
        public string ActionTarget { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;

        public bool IsHighPriority => Priority >= GameNotificationPriority.High;
        public bool IsGameNotification => Category == NotificationCategory.Game;
    }
}