namespace GameProject02.Models;

public class RentalListing
{
    public string ListingId { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);
    public string OwnerId { get; set; } = string.Empty;
    public int EstateId { get; set; } // Estate TYPE (for display)
    public string EstateInstanceId { get; set; } = string.Empty; // ✅ UNIQUE INSTANCE ID (CRITICAL FIX)
    public int TotalPriceFor30Days { get; set; }
    public long ListedTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public bool IsRented { get; set; } = false;
    public string TenantId { get; set; } = string.Empty;
    public long RentStartTime { get; set; } = 0;
    public int ActualDaysRented { get; set; } = 0;
    public int CurrentHappiness { get; set; }
    public List<bool> FixedModifications { get; set; } = new();
    public List<long> ServantContractStartTimes { get; set; } = new();


    public string GetEstateTypeName() =>
        EstateObject.EstateTypes.TryGetValue(EstateId, out var type) ? type.Name : "عقار غير معروف";

    public int CalculatePriceForDays(int days)
    {
        if (days < 1 || days > 30) return 0;
        return (int)Math.Ceiling((TotalPriceFor30Days / 30.0) * days);
    }
}