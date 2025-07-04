using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkyildizYonetim.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIsDeletedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Users tablosuna IsDeleted sütunu ekle (eğer yoksa)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'IsDeleted')
                BEGIN
                    ALTER TABLE [Users] ADD [IsDeleted] bit NOT NULL DEFAULT 0
                END
            ");

            // AidatDefinitions tablosuna IsDeleted sütunu ekle (eğer yoksa)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AidatDefinitions]') AND name = 'IsDeleted')
                BEGIN
                    ALTER TABLE [AidatDefinitions] ADD [IsDeleted] bit NOT NULL DEFAULT 0
                END
            ");

            // AidatDefinitions tablosuna UpdatedAt sütunu ekle (eğer yoksa)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AidatDefinitions]') AND name = 'UpdatedAt')
                BEGIN
                    ALTER TABLE [AidatDefinitions] ADD [UpdatedAt] datetime2 NULL
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sütunları kaldır
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'IsDeleted')
                BEGIN
                    ALTER TABLE [Users] DROP COLUMN [IsDeleted]
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AidatDefinitions]') AND name = 'IsDeleted')
                BEGIN
                    ALTER TABLE [AidatDefinitions] DROP COLUMN [IsDeleted]
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AidatDefinitions]') AND name = 'UpdatedAt')
                BEGIN
                    ALTER TABLE [AidatDefinitions] DROP COLUMN [UpdatedAt]
                END
            ");
        }
    }
} 