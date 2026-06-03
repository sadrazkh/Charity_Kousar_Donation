using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Hubs;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Services;

public class DonationService(
    AppDbContext db,
    SettingsService settings,
    ZarinPalService zarinPal,
    SmsService sms,
    OtpService otp,
    IDonationNotifier notifier,
    IHttpContextAccessor http)
{
    public async Task<decimal> GetTotalCollectedAsync() =>
        await db.Donations.Where(d => d.Status == DonationStatus.Paid).SumAsync(d => d.Amount);

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var paid = db.Donations.Where(d => d.Status == DonationStatus.Paid);
        return new DashboardStatsDto(
            await paid.SumAsync(d => d.Amount),
            await paid.Select(d => d.Phone).Distinct().CountAsync(),
            await db.Campaigns.CountAsync(c => c.IsActive),
            await db.Donations.CountAsync(d => d.Status == DonationStatus.Pending),
            await paid.Where(d => d.PaidAt >= today).SumAsync(d => d.Amount));
    }

    public async Task<List<RecentDonorDto>> GetRecentDonorsAsync(Guid? campaignId = null, int? count = null)
    {
        if (!await settings.GetBoolAsync("donors.show.recent", true))
            return [];

        var take = count ?? (int.TryParse(await settings.GetAsync("donors.show.count", "10"), out var c) ? c : 10);
        var q = db.Donations
            .Include(d => d.Campaign)
            .Where(d => d.Status == DonationStatus.Paid && d.PaidAt != null);

        if (campaignId.HasValue)
            q = q.Where(d => d.CampaignId == campaignId.Value);

        return await q
            .OrderByDescending(d => d.PaidAt)
            .Take(take)
            .Select(d => new RecentDonorDto(
                MaskPhone(d.Phone),
                d.DonorName,
                d.Amount,
                d.PaidAt!.Value,
                d.Campaign.TitleFa))
            .ToListAsync();
    }

    public async Task<StartDonationResponse> StartDonationAsync(StartDonationRequest req)
    {
        var campaign = await db.Campaigns.FindAsync(req.CampaignId)
            ?? throw new InvalidOperationException("پروژه یافت نشد");

        if (!campaign.IsActive)
            throw new InvalidOperationException("این پروژه فعال نیست");

        var minAmount = await settings.GetDecimalAsync("donation.min.amount", 10000);
        if (req.Amount < minAmount)
            throw new InvalidOperationException($"حداقل مبلغ کمک {minAmount:N0} تومان است");

        var phone = string.IsNullOrWhiteSpace(req.Phone) ? "" : NormalizePhone(req.Phone);
        if (!string.IsNullOrEmpty(phone) && phone.Length < 10)
            throw new InvalidOperationException("شماره موبایل معتبر نیست");

        if (await otp.RequiresOtpAsync(req.Amount) && string.IsNullOrEmpty(phone))
            throw new InvalidOperationException("برای این مبلغ ثبت شماره موبایل الزامی است");

        var needsOtp = !string.IsNullOrEmpty(phone) && await otp.RequiresOtpAsync(req.Amount);
        if (needsOtp && string.IsNullOrWhiteSpace(req.OtpCode))
        {
            var pending = new Donation
            {
                CampaignId = req.CampaignId,
                Phone = phone,
                DonorName = req.DonorName,
                Amount = req.Amount,
                PaymentMethod = req.PaymentMethod,
                IsRecurring = req.IsRecurring,
                Status = DonationStatus.Pending
            };
            db.Donations.Add(pending);
            await db.SaveChangesAsync();

            var (sent, err) = await otp.SendOtpForDonationAsync(pending);
            if (!sent)
                throw new InvalidOperationException(err ?? "ارسال کد تأیید ناموفق بود");

            return new StartDonationResponse(pending.Id, null, null, null,
                "کد تأیید به موبایل شما ارسال شد.", true);
        }

        Donation donation;
        if (needsOtp && !string.IsNullOrWhiteSpace(req.OtpCode))
        {
            donation = await db.Donations
                .Where(d => d.CampaignId == req.CampaignId && d.Phone == phone && d.Status == DonationStatus.Pending)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("ابتدا درخواست کمک را ثبت کنید");

            if (!await otp.VerifyOtpAsync(donation, req.OtpCode))
                throw new InvalidOperationException("کد تأیید نامعتبر یا منقضی شده است");
        }
        else
        {
            donation = new Donation
            {
                CampaignId = req.CampaignId,
                Phone = phone,
                DonorName = req.DonorName,
                Amount = req.Amount,
                PaymentMethod = req.PaymentMethod,
                IsRecurring = req.IsRecurring,
                Status = DonationStatus.Pending
            };
            db.Donations.Add(donation);
            await db.SaveChangesAsync();
        }

        return await InitiatePaymentAsync(donation, campaign);
    }

    public async Task<StartDonationResponse> VerifyOtpAndContinueAsync(Guid donationId, string otpCode)
    {
        var donation = await db.Donations.Include(d => d.Campaign).FirstOrDefaultAsync(d => d.Id == donationId)
            ?? throw new InvalidOperationException("تراکنش یافت نشد");

        if (donation.Status != DonationStatus.Pending)
            throw new InvalidOperationException("این تراکنش قابل ادامه نیست");

        if (!await otp.VerifyOtpAsync(donation, otpCode))
            throw new InvalidOperationException("کد تأیید نامعتبر یا منقضی شده است");

        return await InitiatePaymentAsync(donation, donation.Campaign);
    }

    public async Task<(bool Sent, string Message)> ResendOtpAsync(Guid donationId)
    {
        var donation = await db.Donations.FindAsync(donationId)
            ?? throw new InvalidOperationException("تراکنش یافت نشد");

        if (donation.Status != DonationStatus.Pending)
            throw new InvalidOperationException("این تراکنش قابل تأیید نیست");

        var (sent, err) = await otp.SendOtpForDonationAsync(donation);
        return sent ? (true, "کد مجدداً ارسال شد") : (false, err ?? "ارسال ناموفق");
    }

    private async Task<StartDonationResponse> InitiatePaymentAsync(Donation donation, Campaign campaign)
    {
        if (donation.PaymentMethod == PaymentMethod.Crypto)
        {
            if (!await settings.GetBoolAsync("crypto.enabled"))
                throw new InvalidOperationException("پرداخت رمزارز غیرفعال است");

            return new StartDonationResponse(
                donation.Id, null,
                await settings.GetAsync("crypto.wallet.address"),
                await settings.GetAsync("crypto.network", "TRC20"),
                "لطفاً مبلغ را به آدرس کیف پول واریز کنید و کد تراکنش را ثبت نمایید.");
        }

        if (!await settings.GetBoolAsync("zarinpal.enabled", true))
            throw new InvalidOperationException("درگاه زرین‌پال غیرفعال است");

        var baseUrl = $"{http.HttpContext!.Request.Scheme}://{http.HttpContext.Request.Host}";

        if (await settings.GetBoolAsync("payment.bypass.enabled", false))
        {
            donation.Authority = $"BYPASS-{donation.Id:N}"[..24];
            await db.SaveChangesAsync();
            return new StartDonationResponse(
                donation.Id,
                $"{baseUrl}/payment/test?donationId={donation.Id}",
                null, null,
                "در حال انتقال به درگاه آزمایشی...");
        }

        if (!string.IsNullOrEmpty(donation.Authority))
        {
            var existingUrl = await zarinPal.GetPaymentUrlAsync(donation.Authority);
            if (!string.IsNullOrEmpty(existingUrl))
                return new StartDonationResponse(donation.Id, existingUrl, null, null, "در حال انتقال به درگاه پرداخت...");
        }

        var callback = $"{baseUrl}/api/payment/callback?donationId={donation.Id}";
        var desc = $"کمک به {campaign.TitleFa}";

        var (ok, authority, payUrl, error) = await zarinPal.RequestPaymentAsync((int)donation.Amount, desc, callback);
        if (!ok)
        {
            donation.Status = DonationStatus.Failed;
            await db.SaveChangesAsync();
            throw new InvalidOperationException(error ?? "خطا در ایجاد پرداخت");
        }

        donation.Authority = authority;
        await db.SaveChangesAsync();

        return new StartDonationResponse(donation.Id, payUrl, null, null, "در حال انتقال به درگاه پرداخت...");
    }

    public async Task<(bool Success, string Message)> HandleZarinPalCallbackAsync(Guid donationId, string status, string authority)
    {
        var donation = await db.Donations.Include(d => d.Campaign).FirstOrDefaultAsync(d => d.Id == donationId);
        if (donation == null) return (false, "تراکنش یافت نشد");

        if (donation.Status == DonationStatus.Paid)
            return (true, "پرداخت قبلاً تأیید شده است");

        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            donation.Status = DonationStatus.Failed;
            await db.SaveChangesAsync();
            return (false, "پرداخت لغو شد");
        }

        var (ok, refId, error) = await zarinPal.VerifyPaymentAsync(authority, (int)donation.Amount);
        if (!ok)
        {
            donation.Status = DonationStatus.Failed;
            await db.SaveChangesAsync();
            return (false, error ?? "تأیید پرداخت ناموفق");
        }

        await MarkPaidAsync(donation, refId);
        return (true, "پرداخت با موفقیت انجام شد");
    }

    public async Task<(bool Success, string Message)> HandleTestPaymentCallbackAsync(Guid donationId, string status)
    {
        if (!await settings.GetBoolAsync("payment.bypass.enabled", false))
            return (false, "حالت تست پرداخت غیرفعال است");

        var donation = await db.Donations.Include(d => d.Campaign).FirstOrDefaultAsync(d => d.Id == donationId);
        if (donation == null) return (false, "تراکنش یافت نشد");

        if (donation.Status == DonationStatus.Paid)
            return (true, "پرداخت قبلاً تأیید شده است");

        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            donation.Status = DonationStatus.Failed;
            await db.SaveChangesAsync();
            return (false, "پرداخت لغو شد (تست)");
        }

        await MarkPaidAsync(donation, $"TEST-{donation.Id:N}"[..12]);
        return (true, "پرداخت آزمایشی با موفقیت انجام شد");
    }

    public async Task<bool> ConfirmCryptoDonationAsync(Guid donationId, string txHash)
    {
        var donation = await db.Donations.Include(d => d.Campaign).FirstOrDefaultAsync(d => d.Id == donationId);
        if (donation == null || donation.PaymentMethod != PaymentMethod.Crypto) return false;
        if (donation.Status == DonationStatus.Paid) return true;

        donation.CryptoTxHash = txHash;
        await MarkPaidAsync(donation, txHash[..Math.Min(16, txHash.Length)]);
        return true;
    }

    private async Task MarkPaidAsync(Donation donation, string? refId)
    {
        donation.Status = DonationStatus.Paid;
        donation.RefId = refId;
        donation.PaidAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        if (!donation.SmsSent && !string.IsNullOrWhiteSpace(donation.Phone))
        {
            donation.SmsSent = await sms.SendDonationThankYouAsync(
                donation.Phone, donation.DonorName, donation.Amount,
                donation.Campaign.TitleFa, refId ?? donation.Id.ToString()[..8]);
            await db.SaveChangesAsync();
        }

        await notifier.NotifyDonationPaidAsync(donation.Campaign.TitleFa, donation.Amount, donation.Phone);
    }

    public async Task<List<DonationAdminDto>> GetAllForAdminAsync(int page = 1, int pageSize = 50)
    {
        return await db.Donations
            .Include(d => d.Campaign)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DonationAdminDto(
                d.Id, d.Campaign.TitleFa, d.Phone, d.DonorName, d.Amount,
                d.PaymentMethod, d.Status, d.RefId, d.CreatedAt, d.PaidAt, d.SmsSent))
            .ToListAsync();
    }

    private static string NormalizePhone(string phone)
    {
        var p = phone.Trim().Replace(" ", "").Replace("-", "");
        if (p.StartsWith("+98")) p = "0" + p[3..];
        if (p.StartsWith("98") && p.Length == 12) p = "0" + p[2..];
        return p;
    }

    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "ناشناس";
        if (phone.Length < 8) return "***";
        return phone[..4] + "***" + phone[^4..];
    }
}
