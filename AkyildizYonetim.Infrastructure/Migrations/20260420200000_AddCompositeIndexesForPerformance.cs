using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexesForPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Composite index: TenantId + Status + PeriodYear
            // Kullanım: Kiracı portalı, genel borç listesi filtreleme
            migrationBuilder.CreateIndex(
                name: "IX_UtilityDebts_TenantId_Status_PeriodYear",
                table: "UtilityDebts",
                columns: new[] { "TenantId", "Status", "PeriodYear" });

            // Composite index: OwnerId + Status + PeriodYear
            // Kullanım: Mal sahibi portalı borç listesi
            migrationBuilder.CreateIndex(
                name: "IX_UtilityDebts_OwnerId_Status_PeriodYear",
                table: "UtilityDebts",
                columns: new[] { "OwnerId", "Status", "PeriodYear" });

            // Composite index: FlatId + PeriodYear + PeriodMonth + Status
            // Kullanım: Daire bazlı aidat/borç sorgulama
            migrationBuilder.CreateIndex(
                name: "IX_UtilityDebts_FlatId_Period_Status",
                table: "UtilityDebts",
                columns: new[] { "FlatId", "PeriodYear", "PeriodMonth", "Status" });

            // Composite index: Status + DueDate
            // Kullanım: Gecikmiş borç sorguları (Dashboard, Overdue sayfası)
            migrationBuilder.CreateIndex(
                name: "IX_UtilityDebts_Status_DueDate",
                table: "UtilityDebts",
                columns: new[] { "Status", "DueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UtilityDebts_TenantId_Status_PeriodYear",
                table: "UtilityDebts");

            migrationBuilder.DropIndex(
                name: "IX_UtilityDebts_OwnerId_Status_PeriodYear",
                table: "UtilityDebts");

            migrationBuilder.DropIndex(
                name: "IX_UtilityDebts_FlatId_Period_Status",
                table: "UtilityDebts");

            migrationBuilder.DropIndex(
                name: "IX_UtilityDebts_Status_DueDate",
                table: "UtilityDebts");
        }
    }
}
