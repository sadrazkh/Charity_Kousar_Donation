using Charity_Kousar_Donation.Data;
using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Tests;

public class SettingsSeedingTests
{
    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task A_fresh_database_gets_every_setting_in_the_catalog()
    {
        await using var db = NewDb();

        var added = await DbSeeder.SeedSettingsAsync(db);

        Assert.Equal(SettingsCatalog.All.Count, added);
        Assert.Equal(
            SettingsCatalog.All.Select(d => d.Key).OrderBy(k => k),
            db.SiteSettings.Select(s => s.Key).OrderBy(k => k));
    }

    [Fact]
    public async Task Seeding_twice_adds_nothing_the_second_time()
    {
        await using var db = NewDb();
        await DbSeeder.SeedSettingsAsync(db);

        var added = await DbSeeder.SeedSettingsAsync(db);

        Assert.Equal(0, added);
        Assert.Equal(SettingsCatalog.All.Count, db.SiteSettings.Count());
    }

    [Fact]
    public async Task An_upgrade_adds_missing_keys_without_touching_edited_values()
    {
        await using var db = NewDb();
        // An older site: one setting exists and the admin has customised it.
        db.SiteSettings.Add(new SiteSetting
        {
            Key = "site.name.fa", Value = "خیریه من", Group = "site",
            LabelFa = "نام سایت", LabelEn = "Site name"
        });
        await db.SaveChangesAsync();

        var added = await DbSeeder.SeedSettingsAsync(db);

        Assert.Equal(SettingsCatalog.All.Count - 1, added);
        Assert.Equal("خیریه من", db.SiteSettings.Single(s => s.Key == "site.name.fa").Value);
    }
}
