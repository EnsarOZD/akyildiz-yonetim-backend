using AkyildizYonetim.Application;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Infrastructure.Persistence;
using AkyildizYonetim.Infrastructure.Jwt;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AkyildizYonetim.Domain.Entities;
using System.Text.Json.Serialization;
using System.Text;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Akyildiz Yonetim API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// CORS politikasını ekle - Production için spesifik origin'lere izin ver
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://www.akyildizyonetim.com",
                "https://www.akyildizyonetim.com",
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:8080"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .WithExposedHeaders("Content-Disposition", "Content-Length", "Content-Type", "Access-Control-Allow-Origin", "Access-Control-Allow-Methods", "Access-Control-Allow-Headers")
              .SetIsOriginAllowed(origin => 
              {
                  return origin.Contains("akyildizyonetim.com") || 
                         origin.Contains("localhost") || 
                         origin.Contains("127.0.0.1");
              });
    });
    
    // Development için daha geniş CORS politikası
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    
    // Fallback CORS policy for any issues
    options.AddPolicy("Fallback", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JSON ayarları - Enum'ları string olarak serialize et
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Server=localhost;Database=AkyildizYonetimDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false";

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// IApplicationDbContext olarak ApplicationDbContext'i kaydet
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(AkyildizYonetim.Application.Common.Models.Result)));

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining(typeof(AkyildizYonetim.Application.Common.Models.Result));

builder.Services.AddScoped<IEmailSender, AkyildizYonetim.Infrastructure.Email.EmailSender>();

builder.Services.AddControllers();

var app = builder.Build();

