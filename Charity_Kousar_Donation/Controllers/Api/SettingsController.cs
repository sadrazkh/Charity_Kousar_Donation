using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class SettingsController(SettingsService settings) : ControllerBase
{
    [HttpGet("public")]
    public async Task<ActionResult<PublicSiteConfigDto>> GetPublic() =>
        Ok(await settings.GetPublicConfigAsync());

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<SiteSettingsGroupDto>>> GetAll() =>
        Ok(await settings.GetGroupedForAdminAsync());

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update(UpdateSettingsRequest req)
    {
        await settings.UpdateAsync(req.Settings);
        return Ok();
    }

    // Reusable custom page templates (saved by admins from the page builder).
    [Authorize(Roles = "Admin")]
    [HttpGet("templates")]
    public async Task<ActionResult<object>> GetTemplates() =>
        Ok(new { json = await settings.GetTemplatesJsonAsync() });

    [Authorize(Roles = "Admin")]
    [HttpPut("templates")]
    public async Task<IActionResult> SaveTemplates(SaveTemplatesRequest req)
    {
        await settings.SaveTemplatesJsonAsync(string.IsNullOrWhiteSpace(req.Json) ? "[]" : req.Json);
        return Ok();
    }
}
