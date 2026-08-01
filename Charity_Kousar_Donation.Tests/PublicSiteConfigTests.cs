using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.Models;
using Charity_Kousar_Donation.Services;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Tests;

/// <summary>
/// The public config is what every visitor's browser gets. These tests pin the two things
/// that used to be easy to break: values coming from the wrong setting, and a missing row
/// silently turning a feature on.
/// </summary>
public class PublicSiteConfigTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task An_empty_database_falls_back_to_the_catalog_defaults()
    {
        await using var db = NewDb();
        var cfg = await new SettingsService(db).GetPublicConfigAsync();

        Assert.Equal(SettingsCatalog.DefaultOf("site.name.fa"), cfg.SiteNameFa);
        Assert.Equal(SettingsCatalog.DefaultOf("site.progress.flow.style"), cfg.ProgressFlowStyle);
        Assert.Equal(SettingsCatalog.DefaultOf("featured.styles"), cfg.FeaturedStyles);
        Assert.Equal(2400, cfg.ProgressFlowMs);
        Assert.True(cfg.ProgressFlow);
        Assert.True(cfg.ShowCompletedTab);
    }

    [Fact]
    public async Task A_missing_row_never_switches_on_the_payment_bypass()
    {
        await using var db = NewDb();

        var cfg = await new SettingsService(db).GetPublicConfigAsync();

        Assert.False(cfg.PaymentBypassEnabled);
    }

    [Fact]
    public async Task Saved_values_win_over_the_defaults_and_land_on_the_right_field()
    {
        await using var db = NewDb();
        await DbSeeder.SeedSettingsAsync(db);
        foreach (var (key, value) in new[]
                 {
                     ("site.name.fa", "خیریه آزمایشی"),
                     ("site.hero.badge.fa", "نشان تازه"),
                     ("site.tagline.fa", "شعار تازه"),
                     ("site.completed.title.fa", "تمام‌شده‌ها"),
                     ("site.progress.flow.style", "stripes"),
                     ("site.progress.flow.ms", "1500"),
                     ("site.completed.show", "false")
                 })
            db.SiteSettings.Single(s => s.Key == key).Value = value;
        await db.SaveChangesAsync();

        var cfg = await new SettingsService(db).GetPublicConfigAsync();

        Assert.Equal("خیریه آزمایشی", cfg.SiteNameFa);
        // The hero badge and the header tagline are separate settings — they used to share one.
        Assert.Equal("نشان تازه", cfg.HeroBadgeFa);
        Assert.Equal("شعار تازه", cfg.TaglineFa);
        Assert.Equal("تمام‌شده‌ها", cfg.CompletedTitleFa);
        Assert.Equal("stripes", cfg.ProgressFlowStyle);
        Assert.Equal(1500, cfg.ProgressFlowMs);
        Assert.False(cfg.ShowCompletedTab);
    }

    [Fact]
    public async Task Quick_amounts_are_cleaned_up_for_the_donation_form()
    {
        await using var db = NewDb();
        db.SiteSettings.Add(new SiteSetting
        {
            Key = "donation.quick.amounts", Value = " 200000, 50000 ,,50000, abc, -10 ",
            Group = "donation", LabelFa = "-", LabelEn = "-"
        });
        await db.SaveChangesAsync();

        var cfg = await new SettingsService(db).GetPublicConfigAsync();

        Assert.Equal(new List<long> { 50_000, 200_000 }, cfg.QuickDonationAmounts);
    }

    [Fact]
    public async Task A_blank_logo_is_reported_as_no_logo()
    {
        await using var db = NewDb();
        await DbSeeder.SeedSettingsAsync(db);

        var cfg = await new SettingsService(db).GetPublicConfigAsync();
        Assert.Null(cfg.LogoUrl);

        db.SiteSettings.Single(s => s.Key == "site.logo.url").Value = "/uploads/logo.png";
        await db.SaveChangesAsync();
        var withLogo = await new SettingsService(db).GetPublicConfigAsync();
        Assert.Equal("/uploads/logo.png", withLogo.LogoUrl);
    }
}
