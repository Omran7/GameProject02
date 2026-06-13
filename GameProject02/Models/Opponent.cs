namespace GameProject02.Models;

public class Opponent
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int TotalHealth { get; set; }
    public int CurrentHealth { get; set; }
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public int Speed { get; set; }
    public int GoldReward { get; set; }
    public int XPReward { get; set; }
    public string Description { get; set; }
    public string Avatar { get; set; } = "dotnet_bot.png";

    // Predefined opponents matching player progression
    public static Opponent[] GetAvailableOpponents(int playerLevel)
    {
        var opponents = new[]
        {
            new Opponent
            {
                Name = "المبتدئ",
                Level = 1,
                TotalHealth = 80,
                CurrentHealth = 80,
                AttackPower = 8,
                DefensePower = 3,
                Speed = 4,
                GoldReward = 50,
                XPReward = 10,
                Description = "مجرم محلي صغير",
                Avatar = "dotnet_bot.png"
            },
            new Opponent
            {
                Name = "البلطجي",
                Level = 5,
                TotalHealth = 150,
                CurrentHealth = 150,
                AttackPower = 15,
                DefensePower = 8,
                Speed = 7,
                GoldReward = 120,
                XPReward = 25,
                Description = "بلطجي شوارع خبير",
                Avatar = "dotnet_bot.png"
            },
            new Opponent
            {
                Name = "الزعران",
                Level = 10,
                TotalHealth = 250,
                CurrentHealth = 250,
                AttackPower = 25,
                DefensePower = 15,
                Speed = 12,
                GoldReward = 300,
                XPReward = 60,
                Description = "زعيم عصابة صغيرة",
                Avatar = "dotnet_bot.png"
            },
            new Opponent
            {
                Name = "السياف",
                Level = 20,
                TotalHealth = 450,
                CurrentHealth = 450,
                AttackPower = 40,
                DefensePower = 25,
                Speed = 20,
                GoldReward = 800,
                XPReward = 150,
                Description = "مقاتل محترف بالسيف",
                Avatar = "dotnet_bot.png"
            },
            new Opponent
            {
                Name = "الوحش",
                Level = 40,
                TotalHealth = 900,
                CurrentHealth = 900,
                AttackPower = 75,
                DefensePower = 50,
                Speed = 35,
                GoldReward = 2500,
                XPReward = 400,
                Description = "أسطورة الشوارع - نادراً ما يُهزم",
                Avatar = "dotnet_bot.png"
            }
        };

        // Filter opponents appropriate for player level
        return opponents;
    }
}