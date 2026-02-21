using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Infrastructure.Jwt;
using AkyildizYonetim.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AkyildizYonetim.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("DefaultConnection is not configured");

        services.Configure<AkyildizYonetim.Infrastructure.Email.SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.Configure<AkyildizYonetim.Application.Common.Models.ClientSettings>(configuration.GetSection("ClientSettings"));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, AkyildizYonetim.Infrastructure.Services.CurrentUserService>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailSender, AkyildizYonetim.Infrastructure.Email.EmailSender>();

        return services;
    }
}
