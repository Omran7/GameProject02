namespace GameProject02.Models;

public class NotificationState
{
    public int TotalCount { get; set; } = 0;
    public int UnreadCount { get; set; } = 0;
    public bool HasUnreadGameNotifications => UnreadCount > 0;

    // Last checked timestamps (for polling/sync)
    public DateTime LastSyncTime { get; set; } = DateTime.MinValue;
    public string LastNotificationId { get; set; } = string.Empty;
}