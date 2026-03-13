using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AkyildizYonetim.Tests.Integration;

public class RoleAccessTests : AuthTestBase
{
    public RoleAccessTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Admin_CanAccess_AuditLogs()
    {
        var client = CreateClientWithUser(UserContext.Admin());
        var response = await client.GetAsync("/api/auditlogs");
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Manager_CanAccess_AuditLogs()
    {
        var client = CreateClientWithUser(UserContext.Manager());
        var response = await client.GetAsync("/api/auditlogs");
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DataEntry_CannotAccess_AuditLogs()
    {
        var client = CreateClientWithUser(new UserContext { Role = "dataentry" });
        var response = await client.GetAsync("/api/auditlogs");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Observer_CannotAccess_AuditLogs()
    {
        var client = CreateClientWithUser(new UserContext { Role = "observer" });
        var response = await client.GetAsync("/api/auditlogs");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Manager_CannotCreateUser()
    {
        var client = CreateClientWithUser(UserContext.Manager());
        var command = new { UserName = "newuser", Password = "Password123!", Role = "tenant" };
        var response = await client.PostAsJsonAsync("/api/users", command);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_CanCreateUser()
    {
        var client = CreateClientWithUser(UserContext.Admin());
        var command = new { 
            FirstName = "New",
            LastName = "User",
            Email = "new@example.com",
            UserName = "newuser_" + Guid.NewGuid(), 
            Password = "Password123!", 
            Role = "tenant" 
        };
        var response = await client.PostAsJsonAsync("/api/users", command);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tenant_CannotAccess_InternalUserList()
    {
        var client = CreateClientWithUser(UserContext.Tenant(Guid.NewGuid()));
        var response = await client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
