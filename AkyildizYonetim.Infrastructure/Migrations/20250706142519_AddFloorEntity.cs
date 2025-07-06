using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFloorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_ApartmentNumber",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Email",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_PhoneNumber",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Flats_ApartmentNumber",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "ApartmentNumber",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LeaseStartDate",
                table: "Tenants");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Tenants",
                newName: "TaxNumber");

            migrationBuilder.RenameColumn(
                name: "MonthlyRent",
                table: "Tenants",
                newName: "MonthlyAidat");

            migrationBuilder.RenameColumn(
                name: "LeaseEndDate",
                table: "Tenants",
                newName: "ContractStartDate");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Tenants",
                newName: "ContactPersonName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Tenants",
                newName: "BusinessType");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Tenants",
                newName: "ContactPersonEmail");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Tenants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonPhone",
                table: "Tenants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "Tenants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ElectricityRate",
                table: "Tenants",
                type: "decimal(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WaterRate",
                table: "Tenants",
                type: "decimal(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "ShareCount",
                table: "Flats",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Flats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Flats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Normal",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ApartmentNumber",
                table: "Flats",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Flats",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Flats",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsOccupied",
                table: "Flats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRent",
                table: "Flats",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitArea",
                table: "Flats",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnitNumber",
                table: "Flats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CompanyName",
                table: "Tenants",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ContactPersonEmail",
                table: "Tenants",
                column: "ContactPersonEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ContactPersonPhone",
                table: "Tenants",
                column: "ContactPersonPhone");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_TaxNumber",
                table: "Tenants",
                column: "TaxNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flats_Category",
                table: "Flats",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Flats_Floor",
                table: "Flats",
                column: "Floor");

            migrationBuilder.CreateIndex(
                name: "IX_Flats_IsOccupied",
                table: "Flats",
                column: "IsOccupied");

            migrationBuilder.CreateIndex(
                name: "IX_Flats_Number",
                table: "Flats",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_Flats_UnitNumber",
                table: "Flats",
                column: "UnitNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_CompanyName",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_ContactPersonEmail",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_ContactPersonPhone",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_TaxNumber",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Flats_Category",
                table: "Flats");

            migrationBuilder.DropIndex(
                name: "IX_Flats_Floor",
                table: "Flats");

            migrationBuilder.DropIndex(
                name: "IX_Flats_IsOccupied",
                table: "Flats");

            migrationBuilder.DropIndex(
                name: "IX_Flats_Number",
                table: "Flats");

            migrationBuilder.DropIndex(
                name: "IX_Flats_UnitNumber",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ContactPersonPhone",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ElectricityRate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "WaterRate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "IsOccupied",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "MonthlyRent",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "UnitArea",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "UnitNumber",
                table: "Flats");

            migrationBuilder.RenameColumn(
                name: "TaxNumber",
                table: "Tenants",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "MonthlyAidat",
                table: "Tenants",
                newName: "MonthlyRent");

            migrationBuilder.RenameColumn(
                name: "ContractStartDate",
                table: "Tenants",
                newName: "LeaseEndDate");

            migrationBuilder.RenameColumn(
                name: "ContactPersonName",
                table: "Tenants",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "ContactPersonEmail",
                table: "Tenants",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "BusinessType",
                table: "Tenants",
                newName: "FirstName");

            migrationBuilder.AddColumn<string>(
                name: "ApartmentNumber",
                table: "Tenants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaseStartDate",
                table: "Tenants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "ShareCount",
                table: "Flats",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Flats",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Flats",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Normal");

            migrationBuilder.AlterColumn<string>(
                name: "ApartmentNumber",
                table: "Flats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ApartmentNumber",
                table: "Tenants",
                column: "ApartmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Email",
                table: "Tenants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PhoneNumber",
                table: "Tenants",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Flats_ApartmentNumber",
                table: "Flats",
                column: "ApartmentNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }
    }
}
