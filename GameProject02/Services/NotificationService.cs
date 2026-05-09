using GameProject02.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services
{
    public static class NotificationService
    {
        public static event Action? OnNotificationsUpdated;

        // ── Add a notification for the current player ──────────────────
        public static void AddGameNotification(string title, string message,
            GameNotificationPriority priority = GameNotificationPriority.Normal,
            string icon = "🔔", string actionTarget = "")
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return;

            var notification = new NotificationItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Category = NotificationCategory.Game,
                Priority = priority,
                Icon = icon,
                IsRead = false,
                ActionTarget = actionTarget,
                PlayerId = player.PlayerId
            };

            player.Notifications.Insert(0, notification);
            // Keep only last 50 notifications (optional)
            if (player.Notifications.Count > 50)
                player.Notifications = player.Notifications.Take(50).ToList();

            // Sync the entire player to Firestore (fire‑and‑forget)
            _ = FirebaseService.SavePlayerAsync(player);

            OnNotificationsUpdated?.Invoke();
        }

        // ── Get notifications for the current player ──────────────────
        public static List<NotificationItem> GetGameNotifications(bool unreadOnly = false)
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return new List<NotificationItem>();

            var cutoff = DateTime.UtcNow.AddDays(-3);
            var query = player.Notifications
                .Where(n => n.IsGameNotification && n.Timestamp >= cutoff);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            return query.OrderByDescending(n => n.Timestamp).ToList();
        }

        public static int GetUnreadCount()
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return 0;
            var cutoff = DateTime.UtcNow.AddDays(-3);
            return player.Notifications.Count(n => n.IsGameNotification && !n.IsRead && n.Timestamp >= cutoff);
        }

        public static void MarkAsRead(string notificationId)
        {
            var player = AccountService.GetCurrentPlayer();
            var notif = player?.Notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notif != null && !notif.IsRead)
            {
                notif.IsRead = true;
                _ = FirebaseService.SavePlayerAsync(player);
                OnNotificationsUpdated?.Invoke();
            }
        }

        public static void MarkAllAsRead()
        {
            var player = AccountService.GetCurrentPlayer();
            if (player == null) return;
            foreach (var n in player.Notifications)
                n.IsRead = true;
            _ = FirebaseService.SavePlayerAsync(player);
            OnNotificationsUpdated?.Invoke();
        }

        public static void ClearAll()
        {
            // No need to clear – the list belongs to the player object and is replaced on login.
            // This method can be left empty or removed.
        }
    }
}