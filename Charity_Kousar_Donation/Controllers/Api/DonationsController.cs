using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class DonationsController(DonationService donations) : ControllerBase
{
    [HttpGet("stats/total")]
    public async Task<ActionResult<object>> GetTotal() =>
        Ok(new { totalCollected = await donations.GetTotalCollectedAsync() });

    [HttpPost("start")]
    public async Task<ActionResult<StartDonationResponse>> Start(StartDonationRequest req)
    {
        try
        {
            return Ok(await donations.StartDonationAsync(req));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("crypto/confirm")]
    public async Task<IActionResult> ConfirmCrypto([FromBody] CryptoConfirmRequest req)
    {
        var ok = await donations.ConfirmCryptoDonationAsync(req.DonationId, req.TxHash);
        return ok ? Ok(new { message = "ثبت شد" }) : BadRequest(new { message = "خطا" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<List<DonationAdminDto>>> GetAdmin(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        Ok(await donations.GetAllForAdminAsync(page, pageSize));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> Dashboard() =>
        Ok(await donations.GetDashboardStatsAsync());
}

public record CryptoConfirmRequest(Guid DonationId, string TxHash);
