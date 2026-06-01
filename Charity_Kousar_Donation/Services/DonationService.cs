using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.DTOs;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Services;

public class DonationService(
    AppDbContext db,
    SettingsService settings,
    ZarinPalService zarinPal,
    SmsService sms,
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

    public async Task<StartDonationResponse> StartDonationAsync(StartDonationRequest req)
    {
        var campaign = await db.Campaigns.FindAsync(req.CampaignId)
            ?? throw new InvalidOperationException("پروژه یافت نشد");

        if (!campaign.IsActive)
            throw new InvalidOperationException("این پروژه فعال نیست");

        var minAmount = await settings.GetDecimalAsync("donation.min.amount", 10000);
        if (req.Amount < minAmount)
            throw new InvalidOperationException($"حداقل مبلغ کمک {minAmount:N0} تومان است");

        var phone = NormalizePhone(req.Phone);
        if (phone.Length < 10)
            throw new InvalidOperationException("شماره موبایل معتبر نیست");

        var donation = new Donation
        {
            CampaignId = req.CampaignId,
            Phone = phone,
            DonorName = req.DonorName,
            Amount = req.Amount,
            PaymentMethod = req.PaymentMethod,
            Status = DonationStatus.Pending
        };
        db.Donations.Add(donation);
        await db.SaveChangesAsync();

        if (req.PaymentMethod == PaymentMethod.Crypto)
        {
            if (!await settings.GetBoolAsync("crypto.enabled"))
                throw new InvalidOperationException("پرداخت رمزارز غیرفعال است");

            return new StartDonationResponse(
                donation.Id,
                null,
                await settings.GetAsync("crypto.wallet.address"),
                await settings.GetAsync("crypto.network", "TRC20"),
                "لطفاً مبلغ را به آدرس کیف پول واریز کنید و کد تراکنش را ثبت نمایید.");
        }

        if (!await settings.GetBoolAsync("zarinpal.enabled", true))
            throw new InvalidOperationException("درگاه زرین‌پال غیرفعال است");

        var baseUrl = $"{http.HttpContext!.Request.Scheme}://{http.HttpContext.Request.Host}";
        var callback = $"{baseUrl}/api/payment/callback?donationId={donation.Id}";
        var amountToman = (int)req.Amount;
        var desc = $"کمک به {campaign.TitleFa}";

        var (ok, authority, payUrl, error) = await zarinPal.RequestPaymentAsync(amountToman, desc, callback);
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

        donation.Status = DonationStatus.Paid;
        donation.RefId = refId;
        donation.PaidAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        if (!donation.SmsSent)
        {
            var sent = await sms.SendDonationThankYouAsync(
                donation.Phone, donation.DonorName, donation.Amount,
                donation.Campaign.TitleFa, refId ?? donation.Id.ToString()[..8]);
            donation.SmsSent = sent;
            await db.SaveChangesAsync();
        }

        return (true, "پرداخت با موفقیت انجام شد");
    }

    public async Task<bool> ConfirmCryptoDonationAsync(Guid donationId, string txHash)
    {
        var donation = await db.Donations.Include(d => d.Campaign).FirstOrDefaultAsync(d => d.Id == donationId);
        if (donation == null || donation.PaymentMethod != PaymentMethod.Crypto) return false;
        if (donation.Status == DonationStatus.Paid) return true;

        donation.CryptoTxHash = txHash;
        donation.Status = DonationStatus.Paid;
        donation.PaidAt = DateTime.UtcNow;
        donation.RefId = txHash[..Math.Min(16, txHash.Length)];
        await db.SaveChangesAsync();

        if (!donation.SmsSent)
        {
            donation.SmsSent = await sms.SendDonationThankYouAsync(
                donation.Phone, donation.DonorName, donation.Amount,
                donation.Campaign.TitleFa, donation.RefId);
            await db.SaveChangesAsync();
        }
        return true;
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
}
