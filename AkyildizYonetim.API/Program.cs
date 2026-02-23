using AkyildizYonetim.API.Middleware;
using AkyildizYonetim.Application.Common;
using AkyildizYonetim.Infrastructure;
using AkyildizYonetim.Infrastructure.Jwt;
using AkyildizYonetim.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Render/Docker inotify limit fix
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JwtSettings Validation
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .Validate(settings => 
    {
        if (string.IsNullOrWhiteSpace(settings.SecretKey) || settings.SecretKey.Length < 32) return false;
        if (string.IsNullOrWhiteSpace(settings.Issuer)) return false;
        if (string.IsNullOrWhiteSpace(settings.Audience)) return false;
        if (settings.ExpirationInMinutes <= 0) return false;
        return true;
    }, "JwtSettings are invalid. SecretKey must be at least 32 characters, and Issuer/Audience/Expiration are required.")
    .ValidateOnStart();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null) throw new InvalidOperationException("JwtSettings section is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
			ValidateIssuer = true,
			ValidIssuer = jwtSettings.Issuer,
			ValidateAudience = true,
			ValidAudience = jwtSettings.Audience,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};
	});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantRead", policy => policy.RequireRole("admin", "manager", "dataentry", "observer", "tenant"));
    options.AddPolicy("TenantWrite", policy => policy.RequireRole("admin", "manager", "dataentry"));
    options.AddPolicy("FinanceRead", policy => policy.RequireRole("admin", "manager"));
    options.AddPolicy("FinanceWrite", policy => policy.RequireRole("admin", "manager"));
});

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

// CORS
var clientUrlConfig = builder.Configuration["ClientSettings:ClientUrl"];
if (string.IsNullOrEmpty(clientUrlConfig) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("Production ClientUrl is not configured in ClientSettings");
}

var clientOrigins = (clientUrlConfig ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(url => url.Trim().TrimEnd('/'))
    .ToArray();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
	});

	options.AddPolicy("Production", policy =>
	{
		policy.WithOrigins(clientOrigins)
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

// JSON settings
builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers();

var app = builder.Build();

// 1) Global Error Handling
app.UseMiddleware<ExceptionMiddleware>();

// 2) Swagger (Dev Only)
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// 3) HTTPS Redirection
app.UseHttpsRedirection();

// 4) CORS (Auth öncesi)
var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "Production";
app.UseCors(corsPolicy);

// 5) Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6) Database Migration & Seeding
var runMigrations = builder.Configuration.GetValue<bool>("RunMigrationsOnStartup");
if (app.Environment.IsDevelopment()) runMigrations = true; // Dev'de kolaylık için açık kalsın

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var context = services.GetRequiredService<ApplicationDbContext>();
	var logger = services.GetRequiredService<ILogger<Program>>();

	try
	{
		if (runMigrations)
		{
            logger.LogInformation("Veritabanı migration'ları kontrol ediliyor...");
		    context.Database.Migrate();
            logger.LogInformation("Veritabanı migration'ları başarıyla tamamlandı.");
		}
        else
        {
            logger.LogInformation("RunMigrationsOnStartup kapalı. Migration atlanıyor.");
        }
		
		await DataSeeder.SeedAsync(context, logger);
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Veritabanı güncellenirken hata oluştu");
	}
}

// 7) Endpoint Mapping
app.MapControllers();

app.Run();

public partial class Program { }
