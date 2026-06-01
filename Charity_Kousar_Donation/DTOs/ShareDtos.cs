namespace Charity_Kousar_Donation.DTOs;

public record SharePackDto(
    string MessageFa,
    string MessageEn,
    string ShortUrl,
    string PageUrl,
    string? ImageUrl,
    string CampaignTitleFa,
    string CampaignTitleEn,
    decimal TargetAmount,
    decimal CollectedAmount,
    int ProgressPercent,
    string WhatsAppUrlFa,
    string WhatsAppUrlEn,
    string TelegramUrlFa,
    string TelegramUrlEn);
