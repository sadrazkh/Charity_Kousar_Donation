using System.Text;
using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Services;

public class ExportService(AppDbContext db)
{
    public async Task<byte[]> ExportDonationsCsvAsync()
    {
        var rows = await db.Donations
            .Include(d => d.Campaign)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new
            {
                d.Id,
                Campaign = d.Campaign.TitleFa,
                d.Phone,
                d.DonorName,
                d.Amount,
                Method = d.PaymentMethod.ToString(),
                Status = d.Status.ToString(),
                d.RefId,
                d.IsRecurring,
                d.CreatedAt,
                d.PaidAt
            })
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,Campaign,Phone,DonorName,Amount,Method,Status,RefId,Recurring,CreatedAt,PaidAt");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",",
                r.Id, Csv(r.Campaign), Csv(r.Phone), Csv(r.DonorName ?? ""),
                r.Amount.ToString("F0"), r.Method, r.Status, Csv(r.RefId ?? ""),
                r.IsRecurring, r.CreatedAt.ToString("O"), r.PaidAt?.ToString("O") ?? ""));
        }
        return Encoding.UTF8.GetBytes('\uFEFF' + sb.ToString());
    }

    public async Task<byte[]> ExportDonationsPdfHtmlAsync()
    {
        var rows = await db.Donations
            .Include(d => d.Campaign)
            .Where(d => d.Status == DonationStatus.Paid)
            .OrderByDescending(d => d.PaidAt)
            .Take(500)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html dir=\"rtl\" lang=\"fa\"><head><meta charset=\"utf-8\">")
            .Append("<title>گزارش کمک‌ها</title>")
            .Append("<style>body{font-family:Tahoma,sans-serif;padding:24px}table{width:100%;border-collapse:collapse}")
            .Append("th,td{border:1px solid #ccc;padding:8px;text-align:right}th{background:#0d9488;color:#fff}</style>")
            .Append("</head><body><h1>گزارش کمک‌های مالی — خیریه کوثر</h1>")
            .Append("<p>تاریخ: ").Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm"))
            .Append("</p><table><thead><tr>")
            .Append("<th>پروژه</th><th>مبلغ</th><th>موبایل</th><th>تاریخ</th><th>کد پیگیری</th>")
            .Append("</tr></thead><tbody>");

        foreach (var d in rows)
        {
            sb.Append("<tr><td>").Append(H(d.Campaign.TitleFa))
                .Append("</td><td>").Append(d.Amount.ToString("N0"))
                .Append("</td><td>").Append(H(d.Phone))
                .Append("</td><td>").Append(d.PaidAt?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "-")
                .Append("</td><td>").Append(H(d.RefId ?? "-"))
                .Append("</td></tr>");
        }
        sb.Append("</tbody></table></body></html>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Csv(string s) => $"\"{s.Replace("\"", "\"\"")}\"";
    private static string H(string s) => System.Net.WebUtility.HtmlEncode(s);
}
