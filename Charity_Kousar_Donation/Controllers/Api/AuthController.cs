using System.Security.Claims;
using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity_Kousar_Donation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var result = await auth.LoginAsync(req);
        return result == null ? Unauthorized(new { message = "نام کاربری یا رمز عبور اشتباه است" }) : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ok = await auth.ChangePasswordAsync(userId, req);
        return ok ? Ok() : BadRequest(new { message = "رمز فعلی اشتباه است" });
    }
}
