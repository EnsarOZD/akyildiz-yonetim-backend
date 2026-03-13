using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace AkyildizYonetim.Tests.Integration;

public abstract class AuthTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    static AuthTestBase()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
    }

    protected readonly WebApplicationFactory<Program> Factory;

    protected AuthTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
    }

    protected HttpClient CreateClientWithUser(UserContext context)
    {
        var client = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Environment", "Test" },
                    { "ConnectionStrings:DefaultConnection", "Server=__test_dummy__;Database=__test__;" },
                    { "JwtSettings:SecretKey", "ThisIsAVeryLongDummySecretKeyForTestingPurposes" },
                    { "JwtSettings:Issuer", "TestIssuer" },
                    { "JwtSettings:Audience", "TestAudience" },
                    { "JwtSettings:ExpirationInMinutes", "60" }
                });
            });

            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with InMemory database — remove ALL EF descriptors
                var dbDescriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                ).ToList();
                foreach (var d in dbDescriptors) services.Remove(d);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("AuthTestDb_" + Guid.NewGuid().ToString("N"));
                });

                // Replace authentication with test scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Replace ICurrentUserService with mock
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICurrentUserService));
                if (descriptor != null) services.Remove(descriptor);

                var mockUserService = new Mock<ICurrentUserService>();
                mockUserService.Setup(s => s.UserId).Returns(context.UserId?.ToString());
                mockUserService.Setup(s => s.Role).Returns(context.Role);
                mockUserService.Setup(s => s.TenantId).Returns(context.TenantId);
                mockUserService.Setup(s => s.OwnerId).Returns(context.OwnerId);
                mockUserService.Setup(s => s.IsAdmin).Returns(context.Role == "admin");
                mockUserService.Setup(s => s.IsManager).Returns(context.Role == "manager");
                mockUserService.Setup(s => s.IsDataEntry).Returns(context.Role == "dataentry");
                mockUserService.Setup(s => s.IsObserver).Returns(context.Role == "observer");

                services.AddScoped(_ => mockUserService.Object);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Add("X-Test-Role", context.Role);
        if (context.UserId.HasValue) client.DefaultRequestHeaders.Add("X-Test-UserId", context.UserId.Value.ToString());
        return client;
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers["X-Test-Role"].FirstOrDefault() ?? "admin";
        var userId = Request.Headers["X-Test-UserId"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        
        var claims = new[] { 
            new Claim(ClaimTypes.Name, "TestUser"), 
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role) 
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}

public record UserContext
{
    public Guid? UserId { get; init; }
    public string Role { get; init; } = "tenant";
    public Guid? TenantId { get; init; }
    public Guid? OwnerId { get; init; }

    public static UserContext Admin() => new() { Role = "admin", UserId = Guid.NewGuid() };
    public static UserContext Manager() => new() { Role = "manager", UserId = Guid.NewGuid() };
    public static UserContext Tenant(Guid tenantId) => new() { Role = "tenant", TenantId = tenantId, UserId = Guid.NewGuid() };
    public static UserContext Owner(Guid ownerId) => new() { Role = "owner", OwnerId = ownerId, UserId = Guid.NewGuid() };
}
