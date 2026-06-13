namespace GameProject02.Models;

public class GangJoinRequest
{
    public string GangId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = "لاعب";
    public long Timestamp { get; set; }
}