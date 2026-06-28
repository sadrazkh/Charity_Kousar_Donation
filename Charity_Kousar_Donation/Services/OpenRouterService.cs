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

    /// <summary>
    /// Translate text from one language to another. Used by the admin "auto-translate" buttons.
    /// </summary>
    public async Task<(bool Ok, string? Translated, string? Error)> TranslateAsync(
        string text, string fromLang, string toLang)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, null, "متنی برای ترجمه وارد نشده است");

        if (!await settings.GetBoolAsync("openrouter.enabled", true))
            return (false, null, "هوش مصنوعی در تنظیمات غیرفعال است");

        var apiKey = await settings.GetAsync("openrouter.api.key");
        if (string.IsNullOrWhiteSpace(apiKey))
            return (false, null, "کلید OpenRouter در تنظیمات وارد نشده است");

        // A dedicated translation model can be configured; otherwise reuse the main model.
        var model = await settings.GetAsync("openrouter.translate.model");
        if (string.IsNullOrWhiteSpace(model))
            model = await settings.GetAsync("openrouter.model", "google/gemma-2-9b-it:free");

        var fromName = LangName(fromLang);
        var toName = LangName(toLang);

        var systemPrompt =
            $"You are a professional translator for a charity donation website. " +
            $"Translate the user's text from {fromName} to {toName}. " +
            "Keep the tone warm and trustworthy. Preserve line breaks, emojis and numbers. " +
            "Return ONLY the translated text — no quotes, no notes, no explanations.";

        try
        {
            var content = await ChatRawAsync(apiKey, model, systemPrompt, text);
            if (string.IsNullOrWhiteSpace(content))
                return (false, null, "پاسخی از سرویس ترجمه دریافت نشد");
            return (true, content.Trim().Trim('"'), null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Translation request failed");
            return (false, null, "خطای داخلی در ترجمه");
        }
    }

    private static string LangName(string code) => code switch
    {
        "fa" => "Persian (Farsi)",
        "en" => "English",
        "ar" => "Arabic",
        _ => code
    };

    public async Task<(bool Ok, string? MessageFa, string? MessageEn, string? Error)> GenerateShareMessagesAsync(
        string titleFa, string titleEn, string descFa, string descEn,
        string pageContentFa, string pageContentEn,
        decimal target, decimal collected, int progress, string payLink, string pageUrl)
    {
        var targetFmt = target.ToString("N0");
        var collectedFmt = collected.ToString("N0");

        var fallbackFa = await BuildTemplateShareAsync("share.template.fa", titleFa, descFa, collectedFmt, targetFmt, progress, payLink, fa: true);
        var fallbackEn = await BuildTemplateShareAsync("share.template.en", titleEn, descEn, collectedFmt, targetFmt, progress, payLink, fa: false);

        // Admin can disable AI and rely purely on the simple built-in templates.
        if (!await settings.GetBoolAsync("share.ai.enabled", true))
            return (true, fallbackFa, fallbackEn, null);

        if (!await settings.GetBoolAsync("openrouter.enabled", true))
            return (true, fallbackFa, fallbackEn, null);

        var apiKey = await settings.GetAsync("openrouter.api.key");
        if (string.IsNullOrWhiteSpace(apiKey))
            return (true, fallbackFa, fallbackEn, null);

        var model = await settings.GetAsync("openrouter.model", "google/gemma-2-9b-it:free");
        var systemPrompt = await settings.GetAsync("share.ai.system",
            "You write charity share texts for WhatsApp and Telegram. Output ONLY valid JSON with messageFa and messageEn. No markdown.");
        var promptTemplate = await settings.GetAsync("share.ai.prompt", DefaultSharePrompt());

        var userPrompt = ApplySharePrompt(promptTemplate, titleFa, titleEn, descFa, descEn,
            pageContentFa, pageContentEn, collectedFmt, targetFmt, progress.ToString(), payLink, pageUrl);

        try
        {
            var content = await ChatRawAsync(apiKey, model, systemPrompt, userPrompt);
            if (string.IsNullOrWhiteSpace(content))
                return (true, fallbackFa, fallbackEn, null);

            content = ExtractJson(content);
            var result = JsonSerializer.Deserialize<ShareJsonResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (string.IsNullOrWhiteSpace(result?.MessageFa))
                return (true, fallbackFa, fallbackEn, null);

            return (true, EnsureLink(result.MessageFa, payLink), EnsureLink(result.MessageEn ?? result.MessageFa, payLink), null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Share message generation failed");
            return (true, fallbackFa, fallbackEn, null);
        }
    }

    private static string DefaultSharePrompt() => """
        بر اساس محتوای واقعی این کمپین خیریه، متن اشتراک‌گذاری برای واتساپ/تلگرام بنویس.

        عنوان فارسی: {titleFa}
        عنوان انگلیسی: {titleEn}
        توضیح فارسی: {descriptionFa}
        توضیح انگلیسی: {descriptionEn}
        جمع‌آوری: {collected} تومان از {target} تومان ({progress}%)
        لینک پرداخت: {link}
        لینک صفحه: {pageUrl}

        فقط از اطلاعات بالا استفاده کن. در پایان هر متن لینک پرداخت را بیاور.
        خروجی JSON: {"messageFa":"...","messageEn":"..."}
        """;

    private static string ApplySharePrompt(string template, string titleFa, string titleEn,
        string descFa, string descEn, string pageContentFa, string pageContentEn,
        string collected, string target, string progress, string link, string pageUrl) =>
        template
            .Replace("{titleFa}", titleFa).Replace("{titleEn}", titleEn)
            .Replace("{descriptionFa}", descFa).Replace("{descriptionEn}", descEn)
            .Replace("{descFa}", descFa).Replace("{descEn}", descEn)
            .Replace("{pageContentFa}", pageContentFa).Replace("{pageContentEn}", pageContentEn)
            .Replace("{collected}", collected).Replace("{target}", target)
            .Replace("{progress}", progress).Replace("{link}", link).Replace("{pageUrl}", pageUrl);

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

    private async Task<string> BuildTemplateShareAsync(string templateKey, string title, string desc,
        string col, string tgt, int pct, string link, bool fa)
    {
        var defaultTemplate = fa
            ? "🤲 {title}\n\n{desc}\n\n📊 {collected} از {target} تومان جمع شده ({progress}%)\n\n💳 برای کمک:\n{link}"
            : "🤲 {title}\n\n{desc}\n\n📊 {collected} of {target} Toman raised ({progress}%)\n\n💳 Donate here:\n{link}";
        var template = await settings.GetAsync(templateKey, defaultTemplate);
        if (string.IsNullOrWhiteSpace(template)) template = defaultTemplate;

        var shortDesc = desc.Length > 160 ? desc[..160].TrimEnd() + "…" : desc;
        var msg = template
            .Replace("{title}", title)
            .Replace("{desc}", shortDesc)
            .Replace("{collected}", col)
            .Replace("{target}", tgt)
            .Replace("{progress}", pct.ToString())
            .Replace("{link}", link);
        return EnsureLink(msg, link);
    }

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
