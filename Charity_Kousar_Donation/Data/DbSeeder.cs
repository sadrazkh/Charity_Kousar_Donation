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
                new() { Key = "donation.otp.enabled", Value = "false", Group = "donation", LabelFa = "تأیید OTP برای مبالغ بالا", LabelEn = "OTP for large amounts", Type = SettingType.Boolean, SortOrder = 3 },
                new() { Key = "donation.otp.threshold", Value = "5000000", Group = "donation", LabelFa = "آستانه OTP (تومان)", LabelEn = "OTP threshold (Toman)", Type = SettingType.Number, SortOrder = 4 },

                new() { Key = "share.ai.system", Value = "You write charity share texts for WhatsApp and Telegram. Output ONLY valid JSON with messageFa and messageEn. No markdown.", Group = "share", LabelFa = "دستور سیستم AI", LabelEn = "AI system prompt", Type = SettingType.TextArea, SortOrder = 1 },
                new() { Key = "share.ai.prompt", Value = "بر اساس محتوای واقعی این کمپین خیریه، متن اشتراک‌گذاری برای واتساپ/تلگرام بنویس.\n\nعنوان فارسی: {titleFa}\nعنوان انگلیسی: {titleEn}\nتوضیح فارسی: {descriptionFa}\nتوضیح انگلیسی: {descriptionEn}\nمحتوای صفحه (فارسی): {pageContentFa}\nمحتوای صفحه (انگلیسی): {pageContentEn}\nجمع‌آوری: {collected} تومان از {target} تومان ({progress}%)\nلینک پرداخت: {link}\nلینک صفحه: {pageUrl}\n\nفقط از اطلاعات بالا استفاده کن. در پایان هر متن لینک پرداخت را بیاور.\nخروجی JSON: {\"messageFa\":\"...\",\"messageEn\":\"...\"}", Group = "share", LabelFa = "پرامپت AI اشتراک", LabelEn = "Share AI prompt template", Type = SettingType.TextArea, SortOrder = 2 },

                new() { Key = "donors.show.recent", Value = "true", Group = "donors", LabelFa = "نمایش حامیان اخیر", LabelEn = "Show recent donors", Type = SettingType.Boolean, SortOrder = 1 },
                new() { Key = "donors.show.count", Value = "10", Group = "donors", LabelFa = "تعداد حامیان", LabelEn = "Recent donors count", Type = SettingType.Number, SortOrder = 2 },

                new() { Key = "openrouter.enabled", Value = "true", Group = "ai", LabelFa = "فعال بودن AI", LabelEn = "AI enabled", Type = SettingType.Boolean, SortOrder = 1 },
                new() { Key = "openrouter.api.key", Value = "", Group = "ai", LabelFa = "کلید API OpenRouter", LabelEn = "OpenRouter API Key", Type = SettingType.Password, SortOrder = 2 },
                new() { Key = "openrouter.model", Value = "google/gemma-2-9b-it:free", Group = "ai", LabelFa = "مدل (مثلاً openai/gpt-4o-mini)", LabelEn = "Model ID", SortOrder = 3 },
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
