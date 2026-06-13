using System;
using GameProject02.Services;

namespace GameProject02.Models
{
    public class PrivateChatMessage
    {
        public string SenderId { get; set; } = "";
        public string Content { get; set; } = "";
        public long Timestamp { get; set; }
        public string DisplayTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).LocalDateTime.ToString("HH:mm");
        public bool IsCurrentPlayer => SenderId == AccountService.GetCurrentPlayer()?.PlayerId;
    }

    public class ConversationModel
    {
        public string ConversationId { get; set; }
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string LastMessage { get; set; }
        public long Timestamp { get; set; }
        public string TimeLabel => DateTimeOffset.FromUnixTimeSeconds(Timestamp).LocalDateTime.ToString("g");
        public int UnreadCount { get; set; }   // New field
    }
}