using System;

namespace GameProject02.Models
{
    public class GangMemberInfo
    {
        public string PlayerId { get; set; } = string.Empty;
        public string Username { get; set; } = "لاعب";
        public string ProfilePicUrl { get; set; } = "player_default";
        public GangPosition Position { get; set; } = GangPosition.Member;

        public long PersonalLoyalty { get; set; } = 0;
        public long PersonalContribution { get; set; } = 0;
        public bool IsOnline { get; set; } = false;

        // --- Militia Logic ---
        public bool IsInMilitia { get; set; } = false;

        // Helpers to match old game logic
        public bool IsCurrentPlayerBoss => Position == GangPosition.Leader;
        public bool IsAlreadyInMilitia => IsInMilitia;
    }
}