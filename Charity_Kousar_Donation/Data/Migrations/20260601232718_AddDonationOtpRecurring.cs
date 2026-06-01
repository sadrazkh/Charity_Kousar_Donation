using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity_Kousar_Donation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationOtpRecurring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Donations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "Donations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiresAt",
                table: "Donations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OtpVerified",
                table: "Donations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "OtpExpiresAt",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "OtpVerified",
                table: "Donations");
        }
    }
}
