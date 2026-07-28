using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity_Kousar_Donation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignFeaturedStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeaturedStyle",
                table: "Campaigns",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeaturedStyle",
                table: "Campaigns");
        }
    }
}
