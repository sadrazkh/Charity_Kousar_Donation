using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity_Kousar_Donation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedCampaignFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeaturedBannerEn",
                table: "Campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturedBannerFa",
                table: "Campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeaturedTimerEndsAt",
                table: "Campaigns",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeaturedBannerEn",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "FeaturedBannerFa",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "FeaturedTimerEndsAt",
                table: "Campaigns");
        }
    }
}
