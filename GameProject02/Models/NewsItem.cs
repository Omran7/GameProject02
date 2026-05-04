using System;

namespace GameProject02.Models;

public enum NewsType
{
    Information,
    PlayerAd,
    Bounty,
    SystemAlert
}

public class NewsItem
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime Date { get; set; }
    public NewsType Type { get; set; }
    public string Author { get; set; } // For Player Ads
    public string Icon { get; set; }
    public string Reward { get; set; } // For Bounties
}
