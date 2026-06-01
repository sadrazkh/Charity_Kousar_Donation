using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Services;

public class ShareService(AppDbContext db, OpenRouterService ai, IHttpContextAccessor http)
{
    public async Task<SharePackDto?> GetSharePackAsync(string slug)
    {
        var c = await db.Campaigns.FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
        if (c == null) return null;

        var paid = await db.Donations
            .Where(d => d.CampaignId == c.Id && d.Status == DonationStatus.Paid)
            .ToListAsync();
        var collected = paid.Sum(d => d.Amount);
        var pct = c.TargetAmount > 0 ? (int)Math.Min(100, collected / c.TargetAmount * 100) : 0;

        var baseUrl = $"{http.HttpContext!.Request.Scheme}://{http.HttpContext.Request.Host}";
        var shortUrl = $"{baseUrl}/d/{c.ShortCode}";
        var pageUrl = $"{baseUrl}/c/{c.Slug}";
        var blocks = CampaignPageHelper.ParseBlocks(c.PageBlocksJson);
        var pageContentFa = CampaignPageHelper.ExtractPlainText(blocks, true);
        var pageContentEn = CampaignPageHelper.ExtractPlainText(blocks, false);

        var (ok, msgFa, msgEn, _) = await ai.GenerateShareMessagesAsync(
            c.TitleFa, c.TitleEn, c.DescriptionFa, c.DescriptionEn,
            pageContentFa, pageContentEn,
            c.TargetAmount, collected, pct, shortUrl, pageUrl);

        msgFa ??= shortUrl;
        msgEn ??= shortUrl;

        return new SharePackDto(
            msgFa, msgEn, shortUrl, pageUrl, c.ImageUrl,
            c.TitleFa, c.TitleEn, c.TargetAmount, collected, pct,
            BuildWhatsApp(msgFa), BuildWhatsApp(msgEn),
            BuildTelegram(shortUrl, msgFa), BuildTelegram(shortUrl, msgEn));
    }

    private static string BuildWhatsApp(string text) =>
        $"https://api.whatsapp.com/send?text={Uri.EscapeDataString(text)}";

    private static string BuildTelegram(string url, string text) =>
        $"https://t.me/share/url?url={Uri.EscapeDataString(url)}&text={Uri.EscapeDataString(text)}";
}
