using GameProject02.Models;
using System;
using System.Collections.Generic;

namespace GameProject02.Services;

public static class NewsService
{
    public static List<NewsItem> GetLatestNews()
    {
        return new List<NewsItem>
        {
            new NewsItem
            {
                Title = "SYSTEM ALERT",
                Content = "The Industrial District is now open for expansion. New rackets and missions await.",
                Date = DateTime.Now.AddMinutes(-30),
                Type = NewsType.SystemAlert,
                Icon = "📡"
            },
            new NewsItem
            {
                Title = "BOUNTY: EL_CAPITAN",
                Content = "Wanted for repeated hits on the Stock Market. Last seen near the Black Market.",
                Date = DateTime.Now.AddHours(-2),
                Type = NewsType.Bounty,
                Icon = "🎯",
                Reward = "$500,000"
            },
            new NewsItem
            {
                Author = "Killer_4_Hire",
                Content = "Selling high-grade stimulants in block 4. DM for prices.",
                Date = DateTime.Now.AddHours(-1),
                Type = NewsType.PlayerAd,
                Icon = "👤"
            },
            new NewsItem
            {
                Title = "City Maintenance",
                Content = "Server maintenance scheduled for 03:00 UTC. Expect minor downtime.",
                Date = DateTime.Now.AddDays(-1),
                Type = NewsType.Information,
                Icon = "🛠️"
            }
        };
    }
}
