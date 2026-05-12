namespace GameProject02.Models;

public class AirportDestination
{
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public int TravelCostGold { get; set; }
    public int TravelTimeSeconds { get; set; }   // actual flight duration
}