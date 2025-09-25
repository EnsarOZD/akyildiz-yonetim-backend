using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFlatCategoryBusinessTypeRoomCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flats_Category",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "RoomCount",
                table: "Flats");

            migrationBuilder.RenameColumn(
                name: "TaxNumber",
                table: "Tenants",
                newName: "IdentityNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_TaxNumber",
                table: "Tenants",
                newName: "IX_Tenants_IdentityNumber");

            migrationBuilder.AddColumn<string>(
                name: "CompanyType",
                table: "Tenants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyType",
                table: "Tenants");

            migrationBuilder.RenameColumn(
                name: "IdentityNumber",
                table: "Tenants",
                newName: "TaxNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_IdentityNumber",
                table: "Tenants",
                newName: "IX_Tenants_TaxNumber");

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Flats",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Flats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Normal");

            migrationBuilder.AddColumn<int>(
                name: "RoomCount",
                table: "Flats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Flats_Category",
                table: "Flats",
                column: "Category");
        }
    }
}
