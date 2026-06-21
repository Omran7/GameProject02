using System;

namespace GameProject02.Models;

public enum NewsType
{
    Information,
    PlayerAd,
    Bounty,
    SystemAlert=3
}

public class NewsItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public NewsType Type { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Icon { get; set; } = "📡";
    public string Reward { get; set; } = string.Empty;
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // ── Bounty fields ──
    public string BountyPlayerName { get; set; } = string.Empty;
    public int BountyLevel { get; set; }
    public int BountyGender { get; set; } // 0=male, 1=female
    public bool BountyIsVIP { get; set; }
    public bool BountyIsOnline { get; set; }
    public string BountyPlayerId { get; set; } = string.Empty;
    public string BountyProfilePic { get; set; } = string.Empty;
    public long BountyCost { get; set; }
    public string BountyPlace { get; set; } = string.Empty;
    public string BountyDescription { get; set; } = string.Empty;
}