using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Services;

public class CampaignService(AppDbContext db, IHttpContextAccessor http)
{
    private string BaseUrl =>
        $"{http.HttpContext?.Request.Scheme}://{http.HttpContext?.Request.Host}";

    public async Task<List<CampaignListDto>> GetActiveListAsync()
    {
        var campaigns = await db.Campaigns
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();

        return await MapListAsync(campaigns);
    }

    public async Task<List<CampaignListDto>> GetAllForAdminAsync()
    {
        var campaigns = await db.Campaigns
            .OrderBy(c => c.SortOrder)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();
        return await MapListAsync(campaigns);
    }

    public async Task<CampaignDetailDto?> GetBySlugAsync(string slug)
    {
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        return campaign == null ? null : await MapDetailAsync(campaign);
    }

    public async Task<CampaignDetailDto?> GetByShortCodeAsync(string code)
    {
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.ShortCode == code && c.IsActive);
        return campaign == null ? null : await MapDetailAsync(campaign);
    }

    public async Task<Campaign?> GetEntityAsync(Guid id) =>
        await db.Campaigns.FindAsync(id);

    public async Task<Campaign> CreateAsync(CreateCampaignRequest req)
    {
        var slug = string.IsNullOrWhiteSpace(req.Slug)
            ? SlugHelper.ToSlug(req.TitleFa)
            : SlugHelper.ToSlug(req.Slug);

        slug = await EnsureUniqueSlugAsync(slug);
        var shortCode = await EnsureUniqueShortCodeAsync();

        var campaign = new Campaign
        {
            TitleFa = req.TitleFa,
            TitleEn = req.TitleEn,
            DescriptionFa = req.DescriptionFa,
            DescriptionEn = req.DescriptionEn,
            TargetAmount = req.TargetAmount,
            ImageUrl = req.ImageUrl,
            Slug = slug,
            ShortCode = shortCode,
            IsActive = req.IsActive,
            IsFeatured = req.IsFeatured,
            FeaturedBannerFa = req.FeaturedBannerFa,
            FeaturedBannerEn = req.FeaturedBannerEn,
            FeaturedTimerEndsAt = req.FeaturedTimerEndsAt,
            SortOrder = req.SortOrder
        };
        campaign.PageBlocksJson = CampaignPageHelper.SerializeBlocks(
            CampaignPageHelper.CreateDefaultBlocks(campaign));
        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign;
    }

    public async Task<Campaign?> UpdateAsync(Guid id, UpdateCampaignRequest req)
    {
        var campaign = await db.Campaigns.FindAsync(id);
        if (campaign == null) return null;

        var slug = string.IsNullOrWhiteSpace(req.Slug)
            ? SlugHelper.ToSlug(req.TitleFa)
            : SlugHelper.ToSlug(req.Slug);

        if (slug != campaign.Slug)
            slug = await EnsureUniqueSlugAsync(slug, id);

        campaign.TitleFa = req.TitleFa;
        campaign.TitleEn = req.TitleEn;
        campaign.DescriptionFa = req.DescriptionFa;
        campaign.DescriptionEn = req.DescriptionEn;
        campaign.TargetAmount = req.TargetAmount;
        campaign.ImageUrl = req.ImageUrl;
        campaign.Slug = slug;
        campaign.IsActive = req.IsActive;
        campaign.IsFeatured = req.IsFeatured;
        campaign.FeaturedBannerFa = req.FeaturedBannerFa;
        campaign.FeaturedBannerEn = req.FeaturedBannerEn;
        campaign.FeaturedTimerEndsAt = req.FeaturedTimerEndsAt;
        campaign.SortOrder = req.SortOrder;
        campaign.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return campaign;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var campaign = await db.Campaigns.FindAsync(id);
        if (campaign == null) return false;
        db.Campaigns.Remove(campaign);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<CampaignAdminDetailDto?> GetAdminDetailAsync(Guid id)
    {
        var c = await db.Campaigns.FindAsync(id);
        if (c == null) return null;

        if (string.IsNullOrWhiteSpace(c.PageBlocksJson) || c.PageBlocksJson == "[]")
        {
            c.PageBlocksJson = CampaignPageHelper.SerializeBlocks(
                CampaignPageHelper.CreateDefaultBlocks(c));
            await db.SaveChangesAsync();
        }

        return MapAdminDetail(c);
    }

    public async Task<bool> UpdatePageBlocksAsync(Guid id, UpdateCampaignPageRequest req)
    {
        var campaign = await db.Campaigns.FindAsync(id);
        if (campaign == null) return false;

        var blocks = req.Blocks.Select(b => new CampaignPageBlock
        {
            Id = string.IsNullOrWhiteSpace(b.Id) ? Guid.NewGuid().ToString("N")[..8] : b.Id,
            Type = b.Type,
            Data = b.Data
        }).ToList();

        campaign.PageBlocksJson = CampaignPageHelper.SerializeBlocks(blocks);
        campaign.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    private CampaignAdminDetailDto MapAdminDetail(Campaign c)
    {
        var blocks = CampaignPageHelper.ParseBlocks(c.PageBlocksJson)
            .Select(b => new PageBlockDto(b.Id, b.Type, b.Data)).ToList();
        return new CampaignAdminDetailDto(
            c.Id, c.TitleFa, c.TitleEn, c.DescriptionFa, c.DescriptionEn,
            c.TargetAmount, c.ImageUrl, c.Slug, c.ShortCode,
            $"{BaseUrl}/d/{c.ShortCode}", $"{BaseUrl}/c/{c.Slug}",
            c.IsActive, c.IsFeatured, c.FeaturedBannerFa, c.FeaturedBannerEn, c.FeaturedTimerEndsAt, c.SortOrder, blocks);
    }

    private static List<PageBlockDto> MapBlocks(Campaign c) =>
        CampaignPageHelper.ParseBlocks(c.PageBlocksJson)
            .Select(b => new PageBlockDto(b.Id, b.Type, b.Data)).ToList();

    public async Task<string> RegenerateShortCodeAsync(Guid id)
    {
        var campaign = await db.Campaigns.FindAsync(id)
            ?? throw new InvalidOperationException("Campaign not found");
        campaign.ShortCode = await EnsureUniqueShortCodeAsync();
        await db.SaveChangesAsync();
        return $"{BaseUrl}/d/{campaign.ShortCode}";
    }

    private async Task<List<CampaignListDto>> MapListAsync(List<Campaign> campaigns)
    {
        var ids = campaigns.Select(c => c.Id).ToList();
        var collected = await db.Donations
            .Where(d => ids.Contains(d.CampaignId) && d.Status == DonationStatus.Paid)
            .GroupBy(d => d.CampaignId)
            .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Sum);

        return campaigns.Select(c =>
        {
            var col = collected.GetValueOrDefault(c.Id, 0);
            var pct = c.TargetAmount > 0 ? (int)Math.Min(100, col / c.TargetAmount * 100) : 0;
            return new CampaignListDto(c.Id, c.TitleFa, c.TitleEn, c.DescriptionFa, c.DescriptionEn,
                c.TargetAmount, col, pct, c.ImageUrl, c.Slug, c.ShortCode,
                $"{BaseUrl}/d/{c.ShortCode}", c.IsFeatured, c.FeaturedBannerFa, c.FeaturedBannerEn, c.FeaturedTimerEndsAt);
        }).ToList();
    }

    private async Task<CampaignDetailDto> MapDetailAsync(Campaign c)
    {
        var paid = await db.Donations.Where(d => d.CampaignId == c.Id && d.Status == DonationStatus.Paid).ToListAsync();
        var col = paid.Sum(d => d.Amount);
        var pct = c.TargetAmount > 0 ? (int)Math.Min(100, col / c.TargetAmount * 100) : 0;
        var blocks = MapBlocks(c);
        if (blocks.Count == 0)
            blocks = CampaignPageHelper.CreateDefaultBlocks(c)
                .Select(b => new PageBlockDto(b.Id, b.Type, b.Data)).ToList();
        return new CampaignDetailDto(c.Id, c.TitleFa, c.TitleEn, c.DescriptionFa, c.DescriptionEn,
            c.TargetAmount, col, pct, c.ImageUrl, c.Slug, c.ShortCode,
            $"{BaseUrl}/d/{c.ShortCode}", c.IsActive, paid.Count, c.IsFeatured,
            c.FeaturedBannerFa, c.FeaturedBannerEn, c.FeaturedTimerEndsAt, blocks);
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, Guid? excludeId = null)
    {
        var baseSlug = slug;
        var i = 1;
        while (await db.Campaigns.AnyAsync(c => c.Slug == slug && (excludeId == null || c.Id != excludeId)))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }

    private async Task<string> EnsureUniqueShortCodeAsync()
    {
        string code;
        do { code = SlugHelper.GenerateShortCode(); }
        while (await db.Campaigns.AnyAsync(c => c.ShortCode == code));
        return code;
    }
}
