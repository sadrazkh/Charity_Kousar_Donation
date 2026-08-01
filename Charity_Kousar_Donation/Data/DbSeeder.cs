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

        // Settings live in SettingsCatalog. A fresh database gets the whole list; an existing
        // one only receives keys it is missing, so admin-edited values are never touched.
        var existingKeys = await db.SiteSettings.Select(s => s.Key).ToListAsync();
        var missing = SettingsCatalog.All
            .Where(d => !existingKeys.Contains(d.Key))
            .Select(d => d.ToRow())
            .ToList();
        if (missing.Count > 0)
        {
            db.SiteSettings.AddRange(missing);
            await db.SaveChangesAsync();
            logger.LogInformation("Added {Count} site settings from the catalog.", missing.Count);
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
