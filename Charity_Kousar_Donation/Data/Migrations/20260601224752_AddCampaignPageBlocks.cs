using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity_Kousar_Donation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignPageBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PageBlocksJson",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageBlocksJson",
                table: "Campaigns");
        }
    }
}
