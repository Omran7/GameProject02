namespace GameProject02.Models;

public enum NotificationType
{
    Alert,      // Attacks, Prison, etc.
    Reward,     // Daily bonuses, Level up
    System,     // Server updates
    Social      // Friend requests, Gang invites
}

public class NotificationItem
{
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public NotificationType Type { get; set; }
    public string Icon { get; set; }
    public bool IsRead { get; set; }
}
