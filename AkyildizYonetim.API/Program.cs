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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

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

// CORS politikasını ekle - Development için tüm origin'lere izin ver
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
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
    options.UseNpgsql(connectionString));

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
            Floor = 1,
            OwnerId = owner1.Id,
            TenantId = null,
            ApartmentNumber = "A1",
            RoomCount = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var flat2 = new Flat
        {
            Id = Guid.NewGuid(),
            Number = "A2",
            Floor = 1,
            OwnerId = owner2.Id,
            TenantId = null,
            ApartmentNumber = "A2",
            RoomCount = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var flat3 = new Flat
        {
            Id = Guid.NewGuid(),
            Number = "A3",
            Floor = 2,
            OwnerId = owner3.Id,
            TenantId = null,
            ApartmentNumber = "A3",
            RoomCount = 4,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Flats.AddRange(flat1, flat2, flat3);
        await context.SaveChangesAsync();
        
        // Kiracılar oluştur
        var tenant1 = new Tenant
        {
            Id = Guid.NewGuid(),
            FirstName = "Ali",
            LastName = "Özkan",
            PhoneNumber = "0535 456 78 90",
            Email = "ali.ozkan@email.com",
            ApartmentNumber = "A1",
            LeaseStartDate = DateTime.Now.AddMonths(-6),
            LeaseEndDate = DateTime.Now.AddMonths(6),
            MonthlyRent = 2500.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var tenant2 = new Tenant
        {
            Id = Guid.NewGuid(),
            FirstName = "Ayşe",
            LastName = "Çelik",
            PhoneNumber = "0536 567 89 01",
            Email = "ayse.celik@email.com",
            ApartmentNumber = "A2",
            LeaseStartDate = DateTime.Now.AddMonths(-3),
            LeaseEndDate = DateTime.Now.AddMonths(9),
            MonthlyRent = 2800.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var tenant3 = new Tenant
        {
            Id = Guid.NewGuid(),
            FirstName = "Hasan",
            LastName = "Arslan",
            PhoneNumber = "0537 678 90 12",
            Email = "hasan.arslan@email.com",
            ApartmentNumber = "A3",
            LeaseStartDate = DateTime.Now.AddMonths(-1),
            LeaseEndDate = DateTime.Now.AddMonths(11),
            MonthlyRent = 3000.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Tenants.AddRange(tenant1, tenant2, tenant3);
        await context.SaveChangesAsync();
        
        // Kiracıları dairelere ata
        flat1.TenantId = tenant1.Id;
        flat2.TenantId = tenant2.Id;
        flat3.TenantId = tenant3.Id;
        await context.SaveChangesAsync();
        
        await context.SaveChangesAsync();
        
        Console.WriteLine("✅ Seed data başarıyla eklendi!");
        Console.WriteLine($"   - {context.Owners.Count()} mal sahibi");
        Console.WriteLine($"   - {context.Tenants.Count()} kiracı");
        Console.WriteLine($"   - {context.Flats.Count()} daire");
        Console.WriteLine($"   - {context.Users.Count()} kullanıcı");
        
        // Kullanıcıları listele
        var users = await context.Users.ToListAsync();
        foreach (var user in users)
        {
            Console.WriteLine($"   - Kullanıcı: {user.Email} ({user.Role})");
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

    // AidatDefinitions seed
    if (!context.AidatDefinitions.Any())
    {
        var aidat = new AidatDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(), // örnek tenantId, gerçek tenant ile eşleştirilebilir
            Unit = "A1",
            Year = 2024,
            Amount = 500.00m,
            VatIncludedAmount = 600.00m,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        context.AidatDefinitions.Add(aidat);
        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// CORS middleware'ini UseHttpsRedirection'dan önce ekle
app.UseCors("AllowAll");

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
