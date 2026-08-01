using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Services;

public class SettingsService(AppDbContext db)
{
    private Dictionary<string, string>? _cache;

    /// <summary>
    /// Reads a setting. When the database has no row for the key, the built-in value from
    /// <see cref="SettingsCatalog"/> is used unless the caller passes its own fallback.
    /// </summary>
    public async Task<string> GetAsync(string key, string? defaultValue = null)
    {
        _cache ??= await db.SiteSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);
        return _cache.TryGetValue(key, out var v) ? v : defaultValue ?? SettingsCatalog.DefaultOf(key);
    }

    public void InvalidateCache() => _cache = null;

    public async Task<bool> GetBoolAsync(string key, bool? defaultValue = null)
    {
        var v = await GetAsync(key, defaultValue?.ToString());
        return bool.TryParse(v, out var b) ? b : defaultValue ?? false;
    }

    public async Task<decimal> GetDecimalAsync(string key, decimal? defaultValue = null)
    {
        var v = await GetAsync(key, defaultValue?.ToString());
        return decimal.TryParse(v, out var d) ? d : defaultValue ?? 0;
    }

    public async Task<int> GetIntAsync(string key, int? defaultValue = null)
    {
        var v = await GetAsync(key, defaultValue?.ToString());
        return int.TryParse(v, out var i) ? i : defaultValue ?? 0;
    }

    /// <summary>
    /// Everything the public site needs in one call. Values are named, not positional, so a
    /// new setting can be added anywhere without silently shifting its neighbours, and the
    /// fallbacks all come from <see cref="SettingsCatalog"/>.
    /// </summary>
    public async Task<PublicSiteConfigDto> GetPublicConfigAsync() => new(
        SiteNameFa: await GetAsync("site.name.fa"),
        SiteNameEn: await GetAsync("site.name.en"),
        TaglineFa: await GetAsync("site.tagline.fa"),
        TaglineEn: await GetAsync("site.tagline.en"),
        HeroTextFa: await GetAsync("site.hero.fa"),
        HeroTextEn: await GetAsync("site.hero.en"),
        LogoUrl: string.IsNullOrWhiteSpace(await GetAsync("site.logo.url")) ? null : await GetAsync("site.logo.url"),
        LogoHeight: await GetIntAsync("site.logo.height"),
        ShowLogoText: await GetBoolAsync("site.logo.show.text"),
        PrimaryColor: await GetAsync("site.color.primary"),
        AccentColor: await GetAsync("site.color.accent"),
        BackgroundColor: await GetAsync("site.color.background"),
        FooterTextFa: await GetAsync("site.footer.fa"),
        FooterTextEn: await GetAsync("site.footer.en"),
        HomeOrder: await GetAsync("site.home.order"),
        ProgressMode: await GetAsync("site.progress.mode"),
        ProgressColorStart: await GetAsync("site.progress.color.start"),
        ProgressColorEnd: await GetAsync("site.progress.color.end"),
        ShowProgressPercent: await GetBoolAsync("site.progress.show.percent"),
        FeaturedUnits: await GetAsync("featured.units"),
        FeaturedLayout: await GetAsync("featured.layout"),
        FeaturedBadgeShow: await GetBoolAsync("featured.badge.show"),
        FeaturedBadgeFa: await GetAsync("featured.badge.fa"),
        FeaturedBadgeEn: await GetAsync("featured.badge.en"),
        FeaturedColor: await GetAsync("featured.color"),
        FeaturedExpiredFa: await GetAsync("featured.expired.fa"),
        FeaturedExpiredEn: await GetAsync("featured.expired.en"),
        CryptoEnabled: await GetBoolAsync("crypto.enabled"),
        ZarinPalEnabled: await GetBoolAsync("zarinpal.enabled"),
        MinDonationAmount: await GetDecimalAsync("donation.min.amount"),
        QuickDonationAmounts: ParseQuickAmounts(await GetAsync("donation.quick.amounts")),
        ShowRecentDonors: await GetBoolAsync("donors.show.recent"),
        RecentDonorsCount: await GetIntAsync("donors.show.count"),
        ShowDonorsHome: await GetBoolAsync("donors.show.home"),
        ShowDonorName: await GetBoolAsync("donors.show.name"),
        ShowDonorAmount: await GetBoolAsync("donors.show.amount"),
        ShowDonorDate: await GetBoolAsync("donors.show.date"),
        ShowDonorCampaign: await GetBoolAsync("donors.show.campaign"),
        DonorAnonymousFa: await GetAsync("donors.anonymous.fa"),
        DonorAnonymousEn: await GetAsync("donors.anonymous.en"),
        DonorsTitleFa: await GetAsync("donors.title.fa"),
        DonorsTitleEn: await GetAsync("donors.title.en"),
        DonorsSource: await GetAsync("donors.source"),
        DonorsManual: await GetAsync("donors.manual"),
        ShareAiEnabled: await GetBoolAsync("share.ai.enabled"),
        ProgressFormatFa: await GetAsync("donation.progress.format.fa"),
        ProgressFormatEn: await GetAsync("donation.progress.format.en"),
        OtpEnabled: await GetBoolAsync("donation.otp.enabled"),
        OtpThresholdAmount: await GetDecimalAsync("donation.otp.threshold"),
        // Deliberately not the catalog default: a missing row must never switch the
        // "skip the real gateway" test mode on.
        PaymentBypassEnabled: await GetBoolAsync("payment.bypass.enabled", false),
        HomeColumns: await GetAsync("site.home.columns"),
        HomeMergeFeatured: await GetBoolAsync("site.home.merge.featured"),
        ProgressHighlight: await GetAsync("donation.progress.highlight"),
        ProgressAnimate: await GetBoolAsync("site.progress.animate"),
        ProgressAnimateMs: await GetIntAsync("site.progress.animate.ms"),
        ProgressTrackColor: await GetAsync("site.progress.track.color"),
        ProgressFlow: await GetBoolAsync("site.progress.flow"),
        ProgressFlowStyle: await GetAsync("site.progress.flow.style"),
        ProgressFlowMs: await GetIntAsync("site.progress.flow.ms"),
        AmountColorCollected: await GetAsync("donation.progress.color.collected"),
        AmountColorTarget: await GetAsync("donation.progress.color.target"),
        AmountColorRemaining: await GetAsync("donation.progress.color.remaining"),
        AmountColorPercent: await GetAsync("donation.progress.color.percent"),
        AmountTextColor: await GetAsync("donation.progress.color.text"),
        AmountFontScale: await GetIntAsync("donation.progress.size"),
        CardImageFit: await GetAsync("site.card.image.fit"),
        HeroBadgeFa: await GetAsync("site.hero.badge.fa"),
        HeroBadgeEn: await GetAsync("site.hero.badge.en"),
        FeaturedStyles: await GetAsync("featured.styles"),
        ShowCompletedTab: await GetBoolAsync("site.completed.show"),
        CompletedTitleFa: await GetAsync("site.completed.title.fa"),
        CompletedTitleEn: await GetAsync("site.completed.title.en"));

    /// <summary>Built-in highlight styles for featured campaigns (admins can edit the list).</summary>
    public const string DefaultFeaturedStyles = """
        [{"id":"gold","color":"#f59e0b","labelFa":"ویژه","labelEn":"Featured"},
         {"id":"urgent","color":"#ef4444","labelFa":"اضطراری","labelEn":"Urgent"},
         {"id":"limited","color":"#fb923c","labelFa":"فرصت محدود","labelEn":"Limited time"},
         {"id":"important","color":"#3b82f6","labelFa":"مهم","labelEn":"Important"},
         {"id":"almost","color":"#22c55e","labelFa":"نزدیک به تکمیل","labelEn":"Almost funded"},
         {"id":"spotlight","color":"#a855f7","labelFa":"ویژهٔ ماه","labelEn":"Spotlight"}]
        """;

    public async Task<string> GetTemplatesJsonAsync() => await GetAsync("page.templates", "[]");

    public async Task SaveTemplatesJsonAsync(string json) =>
        await SaveRawJsonAsync("page.templates", json, "قالب‌های سفارشی صفحه", "Custom page templates");

    /// <summary>Reusable image library shown in the media picker (uploaded by admins).</summary>
    public async Task<string> GetGalleryJsonAsync() => await GetAsync("media.gallery", "[]");

    public async Task SaveGalleryJsonAsync(string json) =>
        await SaveRawJsonAsync("media.gallery", json, "گالری تصاویر", "Image gallery");

    private async Task SaveRawJsonAsync(string key, string json, string labelFa, string labelEn)
    {
        var s = await db.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);
        if (s == null)
            db.SiteSettings.Add(new SiteSetting
            {
                Key = key, Value = json, Group = "advanced",
                LabelFa = labelFa, LabelEn = labelEn,
                Type = SettingType.TextArea, SortOrder = 1
            });
        else s.Value = json;
        await db.SaveChangesAsync();
        InvalidateCache();
    }

    private static List<long> ParseQuickAmounts(string raw) =>
        raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => long.TryParse(s.Replace("_", ""), out var n) ? n : 0)
            .Where(n => n > 0)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

    public async Task<List<SiteSettingsGroupDto>> GetGroupedForAdminAsync()
    {
        var groupOrder = new[] { "site", "home", "featured", "donors", "donation", "share", "payment", "crypto", "sms", "ai" };
        var groupLabels = new Dictionary<string, (string LabelFa, string LabelEn)>
        {
            ["site"] = ("🎨 ظاهر، لوگو و رنگ‌ها", "Appearance, logo & colors"),
            ["home"] = ("🏠 چیدمان صفحه اصلی", "Home page layout"),
            ["featured"] = ("⭐ بخش ویژه و تایمر", "Featured & countdown timer"),
            ["donors"] = ("👥 نمایش مشارکت‌کنندگان", "Contributors display"),
            ["donation"] = ("💰 کمک مالی و مبالغ", "Donations & amounts"),
            ["share"] = ("📤 اشتراک‌گذاری", "Sharing"),
            ["payment"] = ("💳 زرین‌پال", "ZarinPal"),
            ["crypto"] = ("₿ رمزارز", "Cryptocurrency"),
            ["sms"] = ("📱 پیامک", "SMS"),
            ["ai"] = ("🤖 هوش مصنوعی و ترجمه", "AI & translation"),
        };

        var items = await db.SiteSettings
            // Managed by dedicated screens (page builder / media library / home editor),
            // not the raw settings list.
            .Where(s => s.Key != "page.templates" && s.Key != "media.gallery" && s.Key != "featured.styles")
            .OrderBy(s => s.Group).ThenBy(s => s.SortOrder).ToListAsync();
        var grouped = items.GroupBy(s => s.Group).ToDictionary(g => g.Key, g => g.AsEnumerable());
        return groupOrder
            .Where(g => grouped.ContainsKey(g))
            .Concat(grouped.Keys.Except(groupOrder).OrderBy(x => x))
            .Select(g =>
            {
                if (!groupLabels.TryGetValue(g, out var labels))
                    labels = (g, g);
                return new SiteSettingsGroupDto(g, labels.LabelFa, labels.LabelEn,
                    grouped[g].Select(s => new SettingItemDto(s.Key, s.Value, s.LabelFa, s.LabelEn, s.Type.ToString(), s.SortOrder)).ToList());
            }).ToList();
    }

    public async Task UpdateAsync(Dictionary<string, string> settings)
    {
        var existing = await db.SiteSettings.Where(s => settings.Keys.Contains(s.Key)).ToListAsync();
        foreach (var s in existing)
        {
            if (settings.TryGetValue(s.Key, out var value))
                s.Value = value;
        }
        await db.SaveChangesAsync();
        InvalidateCache();
    }
}
