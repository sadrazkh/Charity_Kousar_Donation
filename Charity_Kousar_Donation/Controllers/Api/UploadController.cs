using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UploadController(UploadService upload) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file)
    {
        // Fall back to the first file in the form if the field name differs.
        file ??= Request.HasFormContentType && Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null;
        var (ok, url, error) = await upload.SaveImageAsync(file, Request);
        return ok ? Ok(new { url }) : BadRequest(new { message = error });
    }
}
