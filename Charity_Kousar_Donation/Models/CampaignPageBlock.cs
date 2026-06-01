namespace Charity_Kousar_Donation.Models;

public class CampaignPageBlock
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Type { get; set; } = "text";
    public Dictionary<string, object?> Data { get; set; } = [];
}
