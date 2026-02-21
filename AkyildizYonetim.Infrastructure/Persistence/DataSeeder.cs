using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AkyildizYonetim.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("FULL SEEDING STARTED");

            // 1. Mal Sahibi (Varsayılan)
            var owner = await context.Owners.FirstOrDefaultAsync(o => o.Email == "yonetim@akyildiz.com");
            if (owner == null)
            {
                owner = new Owner
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Akyıldız",
                    LastName = "Yönetim",
                    PhoneNumber = "0500 000 00 00",
                    Email = "yonetim@akyildiz.com",
                    ApartmentNumber = "Yönetim",
                    MonthlyDues = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Owners.Add(owner);
                await context.SaveChangesAsync();
                logger.LogInformation("Owner added.");
            }

            // 2. Kiracılar
            var tenantYigit = await GetOrCreateTenant(context, "YİĞİT HANDEMİR", "Şahıs", "10000000001", "Yiğit HANDEMİR", "yigithamdemir8@gmail.com", "5398129854");
            var tenantIHC = await GetOrCreateTenant(context, "İHC DANIŞMANLIK HİZMETLERİ SAĞLIK TURİZMİ VE TİCARET LİMİTED ŞİRKETİ", "Sağlık Turizmi", "10000000002", "Deniz SÖNMEZ", "deniz.sonmez@estevienclinic.com", "5379543043");
            var tenantGalore = await GetOrCreateTenant(context, "GALORE GIDA TİCARET LİMİTED ŞİRKETİ", "Gıda", "10000000003", "Cemal Kıkoglu", "cemalkiroglu@gmail.com", "5428476161");
            var tenantGeosante = await GetOrCreateTenant(context, "GEOSANTE SAĞLIK ANONİM ŞİRKETİ", "Sağlık", "10000000004", "Sedef", "info@alphacpoliklinik.com.tr", "5384297107");
            var tenantSapphire = await GetOrCreateTenant(context, "SAPPHIRE ÖZEL SAĞLIK HİZMETLERİ TURİZM VE TİCARET A.Ş", "Sağlık", "10000000005", "Emine SEZER", "dymedhairclinic@gmail.com", "5541164833");
            var tenantResult = await GetOrCreateTenant(context, "RESULT TURİZM GAYRİMENKUL DANIŞMANLIK LİMİTED ŞİRKETİ", "Turizm/Gayrimenkul", "10000000006", "İrem BEKDAŞ", "resultturizm@gmail.com", "5333706086");

            // 3. Daireler
            if (!await context.Flats.AnyAsync())
            {
                var flats = new List<Flat>
                {
                    CreateFlat("-4", -4, owner.Id, tenantYigit.Id),
                    CreateFlat("-1", -1, owner.Id, tenantIHC.Id),
                    CreateFlat("0-A", 0, owner.Id, tenantGalore.Id),
                    CreateFlat("0-B", 0, owner.Id, null),
                    CreateFlat("1", 1, owner.Id, tenantIHC.Id),
                    CreateFlat("2", 2, owner.Id, tenantGeosante.Id),
                    CreateFlat("3-B", 3, owner.Id, tenantSapphire.Id),
                    CreateFlat("3-A", 3, owner.Id, null),
                    CreateFlat("4", 4, owner.Id, null),
                    CreateFlat("5", 5, owner.Id, tenantIHC.Id),
                    CreateFlat("6", 6, owner.Id, tenantSapphire.Id),
                    CreateFlat("7", 7, owner.Id, tenantResult.Id)
                };

                context.Flats.AddRange(flats);
                await context.SaveChangesAsync();
                logger.LogInformation("Flats added.");
            }

            // Admin User Check
            if (!await context.Users.AnyAsync(u => u.Email == "admin@email.com"))
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@email.com",
                    PasswordHash = "rJaJ4ickJwheNbnT4+i+2IyzQ0gotDuG/AWWytTG4nA=", // admin1234
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
                logger.LogInformation("Admin user added.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SEEDING FAILED: {Message}", ex.Message);
            throw;
        }
    }

    private static async Task<Tenant> GetOrCreateTenant(ApplicationDbContext context, string name, string type, string idNo, string contactName, string contactEmail, string phone)
    {
        // Önce Kimlik Numarası (IdentityNumber) ile kontrol et, çünkü bu değer Benzersiz (Unique) bir indekse sahip.
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.IdentityNumber == idNo);
        
        // Eğer kimlik numarası ile bulunamadıysa isimle de bakabiliriz (opsiyonel)
        if (tenant == null)
        {
            tenant = await context.Tenants.FirstOrDefaultAsync(t => t.CompanyName == name);
        }

        if (tenant == null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                CompanyName = name,
                BusinessType = type,
                IdentityNumber = idNo,
                ContactPersonName = contactName,
                ContactPersonEmail = contactEmail,
                ContactPersonPhone = phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
            // logger parametresi yok ama burada ekleyemiyoruz imza değişmesin diye
        }
        return tenant;
    }

    private static Flat CreateFlat(string code, int floor, Guid ownerId, Guid? tenantId)
    {
        return new Flat
        {
            Id = Guid.NewGuid(),
            Code = code,
            Number = code,
            UnitNumber = code,
            ApartmentNumber = code,
            FloorNumber = floor,
            OwnerId = ownerId,
            TenantId = tenantId,
            IsOccupied = tenantId.HasValue,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UnitArea = 100,
            Description = $"{floor}. Kat - {code}"
        };
    }
}
