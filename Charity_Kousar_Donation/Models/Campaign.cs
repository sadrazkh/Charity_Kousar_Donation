namespace Charity_Kousar_Donation.Models;

public class Campaign
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TitleFa { get; set; } = "";
    public string TitleEn { get; set; } = "";
    public string DescriptionFa { get; set; } = "";
    public string DescriptionEn { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public string? ImageUrl { get; set; }
    public string Slug { get; set; } = "";
    public string ShortCode { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    /// <summary>JSON array of page builder blocks for the dedicated campaign page.</summary>
    public string PageBlocksJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Donation> Donations { get; set; } = [];
}
