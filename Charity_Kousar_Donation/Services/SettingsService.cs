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
        await GetBoolAsync("zarinpal.enabled", true));

    public async Task<List<SiteSettingsGroupDto>> GetGroupedForAdminAsync()
    {
        var groupLabels = new Dictionary<string, (string LabelFa, string LabelEn)>
        {
            ["site"] = ("تنظیمات سایت", "Site settings"),
            ["payment"] = ("زرین‌پال", "ZarinPal"),
            ["crypto"] = ("رمزارز", "Cryptocurrency"),
            ["sms"] = ("پیامک", "SMS"),
            ["donation"] = ("کمک مالی", "Donations"),
            ["ai"] = ("هوش مصنوعی (OpenRouter)", "AI (OpenRouter)"),
        };

        var items = await db.SiteSettings.OrderBy(s => s.Group).ThenBy(s => s.SortOrder).ToListAsync();
        return items.GroupBy(s => s.Group).Select(g =>
        {
            if (!groupLabels.TryGetValue(g.Key, out var labels))
                labels = (g.Key, g.Key);
            return new SiteSettingsGroupDto(g.Key, labels.LabelFa, labels.LabelEn,
                g.Select(s => new SettingItemDto(s.Key, s.Value, s.LabelFa, s.LabelEn, s.Type.ToString(), s.SortOrder)).ToList());
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
