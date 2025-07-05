using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkyildizYonetim.Tests.Integration;

public class TestBase
{
    public static WebApplicationFactory<Program> CreateApplication()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // In-memory database kullan
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Logging'i devre dışı bırak
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    });
                });

                builder.UseEnvironment("Test");
            });
    }

    public static async Task<ApplicationDbContext> CreateDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    public static async Task SeedTestDataAsync(ApplicationDbContext context)
    {
        // Test verilerini ekle
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Tenant",
            Email = "test@example.com",
            PhoneNumber = "5551234567",
            ApartmentNumber = "A1",
            LeaseStartDate = DateTime.UtcNow.AddDays(-30),
            MonthlyRent = 1000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var owner = new Owner
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Owner",
            Email = "owner@example.com",
            PhoneNumber = "5551234568",
            ApartmentNumber = "A1",
            MonthlyDues = 150,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var flat = new Flat
        {
            Id = Guid.NewGuid(),
            Number = "A1",
            Floor = 1,
            OwnerId = owner.Id,
            TenantId = tenant.Id,
            ApartmentNumber = "A1",
            RoomCount = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var debt = new UtilityDebt
        {
            Id = Guid.NewGuid(),
            FlatId = flat.Id,
            TenantId = tenant.Id,
            Type = DebtType.Electricity,
            PeriodYear = 2024,
            PeriodMonth = 1,
            Amount = 500,
            RemainingAmount = 500,
            Status = DebtStatus.Unpaid,
            DueDate = DateTime.UtcNow.AddDays(-10),
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var advanceAccount = new AdvanceAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Balance = 1000,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        context.Tenants.Add(tenant);
        context.Owners.Add(owner);
        context.Flats.Add(flat);
        context.UtilityDebts.Add(debt);
        context.AdvanceAccounts.Add(advanceAccount);

        await context.SaveChangesAsync();
    }
} 