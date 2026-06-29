using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Charity_Kousar_Donation.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config, ILogger logger)
    {
        try
        {
            await EnsureDatabaseExistsAsync(config, logger);
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (PostgresException ex) when (ex.SqlState == "28P01")
        {
            logger.LogCritical(
                "PostgreSQL password rejected for user 'postgres'. " +
                "Fix Password in appsettings.Development.json (when running in Development) " +
                "or set User Secret: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Host=localhost;...;Password=YOUR_PASSWORD\"");
            throw;
        }

        if (!await db.SiteSettings.AnyAsync())
        {
            var settings = new List<SiteSetting>
            {
                new() { Key = "site.name.fa", Value = "خیریه کوثر", Group = "site", LabelFa = "نام سایت (فارسی)", LabelEn = "Site name (FA)", SortOrder = 1 },
                new() { Key = "site.name.en", Value = "Kousar Charity", Group = "site", LabelFa = "نام سایت (انگلیسی)", LabelEn = "Site name (EN)", SortOrder = 2 },
                new() { Key = "site.tagline.fa", Value = "همراه شما برای ساختن امید", Group = "site", LabelFa = "شعار (فارسی)", LabelEn = "Tagline (FA)", SortOrder = 3 },
                new() { Key = "site.tagline.en", Value = "Building hope together", Group = "site", LabelFa = "شعار (انگلیسی)", LabelEn = "Tagline (EN)", SortOrder = 4 },
                new() { Key = "site.hero.fa", Value = "با هم می‌توانیم زندگی‌ها را روشن کنیم", Group = "site", LabelFa = "متن بنر اصلی (فارسی)", LabelEn = "Hero text (FA)", Type = SettingType.TextArea, SortOrder = 5 },
                new() { Key = "site.hero.en", Value = "Together we can light up lives", Group = "site", LabelFa = "متن بنر اصلی (انگلیسی)", LabelEn = "Hero text (EN)", Type = SettingType.TextArea, SortOrder = 6 },
                new() { Key = "site.logo.url", Value = "", Group = "site", LabelFa = "آدرس لوگو", LabelEn = "Logo URL", Type = SettingType.Url, SortOrder = 7 },
                new() { Key = "site.color.primary", Value = "#0d9488", Group = "site", LabelFa = "رنگ اصلی", LabelEn = "Primary color", Type = SettingType.Color, SortOrder = 8 },
                new() { Key = "site.color.accent", Value = "#f59e0b", Group = "site", LabelFa = "رنگ تأکید", LabelEn = "Accent color", Type = SettingType.Color, SortOrder = 9 },
                new() { Key = "site.color.background", Value = "#0f172a", Group = "site", LabelFa = "رنگ پس‌زمینه", LabelEn = "Background color", Type = SettingType.Color, SortOrder = 10 },
                new() { Key = "site.footer.fa", Value = "© خیریه کوثر — تمامی حقوق محفوظ است", Group = "site", LabelFa = "متن فوتر (فارسی)", LabelEn = "Footer (FA)", Type = SettingType.TextArea, SortOrder = 11 },
                new() { Key = "site.footer.en", Value = "© Kousar Charity — All rights reserved", Group = "site", LabelFa = "متن فوتر (انگلیسی)", LabelEn = "Footer (EN)", Type = SettingType.TextArea, SortOrder = 12 },
                new() { Key = "site.logo.height", Value = "48", Group = "site", LabelFa = "اندازه لوگو (پیکسل)", LabelEn = "Logo size (px)", Type = SettingType.Number, SortOrder = 13 },
                new() { Key = "site.logo.show.text", Value = "true", Group = "site", LabelFa = "نمایش نام و شعار کنار لوگو", LabelEn = "Show name & tagline next to logo", Type = SettingType.Boolean, SortOrder = 14 },

                new() { Key = "site.home.order", Value = "hero,featured,campaigns,donors", Group = "home", LabelFa = "ترتیب کادرهای صفحه اصلی", LabelEn = "Home sections order", SortOrder = 1 },
                new() { Key = "site.home.columns", Value = "auto", Group = "home", LabelFa = "تعداد ستون کارت‌ها (auto/2/3/4)", LabelEn = "Card columns (auto/2/3/4)", SortOrder = 6 },
                new() { Key = "site.home.merge.featured", Value = "false", Group = "home", LabelFa = "نمایش ویژه و عادی در یک کادر", LabelEn = "Merge featured into one grid", Type = SettingType.Boolean, SortOrder = 7 },
                new() { Key = "site.progress.mode", Value = "shift", Group = "home", LabelFa = "حالت رنگ نوار پیشرفت", LabelEn = "Progress bar color mode", SortOrder = 2 },
                new() { Key = "site.progress.color.start", Value = "#ef4444", Group = "home", LabelFa = "رنگ شروع نوار پیشرفت", LabelEn = "Progress start color", Type = SettingType.Color, SortOrder = 3 },
                new() { Key = "site.progress.color.end", Value = "#22c55e", Group = "home", LabelFa = "رنگ پایان نوار (سبز)", LabelEn = "Progress end color (green)", Type = SettingType.Color, SortOrder = 4 },
                new() { Key = "site.progress.show.percent", Value = "true", Group = "home", LabelFa = "نمایش درصد روی نوار", LabelEn = "Show percent on bar", Type = SettingType.Boolean, SortOrder = 5 },

                new() { Key = "featured.units", Value = "days,hours,minutes,seconds", Group = "featured", LabelFa = "واحدهای شمارش معکوس", LabelEn = "Countdown units", SortOrder = 1 },
                new() { Key = "featured.layout", Value = "boxes", Group = "featured", LabelFa = "چیدمان تایمر (boxes یا inline)", LabelEn = "Timer layout (boxes/inline)", SortOrder = 2 },
                new() { Key = "featured.badge.show", Value = "true", Group = "featured", LabelFa = "نمایش نشان «ویژه»", LabelEn = "Show featured badge", Type = SettingType.Boolean, SortOrder = 3 },
                new() { Key = "featured.badge.fa", Value = "⭐ ویژه", Group = "featured", LabelFa = "متن نشان (فارسی)", LabelEn = "Badge text (FA)", SortOrder = 4 },
                new() { Key = "featured.badge.en", Value = "⭐ Featured", Group = "featured", LabelFa = "متن نشان (انگلیسی)", LabelEn = "Badge text (EN)", SortOrder = 5 },
                new() { Key = "featured.color", Value = "#f59e0b", Group = "featured", LabelFa = "رنگ تایمر", LabelEn = "Timer color", Type = SettingType.Color, SortOrder = 6 },
                new() { Key = "featured.expired.fa", Value = "⏱ فرصت به پایان رسید", Group = "featured", LabelFa = "متن پایان زمان (فارسی)", LabelEn = "Expired text (FA)", SortOrder = 7 },
                new() { Key = "featured.expired.en", Value = "⏱ Time ended", Group = "featured", LabelFa = "متن پایان زمان (انگلیسی)", LabelEn = "Expired text (EN)", SortOrder = 8 },

                new() { Key = "zarinpal.merchant", Value = "", Group = "payment", LabelFa = "مرچنت زرین‌پال", LabelEn = "ZarinPal Merchant ID", SortOrder = 1 },
                new() { Key = "zarinpal.sandbox", Value = "true", Group = "payment", LabelFa = "حالت تست (سندباکس)", LabelEn = "Sandbox mode", Type = SettingType.Boolean, SortOrder = 2 },
                new() { Key = "zarinpal.enabled", Value = "true", Group = "payment", LabelFa = "فعال بودن زرین‌پال", LabelEn = "ZarinPal enabled", Type = SettingType.Boolean, SortOrder = 3 },
                new() { Key = "payment.bypass.enabled", Value = "true", Group = "payment", LabelFa = "حالت تست پرداخت (بدون درگاه واقعی)", LabelEn = "Payment test bypass", Type = SettingType.Boolean, SortOrder = 4 },

                new() { Key = "crypto.enabled", Value = "false", Group = "crypto", LabelFa = "فعال بودن پرداخت رمزارز", LabelEn = "Crypto enabled", Type = SettingType.Boolean, SortOrder = 1 },
                new() { Key = "crypto.api.url", Value = "", Group = "crypto", LabelFa = "آدرس API درگاه رمزارز", LabelEn = "Crypto gateway API URL", Type = SettingType.Url, SortOrder = 2 },
                new() { Key = "crypto.api.key", Value = "", Group = "crypto", LabelFa = "کلید API", LabelEn = "API Key", Type = SettingType.Password, SortOrder = 3 },
                new() { Key = "crypto.wallet.address", Value = "", Group = "crypto", LabelFa = "آدرس کیف پول", LabelEn = "Wallet address", SortOrder = 4 },
                new() { Key = "crypto.network", Value = "TRC20", Group = "crypto", LabelFa = "شبکه", LabelEn = "Network", SortOrder = 5 },

                new() { Key = "sms.provider", Value = "kavenegar", Group = "sms", LabelFa = "سرویس پیامک", LabelEn = "SMS provider", SortOrder = 1 },
                new() { Key = "sms.api.key", Value = "", Group = "sms", LabelFa = "کلید API پیامک", LabelEn = "SMS API Key", Type = SettingType.Password, SortOrder = 2 },
                new() { Key = "sms.sender", Value = "", Group = "sms", LabelFa = "شماره فرستنده", LabelEn = "Sender number", SortOrder = 3 },
                new() { Key = "sms.template", Value = "بانی گرامی {name}، کمک {amount} تومانی شما برای «{campaign}» با موفقیت ثبت شد. کد پیگیری: {ref}. سپاس از همراهی شما — خیریه کوثر", Group = "sms", LabelFa = "متن پیامک", LabelEn = "SMS template", Type = SettingType.TextArea, SortOrder = 4 },
                new() { Key = "sms.enabled", Value = "true", Group = "sms", LabelFa = "ارسال پیامک فعال", LabelEn = "SMS enabled", Type = SettingType.Boolean, SortOrder = 5 },

                new() { Key = "donation.min.amount", Value = "10000", Group = "donation", LabelFa = "حداقل مبلغ (تومان)", LabelEn = "Min amount (Toman)", Type = SettingType.Number, SortOrder = 1 },
                new() { Key = "donation.quick.amounts", Value = "50000,100000,200000,500000,1000000", Group = "donation", LabelFa = "مبالغ پیشنهادی (با کاما)", LabelEn = "Quick amounts (comma-separated)", SortOrder = 2 },
                new() { Key = "donation.progress.format.fa", Value = "*{collected}* از {target} تومان", Group = "donation", LabelFa = "قالب متن مبلغ (فارسی)", LabelEn = "Amount text format (FA)", SortOrder = 5 },
                new() { Key = "donation.progress.format.en", Value = "*{collected}* of {target} Toman", Group = "donation", LabelFa = "قالب متن مبلغ (انگلیسی)", LabelEn = "Amount text format (EN)", SortOrder = 6 },
                new() { Key = "donation.progress.highlight", Value = "#0d9488", Group = "donation", LabelFa = "رنگ تأکید متن مبلغ", LabelEn = "Amount highlight color", Type = SettingType.Color, SortOrder = 7 },
                new() { Key = "donation.otp.enabled", Value = "false", Group = "donation", LabelFa = "تأیید OTP برای مبالغ بالا", LabelEn = "OTP for large amounts", Type = SettingType.Boolean, SortOrder = 3 },
                new() { Key = "donation.otp.threshold", Value = "5000000", Group = "donation", LabelFa = "آستانه OTP (تومان)", LabelEn = "OTP threshold (Toman)", Type = SettingType.Number, SortOrder = 4 },

                new() { Key = "share.ai.enabled", Value = "true", Group = "share", LabelFa = "استفاده از AI برای متن اشتراک", LabelEn = "Use AI for share text", Type = SettingType.Boolean, SortOrder = 0 },
                new() { Key = "share.template.fa", Value = "🤲 {title}\n\n{desc}\n\n📊 {collected} از {target} تومان جمع شده ({progress}%)\n\n💳 برای کمک:\n{link}", Group = "share", LabelFa = "قالب آماده متن اشتراک (فارسی)", LabelEn = "Built-in share template (FA)", Type = SettingType.TextArea, SortOrder = 3 },
                new() { Key = "share.template.en", Value = "🤲 {title}\n\n{desc}\n\n📊 {collected} of {target} Toman raised ({progress}%)\n\n💳 Donate here:\n{link}", Group = "share", LabelFa = "قالب آماده متن اشتراک (انگلیسی)", LabelEn = "Built-in share template (EN)", Type = SettingType.TextArea, SortOrder = 4 },
                new() { Key = "share.ai.system", Value = "You write charity share texts for WhatsApp and Telegram. Output ONLY valid JSON with messageFa and messageEn. No markdown.", Group = "share", LabelFa = "دستور سیستم AI", LabelEn = "AI system prompt", Type = SettingType.TextArea, SortOrder = 1 },
                new() { Key = "share.ai.prompt", Value = "بر اساس محتوای واقعی این کمپین خیریه، متن اشتراک‌گذاری برای واتساپ/تلگرام بنویس.\n\nعنوان فارسی: {titleFa}\nعنوان انگلیسی: {titleEn}\nتوضیح فارسی: {descriptionFa}\nتوضیح انگلیسی: {descriptionEn}\nمحتوای صفحه (فارسی): {pageContentFa}\nمحتوای صفحه (انگلیسی): {pageContentEn}\nجمع‌آوری: {collected} تومان از {target} تومان ({progress}%)\nلینک پرداخت: {link}\nلینک صفحه: {pageUrl}\n\nفقط از اطلاعات بالا استفاده کن. در پایان هر متن لینک پرداخت را بیاور.\nخروجی JSON: {\"messageFa\":\"...\",\"messageEn\":\"...\"}", Group = "share", LabelFa = "پرامپت AI اشتراک", LabelEn = "Share AI prompt template", Type = SettingType.TextArea, SortOrder = 2 },

                new() { Key = "donors.show.recent", Value = "true", Group = "donors", LabelFa = "نمایش مشارکت‌کنندگان اخیر", LabelEn = "Show recent contributors", Type = SettingType.Boolean, SortOrder = 1 },
                new() { Key = "donors.show.count", Value = "10", Group = "donors", LabelFa = "تعداد نمایش", LabelEn = "Recent count", Type = SettingType.Number, SortOrder = 2 },
                new() { Key = "donors.show.home", Value = "true", Group = "donors", LabelFa = "نمایش در صفحه اصلی", LabelEn = "Show on home page", Type = SettingType.Boolean, SortOrder = 3 },
                new() { Key = "donors.show.name", Value = "true", Group = "donors", LabelFa = "نمایش نام", LabelEn = "Show name", Type = SettingType.Boolean, SortOrder = 4 },
                new() { Key = "donors.show.amount", Value = "true", Group = "donors", LabelFa = "نمایش مبلغ", LabelEn = "Show amount", Type = SettingType.Boolean, SortOrder = 5 },
                new() { Key = "donors.show.date", Value = "false", Group = "donors", LabelFa = "نمایش تاریخ", LabelEn = "Show date", Type = SettingType.Boolean, SortOrder = 6 },
                new() { Key = "donors.show.campaign", Value = "false", Group = "donors", LabelFa = "نمایش نام پروژه", LabelEn = "Show campaign name", Type = SettingType.Boolean, SortOrder = 7 },
                new() { Key = "donors.anonymous.fa", Value = "نیکوکار", Group = "donors", LabelFa = "عنوان مشارکت‌کننده ناشناس (فارسی)", LabelEn = "Anonymous label (FA)", SortOrder = 8 },
                new() { Key = "donors.anonymous.en", Value = "Well-wisher", Group = "donors", LabelFa = "عنوان مشارکت‌کننده ناشناس (انگلیسی)", LabelEn = "Anonymous label (EN)", SortOrder = 9 },
                new() { Key = "donors.title.fa", Value = "حامیان اخیر", Group = "donors", LabelFa = "عنوان بخش (فارسی)", LabelEn = "Section title (FA)", SortOrder = 10 },
                new() { Key = "donors.title.en", Value = "Recent supporters", Group = "donors", LabelFa = "عنوان بخش (انگلیسی)", LabelEn = "Section title (EN)", SortOrder = 11 },
                new() { Key = "donors.source", Value = "auto", Group = "donors", LabelFa = "منبع لیست (auto/manual/both)", LabelEn = "List source (auto/manual/both)", SortOrder = 12 },
                new() { Key = "donors.manual", Value = "[]", Group = "donors", LabelFa = "لیست دستی مشارکت‌کنندگان", LabelEn = "Manual contributors list", Type = SettingType.TextArea, SortOrder = 13 },

                new() { Key = "openrouter.enabled", Value = "true", Group = "ai", LabelFa = "فعال بودن AI", LabelEn = "AI enabled", Type = SettingType.Boolean, SortOrder = 1 },
                new() { Key = "openrouter.api.key", Value = "", Group = "ai", LabelFa = "کلید API OpenRouter", LabelEn = "OpenRouter API Key", Type = SettingType.Password, SortOrder = 2 },
                new() { Key = "openrouter.model", Value = "google/gemma-2-9b-it:free", Group = "ai", LabelFa = "مدل (مثلاً openai/gpt-4o-mini)", LabelEn = "Model ID", SortOrder = 3 },
                new() { Key = "openrouter.translate.model", Value = "", Group = "ai", LabelFa = "مدل ترجمه (خالی = همان مدل اصلی)", LabelEn = "Translation model (empty = main model)", SortOrder = 4 },
            };
            db.SiteSettings.AddRange(settings);
            await db.SaveChangesAsync();
        }
        else
        {
            await EnsureSettingAsync(db, "openrouter.enabled", "true", "ai", "فعال بودن AI", "AI enabled", SettingType.Boolean, 1);
            await EnsureSettingAsync(db, "openrouter.api.key", "", "ai", "کلید API OpenRouter", "OpenRouter API Key", SettingType.Password, 2);
            await EnsureSettingAsync(db, "openrouter.model", "google/gemma-2-9b-it:free", "ai", "مدل OpenRouter", "Model ID", SettingType.Text, 3);

            await EnsureSettingAsync(db, "donation.quick.amounts", "50000,100000,200000,500000,1000000", "donation",
                "مبالغ پیشنهادی (با کاما)", "Quick amounts (comma-separated)", SettingType.Text, 2);
            await EnsureSettingAsync(db, "donation.otp.enabled", "false", "donation",
                "تأیید OTP برای مبالغ بالا", "OTP for large amounts", SettingType.Boolean, 3);
            await EnsureSettingAsync(db, "donation.otp.threshold", "5000000", "donation",
                "آستانه OTP (تومان)", "OTP threshold (Toman)", SettingType.Number, 4);

            await EnsureSettingAsync(db, "share.ai.system",
                "You write charity share texts for WhatsApp and Telegram. Output ONLY valid JSON with messageFa and messageEn. No markdown.",
                "share", "دستور سیستم AI", "AI system prompt", SettingType.TextArea, 1);
            await EnsureSettingAsync(db, "share.ai.prompt", """
                بر اساس محتوای واقعی این کمپین خیریه، متن اشتراک‌گذاری برای واتساپ/تلگرام بنویس.

                عنوان فارسی: {titleFa}
                عنوان انگلیسی: {titleEn}
                توضیح فارسی: {descriptionFa}
                توضیح انگلیسی: {descriptionEn}
                محتوای صفحه (فارسی): {pageContentFa}
                محتوای صفحه (انگلیسی): {pageContentEn}
                جمع‌آوری: {collected} تومان از {target} تومان ({progress}%)
                لینک پرداخت: {link}
                لینک صفحه: {pageUrl}

                فقط از اطلاعات بالا استفاده کن. در پایان هر متن لینک پرداخت را بیاور.
                خروجی JSON: {"messageFa":"...","messageEn":"..."}
                """, "share", "پرامپت AI اشتراک (قابل ویرایش)", "Share AI prompt template", SettingType.TextArea, 2);

            await EnsureSettingAsync(db, "donors.show.recent", "true", "donors",
                "نمایش حامیان اخیر", "Show recent donors", SettingType.Boolean, 1);
            await EnsureSettingAsync(db, "donors.show.count", "10", "donors",
                "تعداد حامیان نمایش داده شده", "Recent donors count", SettingType.Number, 2);

            await EnsureSettingAsync(db, "payment.bypass.enabled", "true", "payment",
                "حالت تست پرداخت (بدون درگاه واقعی)", "Payment test bypass", SettingType.Boolean, 4);

            // Logo / branding
            await EnsureSettingAsync(db, "site.logo.height", "48", "site", "اندازه لوگو (پیکسل)", "Logo size (px)", SettingType.Number, 13);
            await EnsureSettingAsync(db, "site.logo.show.text", "true", "site", "نمایش نام و شعار کنار لوگو", "Show name & tagline next to logo", SettingType.Boolean, 14);

            // Home layout + progress bar
            await EnsureSettingAsync(db, "site.home.order", "hero,featured,campaigns,donors", "home", "ترتیب کادرهای صفحه اصلی", "Home sections order", SettingType.Text, 1);
            await EnsureSettingAsync(db, "site.home.columns", "auto", "home", "تعداد ستون کارت‌ها (auto/2/3/4)", "Card columns (auto/2/3/4)", SettingType.Text, 6);
            await EnsureSettingAsync(db, "site.home.merge.featured", "false", "home", "نمایش ویژه و عادی در یک کادر", "Merge featured into one grid", SettingType.Boolean, 7);
            await EnsureSettingAsync(db, "donation.progress.highlight", "#0d9488", "donation", "رنگ تأکید متن مبلغ", "Amount highlight color", SettingType.Color, 7);
            await EnsureSettingAsync(db, "site.progress.mode", "shift", "home", "حالت رنگ نوار پیشرفت", "Progress bar color mode", SettingType.Text, 2);
            await EnsureSettingAsync(db, "site.progress.color.start", "#ef4444", "home", "رنگ شروع نوار پیشرفت", "Progress start color", SettingType.Color, 3);
            await EnsureSettingAsync(db, "site.progress.color.end", "#22c55e", "home", "رنگ پایان نوار (سبز)", "Progress end color (green)", SettingType.Color, 4);
            await EnsureSettingAsync(db, "site.progress.show.percent", "true", "home", "نمایش درصد روی نوار", "Show percent on bar", SettingType.Boolean, 5);

            // Featured / countdown timer
            await EnsureSettingAsync(db, "featured.units", "days,hours,minutes,seconds", "featured", "واحدهای شمارش معکوس", "Countdown units", SettingType.Text, 1);
            await EnsureSettingAsync(db, "featured.layout", "boxes", "featured", "چیدمان تایمر (boxes یا inline)", "Timer layout (boxes/inline)", SettingType.Text, 2);
            await EnsureSettingAsync(db, "featured.badge.show", "true", "featured", "نمایش نشان «ویژه»", "Show featured badge", SettingType.Boolean, 3);
            await EnsureSettingAsync(db, "featured.badge.fa", "⭐ ویژه", "featured", "متن نشان (فارسی)", "Badge text (FA)", SettingType.Text, 4);
            await EnsureSettingAsync(db, "featured.badge.en", "⭐ Featured", "featured", "متن نشان (انگلیسی)", "Badge text (EN)", SettingType.Text, 5);
            await EnsureSettingAsync(db, "featured.color", "#f59e0b", "featured", "رنگ تایمر", "Timer color", SettingType.Color, 6);
            await EnsureSettingAsync(db, "featured.expired.fa", "⏱ فرصت به پایان رسید", "featured", "متن پایان زمان (فارسی)", "Expired text (FA)", SettingType.Text, 7);
            await EnsureSettingAsync(db, "featured.expired.en", "⏱ Time ended", "featured", "متن پایان زمان (انگلیسی)", "Expired text (EN)", SettingType.Text, 8);

            // Contributors display
            await EnsureSettingAsync(db, "donors.show.home", "true", "donors", "نمایش در صفحه اصلی", "Show on home page", SettingType.Boolean, 3);
            await EnsureSettingAsync(db, "donors.show.name", "true", "donors", "نمایش نام", "Show name", SettingType.Boolean, 4);
            await EnsureSettingAsync(db, "donors.show.amount", "true", "donors", "نمایش مبلغ", "Show amount", SettingType.Boolean, 5);
            await EnsureSettingAsync(db, "donors.show.date", "false", "donors", "نمایش تاریخ", "Show date", SettingType.Boolean, 6);
            await EnsureSettingAsync(db, "donors.show.campaign", "false", "donors", "نمایش نام پروژه", "Show campaign name", SettingType.Boolean, 7);
            await EnsureSettingAsync(db, "donors.anonymous.fa", "نیکوکار", "donors", "عنوان مشارکت‌کننده ناشناس (فارسی)", "Anonymous label (FA)", SettingType.Text, 8);
            await EnsureSettingAsync(db, "donors.anonymous.en", "Well-wisher", "donors", "عنوان مشارکت‌کننده ناشناس (انگلیسی)", "Anonymous label (EN)", SettingType.Text, 9);
            await EnsureSettingAsync(db, "donors.title.fa", "حامیان اخیر", "donors", "عنوان بخش (فارسی)", "Section title (FA)", SettingType.Text, 10);
            await EnsureSettingAsync(db, "donors.title.en", "Recent supporters", "donors", "عنوان بخش (انگلیسی)", "Section title (EN)", SettingType.Text, 11);
            await EnsureSettingAsync(db, "donors.source", "auto", "donors", "منبع لیست (auto/manual/both)", "List source (auto/manual/both)", SettingType.Text, 12);
            await EnsureSettingAsync(db, "donors.manual", "[]", "donors", "لیست دستی مشارکت‌کنندگان", "Manual contributors list", SettingType.TextArea, 13);

            await EnsureSettingAsync(db, "donation.progress.format.fa", "{collected} از {target} تومان", "donation", "قالب متن مبلغ (فارسی)", "Amount text format (FA)", SettingType.Text, 5);
            await EnsureSettingAsync(db, "donation.progress.format.en", "{collected} of {target} Toman", "donation", "قالب متن مبلغ (انگلیسی)", "Amount text format (EN)", SettingType.Text, 6);

            // Sharing (built-in templates + AI toggle)
            await EnsureSettingAsync(db, "share.ai.enabled", "true", "share", "استفاده از AI برای متن اشتراک", "Use AI for share text", SettingType.Boolean, 0);
            await EnsureSettingAsync(db, "share.template.fa",
                "🤲 {title}\n\n{desc}\n\n📊 {collected} از {target} تومان جمع شده ({progress}%)\n\n💳 برای کمک:\n{link}",
                "share", "قالب آماده متن اشتراک (فارسی)", "Built-in share template (FA)", SettingType.TextArea, 3);
            await EnsureSettingAsync(db, "share.template.en",
                "🤲 {title}\n\n{desc}\n\n📊 {collected} of {target} Toman raised ({progress}%)\n\n💳 Donate here:\n{link}",
                "share", "قالب آماده متن اشتراک (انگلیسی)", "Built-in share template (EN)", SettingType.TextArea, 4);

            // Translation model
            await EnsureSettingAsync(db, "openrouter.translate.model", "", "ai",
                "مدل ترجمه (خالی = همان مدل اصلی)", "Translation model (empty = main model)", SettingType.Text, 4);
        }

        if (!await db.AdminUsers.AnyAsync())
        {
            var username = config["Seed:AdminUsername"] ?? "admin";
            var password = config["Seed:AdminPassword"] ?? "Admin@12345";
            db.AdminUsers.Add(new AdminUser
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Campaigns.AnyAsync())
        {
            db.Campaigns.Add(new Campaign
            {
                TitleFa = "کمک به خانواده‌های نیازمند",
                TitleEn = "Support for families in need",
                DescriptionFa = "با کمک شما می‌توانیم بسته‌های معیشتی و هزینه‌های درمان را برای خانواده‌های تحت پوشش تأمین کنیم.",
                DescriptionEn = "Your donation helps provide food packages and medical support for families we serve.",
                TargetAmount = 50_000_000,
                Slug = "families-in-need",
                ShortCode = "kousar1",
                IsActive = true,
                IsFeatured = true,
                SortOrder = 0
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureSettingAsync(AppDbContext db, string key, string value, string group,
        string labelFa, string labelEn, SettingType type, int sort)
    {
        if (await db.SiteSettings.AnyAsync(s => s.Key == key)) return;
        db.SiteSettings.Add(new SiteSetting
        {
            Key = key, Value = value, Group = group,
            LabelFa = labelFa, LabelEn = labelEn, Type = type, SortOrder = sort
        });
        await db.SaveChangesAsync();
    }

  private static async Task EnsureDatabaseExistsAsync(IConfiguration config, ILogger logger)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        if (string.IsNullOrWhiteSpace(databaseName)) return;

        builder.Database = "postgres";
        await using var conn = new NpgsqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName.Replace("'", "''")}'";
        var exists = await cmd.ExecuteScalarAsync() != null;
        if (!exists)
        {
            logger.LogInformation("Creating database {Database}...", databaseName);
            await using var create = conn.CreateCommand();
            create.CommandText = $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"")}\"";
            await create.ExecuteNonQueryAsync();
        }
    }
}
