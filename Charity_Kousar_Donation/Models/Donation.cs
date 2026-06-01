namespace Charity_Kousar_Donation.Models;

public enum PaymentMethod
{
    ZarinPal = 0,
    Crypto = 1
}

public enum DonationStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Cancelled = 3
}

public class Donation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public string Phone { get; set; } = "";
    public string? DonorName { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DonationStatus Status { get; set; } = DonationStatus.Pending;
    public string? Authority { get; set; }
    public string? RefId { get; set; }
    public string? CryptoTxHash { get; set; }
    public bool SmsSent { get; set; }
    public bool IsRecurring { get; set; }
    public bool OtpVerified { get; set; }
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
}
