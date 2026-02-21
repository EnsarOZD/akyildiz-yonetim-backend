IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250702205218_Initial', N'9.0.6');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250703104133_AddMeterReadingsAndUtilityBills', N'9.0.6');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250705085838_AddPaymentDebtTable', N'9.0.6');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250705090833_AddAuditLogTable', N'9.0.6');

ALTER TABLE [Flats] ADD [Category] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Flats] ADD [ShareCount] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250706110811_AddFlatCategoryAndShareCount', N'9.0.6');

ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Tenants_TenantId];

DROP INDEX [IX_Tenants_ApartmentNumber] ON [Tenants];

DROP INDEX [IX_Tenants_Email] ON [Tenants];

DROP INDEX [IX_Tenants_PhoneNumber] ON [Tenants];

DROP INDEX [IX_Flats_ApartmentNumber] ON [Flats];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'ApartmentNumber');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Tenants] DROP COLUMN [ApartmentNumber];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'LeaseStartDate');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Tenants] DROP COLUMN [LeaseStartDate];

EXEC sp_rename N'[Tenants].[PhoneNumber]', N'TaxNumber', 'COLUMN';

EXEC sp_rename N'[Tenants].[MonthlyRent]', N'MonthlyAidat', 'COLUMN';

EXEC sp_rename N'[Tenants].[LeaseEndDate]', N'ContractStartDate', 'COLUMN';

EXEC sp_rename N'[Tenants].[LastName]', N'ContactPersonName', 'COLUMN';

EXEC sp_rename N'[Tenants].[FirstName]', N'BusinessType', 'COLUMN';

EXEC sp_rename N'[Tenants].[Email]', N'ContactPersonEmail', 'COLUMN';

ALTER TABLE [Tenants] ADD [CompanyName] nvarchar(200) NOT NULL DEFAULT N'';

ALTER TABLE [Tenants] ADD [ContactPersonPhone] nvarchar(20) NOT NULL DEFAULT N'';

ALTER TABLE [Tenants] ADD [ContractEndDate] datetime2 NULL;

ALTER TABLE [Tenants] ADD [ElectricityRate] decimal(10,4) NOT NULL DEFAULT 0.0;

ALTER TABLE [Tenants] ADD [WaterRate] decimal(10,4) NOT NULL DEFAULT 0.0;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'ShareCount');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Flats] ADD DEFAULT 1 FOR [ShareCount];

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'Number');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Flats] ALTER COLUMN [Number] nvarchar(50) NOT NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'Category');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Flats] ALTER COLUMN [Category] nvarchar(50) NOT NULL;
ALTER TABLE [Flats] ADD DEFAULT N'Normal' FOR [Category];

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'ApartmentNumber');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Flats] ALTER COLUMN [ApartmentNumber] nvarchar(max) NOT NULL;

ALTER TABLE [Flats] ADD [BusinessType] nvarchar(100) NOT NULL DEFAULT N'';

ALTER TABLE [Flats] ADD [Description] nvarchar(500) NOT NULL DEFAULT N'';

ALTER TABLE [Flats] ADD [IsOccupied] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Flats] ADD [MonthlyRent] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Flats] ADD [UnitArea] decimal(10,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Flats] ADD [UnitNumber] nvarchar(50) NOT NULL DEFAULT N'';

CREATE INDEX [IX_Tenants_CompanyName] ON [Tenants] ([CompanyName]);

CREATE INDEX [IX_Tenants_ContactPersonEmail] ON [Tenants] ([ContactPersonEmail]);

CREATE INDEX [IX_Tenants_ContactPersonPhone] ON [Tenants] ([ContactPersonPhone]);

CREATE UNIQUE INDEX [IX_Tenants_TaxNumber] ON [Tenants] ([TaxNumber]);

CREATE INDEX [IX_Flats_Category] ON [Flats] ([Category]);

CREATE INDEX [IX_Flats_Floor] ON [Flats] ([Floor]);

CREATE INDEX [IX_Flats_IsOccupied] ON [Flats] ([IsOccupied]);

CREATE INDEX [IX_Flats_Number] ON [Flats] ([Number]);

CREATE INDEX [IX_Flats_UnitNumber] ON [Flats] ([UnitNumber]);

ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250706144413_UpdateTenantModel', N'9.0.6');

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'ElectricityRate');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Tenants] DROP COLUMN [ElectricityRate];

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'WaterRate');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Tenants] DROP COLUMN [WaterRate];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250724111202_RemoveElectricityAndWaterRatesFromTenants', N'9.0.6');

DROP INDEX [IX_Flats_Category] ON [Flats];

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'BusinessType');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Flats] DROP COLUMN [BusinessType];

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'Category');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Flats] DROP COLUMN [Category];

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'RoomCount');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [Flats] DROP COLUMN [RoomCount];

EXEC sp_rename N'[Tenants].[TaxNumber]', N'IdentityNumber', 'COLUMN';

EXEC sp_rename N'[Tenants].[IX_Tenants_TaxNumber]', N'IX_Tenants_IdentityNumber', 'INDEX';

ALTER TABLE [Tenants] ADD [CompanyType] nvarchar(20) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250728200603_RemoveFlatCategoryBusinessTypeRoomCount', N'9.0.6');

ALTER TABLE [PaymentDebts] DROP CONSTRAINT [FK_PaymentDebts_Payments_PaymentId];

ALTER TABLE [PaymentDebts] DROP CONSTRAINT [FK_PaymentDebts_UtilityDebts_DebtId];

ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Owners_OwnerId];

ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Tenants_TenantId];

DROP INDEX [IX_Users_Email] ON [Users];

DROP INDEX [IX_Tenants_IdentityNumber] ON [Tenants];

