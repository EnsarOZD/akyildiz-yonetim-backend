using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AkyildizYonetim.Tests.Integration;

public class EndpointCoverageTests : AuthTestBase
{
    public EndpointCoverageTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("/api/tenants")]
    [InlineData("/api/owners")]
    [InlineData("/api/users")]
    [InlineData("/api/payments")]
    [InlineData("/api/utilitydebts")]
    [InlineData("/api/flats")]
    [InlineData("/api/reports/finance/summary")]
    [InlineData("/api/advanceaccounts")]
    [InlineData("/api/notifications")]
    public async Task CoreEndpoints_RequireAuthentication(string url)
    {
        // Arrange
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { 
            AllowAutoRedirect = false 
        });

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
