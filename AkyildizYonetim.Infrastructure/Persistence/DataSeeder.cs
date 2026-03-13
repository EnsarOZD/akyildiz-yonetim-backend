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
            var tenantA = await GetOrCreateTenant(context, "TENANT A LIMITED", "Services", "10000000001", "Lead A", "contact.a@example.test", "0555 111 22 33");
            var tenantB = await GetOrCreateTenant(context, "TENANT B SERVICES", "Consultancy", "10000000002", "Lead B", "contact.b@example.test", "0555 221 22 44");
            var tenantC = await GetOrCreateTenant(context, "TENANT C FOOD", "Food", "10000000003", "Lead C", "contact.c@example.test", "0555 331 44 55");
            var tenantD = await GetOrCreateTenant(context, "TENANT D HEALTH", "Health", "10000000004", "Lead D", "contact.d@example.test", "0555 441 55 66");
            var tenantE = await GetOrCreateTenant(context, "TENANT E MEDICAL", "Health", "10000000005", "Lead E", "contact.e@example.test", "0555 551 66 77");
            var tenantF = await GetOrCreateTenant(context, "TENANT F TOURISM", "Tourism", "10000000006", "Lead F", "contact.f@example.test", "0555 661 77 88");

            // 3. Daireler
            if (!await context.Flats.AnyAsync())
            {
                var flats = new List<Flat>
                {
                    CreateFlat("-4", -4, owner.Id, tenantA.Id),
                    CreateFlat("-1", -1, owner.Id, tenantB.Id),
                    CreateFlat("0-A", 0, owner.Id, tenantC.Id),
                    CreateFlat("0-B", 0, owner.Id, null),
                    CreateFlat("1", 1, owner.Id, tenantB.Id),
                    CreateFlat("2", 2, owner.Id, tenantD.Id),
                    CreateFlat("3-B", 3, owner.Id, tenantE.Id),
                    CreateFlat("3-A", 3, owner.Id, null),
                    CreateFlat("4", 4, owner.Id, null),
                    CreateFlat("5", 5, owner.Id, tenantB.Id),
                    CreateFlat("6", 6, owner.Id, tenantE.Id),
                    CreateFlat("7", 7, owner.Id, tenantF.Id)
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
