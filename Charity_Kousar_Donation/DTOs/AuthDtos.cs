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
    // Logo / branding
    string? LogoUrl,
    int LogoHeight,
    bool ShowLogoText,
    // Theme colors
    string PrimaryColor,
    string AccentColor,
    string BackgroundColor,
    string? FooterTextFa,
    string? FooterTextEn,
    // Home page layout
    string HomeOrder,
    // Progress bar
    string ProgressMode,
    string ProgressColorStart,
    string ProgressColorEnd,
    bool ShowProgressPercent,
    // Featured / countdown timer
    string FeaturedUnits,
    string FeaturedLayout,
    bool FeaturedBadgeShow,
    string FeaturedBadgeFa,
    string FeaturedBadgeEn,
    string FeaturedColor,
    string FeaturedExpiredFa,
    string FeaturedExpiredEn,
    // Payments
    bool CryptoEnabled,
    bool ZarinPalEnabled,
    decimal MinDonationAmount,
    List<long> QuickDonationAmounts,
    // Donors / contributors display
    bool ShowRecentDonors,
    int RecentDonorsCount,
    bool ShowDonorsHome,
    bool ShowDonorName,
    bool ShowDonorAmount,
    bool ShowDonorDate,
    bool ShowDonorCampaign,
    string DonorAnonymousFa,
    string DonorAnonymousEn,
    string DonorsTitleFa,
    string DonorsTitleEn,
    string DonorsSource,
    string DonorsManual,
    // Sharing
    bool ShareAiEnabled,
    // Amount/progress text format
    string ProgressFormatFa,
    string ProgressFormatEn,
    // OTP / misc
    bool OtpEnabled,
    decimal OtpThresholdAmount,
    bool PaymentBypassEnabled);
