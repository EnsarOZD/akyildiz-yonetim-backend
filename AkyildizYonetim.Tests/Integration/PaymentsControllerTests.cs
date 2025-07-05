using System.Net;
using System.Text;
using System.Text.Json;
using AkyildizYonetim.Application.Payments.Commands.CreatePayment;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AkyildizYonetim.Tests.Integration;

public class PaymentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ApplicationDbContext _context;

    public PaymentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _context = TestBase.CreateDbContextAsync().Result;
    }

    [Fact]
    public async Task CreatePaymentWithAllocation_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        await TestBase.SeedTestDataAsync(_context);
        var tenant = await _context.Tenants.FirstAsync();
        var debt = await _context.UtilityDebts.FirstAsync();

        var client = _factory.CreateClient();
        var command = new CreatePaymentWithDebtAllocationCommand
        {
            Amount = 1000,
            Type = PaymentType.Utility,
            PaymentDate = DateTime.UtcNow,
            TenantId = tenant.Id,
            AutoAllocate = true
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/payments/with-allocation", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreatePaymentWithAllocation_WithInvalidTenantId_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new CreatePaymentWithDebtAllocationCommand
        {
            Amount = 1000,
            Type = PaymentType.Utility,
            PaymentDate = DateTime.UtcNow,
            TenantId = Guid.NewGuid(), // Geçersiz tenant ID
            AutoAllocate = true
        };

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/payments/with-allocation", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPayments_ShouldReturnPayments()
    {
        // Arrange
        await TestBase.SeedTestDataAsync(_context);
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/payments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPaymentById_WithValidId_ShouldReturnPayment()
    {
        // Arrange
        await TestBase.SeedTestDataAsync(_context);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = 500,
            Type = PaymentType.Utility,
            Status = PaymentStatus.Completed,
            PaymentDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/payments/{payment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPaymentById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/payments/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
} 