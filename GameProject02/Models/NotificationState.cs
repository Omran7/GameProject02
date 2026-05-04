namespace GameProject02.Models;

public class NotificationState
{
    public bool HasUnreadNews { get; set; }
    public bool HasUnreadMessages { get; set; }
    public bool HasUnseenProfileUpdate { get; set; }
    public bool HasUncollectedRewards { get; set; }

    public bool HasAny => HasUnreadNews || HasUnreadMessages || HasUnseenProfileUpdate || HasUncollectedRewards;
}
