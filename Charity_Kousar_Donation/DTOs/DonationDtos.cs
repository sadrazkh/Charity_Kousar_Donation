using Charity_Kousar_Donation.Models;

namespace Charity_Kousar_Donation.DTOs;

public record StartDonationRequest(
    Guid CampaignId,
    string Phone,
    decimal Amount,
    string? DonorName,
    PaymentMethod PaymentMethod = PaymentMethod.ZarinPal);

public record StartDonationResponse(
    Guid DonationId,
    string? PaymentUrl,
    string? CryptoAddress,
    string? CryptoNetwork,
    string Message);

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
