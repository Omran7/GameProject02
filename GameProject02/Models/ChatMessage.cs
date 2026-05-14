using GameProject02.Services;

namespace GameProject02.Models
{
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string GangTag { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public bool FromSystem { get; set; } = false;
        public bool FromAdmin { get; set; } = false;
        public bool IsCurrentPlayer => PlayerId == AccountService.GetCurrentPlayer()?.PlayerId;

        public string DisplayTime
        {
            get
            {
                var dt = DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
                return dt.ToString("HH:mm");
            }
        }
    }
}