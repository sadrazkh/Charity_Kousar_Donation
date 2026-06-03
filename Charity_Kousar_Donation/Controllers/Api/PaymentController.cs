using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(DonationService donations) : ControllerBase
{
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] Guid donationId, [FromQuery] string Status, [FromQuery] string Authority)
    {
        var (success, message) = await donations.HandleZarinPalCallbackAsync(donationId, Status, Authority);
        return RedirectToResult(success, message);
    }

    [HttpGet("test/callback")]
    public async Task<IActionResult> TestCallback([FromQuery] Guid donationId, [FromQuery] string Status)
    {
        var (success, message) = await donations.HandleTestPaymentCallbackAsync(donationId, Status);
        return RedirectToResult(success, message);
    }

    private IActionResult RedirectToResult(bool success, string message)
    {
        var redirect = success ? "/payment/success" : "/payment/failed";
        return Redirect($"{redirect}?message={Uri.EscapeDataString(message)}");
    }
}
