using Charity_Kousar_Donation.Models;
using Charity_Kousar_Donation.Services;

namespace Charity_Kousar_Donation.Data;

/// <summary>One site setting: its key, default value, and how the admin panel shows it.</summary>
public sealed record SettingDef(
    string Key,
    string Default,
    string Group,
    string LabelFa,
    string LabelEn,
    SettingType Type = SettingType.Text,
    int SortOrder = 0);

/// <summary>
/// The single source of truth for every site setting. Seeding a fresh database,
/// adding settings to an existing one, and the API's fallback values all read from
/// this list, so a new setting is added in exactly one place.
/// </summary>
public static class SettingsCatalog
{
    public static readonly IReadOnlyList<SettingDef> All =
    [
        // ── Appearance, logo & colors ────────────────────────────────────────
        new("site.name.fa", "خیریه کوثر", "site", "نام سایت (فارسی)", "Site name (FA)", SortOrder: 1),
        new("site.name.en", "Kousar Charity", "site", "نام سایت (انگلیسی)", "Site name (EN)", SortOrder: 2),
        new("site.tagline.fa", "همراه شما برای ساختن امید", "site", "شعار (فارسی)", "Tagline (FA)", SortOrder: 3),
        new("site.tagline.en", "Building hope together", "site", "شعار (انگلیسی)", "Tagline (EN)", SortOrder: 4),
        new("site.hero.fa", "با هم می‌توانیم زندگی‌ها را روشن کنیم", "site", "متن بنر اصلی (فارسی)", "Hero text (FA)", SettingType.TextArea, 5),
        new("site.hero.en", "Together we can light up lives", "site", "متن بنر اصلی (انگلیسی)", "Hero text (EN)", SettingType.TextArea, 6),
        new("site.logo.url", "", "site", "آدرس لوگو", "Logo URL", SettingType.Url, 7),
        new("site.color.primary", "#0d9488", "site", "رنگ اصلی", "Primary color", SettingType.Color, 8),
        new("site.color.accent", "#f59e0b", "site", "رنگ تأکید", "Accent color", SettingType.Color, 9),
        new("site.color.background", "#0f172a", "site", "رنگ پس‌زمینه", "Background color", SettingType.Color, 10),
        new("site.footer.fa", "© خیریه کوثر — تمامی حقوق محفوظ است", "site", "متن فوتر (فارسی)", "Footer (FA)", SettingType.TextArea, 11),
        new("site.footer.en", "© Kousar Charity — All rights reserved", "site", "متن فوتر (انگلیسی)", "Footer (EN)", SettingType.TextArea, 12),
        new("site.logo.height", "48", "site", "اندازه لوگو (پیکسل)", "Logo size (px)", SettingType.Number, 13),
        new("site.logo.show.text", "true", "site", "نمایش نام و شعار کنار لوگو", "Show name & tagline next to logo", SettingType.Boolean, 14),
        // The hero badge has its own text, separate from the tagline beside the logo.
        new("site.hero.badge.fa", "همراه شما برای ساختن امید", "site", "متن نشان بالای بنر (فارسی)", "Hero badge text (FA)", SortOrder: 15),
        new("site.hero.badge.en", "Building hope together", "site", "متن نشان بالای بنر (انگلیسی)", "Hero badge text (EN)", SortOrder: 16),

        // ── Home page layout & progress bars ─────────────────────────────────
        new("site.home.order", "hero,featured,campaigns,donors", "home", "ترتیب کادرهای صفحه اصلی", "Home sections order", SortOrder: 1),
        new("site.progress.mode", "shift", "home", "حالت رنگ نوار پیشرفت", "Progress bar color mode", SortOrder: 2),
        new("site.progress.color.start", "#ef4444", "home", "رنگ شروع نوار پیشرفت", "Progress start color", SettingType.Color, 3),
        new("site.progress.color.end", "#22c55e", "home", "رنگ پایان نوار (سبز)", "Progress end color (green)", SettingType.Color, 4),
        new("site.progress.show.percent", "true", "home", "نمایش درصد روی نوار", "Show percent on bar", SettingType.Boolean, 5),
        new("site.home.columns", "auto", "home", "تعداد ستون کارت‌ها (auto/2/3/4)", "Card columns (auto/2/3/4)", SortOrder: 6),
        new("site.home.merge.featured", "false", "home", "نمایش ویژه و عادی در یک کادر", "Merge featured into one grid", SettingType.Boolean, 7),
        new("site.progress.animate", "true", "home", "پر شدن متحرک نوار پیشرفت", "Animate progress fill", SettingType.Boolean, 8),
        new("site.progress.animate.ms", "1400", "home", "مدت انیمیشن نوار (میلی‌ثانیه)", "Fill animation duration (ms)", SettingType.Number, 9),
        new("site.progress.track.color", "", "home", "رنگ زمینه نوار (خالی = پیش‌فرض)", "Progress track color (empty = default)", SettingType.Color, 10),
        new("site.card.image.fit", "cover", "home", "نمایش تصویر کارت (cover/contain)", "Card image fit (cover/contain)", SortOrder: 11),
        new("site.completed.show", "true", "home", "تب پرونده‌های تکمیل‌شده", "Completed projects tab", SettingType.Boolean, 12),
        new("site.completed.title.fa", "پرونده‌های تکمیل‌شده", "home", "عنوان تب تکمیل‌شده (فارسی)", "Completed tab title (FA)", SortOrder: 13),
        new("site.completed.title.en", "Completed projects", "home", "عنوان تب تکمیل‌شده (انگلیسی)", "Completed tab title (EN)", SortOrder: 14),
        // Motion that keeps running after a bar is full, so it never looks frozen.
        new("site.progress.flow", "true", "home", "جریان پیوسته روی نوار پیشرفت", "Continuous flow on progress bars", SettingType.Boolean, 17),
        new("site.progress.flow.style", "shimmer", "home", "نوع جریان (shimmer/stripes/glow/pulse)", "Flow style (shimmer/stripes/glow/pulse)", SortOrder: 18),
        new("site.progress.flow.ms", "2400", "home", "مدت هر دور جریان (میلی‌ثانیه)", "Flow loop duration (ms)", SettingType.Number, 19),

        // ── Featured section & countdown timer ───────────────────────────────
        new("featured.units", "days,hours,minutes,seconds", "featured", "واحدهای شمارش معکوس", "Countdown units", SortOrder: 1),
        new("featured.layout", "boxes", "featured", "چیدمان تایمر (boxes یا inline)", "Timer layout (boxes/inline)", SortOrder: 2),
        new("featured.badge.show", "true", "featured", "نمایش نشان «ویژه»", "Show featured badge", SettingType.Boolean, 3),
        new("featured.badge.fa", "⭐ ویژه", "featured", "متن نشان (فارسی)", "Badge text (FA)", SortOrder: 4),
        new("featured.badge.en", "⭐ Featured", "featured", "متن نشان (انگلیسی)", "Badge text (EN)", SortOrder: 5),
        new("featured.color", "#f59e0b", "featured", "رنگ تایمر", "Timer color", SettingType.Color, 6),
        new("featured.expired.fa", "⏱ فرصت به پایان رسید", "featured", "متن پایان زمان (فارسی)", "Expired text (FA)", SortOrder: 7),
        new("featured.expired.en", "⏱ Time ended", "featured", "متن پایان زمان (انگلیسی)", "Expired text (EN)", SortOrder: 8),
        new("featured.styles", SettingsService.DefaultFeaturedStyles, "featured", "حالت‌های نشان ویژه", "Featured highlight styles", SettingType.TextArea, 9),

        // ── Payments ─────────────────────────────────────────────────────────
        new("zarinpal.merchant", "", "payment", "مرچنت زرین‌پال", "ZarinPal Merchant ID", SortOrder: 1),
        new("zarinpal.sandbox", "true", "payment", "حالت تست (سندباکس)", "Sandbox mode", SettingType.Boolean, 2),
        new("zarinpal.enabled", "true", "payment", "فعال بودن زرین‌پال", "ZarinPal enabled", SettingType.Boolean, 3),
        new("payment.bypass.enabled", "true", "payment", "حالت تست پرداخت (بدون درگاه واقعی)", "Payment test bypass", SettingType.Boolean, 4),

        // ── Cryptocurrency ───────────────────────────────────────────────────
        new("crypto.enabled", "false", "crypto", "فعال بودن پرداخت رمزارز", "Crypto enabled", SettingType.Boolean, 1),
        new("crypto.api.url", "", "crypto", "آدرس API درگاه رمزارز", "Crypto gateway API URL", SettingType.Url, 2),
        new("crypto.api.key", "", "crypto", "کلید API", "API Key", SettingType.Password, 3),
        new("crypto.wallet.address", "", "crypto", "آدرس کیف پول", "Wallet address", SortOrder: 4),
        new("crypto.network", "TRC20", "crypto", "شبکه", "Network", SortOrder: 5),

        // ── SMS ──────────────────────────────────────────────────────────────
        new("sms.provider", "kavenegar", "sms", "سرویس پیامک", "SMS provider", SortOrder: 1),
        new("sms.api.key", "", "sms", "کلید API پیامک", "SMS API Key", SettingType.Password, 2),
        new("sms.sender", "", "sms", "شماره فرستنده", "Sender number", SortOrder: 3),
        new("sms.template",
            "بانی گرامی {name}، کمک {amount} تومانی شما برای «{campaign}» با موفقیت ثبت شد. کد پیگیری: {ref}. سپاس از همراهی شما — خیریه کوثر",
            "sms", "متن پیامک", "SMS template", SettingType.TextArea, 4),
        new("sms.enabled", "true", "sms", "ارسال پیامک فعال", "SMS enabled", SettingType.Boolean, 5),

        // ── Donations & amounts ──────────────────────────────────────────────
        new("donation.min.amount", "10000", "donation", "حداقل مبلغ (تومان)", "Min amount (Toman)", SettingType.Number, 1),
        new("donation.quick.amounts", "50000,100000,200000,500000,1000000", "donation", "مبالغ پیشنهادی (با کاما)", "Quick amounts (comma-separated)", SortOrder: 2),
        new("donation.otp.enabled", "false", "donation", "تأیید OTP برای مبالغ بالا", "OTP for large amounts", SettingType.Boolean, 3),
        new("donation.otp.threshold", "5000000", "donation", "آستانه OTP (تومان)", "OTP threshold (Toman)", SettingType.Number, 4),
        // Stars mark the part that gets the highlight color, e.g. *{collected}*.
        new("donation.progress.format.fa", "*{collected}* از {target} تومان", "donation", "قالب متن مبلغ (فارسی)", "Amount text format (FA)", SortOrder: 5),
        new("donation.progress.format.en", "*{collected}* of {target} Toman", "donation", "قالب متن مبلغ (انگلیسی)", "Amount text format (EN)", SortOrder: 6),
        new("donation.progress.highlight", "#0d9488", "donation", "رنگ تأکید متن مبلغ", "Amount highlight color", SettingType.Color, 7),
        new("donation.progress.color.collected", "", "donation", "رنگ مبلغ جمع‌آوری‌شده", "Raised amount color", SettingType.Color, 8),
        new("donation.progress.color.target", "", "donation", "رنگ مبلغ هدف", "Goal amount color", SettingType.Color, 9),
        new("donation.progress.color.remaining", "", "donation", "رنگ مبلغ باقی‌مانده", "Remaining amount color", SettingType.Color, 10),
        new("donation.progress.color.percent", "", "donation", "رنگ درصد پیشرفت", "Percent color", SettingType.Color, 11),
        new("donation.progress.color.text", "", "donation", "رنگ متن ساده مبلغ", "Plain amount text color", SettingType.Color, 12),
        new("donation.progress.size", "100", "donation", "اندازه متن مبلغ (درصد)", "Amount text size (%)", SettingType.Number, 13),

        // ── Sharing ──────────────────────────────────────────────────────────
        new("share.ai.enabled", "true", "share", "استفاده از AI برای متن اشتراک", "Use AI for share text", SettingType.Boolean, 0),
        new("share.ai.system",
            "You write charity share texts for WhatsApp and Telegram. Output ONLY valid JSON with messageFa and messageEn. No markdown.",
            "share", "دستور سیستم AI", "AI system prompt", SettingType.TextArea, 1),
        new("share.ai.prompt",
            "بر اساس محتوای واقعی این کمپین خیریه، متن اشتراک‌گذاری برای واتساپ/تلگرام بنویس.\n\n" +
            "عنوان فارسی: {titleFa}\nعنوان انگلیسی: {titleEn}\nتوضیح فارسی: {descriptionFa}\nتوضیح انگلیسی: {descriptionEn}\n" +
            "محتوای صفحه (فارسی): {pageContentFa}\nمحتوای صفحه (انگلیسی): {pageContentEn}\n" +
            "جمع‌آوری: {collected} تومان از {target} تومان ({progress}%)\nلینک پرداخت: {link}\nلینک صفحه: {pageUrl}\n\n" +
            "فقط از اطلاعات بالا استفاده کن. در پایان هر متن لینک پرداخت را بیاور.\n" +
            "خروجی JSON: {\"messageFa\":\"...\",\"messageEn\":\"...\"}",
            "share", "پرامپت AI اشتراک", "Share AI prompt template", SettingType.TextArea, 2),
        new("share.template.fa",
            "🤲 {title}\n\n{desc}\n\n📊 {collected} از {target} تومان جمع شده ({progress}%)\n\n💳 برای کمک:\n{link}",
            "share", "قالب آماده متن اشتراک (فارسی)", "Built-in share template (FA)", SettingType.TextArea, 3),
        new("share.template.en",
            "🤲 {title}\n\n{desc}\n\n📊 {collected} of {target} Toman raised ({progress}%)\n\n💳 Donate here:\n{link}",
            "share", "قالب آماده متن اشتراک (انگلیسی)", "Built-in share template (EN)", SettingType.TextArea, 4),

        // ── Contributors display ─────────────────────────────────────────────
        new("donors.show.recent", "true", "donors", "نمایش مشارکت‌کنندگان اخیر", "Show recent contributors", SettingType.Boolean, 1),
        new("donors.show.count", "10", "donors", "تعداد نمایش", "Recent count", SettingType.Number, 2),
        new("donors.show.home", "true", "donors", "نمایش در صفحه اصلی", "Show on home page", SettingType.Boolean, 3),
        new("donors.show.name", "true", "donors", "نمایش نام", "Show name", SettingType.Boolean, 4),
        new("donors.show.amount", "true", "donors", "نمایش مبلغ", "Show amount", SettingType.Boolean, 5),
        new("donors.show.date", "false", "donors", "نمایش تاریخ", "Show date", SettingType.Boolean, 6),
        new("donors.show.campaign", "false", "donors", "نمایش نام پروژه", "Show campaign name", SettingType.Boolean, 7),
        new("donors.anonymous.fa", "نیکوکار", "donors", "عنوان مشارکت‌کننده ناشناس (فارسی)", "Anonymous label (FA)", SortOrder: 8),
        new("donors.anonymous.en", "Well-wisher", "donors", "عنوان مشارکت‌کننده ناشناس (انگلیسی)", "Anonymous label (EN)", SortOrder: 9),
        new("donors.title.fa", "حامیان اخیر", "donors", "عنوان بخش (فارسی)", "Section title (FA)", SortOrder: 10),
        new("donors.title.en", "Recent supporters", "donors", "عنوان بخش (انگلیسی)", "Section title (EN)", SortOrder: 11),
        new("donors.source", "auto", "donors", "منبع لیست (auto/manual/both)", "List source (auto/manual/both)", SortOrder: 12),
        new("donors.manual", "[]", "donors", "لیست دستی مشارکت‌کنندگان", "Manual contributors list", SettingType.TextArea, 13),

        // ── AI & translation ─────────────────────────────────────────────────
        new("openrouter.enabled", "true", "ai", "فعال بودن AI", "AI enabled", SettingType.Boolean, 1),
        new("openrouter.api.key", "", "ai", "کلید API OpenRouter", "OpenRouter API Key", SettingType.Password, 2),
        new("openrouter.model", "google/gemma-2-9b-it:free", "ai", "مدل (مثلاً openai/gpt-4o-mini)", "Model ID", SortOrder: 3),
        new("openrouter.translate.model", "", "ai", "مدل ترجمه (خالی = همان مدل اصلی)", "Translation model (empty = main model)", SortOrder: 4),
    ];

    private static readonly Dictionary<string, SettingDef> ByKey =
        All.ToDictionary(d => d.Key, StringComparer.Ordinal);

    /// <summary>The built-in value for a key, used when the database has no row for it.</summary>
    public static string DefaultOf(string key) => ByKey.TryGetValue(key, out var d) ? d.Default : "";

    public static SiteSetting ToRow(this SettingDef d) => new()
    {
        Key = d.Key,
        Value = d.Default,
        Group = d.Group,
        LabelFa = d.LabelFa,
        LabelEn = d.LabelEn,
        Type = d.Type,
        SortOrder = d.SortOrder
    };
}
