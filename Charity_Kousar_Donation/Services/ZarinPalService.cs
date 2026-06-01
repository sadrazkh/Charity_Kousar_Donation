using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Charity_Kousar_Donation.Services;

public class ZarinPalService(IHttpClientFactory httpFactory, SettingsService settings)
{
    public async Task<(bool Ok, string? Authority, string? PaymentUrl, string? Error)> RequestPaymentAsync(
        int amountToman, string description, string callbackUrl)
    {
        var merchant = await settings.GetAsync("zarinpal.merchant");
        if (string.IsNullOrWhiteSpace(merchant))
            return (false, null, null, "مرچنت زرین‌پال تنظیم نشده است");

        var sandbox = await settings.GetBoolAsync("zarinpal.sandbox", true);
        var baseUrl = sandbox ? "https://sandbox.zarinpal.com" : "https://api.zarinpal.com";
        var client = httpFactory.CreateClient();

        var body = new
        {
            merchant_id = merchant,
            amount = amountToman,
            description,
            callback_url = callbackUrl
        };

        var response = await client.PostAsJsonAsync($"{baseUrl}/pg/v4/payment/request.json", body);
        if (!response.IsSuccessStatusCode)
            return (false, null, null, "خطا در اتصال به زرین‌پال");

        var result = await response.Content.ReadFromJsonAsync<ZarinPalRequestResponse>();
        if (result?.Data?.Code != 100)
            return (false, null, null, result?.Errors?.Message ?? "درخواست پرداخت رد شد");

        var authority = result.Data.Authority!;
        var payUrl = sandbox
            ? $"https://sandbox.zarinpal.com/pg/StartPay/{authority}"
            : $"https://www.zarinpal.com/pg/StartPay/{authority}";
        return (true, authority, payUrl, null);
    }

    public async Task<(bool Ok, string? RefId, string? Error)> VerifyPaymentAsync(string authority, int amountToman)
    {
        var merchant = await settings.GetAsync("zarinpal.merchant");
        var sandbox = await settings.GetBoolAsync("zarinpal.sandbox", true);
        var baseUrl = sandbox ? "https://sandbox.zarinpal.com" : "https://api.zarinpal.com";
        var client = httpFactory.CreateClient();

        var body = new { merchant_id = merchant, amount = amountToman, authority };
        var response = await client.PostAsJsonAsync($"{baseUrl}/pg/v4/payment/verify.json", body);
        var result = await response.Content.ReadFromJsonAsync<ZarinPalVerifyResponse>();

        if (result?.Data?.Code is 100 or 101)
            return (true, result.Data.RefId?.ToString(), null);
        return (false, null, result?.Errors?.Message ?? "تأیید پرداخت ناموفق");
    }

    public async Task<string?> GetPaymentUrlAsync(string authority)
    {
        if (string.IsNullOrWhiteSpace(authority)) return null;
        var sandbox = await settings.GetBoolAsync("zarinpal.sandbox", true);
        return sandbox
            ? $"https://sandbox.zarinpal.com/pg/StartPay/{authority}"
            : $"https://www.zarinpal.com/pg/StartPay/{authority}";
    }

    private class ZarinPalRequestResponse
    {
        public ZarinPalData? Data { get; set; }
        public ZarinPalError? Errors { get; set; }
    }

    private class ZarinPalVerifyResponse
    {
        public ZarinPalVerifyData? Data { get; set; }
        public ZarinPalError? Errors { get; set; }
    }

    private class ZarinPalData
    {
        public int Code { get; set; }
        public string? Authority { get; set; }
    }

    private class ZarinPalVerifyData
    {
        public int Code { get; set; }
        [JsonPropertyName("ref_id")]
        public long? RefId { get; set; }
    }

    private class ZarinPalError
    {
        public string? Message { get; set; }
    }
}
