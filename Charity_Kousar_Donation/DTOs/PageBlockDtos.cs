namespace Charity_Kousar_Donation.DTOs;

public record PageBlockDto(string Id, string Type, Dictionary<string, object?> Data);

public record UpdateCampaignPageRequest(List<PageBlockDto> Blocks);

public record CampaignAdminDetailDto(
    Guid Id,
    string TitleFa,
    string TitleEn,
    string DescriptionFa,
    string DescriptionEn,
    decimal TargetAmount,
    string? ImageUrl,
    string Slug,
    string ShortCode,
    string ShortUrl,
    string PageUrl,
    bool IsActive,
    bool IsFeatured,
    int SortOrder,
    List<PageBlockDto> PageBlocks);
