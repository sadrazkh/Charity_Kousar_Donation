using System.Net.Http.Json;

namespace Charity_Kousar_Donation.Services;

public class SmsService(IHttpClientFactory httpFactory, SettingsService settings, ILogger<SmsService> logger)
{
    public async Task<bool> SendDonationThankYouAsync(string phone, string? donorName, decimal amountToman, string campaignTitle, string refId)
    {
        if (!await settings.GetBoolAsync("sms.enabled", true))
            return false;

        var apiKey = await settings.GetAsync("sms.api.key");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("SMS API key not configured");
            return false;
        }

        var template = await settings.GetAsync("sms.template");
        var message = template
            .Replace("{name}", string.IsNullOrWhiteSpace(donorName) ? "بانی" : donorName)
            .Replace("{amount}", amountToman.ToString("N0"))
            .Replace("{campaign}", campaignTitle)
            .Replace("{ref}", refId);

        var provider = await settings.GetAsync("sms.provider", "kavenegar");
        return provider.ToLowerInvariant() switch
        {
            "kavenegar" => await SendKavenegarAsync(apiKey, phone, message),
            _ => await SendKavenegarAsync(apiKey, phone, message)
        };
    }

    private async Task<bool> SendKavenegarAsync(string apiKey, string phone, string message)
    {
        try
        {
            var sender = await settings.GetAsync("sms.sender");
            var client = httpFactory.CreateClient();
            var normalizedPhone = NormalizePhone(phone);
            var url = $"https://api.kavenegar.com/v1/{apiKey}/sms/send.json?receptor={normalizedPhone}&message={Uri.EscapeDataString(message)}";
            if (!string.IsNullOrWhiteSpace(sender))
                url += $"&sender={Uri.EscapeDataString(sender)}";

            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send SMS");
            return false;
        }
    }

    private static string NormalizePhone(string phone)
    {
        var p = phone.Trim().Replace(" ", "").Replace("-", "");
        if (p.StartsWith('0')) p = "98" + p[1..];
        if (!p.StartsWith("98")) p = "98" + p;
        return p;
    }
}
