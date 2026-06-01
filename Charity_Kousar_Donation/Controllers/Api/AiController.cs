using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AiController(OpenRouterService ai) : ControllerBase
{
    [HttpPost("optimize")]
    public async Task<ActionResult<AiOptimizeResponse>> Optimize(AiOptimizeRequest req)
    {
        var (ok, optimized, alternative, tips, error) = await ai.OptimizeTextAsync(
            req.Text, req.Language, req.FieldType, req.CampaignTitle, req.Context);

        if (!ok)
            return BadRequest(new { message = error });

        return Ok(new AiOptimizeResponse(optimized!, alternative, tips));
    }
}
