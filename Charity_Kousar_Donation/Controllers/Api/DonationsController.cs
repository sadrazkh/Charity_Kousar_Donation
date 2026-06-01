using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class DonationsController(
    DonationService donations,
    ExportService export) : ControllerBase
{
    [HttpGet("stats/total")]
    public async Task<ActionResult<object>> GetTotal() =>
        Ok(new { totalCollected = await donations.GetTotalCollectedAsync() });

    [HttpGet("recent")]
    public async Task<ActionResult<List<RecentDonorDto>>> GetRecent(
        [FromQuery] Guid? campaignId = null,
        [FromQuery] int? count = null) =>
        Ok(await donations.GetRecentDonorsAsync(campaignId, count));

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

    [HttpPost("verify-otp")]
    public async Task<ActionResult<StartDonationResponse>> VerifyOtp([FromBody] SendOtpRequest req)
    {
        try
        {
            return Ok(await donations.VerifyOtpAndContinueAsync(req.DonationId, req.OtpCode));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest req)
    {
        try
        {
            var (sent, msg) = await donations.ResendOtpAsync(req.DonationId);
            return sent ? Ok(new { message = msg }) : BadRequest(new { message = msg });
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

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/export/csv")]
    public async Task<IActionResult> ExportCsv()
    {
        var bytes = await export.ExportDonationsCsvAsync();
        return File(bytes, "text/csv; charset=utf-8", $"donations-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/export/report")]
    public async Task<IActionResult> ExportReport()
    {
        var bytes = await export.ExportDonationsPdfHtmlAsync();
        return File(bytes, "text/html; charset=utf-8", $"donations-report-{DateTime.UtcNow:yyyyMMdd}.html");
    }
}

public record CryptoConfirmRequest(Guid DonationId, string TxHash);
public record ResendOtpRequest(Guid DonationId);
