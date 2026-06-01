using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Charity_Kousar_Donation.Services;

public class OpenRouterService(IHttpClientFactory httpFactory, SettingsService settings, ILogger<OpenRouterService> logger)
{
    public async Task<(bool Ok, string? Optimized, string? Alternative, string? Tips, string? Error)> OptimizeTextAsync(
        string text, string language, string fieldType, string? campaignTitle, string? context)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, null, null, null, "متن خالی است");

        if (!await settings.GetBoolAsync("openrouter.enabled", true))
            return (false, null, null, null, "هوش مصنوعی در تنظیمات غیرفعال است");

        var apiKey = await settings.GetAsync("openrouter.api.key");
        if (string.IsNullOrWhiteSpace(apiKey))
            return (false, null, null, null, "کلید OpenRouter در تنظیمات وارد نشده است");

        var model = await settings.GetAsync("openrouter.model", "google/gemma-2-9b-it:free");
        var langName = language == "fa" ? "Persian (Farsi)" : "English";
        var fieldDesc = fieldType switch
        {
            "heading" => "a compelling campaign heading",
            "body" => "charity campaign body text",
            "quote" => "an inspiring quote for donors",
            "cta" => "a short call-to-action button label",
            "caption" => "an image caption",
            _ => "charity campaign content"
        };

        var systemPrompt = """
            You are a copywriter for a Persian/English charity donation platform.
            Improve the user's draft to be clear, emotional, trustworthy, and concise.
            Return ONLY valid JSON with keys: optimized (string), alternative (string, optional second version), tips (string, one short tip in the same language as optimized).
            Do not wrap JSON in markdown.
            """;

        var userPrompt = $"""
            Language: {langName}
            Content type: {fieldDesc}
            Campaign: {campaignTitle ?? "charity campaign"}
            Extra context: {context ?? "none"}
            Draft text:
            {text}
            """;

        try
        {
            var content = await ChatRawAsync(apiKey, model, systemPrompt, userPrompt);
            if (string.IsNullOrWhiteSpace(content))
                return (false, null, null, null, "پاسخی از AI دریافت نشد");

            content = ExtractJson(content);
            var result = JsonSerializer.Deserialize<AiJsonResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result?.Optimized == null)
                return (true, content, null, null, null);

            return (true, result.Optimized, result.Alternative, result.Tips, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OpenRouter request failed");
            return (false, null, null, null, "خطای داخلی AI");
        }
    }

    public async Task<(bool Ok, string? MessageFa, string? MessageEn, string? Error)> GenerateShareMessagesAsync(
        string titleFa, string titleEn, string descFa, string descEn,
        decimal target, decimal collected, int progress, string payLink)
    {
        var targetFmt = target.ToString("N0");
        var collectedFmt = collected.ToString("N0");

        if (!await settings.GetBoolAsync("openrouter.enabled", true))
            return (true, BuildFallbackShareFa(titleFa, descFa, collectedFmt, targetFmt, progress, payLink),
                BuildFallbackShareEn(titleEn, descEn, collectedFmt, targetFmt, progress, payLink), null);

        var apiKey = await settings.GetAsync("openrouter.api.key");
        if (string.IsNullOrWhiteSpace(apiKey))
            return (true, BuildFallbackShareFa(titleFa, descFa, collectedFmt, targetFmt, progress, payLink),
                BuildFallbackShareEn(titleEn, descEn, collectedFmt, targetFmt, progress, payLink), null);

        var model = await settings.GetAsync("openrouter.model", "google/gemma-2-9b-it:free");
        var systemPrompt = """
            You write viral charity share messages for Telegram and WhatsApp in Persian and English.
            Return ONLY JSON: {"messageFa":"...","messageEn":"..."}
            Rules: warm tone, 2-4 short paragraphs max, use 2-3 relevant emojis, include progress stats,
            MUST end with payment link on its own line prefixed with 🔗 or 💳
            Do not invent fake stats. Do not wrap in markdown.
            """;

        var userPrompt = $"""
            Title FA: {titleFa}
            Title EN: {titleEn}
            Description FA: {descFa}
            Description EN: {descEn}
            Collected: {collectedFmt} Toman
            Target: {targetFmt} Toman
            Progress: {progress}%
            Payment link (include exactly at end): {payLink}
            """;

        try
        {
            var content = await ChatRawAsync(apiKey, model, systemPrompt, userPrompt);
            if (string.IsNullOrWhiteSpace(content))
                return (true, BuildFallbackShareFa(titleFa, descFa, collectedFmt, targetFmt, progress, payLink),
                    BuildFallbackShareEn(titleEn, descEn, collectedFmt, targetFmt, progress, payLink), null);

            content = ExtractJson(content);
            var result = JsonSerializer.Deserialize<ShareJsonResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (string.IsNullOrWhiteSpace(result?.MessageFa))
                return (true, BuildFallbackShareFa(titleFa, descFa, collectedFmt, targetFmt, progress, payLink),
                    BuildFallbackShareEn(titleEn, descEn, collectedFmt, targetFmt, progress, payLink), null);

            return (true, EnsureLink(result.MessageFa, payLink), EnsureLink(result.MessageEn ?? result.MessageFa, payLink), null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Share message generation failed");
            return (true, BuildFallbackShareFa(titleFa, descFa, collectedFmt, targetFmt, progress, payLink),
                BuildFallbackShareEn(titleEn, descEn, collectedFmt, targetFmt, progress, payLink), null);
        }
    }

    private async Task<string?> ChatRawAsync(string apiKey, string model, string systemPrompt, string userPrompt)
    {
        var client = httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", "https://kousar-charity.local");
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", "Kousar Charity");

        var body = new
        {
            model,
            temperature = 0.75,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        var response = await client.PostAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        var raw = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return null;

        var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(raw);
        return parsed?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
    }

    private static string ExtractJson(string content)
    {
        if (content.StartsWith("```"))
        {
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');
            if (start >= 0 && end > start) return content[start..(end + 1)];
        }
        return content;
    }

    private static string EnsureLink(string msg, string link) =>
        msg.Contains(link, StringComparison.OrdinalIgnoreCase) ? msg : $"{msg.Trim()}\n\n🔗 {link}";

    private static string BuildFallbackShareFa(string title, string desc, string col, string tgt, int pct, string link) =>
        $"🤲 {title}\n\n{(desc.Length > 120 ? desc[..120] + "…" : desc)}\n\n📊 {col} از {tgt} تومان جمع شده ({pct}%)\n\n💳 لینک کمک:\n{link}";

    private static string BuildFallbackShareEn(string title, string desc, string col, string tgt, int pct, string link) =>
        $"🤲 {title}\n\n{(desc.Length > 120 ? desc[..120] + "…" : desc)}\n\n📊 {col} of {tgt} Toman raised ({pct}%)\n\n💳 Donate here:\n{link}";

    private class ShareJsonResult
    {
        public string? MessageFa { get; set; }
        public string? MessageEn { get; set; }
    }

    private class OpenRouterResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }

    private class AiJsonResult
    {
        public string? Optimized { get; set; }
        public string? Alternative { get; set; }
        public string? Tips { get; set; }
    }
}
