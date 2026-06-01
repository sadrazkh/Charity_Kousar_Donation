using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers;

public class ShortLinkController(CampaignService campaigns) : Controller
{
    [HttpGet("/d/{code}")]
    public async Task<IActionResult> RedirectShort(string code)
    {
        var campaign = await campaigns.GetByShortCodeAsync(code);
        if (campaign == null) return NotFound();
        return Redirect($"/c/{campaign.Slug}");
    }
}
