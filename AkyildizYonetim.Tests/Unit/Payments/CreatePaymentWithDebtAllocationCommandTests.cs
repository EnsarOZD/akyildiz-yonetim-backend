using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Payments.Commands.CreatePayment;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AkyildizYonetim.Tests.Unit.Payments;

public class CreatePaymentWithDebtAllocationCommandTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ILogger<CreatePaymentWithDebtAllocationCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockUserService;
    private readonly CreatePaymentWithDebtAllocationCommandHandler _handler;

    public CreatePaymentWithDebtAllocationCommandTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockLogger = new Mock<ILogger<CreatePaymentWithDebtAllocationCommandHandler>>();
        _mockUserService = new Mock<ICurrentUserService>();
        
        // Default to Admin
        _mockUserService.Setup(s => s.IsAdmin).Returns(true);
        _mockUserService.Setup(s => s.IsManager).Returns(true);
        _mockUserService.Setup(s => s.IsDataEntry).Returns(true);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new CreatePaymentWithDebtAllocationCommandHandler(_mockContext.Object, _mockLogger.Object, _mockUserService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreatePaymentAndAllocateDebts()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var debtId = Guid.NewGuid();
        var command = new CreatePaymentWithDebtAllocationCommand
        {
            Amount = 1000,
            Type = PaymentType.Utility,
            PaymentDate = DateTime.UtcNow,
            TenantId = tenantId,
            AutoAllocate = true
        };

        var mockDbSet = GetMockDbSet(new List<UtilityDebt>
        {
            new()
            {
                Id = debtId,
                TenantId = tenantId,
                Amount = 800,
                RemainingAmount = 800,
                Type = DebtType.Electricity,
                Status = DebtStatus.Unpaid,
                DueDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Description = "Test Elektrik Borcu",
                PeriodYear = 2024,
                PeriodMonth = 1
            }
        });

        _mockContext.Setup(c => c.UtilityDebts).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.Payments).Returns(GetMockDbSet<Payment>(new List<Payment>()).Object);
        _mockContext.Setup(c => c.PaymentDebts).Returns(GetMockDbSet<PaymentDebt>(new List<PaymentDebt>()).Object);
        _mockContext.Setup(c => c.AdvanceAccounts).Returns(GetMockDbSet<AdvanceAccount>(new List<AdvanceAccount>()).Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Payment.Amount.Should().Be(1000);
        result.Data.TotalAllocated.Should().Be(800);
        result.Data.RemainingAmount.Should().Be(200);
        result.Data.Allocations.Should().HaveCount(1);
        result.Data.Allocations[0].DebtId.Should().Be(debtId);
        result.Data.Allocations[0].AllocatedAmount.Should().Be(800);
    }

    [Fact]
    public async Task Handle_WithManualAllocation_ShouldAllocateSpecifiedAmounts()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var debtId = Guid.NewGuid();
        var command = new CreatePaymentWithDebtAllocationCommand
        {
            Amount = 1000,
            Type = PaymentType.Utility,
            PaymentDate = DateTime.UtcNow,
            TenantId = tenantId,
            AutoAllocate = false,
            DebtAllocations = new List<DebtAllocationRequest>
            {
                new() { DebtId = debtId, Amount = 500 }
            }
        };

        var mockDbSet = GetMockDbSet(new List<UtilityDebt>
        {
            new()
            {
                Id = debtId,
                TenantId = tenantId,
                Amount = 800,
                RemainingAmount = 800,
                Type = DebtType.Electricity,
                Status = DebtStatus.Unpaid,
                DueDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Description = "Test Elektrik Borcu",
                PeriodYear = 2024,
                PeriodMonth = 1
            }
        });

        _mockContext.Setup(c => c.UtilityDebts).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.Payments).Returns(GetMockDbSet<Payment>(new List<Payment>()).Object);
        _mockContext.Setup(c => c.PaymentDebts).Returns(GetMockDbSet<PaymentDebt>(new List<PaymentDebt>()).Object);
        _mockContext.Setup(c => c.AdvanceAccounts).Returns(GetMockDbSet<AdvanceAccount>(new List<AdvanceAccount>()).Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalAllocated.Should().Be(500);
        result.Data.RemainingAmount.Should().Be(500);
        result.Data.Allocations.Should().HaveCount(1);
        result.Data.Allocations[0].AllocatedAmount.Should().Be(500);
    }

    [Fact]
    public async Task Handle_WithExcessPayment_ShouldAddToAdvanceAccount()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreatePaymentWithDebtAllocationCommand
        {
            Amount = 1000,
            Type = PaymentType.Utility,
            PaymentDate = DateTime.UtcNow,
            TenantId = tenantId,
            AutoAllocate = true
        };

        var mockDbSet = GetMockDbSet(new List<UtilityDebt>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Amount = 300,
                RemainingAmount = 300,
                Type = DebtType.Electricity,
                Status = DebtStatus.Unpaid,
                DueDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Description = "Test Elektrik Borcu",
                PeriodYear = 2024,
                PeriodMonth = 1
            }
        });

        _mockContext.Setup(c => c.UtilityDebts).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.Payments).Returns(GetMockDbSet<Payment>(new List<Payment>()).Object);
        _mockContext.Setup(c => c.PaymentDebts).Returns(GetMockDbSet<PaymentDebt>(new List<PaymentDebt>()).Object);
        _mockContext.Setup(c => c.AdvanceAccounts).Returns(GetMockDbSet<AdvanceAccount>(new List<AdvanceAccount>()).Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalAllocated.Should().Be(300);
        result.Data.RemainingAmount.Should().Be(700);
        result.Data.Allocations.Should().HaveCount(2);
        result.Data.Allocations.Should().Contain(a => a.DebtDescription == "Avans Hesabı");
    }

    [Fact]
    public async Task Handle_WithInvalidDebtId_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invalidDebtId = Guid.NewGuid();
        var command = new CreatePaymentWithDebtAllocationCommand
        {
            Amount = 1000,
            Type = PaymentType.Utility,
            PaymentDate = DateTime.UtcNow,
            TenantId = tenantId,
            AutoAllocate = false,
            DebtAllocations = new List<DebtAllocationRequest>
            {
                new() { DebtId = invalidDebtId, Amount = 500 }
            }
        };

        var mockDbSet = GetMockDbSet(new List<UtilityDebt>());
        _mockContext.Setup(c => c.UtilityDebts).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Borç bulunamadı");
    }

    private static Mock<DbSet<T>> GetMockDbSet<T>(List<T> list) where T : class
    {
        var queryable = list.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        
        // Async support
        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        
        mockDbSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        
        return mockDbSet;
    }

    private static Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade> GetMockDatabase()
    {
        var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(MockBehavior.Loose, null);
        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
        
        return mockDatabase;
    }
} 