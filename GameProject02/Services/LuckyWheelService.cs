using GameProject02.Models; // ✅ Uses Models.WheelReward
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameProject02.Services;

public static class LuckyWheelService
{
    // 🎡 AUTHENTIC OLD GAME SEGMENTS
    public static List<WheelReward> GetSegments() => new()
    {
        new WheelReward { Id = "gold_500", Name = "500 ذهب", Value = 500, Type = "Gold", Weight = 25 },
        new WheelReward { Id = "gold_2000", Name = "2,000 ذهب", Value = 2000, Type = "Gold", Weight = 15 },
        new WheelReward { Id = "diamonds_5", Name = "5 ماس", Value = 5, Type = "Diamonds", Weight = 12 },
        new WheelReward { Id = "diamonds_20", Name = "20 ماس", Value = 20, Type = "Diamonds", Weight = 5 },
        new WheelReward { Id = "merits_20", Name = "20 استحقاق", Value = 20, Type = "Merits", Weight = 20 },
        new WheelReward { Id = "merits_100", Name = "100 استحقاق", Value = 100, Type = "Merits", Weight = 8 },
        new WheelReward { Id = "medals_3", Name = "3 ميداليات", Value = 3, Type = "Medals", Weight = 15 }
    };

    // 🎲 WEIGHTED RANDOM SPIN
    public static WheelReward Spin()
    {
        var segments = GetSegments();
        int totalWeight = segments.Sum(s => s.Weight);
        var random = new Random();
        int target = random.Next(1, totalWeight + 1);
        int current = 0;

        foreach (var seg in segments)
        {
            current += seg.Weight;
            if (target <= current) return seg;
        }
        return segments[0];
    }

    // 🏆 APPLY REWARD & SAVE
    public static (bool success, string message) ApplyReward(PlayerAccount player, WheelReward reward, int spins = 1)
    {
        for (int i = 0; i < spins; i++)
        {
            switch (reward.Type)
            {
                case "Merits": player.Merits += reward.Value; break;
                case "Diamonds": player.Diamonds += reward.Value; break;
                case "Gold": player.Gold += reward.Value; break;
                case "Medals": player.Medals += reward.Value; break;
            }
        }
        AccountService.SavePlayer(player);
        return (true, $"🎉 ربحت {reward.Name}!");
    }
}