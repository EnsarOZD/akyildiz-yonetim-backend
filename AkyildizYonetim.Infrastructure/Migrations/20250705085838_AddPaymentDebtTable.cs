using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentDebtTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentDebts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebtId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UtilityDebtId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDebts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentDebts_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentDebts_UtilityDebts_DebtId",
                        column: x => x.DebtId,
                        principalTable: "UtilityDebts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentDebts_UtilityDebts_UtilityDebtId",
                        column: x => x.UtilityDebtId,
                        principalTable: "UtilityDebts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDebts_DebtId",
                table: "PaymentDebts",
                column: "DebtId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDebts_PaymentId",
                table: "PaymentDebts",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDebts_UtilityDebtId",
                table: "PaymentDebts",
                column: "UtilityDebtId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentDebts");
        }
    }
}
