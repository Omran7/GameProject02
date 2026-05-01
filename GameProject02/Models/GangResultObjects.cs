using System;

namespace GameProject02.Models
{
    // Exact translation of JoinGangMilitiaResultObject from Java
    public class JoinGangMilitiaResultObject
    {
        public bool HasError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsAllProcessSuccess { get; set; } = false;
        public bool IsPlayerMainStatesChanged { get; set; } = true;
        public bool IsPlayerAlreadyJoined { get; set; } = false;

        // In the old game, joining the militia used/returned a CrystalId
        public string CrystalId { get; set; } = string.Empty;

        // Comma-separated string of IDs to match the old Java format
        public string MembersIdsJoinedMilitia { get; set; } = string.Empty;
    }

    // Exact translation of the 'p' class (LeaveGangResultObject)
    public class LeaveGangResultObject
    {
        public bool HasError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsAllProcessSuccess { get; set; } = false;
        public bool IsCurrentPlayerBoss { get; set; } = true;
    }
}