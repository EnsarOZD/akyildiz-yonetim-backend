using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FlatUnitModelV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // Eski Floor indexini kaldır (kolon bu migration'da kaldırılıyor)
    migrationBuilder.DropIndex(
        name: "IX_Flats_Floor",
        table: "Flats");

    // Eski Floor kolonunu kaldır
    migrationBuilder.DropColumn(
        name: "Floor",
        table: "Flats");

    // OwnerId nullable
    migrationBuilder.AlterColumn<Guid>(
        name: "OwnerId",
        table: "Flats",
        type: "uniqueidentifier",
        nullable: true,
        oldClrType: typeof(Guid),
        oldType: "uniqueidentifier");

    // Yeni alanlar
    migrationBuilder.AddColumn<string>(
        name: "Code",
        table: "Flats",
        type: "nvarchar(32)",
        maxLength: 32,
        nullable: false,
        defaultValue: "");

    migrationBuilder.AddColumn<int>(
        name: "FloorNumber",
        table: "Flats",
        type: "int",
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "GroupKey",
        table: "Flats",
        type: "nvarchar(8)",
        maxLength: 8,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "GroupStrategy",
        table: "Flats",
        type: "nvarchar(24)",
        maxLength: 24,
        nullable: false,
        defaultValue: "None");

    migrationBuilder.AddColumn<string>(
        name: "Section",
        table: "Flats",
        type: "nvarchar(4)",
        maxLength: 4,
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "Type",
        table: "Flats",
        type: "nvarchar(16)",
        maxLength: 16,
        nullable: false,
        defaultValue: "Floor");

    // =======================
    // CODE NORMALİZASYON & TEKİLLEŞTİRME
    // =======================

    // 1) Trimle (baş/son boşlukları temizle)
    migrationBuilder.Sql(@"
UPDATE Flats
SET Code = LTRIM(RTRIM(Code))
WHERE Code IS NOT NULL;
");

    // 2) Aktif satırlarda boş/NULL Code varsa UnitNumber ile doldur,
    //    o da yoksa FloorNumber (varsa) ile doldur
    migrationBuilder.Sql(@"
UPDATE F
SET Code = 
    CASE 
        WHEN NULLIF(LTRIM(RTRIM(F.Code)), '') IS NOT NULL THEN LTRIM(RTRIM(F.Code))
        WHEN NULLIF(LTRIM(RTRIM(F.UnitNumber)), '') IS NOT NULL THEN LTRIM(RTRIM(F.UnitNumber))
        WHEN F.FloorNumber IS NOT NULL THEN CAST(F.FloorNumber AS nvarchar(32))
        ELSE 'CODE-' + CONVERT(nvarchar(36), F.Id)
    END
FROM Flats F
WHERE F.IsDeleted = 0;
");

    // 3) Aktif satırlarda Code çakışmalarını çöz (aynı Code için 2.,3.,... kayıtlara -2/-3 ekle)
    migrationBuilder.Sql(@"
;WITH d AS
(
    SELECT Id, Code,
           rn = ROW_NUMBER() OVER (PARTITION BY Code ORDER BY CreatedAt, Id)
    FROM Flats
    WHERE IsDeleted = 0
)
UPDATE F
SET Code = CASE 
              WHEN d.rn = 1 THEN F.Code
              ELSE LEFT(F.Code, 30) + '-' + CAST(d.rn AS nvarchar(2))
           END
FROM Flats F
JOIN d ON d.Id = F.Id
WHERE d.rn > 1;
");

    // =======================
    // İndeksler
    // =======================

    migrationBuilder.CreateIndex(
        name: "IX_Flats_FloorNumber",
        table: "Flats",
        column: "FloorNumber");

    migrationBuilder.CreateIndex(
        name: "IX_Flats_GroupKey",
        table: "Flats",
        column: "GroupKey");

    migrationBuilder.CreateIndex(
        name: "IX_Flats_GroupStrategy",
        table: "Flats",
        column: "GroupStrategy");

    migrationBuilder.CreateIndex(
        name: "IX_Flats_Type",
        table: "Flats",
        column: "Type");

    // Aktif satırlar için Code benzersiz
    migrationBuilder.CreateIndex(
        name: "UX_Flats_Code_ActiveOnly",
        table: "Flats",
        column: "Code",
        unique: true,
        filter: "[IsDeleted] = 0");
}


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Yeni indexleri kaldır
            migrationBuilder.DropIndex(
                name: "IX_Flats_FloorNumber",
                table: "Flats");

            migrationBuilder.DropIndex(
                name: "IX_Flats_GroupKey",
                table: "Flats");

            migrationBuilder.DropIndex(
                name: "IX_Flats_GroupStrategy",
                table: "Flats");

            migrationBuilder.DropIndex(
                name: "IX_Flats_Type",
                table: "Flats");

            migrationBuilder.DropIndex(
                name: "UX_Flats_Code_ActiveOnly",
                table: "Flats");

            // Yeni kolonları kaldır
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "FloorNumber",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "GroupKey",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "GroupStrategy",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "Flats");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Flats");

            // OwnerId geri zorunlu
            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Flats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            // Eski indexi geri getir (Floor kolonu hala var)
            migrationBuilder.CreateIndex(
                name: "IX_Flats_Floor",
                table: "Flats",
                column: "Floor");
        }
    }
}