DROP INDEX [IX_AdvanceAccounts_TenantId] ON [AdvanceAccounts];

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Users] ALTER COLUMN [Email] varchar(255) NOT NULL;

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'IdentityNumber');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Tenants] ALTER COLUMN [IdentityNumber] varchar(20) NOT NULL;

DROP INDEX [IX_Tenants_ContactPersonPhone] ON [Tenants];
DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'ContactPersonPhone');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Tenants] ALTER COLUMN [ContactPersonPhone] varchar(20) NOT NULL;
CREATE INDEX [IX_Tenants_ContactPersonPhone] ON [Tenants] ([ContactPersonPhone]);

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'ReceiptNumber');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Payments] ALTER COLUMN [ReceiptNumber] varchar(100) NULL;

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'Description');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [Payments] ALTER COLUMN [Description] varchar(500) NULL;

ALTER TABLE [PaymentDebts] ADD [TenantId] uniqueidentifier NULL;

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AdvanceAccounts]') AND [c].[name] = N'IsActive');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [AdvanceAccounts] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [AdvanceAccounts] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AdvanceAccounts]') AND [c].[name] = N'Balance');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [AdvanceAccounts] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [AdvanceAccounts] ADD DEFAULT 0.0 FOR [Balance];

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [IsDeleted] = 0;

CREATE UNIQUE INDEX [IX_Tenants_IdentityNumber] ON [Tenants] ([IdentityNumber]) WHERE [IsDeleted] = 0;

CREATE INDEX [IX_Payments_OwnerId_PaymentDate] ON [Payments] ([OwnerId], [PaymentDate]);

CREATE UNIQUE INDEX [IX_Payments_ReceiptNumber] ON [Payments] ([ReceiptNumber]) WHERE [IsDeleted] = 0 AND [ReceiptNumber] IS NOT NULL;

CREATE INDEX [IX_Payments_TenantId_PaymentDate] ON [Payments] ([TenantId], [PaymentDate]);

ALTER TABLE [Payments] ADD CONSTRAINT [CK_Payments_Amount_Positive] CHECK ([Amount] >= 0);

CREATE INDEX [IX_PaymentDebts_TenantId] ON [PaymentDebts] ([TenantId]);

ALTER TABLE [PaymentDebts] ADD CONSTRAINT [CK_PaymentDebts_PaidAmount_Positive] CHECK ([PaidAmount] >= 0);

CREATE UNIQUE INDEX [IX_AdvanceAccounts_TenantId] ON [AdvanceAccounts] ([TenantId]) WHERE [IsDeleted] = 0 AND [IsActive] = 1;

CREATE INDEX [IX_AdvanceAccounts_TenantId_IsActive] ON [AdvanceAccounts] ([TenantId], [IsActive]);

ALTER TABLE [PaymentDebts] ADD CONSTRAINT [FK_PaymentDebts_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [PaymentDebts] ADD CONSTRAINT [FK_PaymentDebts_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]);

ALTER TABLE [PaymentDebts] ADD CONSTRAINT [FK_PaymentDebts_UtilityDebts_DebtId] FOREIGN KEY ([DebtId]) REFERENCES [UtilityDebts] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Owners_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Owners] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250825204017_RelConfigsCleanup_20250825', N'9.0.6');

DROP INDEX [IX_Flats_Floor] ON [Flats];

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'Floor');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [Flats] DROP COLUMN [Floor];

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flats]') AND [c].[name] = N'OwnerId');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Flats] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [Flats] ALTER COLUMN [OwnerId] uniqueidentifier NULL;

ALTER TABLE [Flats] ADD [Code] nvarchar(32) NOT NULL DEFAULT N'';

ALTER TABLE [Flats] ADD [FloorNumber] int NULL;

ALTER TABLE [Flats] ADD [GroupKey] nvarchar(8) NULL;

ALTER TABLE [Flats] ADD [GroupStrategy] nvarchar(24) NOT NULL DEFAULT N'None';

ALTER TABLE [Flats] ADD [Section] nvarchar(4) NULL;

ALTER TABLE [Flats] ADD [Type] nvarchar(16) NOT NULL DEFAULT N'Floor';


UPDATE Flats
SET Code = LTRIM(RTRIM(Code))
WHERE Code IS NOT NULL;



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


CREATE INDEX [IX_Flats_FloorNumber] ON [Flats] ([FloorNumber]);

CREATE INDEX [IX_Flats_GroupKey] ON [Flats] ([GroupKey]);

CREATE INDEX [IX_Flats_GroupStrategy] ON [Flats] ([GroupStrategy]);

CREATE INDEX [IX_Flats_Type] ON [Flats] ([Type]);

CREATE UNIQUE INDEX [UX_Flats_Code_ActiveOnly] ON [Flats] ([Code]) WHERE [IsDeleted] = 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250827153115_FlatUnitModelV2', N'9.0.6');

CREATE TABLE [UtilityPricingConfigurations] (
    [Id] uniqueidentifier NOT NULL,
    [MeterType] int NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [VatRate] decimal(5,2) NOT NULL,
    [BtvRate] decimal(5,2) NOT NULL,
    [EffectiveDate] datetime2 NOT NULL,
    [ExpiryDate] datetime2 NULL,
    [Description] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_UtilityPricingConfigurations] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_UtilityPricing_EffectiveDate] ON [UtilityPricingConfigurations] ([EffectiveDate], [ExpiryDate]);

CREATE INDEX [IX_UtilityPricing_IsActive] ON [UtilityPricingConfigurations] ([IsActive]);

CREATE INDEX [IX_UtilityPricing_Type_Year_Month] ON [UtilityPricingConfigurations] ([MeterType], [Year], [Month]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250924140516_AddUtilityPricingConfiguration', N'9.0.6');

COMMIT;
GO

