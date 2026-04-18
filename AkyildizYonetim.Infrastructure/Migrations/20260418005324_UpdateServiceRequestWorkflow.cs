using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceRequestWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedPersonnelId",
                table: "ServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNote",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_AssignedPersonnelId",
                table: "ServiceRequests",
                column: "AssignedPersonnelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Users_AssignedPersonnelId",
                table: "ServiceRequests",
                column: "AssignedPersonnelId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Users_AssignedPersonnelId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_AssignedPersonnelId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedPersonnelId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ResolutionNote",
                table: "ServiceRequests");
        }
    }
}
