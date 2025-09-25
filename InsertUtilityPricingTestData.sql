-- UtilityPricingConfigurations Test Verileri
-- Bu script UtilityPricingConfigurations tablosuna test verileri ekler

-- Elektrik Fiyatlandırması (MeterType = 0)
INSERT INTO UtilityPricingConfigurations 
(Id, MeterType, Year, Month, UnitPrice, VatRate, BtvRate, EffectiveDate, Description, IsActive, IsDeleted, CreatedAt)
VALUES 
(NEWID(), 0, 2025, 9, 2.75, 20.00, 5.00, '2025-09-01', '2025 Eylül ayı elektrik fiyatlandırması', 1, 0, GETUTCDATE());

INSERT INTO UtilityPricingConfigurations 
(Id, MeterType, Year, Month, UnitPrice, VatRate, BtvRate, EffectiveDate, Description, IsActive, IsDeleted, CreatedAt)
VALUES 
(NEWID(), 0, 2025, 10, 2.80, 20.00, 5.00, '2025-10-01', '2025 Ekim ayı elektrik fiyatlandırması', 1, 0, GETUTCDATE());

INSERT INTO UtilityPricingConfigurations 
(Id, MeterType, Year, Month, UnitPrice, VatRate, BtvRate, EffectiveDate, Description, IsActive, IsDeleted, CreatedAt)
VALUES 
(NEWID(), 0, 0, 0, 2.50, 18.00, 3.00, '2025-01-01', 'Genel 2025 yılı elektrik fiyatlandırması', 1, 0, GETUTCDATE());

-- Su Fiyatlandırması (MeterType = 1)
INSERT INTO UtilityPricingConfigurations 
(Id, MeterType, Year, Month, UnitPrice, VatRate, BtvRate, EffectiveDate, Description, IsActive, IsDeleted, CreatedAt)
VALUES 
(NEWID(), 1, 2025, 9, 1.50, 10.00, 0.00, '2025-09-01', '2025 Eylül ayı su fiyatlandırması', 1, 0, GETUTCDATE());

INSERT INTO UtilityPricingConfigurations 
(Id, MeterType, Year, Month, UnitPrice, VatRate, BtvRate, EffectiveDate, Description, IsActive, IsDeleted, CreatedAt)
VALUES 
(NEWID(), 1, 0, 0, 1.20, 8.00, 0.00, '2025-01-01', 'Genel 2025 yılı su fiyatlandırması', 1, 0, GETUTCDATE());

-- Kontrol sorgusu
SELECT 
    Id,
    MeterType,
    Year,
    Month,
    UnitPrice,
    VatRate,
    BtvRate,
    EffectiveDate,
    Description,
    IsActive
FROM UtilityPricingConfigurations 
WHERE IsDeleted = 0
ORDER BY MeterType, Year, Month;