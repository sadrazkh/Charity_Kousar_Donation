using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Services;

public class OtpService(AppDbContext db, SmsService sms, SettingsService settings)
{
    public async Task<(bool Sent, string? Error)> SendOtpForDonationAsync(Donation donation)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        donation.OtpCode = code;
        donation.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
        donation.OtpVerified = false;
        await db.SaveChangesAsync();

        var msg = $"کد تأیید خیریه کوثر: {code}\nاعتبار: ۵ دقیقه";
        var sent = await sms.SendRawAsync(donation.Phone, msg);
        return sent ? (true, null) : (false, "ارسال پیامک ناموفق بود");
    }

    public async Task<bool> VerifyOtpAsync(Donation donation, string code)
    {
        if (donation.OtpVerified) return true;
        if (string.IsNullOrWhiteSpace(donation.OtpCode) || donation.OtpExpiresAt < DateTime.UtcNow)
            return false;
        if (donation.OtpCode != code.Trim()) return false;
        donation.OtpVerified = true;
        donation.OtpCode = null;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RequiresOtpAsync(decimal amount) =>
        await settings.GetBoolAsync("donation.otp.enabled", false) &&
        amount >= await settings.GetDecimalAsync("donation.otp.threshold", 5_000_000);
}
