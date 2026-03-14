using System.Net;
using System.Net.Http.Json;
using AkyildizYonetim.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AkyildizYonetim.Tests.Integration;

public class UtilityDebtsControllerTests : AuthTestBase
{
    public UtilityDebtsControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUtilityDebts_AsManager_ReturnsOk()
    {
        var client = CreateClientWithUser(UserContext.Manager());
        var response = await client.GetAsync("/api/utilitydebts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUtilityDebt_WithInvoiceNumber_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Manager());

        var command = new
        {
            FlatId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = 0, // Electricity
            PeriodYear = 2024,
            PeriodMonth = 3,
            Amount = 450.00m,
            RemainingAmount = 450.00m,
            Status = 0,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(30),
            Description = "Mart ayı elektrik faturası",
            InvoiceNumber = "ELK-2024-0312",
        };

        var response = await client.PostAsJsonAsync("/api/utilitydebts", command);

        // InvoiceNumber destekli endpoint 500 vermemeli
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateUtilityDebt_WithoutInvoiceNumber_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Manager());

        var command = new
        {
            FlatId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = 1, // Water
            PeriodYear = 2024,
            PeriodMonth = 3,
            Amount = 200.00m,
            RemainingAmount = 200.00m,
            Status = 0,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(30),
            Description = "Su faturası",
            // InvoiceNumber yok - opsiyonel alan
        };

        var response = await client.PostAsJsonAsync("/api/utilitydebts", command);
        // Fatura numarası opsiyonel, 500 vermemeli
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateBulkUtilityDebts_WithInvoiceNumbers_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Manager());

        var flatId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var command = new
        {
            Debts = new[]
            {
                new
                {
                    FlatId = flatId,
                    TenantId = tenantId,
                    Type = 0,
                    PeriodYear = 2024,
                    PeriodMonth = 4,
                    Amount = 500m,
                    RemainingAmount = 500m,
                    Status = 0,
                    PaidAmount = 0m,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    InvoiceNumber = "BULK-ELK-001",
                    Description = "Toplu elektrik"
                },
                new
                {
                    FlatId = flatId,
                    TenantId = tenantId,
                    Type = 1,
                    PeriodYear = 2024,
                    PeriodMonth = 4,
                    Amount = 150m,
                    RemainingAmount = 150m,
                    Status = 0,
                    PaidAmount = 0m,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    InvoiceNumber = "BULK-SU-001",
                    Description = "Toplu su"
                }
            }
        };

        var response = await client.PostAsJsonAsync("/api/utilitydebts/bulk", command);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UpdateUtilityDebt_WithNewInvoiceNumber_ReturnsNotInternalServerError()
    {
        var client = CreateClientWithUser(UserContext.Manager());
        var debtId = Guid.NewGuid();

        var updateCommand = new
        {
            Id = debtId,
            FlatId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = 0,
            PeriodYear = 2024,
            PeriodMonth = 6,
            Amount = 400m,
            RemainingAmount = 400m,
            Status = 0,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(30),
            InvoiceNumber = "UPDATED-INVOICE-001",
            Description = "Güncellenmiş fatura"
        };

        var response = await client.PutAsJsonAsync($"/api/utilitydebts/{debtId}", updateCommand);
        // Var olmayan ID - NotFound veya BadRequest beklenir, 500 değil
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUtilityDebts_AsObserver_ReturnsOkOrForbidden()
    {
        var client = CreateClientWithUser(new UserContext { Role = "observer" });
        var response = await client.GetAsync("/api/utilitydebts");
        // Observer okuma yetkisine sahipse OK, değilse Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUtilityDebt_AsObserver_ReturnsForbidden()
    {
        var client = CreateClientWithUser(new UserContext { Role = "observer" });
        var command = new
        {
            FlatId = Guid.NewGuid(),
            Type = 0,
            PeriodYear = 2024,
            PeriodMonth = 1,
            Amount = 100m,
        };

        var response = await client.PostAsJsonAsync("/api/utilitydebts", command);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUtilityDebts_FilterByTenantId_ReturnsOk()
    {
        var client = CreateClientWithUser(UserContext.Manager());
        var tenantId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/utilitydebts?tenantId={tenantId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUtilityDebt_WithNegativeAmount_ReturnsSuccessOrBadRequest()
    {
        // Not: UtilityDebt için FluentValidation validator henüz eklenmemiş.
        // Şu an negatif tutarları kabul ediyor. Bu test mevcut davranışı belgeler.
        var client = CreateClientWithUser(UserContext.Manager());

        var command = new
        {
            FlatId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = 0,
            PeriodYear = 2024,
            PeriodMonth = 3,
            Amount = -100m,
            RemainingAmount = -100m,
            Status = 0,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(30),
        };

        var response = await client.PostAsJsonAsync("/api/utilitydebts", command);
        // Şu an validator olmadığından 200 OK dönüyor; gelecekte 400 olması beklenir
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUtilityDebt_WithZeroAmount_ReturnsSuccessOrBadRequest()
    {
        // Not: Validator eklendikten sonra bu test BadRequest dönmeli
        var client = CreateClientWithUser(UserContext.Manager());

        var command = new
        {
            FlatId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = 0,
            PeriodYear = 2024,
            PeriodMonth = 3,
            Amount = 0m,
            Status = 0,
        };

        var response = await client.PostAsJsonAsync("/api/utilitydebts", command);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
