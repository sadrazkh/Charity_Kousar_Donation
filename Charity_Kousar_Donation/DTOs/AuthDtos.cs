namespace Charity_Kousar_Donation.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, DateTime ExpiresAt);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record SiteSettingsGroupDto(string Group, string GroupLabelFa, string GroupLabelEn, List<SettingItemDto> Items);
public record SettingItemDto(string Key, string Value, string LabelFa, string LabelEn, string Type, int SortOrder);
public record UpdateSettingsRequest(Dictionary<string, string> Settings);

public record PublicSiteConfigDto(
    string SiteNameFa,
    string SiteNameEn,
    string TaglineFa,
    string TaglineEn,
    string HeroTextFa,
    string HeroTextEn,
    string? LogoUrl,
    string PrimaryColor,
    string AccentColor,
    string BackgroundColor,
    string? FooterTextFa,
    string? FooterTextEn,
    bool CryptoEnabled,
    bool ZarinPalEnabled,
    decimal MinDonationAmount,
    List<long> QuickDonationAmounts,
    bool ShowRecentDonors,
    int RecentDonorsCount,
    bool OtpEnabled,
    decimal OtpThresholdAmount,
    bool PaymentBypassEnabled);
