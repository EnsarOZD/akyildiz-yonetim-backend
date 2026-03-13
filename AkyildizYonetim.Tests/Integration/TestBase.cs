using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using AkyildizYonetim.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AkyildizYonetim.Tests.Integration;

public class TestBase
{
    public static WebApplicationFactory<Program> CreateApplication()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "JwtSettings:SecretKey", "ThisIsAVeryLongDummySecretKeyForTestingPurposes" },
                        { "JwtSettings:Issuer", "TestIssuer" },
                        { "JwtSettings:Audience", "TestAudience" },
                        { "JwtSettings:ExpirationInMinutes", "60" }
                    });
                });

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

                    // Mock ICurrentUserService
                    var userServiceDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICurrentUserService));
                    if (userServiceDescriptor != null) services.Remove(userServiceDescriptor);

                    var mockUserService = new Mock<ICurrentUserService>();
                    mockUserService.Setup(s => s.IsAdmin).Returns(true);
                    mockUserService.Setup(s => s.IsManager).Returns(true);
                    mockUserService.Setup(s => s.IsDataEntry).Returns(true);
                    services.AddScoped(_ => mockUserService.Object);

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

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(s => s.IsAdmin).Returns(true);
        mockUserService.Setup(s => s.IsManager).Returns(true);
        mockUserService.Setup(s => s.IsDataEntry).Returns(true);

        var context = new ApplicationDbContext(options, mockUserService.Object);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    public static async Task SeedTestDataAsync(ApplicationDbContext context)
    {
        // Test verilerini ekle
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = "Test Company",
            BusinessType = "Ticaret",
            ContactPersonName = "Test Contact",
            ContactPersonPhone = "5551234567",
            ContactPersonEmail = "test@example.com",
            MonthlyAidat = 150,
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
            FloorNumber = 1,
            OwnerId = owner.Id,
            TenantId = tenant.Id,
            ApartmentNumber = "A1",
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