using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Services;

public class SettingsService(AppDbContext db)
{
    private Dictionary<string, string>? _cache;

    public async Task<string> GetAsync(string key, string defaultValue = "")
    {
        _cache ??= await db.SiteSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);
        return _cache.TryGetValue(key, out var v) ? v : defaultValue;
    }

    public void InvalidateCache() => _cache = null;

    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
    {
        var v = await GetAsync(key);
        return bool.TryParse(v, out var b) ? b : defaultValue;
    }

    public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0)
    {
        var v = await GetAsync(key);
        return decimal.TryParse(v, out var d) ? d : defaultValue;
    }

    public async Task<int> GetIntAsync(string key, int defaultValue = 0)
    {
        var v = await GetAsync(key);
        return int.TryParse(v, out var i) ? i : defaultValue;
    }

    public async Task<PublicSiteConfigDto> GetPublicConfigAsync() => new(
        await GetAsync("site.name.fa", "خیریه کوثر"),
        await GetAsync("site.name.en", "Kousar Charity"),
        await GetAsync("site.tagline.fa"),
        await GetAsync("site.tagline.en"),
        await GetAsync("site.hero.fa", "با هم می‌توانیم زندگی‌ها را روشن کنیم"),
        await GetAsync("site.hero.en", "Together we can light up lives"),
        // Logo / branding
        string.IsNullOrWhiteSpace(await GetAsync("site.logo.url")) ? null : await GetAsync("site.logo.url"),
        await GetIntAsync("site.logo.height", 48),
        await GetBoolAsync("site.logo.show.text", true),
        // Theme colors
        await GetAsync("site.color.primary", "#0d9488"),
        await GetAsync("site.color.accent", "#f59e0b"),
        await GetAsync("site.color.background", "#0f172a"),
        await GetAsync("site.footer.fa"),
        await GetAsync("site.footer.en"),
        // Home page layout
        await GetAsync("site.home.order", "hero,featured,campaigns,donors"),
        // Progress bar
        await GetAsync("site.progress.mode", "shift"),
        await GetAsync("site.progress.color.start", "#ef4444"),
        await GetAsync("site.progress.color.end", "#22c55e"),
        await GetBoolAsync("site.progress.show.percent", true),
        // Featured / countdown timer
        await GetAsync("featured.units", "days,hours,minutes,seconds"),
        await GetAsync("featured.layout", "boxes"),
        await GetBoolAsync("featured.badge.show", true),
        await GetAsync("featured.badge.fa", "⭐ ویژه"),
        await GetAsync("featured.badge.en", "⭐ Featured"),
        await GetAsync("featured.color", "#f59e0b"),
        await GetAsync("featured.expired.fa", "⏱ فرصت به پایان رسید"),
        await GetAsync("featured.expired.en", "⏱ Time ended"),
        // Payments
        await GetBoolAsync("crypto.enabled"),
        await GetBoolAsync("zarinpal.enabled", true),
        await GetDecimalAsync("donation.min.amount", 10000),
        ParseQuickAmounts(await GetAsync("donation.quick.amounts", "50000,100000,200000,500000,1000000")),
        // Donors / contributors display
        await GetBoolAsync("donors.show.recent", true),
        await GetIntAsync("donors.show.count", 10),
        await GetBoolAsync("donors.show.home", true),
        await GetBoolAsync("donors.show.name", true),
        await GetBoolAsync("donors.show.amount", true),
        await GetBoolAsync("donors.show.date", false),
        await GetBoolAsync("donors.show.campaign", false),
        await GetAsync("donors.anonymous.fa", "نیکوکار"),
        await GetAsync("donors.anonymous.en", "Well-wisher"),
        await GetAsync("donors.title.fa", "حامیان اخیر"),
        await GetAsync("donors.title.en", "Recent supporters"),
        await GetAsync("donors.source", "auto"),
        await GetAsync("donors.manual", "[]"),
        // Sharing
        await GetBoolAsync("share.ai.enabled", true),
        // Amount/progress text format
        await GetAsync("donation.progress.format.fa", "{collected} از {target} تومان"),
        await GetAsync("donation.progress.format.en", "{collected} of {target} Toman"),
        // OTP / misc
        await GetBoolAsync("donation.otp.enabled", false),
        await GetDecimalAsync("donation.otp.threshold", 5_000_000),
        await GetBoolAsync("payment.bypass.enabled", false));

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

        var items = await db.SiteSettings.OrderBy(s => s.Group).ThenBy(s => s.SortOrder).ToListAsync();
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
