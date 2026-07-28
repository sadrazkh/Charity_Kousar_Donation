using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

/// <summary>
/// Image library used by the media picker: the ready-made illustrations that ship with
/// the site plus the gallery an admin builds by uploading their own images.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MediaController(UploadService upload, SettingsService settings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> GetAll() => Ok(new
    {
        presets = upload.ListPresetUrls(),
        gallery = await settings.GetGalleryJsonAsync()
    });

    [HttpPut("gallery")]
    public async Task<IActionResult> SaveGallery(SaveGalleryRequest req)
    {
        await settings.SaveGalleryJsonAsync(string.IsNullOrWhiteSpace(req.Json) ? "[]" : req.Json);
        return Ok();
    }
}
