namespace GameProject02.Models;

public class UsedEstateListing
{
    public string ListingId { get; set; } = System.Guid.NewGuid().ToString().Substring(0, 8);
    public string SellerId { get; set; } = string.Empty;
    public int EstateId { get; set; }
    public int SalePrice { get; set; }
    public long ListedTime { get; set; } = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public bool IsSold { get; set; } = false;
    public string BuyerId { get; set; } = string.Empty;
    // ✅ TRACK SPECIFIC ESTATE INSTANCE (NOT JUST TYPE)
    public string EstateInstanceId { get; set; } 

    // Store upgraded estate snapshot
    public System.Collections.Generic.List<bool> FixedModifications { get; set; } = new();
    public System.Collections.Generic.List<long> ServantContractStartTimes { get; set; } = new();
    public int CurrentHappiness { get; set; }

    public string GetEstateTypeName() =>
        EstateObject.EstateTypes.TryGetValue(EstateId, out var type) ? type.Name : "عقار غير معروف";
}