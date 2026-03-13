using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace AkyildizYonetim.Tests.Integration;

public class TenantIsolationTests : AuthTestBase
{
    public TenantIsolationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task TenantA_CannotReadTenantB_Debts()
    {
        // Arrange
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        // Tenant A requests debts — the handler should filter by current user TenantId
        var client = CreateClientWithUser(UserContext.Tenant(tenantAId));

        // Act
        var response = await client.GetAsync($"/api/utilitydebts?tenantId={tenantBId}");

        // Assert — should get OK but response must NOT contain Tenant B's data
        // The handler overrides client-provided tenantId for external roles
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain(tenantBId.ToString(),
            "Tenant A should not be able to read Tenant B debts even when explicitly requesting them");
    }

    [Fact]
    public async Task TenantA_CannotAccessBuildingWideStats()
    {
        // Arrange
        var tenantAId = Guid.NewGuid();
        var client = CreateClientWithUser(UserContext.Tenant(tenantAId));

        // Act
        var response = await client.GetAsync("/api/tenants/stats");

        // Assert
        // Now restricted to FinanceRead policy (Admin/Manager)
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tenant_CannotSpoofIdentity_ByPassingManualTenantIdInCreatePayment()
    {
        // Arrange
        var realTenantId = Guid.NewGuid();
        var spoofedTenantId = Guid.NewGuid();
        var client = CreateClientWithUser(UserContext.Tenant(realTenantId));

        var command = new {
            Amount = 500,
            Type = "Other",
            PaymentDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            TenantId = spoofedTenantId, // Attempt to spoof
            Description = "Spoof attempt"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/payments", command);

        // Assert — the handler should reject the cross-tenant request
        // because tenant user's TenantId != spoofedTenantId
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Tenant user should not be able to create payments for another tenant");
    }
}
