namespace GameProject02.Models;

public class FightClubPlayer
{
    public string PlayerId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ImageResource { get; set; } = "default_avatar.png";
    public int Level { get; set; }
    public int HealthCurrent { get; set; }
    public int HealthMax { get; set; }
    public int Gender { get; set; } // 0=male, 1=female
    public bool IsVIP { get; set; }
    public bool IsOnline { get; set; }
    public int Strength { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public int Dexterity { get; set; }
    public string CurrentCity { get; set; } = string.Empty;
    public int CurrentPlace { get; set; }
    public int Gold { get; set; }
    public int MaxHealth { get; set; }
}