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

    public async Task<PublicSiteConfigDto> GetPublicConfigAsync() => new(
        await GetAsync("site.name.fa", "خیریه کوثر"),
        await GetAsync("site.name.en", "Kousar Charity"),
        await GetAsync("site.tagline.fa"),
        await GetAsync("site.tagline.en"),
        await GetAsync("site.hero.fa", "با هم می‌توانیم زندگی‌ها را روشن کنیم"),
        await GetAsync("site.hero.en", "Together we can light up lives"),
        string.IsNullOrWhiteSpace(await GetAsync("site.logo.url")) ? null : await GetAsync("site.logo.url"),
        await GetAsync("site.color.primary", "#0d9488"),
        await GetAsync("site.color.accent", "#f59e0b"),
        await GetAsync("site.color.background", "#0f172a"),
        await GetAsync("site.footer.fa"),
        await GetAsync("site.footer.en"),
        await GetBoolAsync("crypto.enabled"),
        await GetBoolAsync("zarinpal.enabled", true),
        await GetDecimalAsync("donation.min.amount", 10000),
        ParseQuickAmounts(await GetAsync("donation.quick.amounts", "50000,100000,200000,500000,1000000")),
        await GetBoolAsync("donors.show.recent", true),
        int.TryParse(await GetAsync("donors.show.count", "10"), out var dc) ? dc : 10,
        await GetBoolAsync("donation.otp.enabled", false),
        await GetDecimalAsync("donation.otp.threshold", 5_000_000));

    private static List<long> ParseQuickAmounts(string raw) =>
        raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => long.TryParse(s.Replace("_", ""), out var n) ? n : 0)
            .Where(n => n > 0)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

    public async Task<List<SiteSettingsGroupDto>> GetGroupedForAdminAsync()
    {
        var groupOrder = new[] { "site", "donation", "share", "payment", "crypto", "sms", "donors", "ai" };
        var groupLabels = new Dictionary<string, (string LabelFa, string LabelEn)>
        {
            ["site"] = ("🎨 ظاهر و متن سایت", "Site appearance"),
            ["donation"] = ("💰 کمک مالی و مبالغ", "Donations & amounts"),
            ["share"] = ("📤 اشتراک شبکه‌های اجتماعی", "Social sharing (AI)"),
            ["payment"] = ("💳 زرین‌پال", "ZarinPal"),
            ["crypto"] = ("₿ رمزارز", "Cryptocurrency"),
            ["sms"] = ("📱 پیامک", "SMS"),
            ["donors"] = ("👥 نمایش حامیان", "Donors display"),
            ["ai"] = ("🤖 هوش مصنوعی OpenRouter", "AI (OpenRouter)"),
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
