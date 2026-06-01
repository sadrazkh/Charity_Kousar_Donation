using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController(CampaignService campaigns, ShareService share) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CampaignListDto>>> GetActive() =>
        Ok(await campaigns.GetActiveListAsync());

    [HttpGet("{slug}/share-pack")]
    public async Task<ActionResult<SharePackDto>> GetSharePack(string slug)
    {
        var pack = await share.GetSharePackAsync(slug);
        return pack == null ? NotFound() : Ok(pack);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<CampaignDetailDto>> GetBySlug(string slug)
    {
        var c = await campaigns.GetBySlugAsync(slug);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpGet("short/{code}")]
    public async Task<ActionResult<CampaignDetailDto>> GetByShortCode(string code)
    {
        var c = await campaigns.GetByShortCodeAsync(code);
        return c == null ? NotFound() : Ok(c);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<List<CampaignListDto>>> GetAllAdmin() =>
        Ok(await campaigns.GetAllForAdminAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CampaignDetailDto>> Create(CreateCampaignRequest req)
    {
        var c = await campaigns.CreateAsync(req);
        var detail = await campaigns.GetBySlugAsync(c.Slug);
        return CreatedAtAction(nameof(GetBySlug), new { slug = c.Slug }, detail);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCampaignRequest req)
    {
        var c = await campaigns.UpdateAsync(id, req);
        return c == null ? NotFound() : NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await campaigns.DeleteAsync(id) ? NoContent() : NotFound();

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<CampaignAdminDetailDto>> GetAdminDetail(Guid id)
    {
        var c = await campaigns.GetAdminDetailAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/page")]
    public async Task<IActionResult> UpdatePage(Guid id, UpdateCampaignPageRequest req) =>
        await campaigns.UpdatePageBlocksAsync(id, req) ? Ok() : NotFound();

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/regenerate-short-link")]
    public async Task<ActionResult<object>> RegenerateShortLink(Guid id)
    {
        try
        {
            var url = await campaigns.RegenerateShortCodeAsync(id);
            return Ok(new { shortUrl = url });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
