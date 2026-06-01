using Charity_Kousar_Donation.Models;
using Microsoft.EntityFrameworkCore;

namespace Charity_Kousar_Donation.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campaign>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.HasIndex(c => c.ShortCode).IsUnique();
        });

        modelBuilder.Entity<Donation>(e =>
        {
            e.HasOne(d => d.Campaign).WithMany(c => c.Donations).HasForeignKey(d => d.CampaignId);
            e.HasIndex(d => d.Authority);
        });

        modelBuilder.Entity<AdminUser>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<SiteSetting>(e =>
        {
            e.HasKey(s => s.Key);
        });
    }
}
