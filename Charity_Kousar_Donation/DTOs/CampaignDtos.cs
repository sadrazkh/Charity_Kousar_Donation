namespace Charity_Kousar_Donation.DTOs;

public record CampaignListDto(
    Guid Id,
    string TitleFa,
    string TitleEn,
    string DescriptionFa,
    string DescriptionEn,
    decimal TargetAmount,
    decimal CollectedAmount,
    int ProgressPercent,
    string? ImageUrl,
    string Slug,
    string ShortCode,
    string ShortUrl,
    bool IsFeatured);

public record CampaignDetailDto(
    Guid Id,
    string TitleFa,
    string TitleEn,
    string DescriptionFa,
    string DescriptionEn,
    decimal TargetAmount,
    decimal CollectedAmount,
    int ProgressPercent,
    string? ImageUrl,
    string Slug,
    string ShortCode,
    string ShortUrl,
    bool IsActive,
    int DonorCount,
    List<PageBlockDto> PageBlocks);

public record CreateCampaignRequest(
    string TitleFa,
    string TitleEn,
    string DescriptionFa,
    string DescriptionEn,
    decimal TargetAmount,
    string? ImageUrl,
    string? Slug,
    bool IsActive,
    bool IsFeatured,
    int SortOrder);

public record UpdateCampaignRequest(
    string TitleFa,
    string TitleEn,
    string DescriptionFa,
    string DescriptionEn,
    decimal TargetAmount,
    string? ImageUrl,
    string? Slug,
    bool IsActive,
    bool IsFeatured,
    int SortOrder);
