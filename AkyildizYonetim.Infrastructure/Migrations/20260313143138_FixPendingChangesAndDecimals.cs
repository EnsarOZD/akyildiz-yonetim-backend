using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChangesAndDecimals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UtilityDebts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "UtilityDebts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserPushSubscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "UserPushSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Tenants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PaymentDebts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "PaymentDebts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Owners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Owners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Flats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Flats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AuditLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AidatDefinitions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "AidatDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AdvanceAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "AdvanceAccounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UtilityDebts");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "UtilityDebts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserPushSubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "UserPushSubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PaymentDebts");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "PaymentDebts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AidatDefinitions");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "AidatDefinitions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AdvanceAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "AdvanceAccounts");
        }
    }
}