// Veritabanını oluştur (EnsureCreated)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        if (builder.Environment.IsProduction())
        {
            context.Database.Migrate(); // Render/Production ortamında migration'ı otomatik uygula
        }

        // Owners, Flats, Tenants seed
        if (!context.Owners.Any())
    {
        var owner1 = new Owner
        {
            Id = Guid.NewGuid(),
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            PhoneNumber = "0532 123 45 67",
            Email = "ahmet.yilmaz@email.com",
            ApartmentNumber = "A1",
            MonthlyDues = 150.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var owner2 = new Owner
        {
            Id = Guid.NewGuid(),
            FirstName = "Fatma",
            LastName = "Demir",
            PhoneNumber = "0533 234 56 78",
            Email = "fatma.demir@email.com",
            ApartmentNumber = "A2",
            MonthlyDues = 150.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var owner3 = new Owner
        {
            Id = Guid.NewGuid(),
            FirstName = "Mehmet",
            LastName = "Kaya",
            PhoneNumber = "0534 345 67 89",
            Email = "mehmet.kaya@email.com",
            ApartmentNumber = "A3",
            MonthlyDues = 150.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Owners.AddRange(owner1, owner2, owner3);
        await context.SaveChangesAsync();
        
        // Daireler oluştur
        var flat1 = new Flat
        {
            Id = Guid.NewGuid(),
            Number = "A1",
            UnitNumber = "A-101",
            Floor = 1,
            UnitArea = 120.50m,
            OwnerId = owner1.Id,
            TenantId = null,
            RoomCount = 3,
            IsActive = true,
            IsOccupied = false,
            Category = "Normal",
            ShareCount = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        var flat2 = new Flat
        {
            Id = Guid.NewGuid(),
            Number = "A2",
            UnitNumber = "A-102",
            Floor = 1,
            UnitArea = 95.30m,
            OwnerId = owner2.Id,
            TenantId = null,
            RoomCount = 2,
            IsActive = true,
            IsOccupied = false,
            Category = "Normal",
            ShareCount = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        var flat3 = new Flat
        {
            Id = Guid.NewGuid(),
            Number = "A3",
            UnitNumber = "A-201",
            Floor = 2,
            UnitArea = 150.75m,
            OwnerId = owner3.Id,
            TenantId = null,
            RoomCount = 4,
            IsActive = true,
            IsOccupied = false,
            Category = "Normal",
            ShareCount = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Flats.AddRange(flat1, flat2, flat3);
        await context.SaveChangesAsync();
        
        // İş Hanı Kiracıları oluştur
        var tenant1 = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = "ABC Ticaret Ltd. Şti.",
            BusinessType = "Ticaret",
            TaxNumber = "1234567890",
            ContactPersonName = "Ali Özkan",
            ContactPersonPhone = "0535 456 78 90",
            ContactPersonEmail = "ali.ozkan@abc.com",
            MonthlyAidat = 500.00m,
            ContractStartDate = DateTime.Now.AddMonths(-6),
            ContractEndDate = DateTime.Now.AddMonths(6),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var tenant2 = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = "XYZ Hizmet A.Ş.",
            BusinessType = "Hizmet",
            TaxNumber = "9876543210",
            ContactPersonName = "Ayşe Çelik",
            ContactPersonPhone = "0536 567 89 01",
            ContactPersonEmail = "ayse.celik@xyz.com",
            MonthlyAidat = 450.00m,
            ContractStartDate = DateTime.Now.AddMonths(-3),
            ContractEndDate = DateTime.Now.AddMonths(9),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var tenant3 = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = "DEF Üretim Ltd. Şti.",
            BusinessType = "Üretim",
            TaxNumber = "5556667778",
            ContactPersonName = "Hasan Arslan",
            ContactPersonPhone = "0537 678 90 12",
            ContactPersonEmail = "hasan.arslan@def.com",
            MonthlyAidat = 600.00m,
            ContractStartDate = DateTime.Now.AddMonths(-1),
            ContractEndDate = DateTime.Now.AddMonths(11),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Tenants.AddRange(tenant1, tenant2, tenant3);
        await context.SaveChangesAsync();
        
        // Kiracıları dairelere ata ve dolu olarak işaretle
        flat1.TenantId = tenant1.Id;
        flat1.IsOccupied = true;
        flat1.BusinessType = "Ticaret";
        
        flat2.TenantId = tenant2.Id;
        flat2.IsOccupied = true;
        flat2.BusinessType = "Hizmet";
        
        flat3.TenantId = tenant3.Id;
        flat3.IsOccupied = true;
        flat3.BusinessType = "Üretim";
        
        await context.SaveChangesAsync();
        
        await context.SaveChangesAsync();
        
        logger.LogInformation("✅ Seed data başarıyla eklendi!");
        logger.LogInformation($"   - {context.Owners.Count()} mal sahibi");
        logger.LogInformation($"   - {context.Tenants.Count()} kiracı");
        logger.LogInformation($"   - {context.Flats.Count()} daire");
        logger.LogInformation($"   - {context.Users.Count()} kullanıcı");
        logger.LogInformation($"   - {context.AidatDefinitions.Count()} aidat tanımı");
        logger.LogInformation($"   - {context.Expenses.Count()} gider");
        logger.LogInformation($"   - {context.Payments.Count()} ödeme");
        logger.LogInformation($"   - {context.UtilityDebts.Count()} borç");
        logger.LogInformation($"   - {context.AdvanceAccounts.Count()} avans hesabı");
        logger.LogInformation($"   - {context.MeterReadings.Count()} sayaç okuması");
        logger.LogInformation($"   - {context.UtilityBills.Count()} genel fatura");
        
        // Kullanıcıları listele
        var users = await context.Users.ToListAsync();
        foreach (var user in users)
        {
            logger.LogInformation($"   - Kullanıcı: {user.Email} ({user.Role})");
        }
    }

    // Users seed (her zaman çalışsın diye ayrı kontrol)
    if (!context.Users.Any())
    {
        var owner = context.Owners.FirstOrDefault();
        var tenant = context.Tenants.FirstOrDefault();

        // Admin kullanıcı ekle
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@email.com",
            PasswordHash = "rJaJ4ickJwheNbnT4+i+2IyzQ0gotDuG/AWWytTG4nA=", // admin1234 SHA256 hash
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        // Owner kullanıcı ekle
        var ownerUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Owner",
            LastName = "User",
            Email = "owner@email.com",
            PasswordHash = "rJaJ4ickJwheNbnT4+i+2IyzQ0gotDuG/AWWytTG4nA=", // admin1234 SHA256 hash
            Role = UserRole.Owner,
            OwnerId = owner?.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(ownerUser);

        // Tenant kullanıcı ekle
        var tenantUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Tenant",
            LastName = "User",
            Email = "tenant@email.com",
            PasswordHash = "rJaJ4ickJwheNbnT4+i+2IyzQ0gotDuG/AWWytTG4nA=", // admin1234 SHA256 hash
            Role = UserRole.Tenant,
            TenantId = tenant?.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(tenantUser);

        // Observer kullanıcı ekle
        var observerUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Observer",
            LastName = "User",
            Email = "observer@email.com",
            PasswordHash = "rJaJ4ickJwheNbnT4+i+2IyzQ0gotDuG/AWWytTG4nA=", // admin1234 SHA256 hash
            Role = UserRole.Observer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(observerUser);

        await context.SaveChangesAsync();
    }

    // AidatDefinitions seed - gerçek tenant'lar ile eşleştir
    if (!context.AidatDefinitions.Any())
    {
        var tenants = context.Tenants.ToList();
        if (tenants.Any())
        {
            var aidats = new List<AidatDefinition>();
            
            // İlk tenant için aidat tanımı
            if (tenants.Count > 0)
            {
                aidats.Add(new AidatDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenants[0].Id,
                    Unit = "A1",
                    Year = 2024,
                    Amount = 500.00m,
                    VatIncludedAmount = 600.00m,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
            
            // İkinci tenant için aidat tanımı (varsa)
            if (tenants.Count > 1)
            {
                aidats.Add(new AidatDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenants[1].Id,
                    Unit = "A2",
                    Year = 2024,
                    Amount = 500.00m,
                    VatIncludedAmount = 600.00m,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
            
            // Üçüncü tenant için aidat tanımı (varsa)
            if (tenants.Count > 2)
            {
                aidats.Add(new AidatDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenants[2].Id,
                    Unit = "A3",
                    Year = 2024,
                    Amount = 500.00m,
                    VatIncludedAmount = 600.00m,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
            
            if (aidats.Any())
            {
                context.AidatDefinitions.AddRange(aidats);
                await context.SaveChangesAsync();
            }
        }
    }

    // Expenses seed
    if (!context.Expenses.Any())
    {
        var owners = context.Owners.ToList();
        if (owners.Any())
        {
            var expenses = new List<Expense>();
            
            // İlk owner için gider
            if (owners.Count > 0)
            {
                expenses.Add(new Expense
                {
                    Id = Guid.NewGuid(),
                    Title = "Elektrik Faturası - Ocak 2024",
                    Amount = 1200.00m,
                    Type = ExpenseType.Electricity,
                    ExpenseDate = new DateTime(2024, 1, 15),
                    Description = "Ocak ayı elektrik faturası",
                    ReceiptNumber = "ELK-2024-001",
                    OwnerId = owners[0].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // İkinci owner için gider (varsa)
            if (owners.Count > 1)
            {
                expenses.Add(new Expense
                {
                    Id = Guid.NewGuid(),
                    Title = "Su Faturası - Ocak 2024",
                    Amount = 450.00m,
                    Type = ExpenseType.Water,
                    ExpenseDate = new DateTime(2024, 1, 20),
                    Description = "Ocak ayı su faturası",
                    ReceiptNumber = "SU-2024-001",
                    OwnerId = owners[1].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Üçüncü owner için gider (varsa)
            if (owners.Count > 2)
            {
                expenses.Add(new Expense
                {
                    Id = Guid.NewGuid(),
                    Title = "Temizlik Hizmeti - Ocak 2024",
                    Amount = 800.00m,
                    Type = ExpenseType.Cleaning,
                    ExpenseDate = new DateTime(2024, 1, 25),
                    Description = "Ocak ayı temizlik hizmeti",
                    ReceiptNumber = "TEM-2024-001",
                    OwnerId = owners[2].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            if (expenses.Any())
            {
                context.Expenses.AddRange(expenses);
                await context.SaveChangesAsync();
            }
        }
    }

    // Payments seed
    if (!context.Payments.Any())
    {
        var owners = context.Owners.ToList();
        var tenants = context.Tenants.ToList();
        
        if (owners.Any() && tenants.Any())
        {
            var payments = new List<Payment>();
            
            // İlk tenant için ödeme
            if (tenants.Count > 0)
            {
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    Amount = 2500.00m,
                    Type = PaymentType.Rent,
                    Status = PaymentStatus.Completed,
                    PaymentDate = new DateTime(2024, 1, 5),
                    Description = "Ocak ayı kira ödemesi",
                    ReceiptNumber = "KIRA-2024-001",
                    TenantId = tenants[0].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // İkinci tenant için ödeme (varsa)
            if (tenants.Count > 1)
            {
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    Amount = 2800.00m,
                    Type = PaymentType.Rent,
                    Status = PaymentStatus.Completed,
                    PaymentDate = new DateTime(2024, 1, 5),
                    Description = "Ocak ayı kira ödemesi",
                    ReceiptNumber = "KIRA-2024-002",
                    TenantId = tenants[1].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // İlk owner için ödeme
            if (owners.Count > 0)
            {
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    Amount = 150.00m,
                    Type = PaymentType.Dues,
                    Status = PaymentStatus.Completed,
                    PaymentDate = new DateTime(2024, 1, 10),
                    Description = "Ocak ayı aidat ödemesi",
                    ReceiptNumber = "AIDAT-2024-001",
                    OwnerId = owners[0].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            if (payments.Any())
            {
                context.Payments.AddRange(payments);
                await context.SaveChangesAsync();
            }
        }
    }

    // UtilityDebts seed
    if (!context.UtilityDebts.Any())
    {
        var flats = context.Flats.ToList();
        var tenants = context.Tenants.ToList();
        var owners = context.Owners.ToList();
        
        if (flats.Any() && tenants.Any() && owners.Any())
        {
            var debts = new List<UtilityDebt>();
            
            // İlk flat, tenant ve owner için borç
            if (flats.Count > 0 && tenants.Count > 0 && owners.Count > 0)
            {
                debts.Add(new UtilityDebt
                {
                    Id = Guid.NewGuid(),
                    FlatId = flats[0].Id,
                    Type = DebtType.Electricity,
                    PeriodYear = 2024,
                    PeriodMonth = 1,
                    Amount = 150.00m,
                    Status = DebtStatus.Unpaid,
                    Description = "Ocak ayı elektrik borcu",
                    TenantId = tenants[0].Id,
                    OwnerId = owners[0].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // İkinci flat, tenant ve owner için borç (varsa)
            if (flats.Count > 1 && tenants.Count > 1 && owners.Count > 1)
            {
                debts.Add(new UtilityDebt
                {
                    Id = Guid.NewGuid(),
                    FlatId = flats[1].Id,
                    Type = DebtType.Water,
                    PeriodYear = 2024,
                    PeriodMonth = 1,
                    Amount = 80.00m,
                    Status = DebtStatus.Paid,
                    PaidAmount = 80.00m,
                    PaidDate = new DateTime(2024, 1, 15),
                    Description = "Ocak ayı su borcu",
                    TenantId = tenants[1].Id,
                    OwnerId = owners[1].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Üçüncü flat, tenant ve owner için borç (varsa)
            if (flats.Count > 2 && tenants.Count > 2 && owners.Count > 2)
            {
                debts.Add(new UtilityDebt
                {
                    Id = Guid.NewGuid(),
                    FlatId = flats[2].Id,
                    Type = DebtType.Aidat,
                    PeriodYear = 2024,
                    PeriodMonth = 1,
                    Amount = 500.00m,
                    Status = DebtStatus.Partial,
                    PaidAmount = 300.00m,
                    PaidDate = new DateTime(2024, 1, 10),
                    Description = "Ocak ayı aidat borcu",
                    TenantId = tenants[2].Id,
                    OwnerId = owners[2].Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            if (debts.Any())
            {
                context.UtilityDebts.AddRange(debts);
                await context.SaveChangesAsync();
            }
        }
    }

    // AdvanceAccounts seed
    if (!context.AdvanceAccounts.Any())
    {
        var tenants = context.Tenants.ToList();
        
        if (tenants.Any())
        {
            var advances = new List<AdvanceAccount>();
            
            // İlk tenant için avans hesabı
            if (tenants.Count > 0)
            {
                advances.Add(new AdvanceAccount
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenants[0].Id,
                    Balance = 500.00m,
                    Description = "Kira depozitosu",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // İkinci tenant için avans hesabı (varsa)
            if (tenants.Count > 1)
            {
                advances.Add(new AdvanceAccount
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenants[1].Id,
                    Balance = 750.00m,
                    Description = "Fatura depozitosu",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Üçüncü tenant için avans hesabı (varsa)
            if (tenants.Count > 2)
            {
                advances.Add(new AdvanceAccount
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenants[2].Id,
                    Balance = 1000.00m,
                    Description = "Güvenlik depozitosu",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            if (advances.Any())
            {
                context.AdvanceAccounts.AddRange(advances);
                await context.SaveChangesAsync();
            }
        }
    }

    // MeterReadings seed
    if (!context.MeterReadings.Any())
    {
        var flats = context.Flats.ToList();
        
        if (flats.Any())
        {
            var readings = new List<MeterReading>();
            
            // İlk flat için sayaç okuması
            if (flats.Count > 0)
            {
                readings.Add(new MeterReading
                {
                    Id = Guid.NewGuid(),
                    FlatId = flats[0].Id,
                    Type = MeterType.Electricity,
                    PeriodYear = 2024,
                    PeriodMonth = 1,
                    ReadingValue = 1250.50m,
                    Consumption = 150.30m,
                    ReadingDate = new DateTime(2024, 1, 31),
                    Note = "Ocak ayı elektrik sayacı okuması",
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // İkinci flat için sayaç okuması (varsa)
            if (flats.Count > 1)
            {
                readings.Add(new MeterReading
                {
                    Id = Guid.NewGuid(),
                    FlatId = flats[1].Id,
                    Type = MeterType.Water,
                    PeriodYear = 2024,
                    PeriodMonth = 1,
                    ReadingValue = 85.20m,
                    Consumption = 12.50m,
                    ReadingDate = new DateTime(2024, 1, 31),
                    Note = "Ocak ayı su sayacı okuması",
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Üçüncü flat için sayaç okuması (varsa)
            if (flats.Count > 2)
            {
                readings.Add(new MeterReading
                {
                    Id = Guid.NewGuid(),
                    FlatId = flats[2].Id,
                    Type = MeterType.Electricity,
                    PeriodYear = 2024,
                    PeriodMonth = 1,
                    ReadingValue = 2100.75m,
                    Consumption = 200.45m,
                    ReadingDate = new DateTime(2024, 1, 31),
                    Note = "Ocak ayı elektrik sayacı okuması",
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            if (readings.Any())
            {
                context.MeterReadings.AddRange(readings);
                await context.SaveChangesAsync();
            }
        }
    }

    // UtilityBills seed
    if (!context.UtilityBills.Any())
    {
        var bill1 = new UtilityBill
        {
            Id = Guid.NewGuid(),
            Type = UtilityType.Electricity,
            PeriodYear = 2024,
            PeriodMonth = 1,
            TotalAmount = 2500.00m,
            BillDate = new DateTime(2024, 1, 15),
            Description = "Ocak ayı genel elektrik faturası",
            CreatedAt = DateTime.UtcNow
        };
        
        var bill2 = new UtilityBill
        {
            Id = Guid.NewGuid(),
            Type = UtilityType.Water,
            PeriodYear = 2024,
            PeriodMonth = 1,
            TotalAmount = 800.00m,
            BillDate = new DateTime(2024, 1, 20),
            Description = "Ocak ayı genel su faturası",
            CreatedAt = DateTime.UtcNow
        };
        
        context.UtilityBills.AddRange(bill1, bill2);
        await context.SaveChangesAsync();
    }
    
    logger.LogInformation("✅ Seed data başarıyla tamamlandı!");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Seed data oluşturulurken hata oluştu: {Message}", ex.Message);
    // Seed data hatası API'yi durdurmamalı, sadece log'lanmalı
}
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// CORS middleware'ini en başta ekle
if (builder.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("AllowAll");
}

// Fallback CORS middleware for any issues
app.UseCors("Fallback");

// CORS preflight requests için ek middleware
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var origin = context.Request.Headers["Origin"].FirstOrDefault();
        if (!string.IsNullOrEmpty(origin) && (origin.Contains("akyildizyonetim.com") || origin.Contains("localhost")))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
        }
        context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
        context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    
    // Log all requests for debugging
    var logger = context.RequestServices.GetService<ILogger<Program>>();
    logger?.LogInformation($"Request: {context.Request.Method} {context.Request.Path} from {context.Request.Headers["Origin"]}");
    
    // Add CORS headers to all responses
    var responseOrigin = context.Request.Headers["Origin"].FirstOrDefault();
    if (!string.IsNullOrEmpty(responseOrigin) && (responseOrigin.Contains("akyildizyonetim.com") || responseOrigin.Contains("localhost")))
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = responseOrigin;
    }
    context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
    
    await next();
});

// Authentication ve Authorization middleware'lerini ekle
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();

public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
