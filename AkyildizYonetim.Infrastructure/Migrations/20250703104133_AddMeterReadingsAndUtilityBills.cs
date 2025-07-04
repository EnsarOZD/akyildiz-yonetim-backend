using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMeterReadingsAndUtilityBills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "AidatDefinitions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AidatDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AidatDefinitions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeterReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PeriodYear = table.Column<int>(type: "int", nullable: false),
                    PeriodMonth = table.Column<int>(type: "int", nullable: false),
                    ReadingValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Consumption = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReadingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterReadings_Flats_FlatId",
                        column: x => x.FlatId,
                        principalTable: "Flats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UtilityBills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PeriodYear = table.Column<int>(type: "int", nullable: false),
                    PeriodMonth = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BillDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityBills", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AidatDefinitions_IsActive",
                table: "AidatDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AidatDefinitions_TenantId",
                table: "AidatDefinitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AidatDefinitions_Unit",
                table: "AidatDefinitions",
                column: "Unit");

            migrationBuilder.CreateIndex(
                name: "IX_AidatDefinitions_Year",
                table: "AidatDefinitions",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_FlatId",
                table: "MeterReadings",
                column: "FlatId");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_ReadingDate",
                table: "MeterReadings",
                column: "ReadingDate");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_Type",
                table: "MeterReadings",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityBills_BillDate",
                table: "UtilityBills",
                column: "BillDate");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityBills_PeriodMonth",
                table: "UtilityBills",
                column: "PeriodMonth");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityBills_PeriodYear",
                table: "UtilityBills",
                column: "PeriodYear");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityBills_Type",
                table: "UtilityBills",
                column: "Type");

            // Foreign key constraint already exists, skipping
            // migrationBuilder.AddForeignKey(
            //     name: "FK_AidatDefinitions_Tenants_TenantId",
            //     table: "AidatDefinitions",
            //     column: "TenantId",
            //     principalTable: "Tenants",
            //     principalColumn: "Id",
            //     onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AidatDefinitions_Tenants_TenantId",
                table: "AidatDefinitions");

            migrationBuilder.DropTable(
                name: "MeterReadings");

            migrationBuilder.DropTable(
                name: "UtilityBills");

            migrationBuilder.DropIndex(
                name: "IX_AidatDefinitions_IsActive",
                table: "AidatDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_AidatDefinitions_TenantId",
                table: "AidatDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_AidatDefinitions_Unit",
                table: "AidatDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_AidatDefinitions_Year",
                table: "AidatDefinitions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AidatDefinitions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AidatDefinitions");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "AidatDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
