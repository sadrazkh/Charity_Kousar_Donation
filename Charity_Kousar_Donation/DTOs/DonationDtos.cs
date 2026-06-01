using Charity_Kousar_Donation.Models;

namespace Charity_Kousar_Donation.DTOs;

public record StartDonationRequest(
    Guid CampaignId,
    string Phone,
    decimal Amount,
    string? DonorName,
    PaymentMethod PaymentMethod = PaymentMethod.ZarinPal,
    bool IsRecurring = false,
    string? OtpCode = null);

public record StartDonationResponse(
    Guid DonationId,
    string? PaymentUrl,
    string? CryptoAddress,
    string? CryptoNetwork,
    string Message,
    bool RequiresOtp = false);

public record SendOtpRequest(Guid DonationId, string OtpCode);

public record RecentDonorDto(string MaskedPhone, string? DonorName, decimal Amount, DateTime PaidAt, string CampaignTitle);

public record DonationAdminDto(
    Guid Id,
    string CampaignTitle,
    string Phone,
    string? DonorName,
    decimal Amount,
    PaymentMethod PaymentMethod,
    DonationStatus Status,
    string? RefId,
    DateTime CreatedAt,
    DateTime? PaidAt,
    bool SmsSent);

public record DashboardStatsDto(
    decimal TotalCollected,
    int TotalDonors,
    int ActiveCampaigns,
    int PendingDonations,
    decimal TodayCollected);
