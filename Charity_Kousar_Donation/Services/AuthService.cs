using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Charity_Kousar_Donation.Services;

public class AuthService(AppDbContext db, IConfiguration config)
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest req)
    {
        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return null;

        var expiry = DateTime.UtcNow.AddHours(12);
        var token = GenerateToken(user.Id, user.Username, expiry);
        return new LoginResponse(token, expiry);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest req)
    {
        var user = await db.AdminUsers.FindAsync(userId);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            return false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await db.SaveChangesAsync();
        return true;
    }

    private string GenerateToken(Guid userId, string username, DateTime expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
