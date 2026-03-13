using AkyildizYonetim.Application.AdvanceAccounts.Commands.UseAdvanceAccount;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AkyildizYonetim.Tests.Unit.AdvanceAccounts;

public class UseAdvanceAccountCommandTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ILogger<UseAdvanceAccountCommandHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockUserService;
    private readonly UseAdvanceAccountCommandHandler _handler;

    public UseAdvanceAccountCommandTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockLogger = new Mock<ILogger<UseAdvanceAccountCommandHandler>>();
        _mockUserService = new Mock<ICurrentUserService>();
        
        // Default to Admin to keep existing tests working
        _mockUserService.Setup(s => s.IsAdmin).Returns(true);
        _mockUserService.Setup(s => s.IsManager).Returns(true);
        _mockUserService.Setup(s => s.IsDataEntry).Returns(true);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        _handler = new UseAdvanceAccountCommandHandler(_mockContext.Object, _mockLogger.Object, _mockUserService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUseAdvanceAccountAndPayDebts()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var debtId = Guid.NewGuid();
        var command = new UseAdvanceAccountCommand
        {
            TenantId = tenantId,
            DebtPayments = new List<DebtPaymentRequest>
            {
                new() { DebtId = debtId, Amount = 500 }
            },
            Description = "Test ödeme"
        };

        var advanceAccount = new AdvanceAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Balance = 1000,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var debt = new UtilityDebt
        {
            Id = debtId,
            TenantId = tenantId,
            Amount = 800,
            RemainingAmount = 800,
            Type = DebtType.Electricity,
            Status = DebtStatus.Unpaid,
            DueDate = DateTime.UtcNow.AddDays(-10),
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var mockAdvanceAccounts = GetMockDbSet(new List<AdvanceAccount> { advanceAccount });
        var mockDebts = GetMockDbSet(new List<UtilityDebt> { debt });
        var mockPayments = GetMockDbSet(new List<Payment>());
        var mockPaymentDebts = GetMockDbSet(new List<PaymentDebt>());

        _mockContext.Setup(c => c.AdvanceAccounts).Returns(mockAdvanceAccounts.Object);
        _mockContext.Setup(c => c.UtilityDebts).Returns(mockDebts.Object);
        _mockContext.Setup(c => c.Payments).Returns(mockPayments.Object);
        _mockContext.Setup(c => c.PaymentDebts).Returns(mockPaymentDebts.Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalAmount.Should().Be(500);
        result.Data.PreviousBalance.Should().Be(1000);
        result.Data.NewBalance.Should().Be(500);
        result.Data.DebtPayments.Should().HaveCount(1);
        result.Data.DebtPayments[0].PaidAmount.Should().Be(500);
        result.Data.DebtPayments[0].RemainingDebtAmount.Should().Be(300);
    }

    [Fact]
    public async Task Handle_WithInsufficientBalance_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var debtId = Guid.NewGuid();
        var command = new UseAdvanceAccountCommand
        {
            TenantId = tenantId,
            DebtPayments = new List<DebtPaymentRequest>
            {
                new() { DebtId = debtId, Amount = 1500 }
            }
        };

        var advanceAccount = new AdvanceAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Balance = 1000,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var mockAdvanceAccounts = GetMockDbSet(new List<AdvanceAccount> { advanceAccount });
        _mockContext.Setup(c => c.AdvanceAccounts).Returns(mockAdvanceAccounts.Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Yetersiz bakiye");
    }

    [Fact]
    public async Task Handle_WithNonExistentAdvanceAccount_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new UseAdvanceAccountCommand
        {
            TenantId = tenantId,
            DebtPayments = new List<DebtPaymentRequest>
            {
                new() { DebtId = Guid.NewGuid(), Amount = 500 }
            }
        };

        var mockAdvanceAccounts = GetMockDbSet(new List<AdvanceAccount>());
        _mockContext.Setup(c => c.AdvanceAccounts).Returns(mockAdvanceAccounts.Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Avans hesabı bulunamadı");
    }

    [Fact]
    public async Task Handle_WithInvalidDebtId_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invalidDebtId = Guid.NewGuid();
        var command = new UseAdvanceAccountCommand
        {
            TenantId = tenantId,
            DebtPayments = new List<DebtPaymentRequest>
            {
                new() { DebtId = invalidDebtId, Amount = 500 }
            }
        };

        var advanceAccount = new AdvanceAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Balance = 1000,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var mockAdvanceAccounts = GetMockDbSet(new List<AdvanceAccount> { advanceAccount });
        var mockDebts = GetMockDbSet(new List<UtilityDebt>());

        _mockContext.Setup(c => c.AdvanceAccounts).Returns(mockAdvanceAccounts.Object);
        _mockContext.Setup(c => c.UtilityDebts).Returns(mockDebts.Object);
        _mockContext.Setup(c => c.Payments).Returns(GetMockDbSet(new List<Payment>()).Object);
        _mockContext.Setup(c => c.PaymentDebts).Returns(GetMockDbSet(new List<PaymentDebt>()).Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Borç bulunamadı");
    }

    [Fact]
    public async Task Handle_WithDebtFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var debtId = Guid.NewGuid();
        var command = new UseAdvanceAccountCommand
        {
            TenantId = tenantId,
            DebtPayments = new List<DebtPaymentRequest>
            {
                new() { DebtId = debtId, Amount = 500 }
            }
        };

        var advanceAccount = new AdvanceAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Balance = 1000,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var debt = new UtilityDebt
        {
            Id = debtId,
            TenantId = differentTenantId, // Farklı kiracı
            Amount = 800,
            RemainingAmount = 800,
            Type = DebtType.Electricity,
            Status = DebtStatus.Unpaid,
            DueDate = DateTime.UtcNow.AddDays(-10),
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var mockAdvanceAccounts = GetMockDbSet(new List<AdvanceAccount> { advanceAccount });
        var mockDebts = GetMockDbSet(new List<UtilityDebt> { debt });

        _mockContext.Setup(c => c.AdvanceAccounts).Returns(mockAdvanceAccounts.Object);
        _mockContext.Setup(c => c.UtilityDebts).Returns(mockDebts.Object);
        _mockContext.Setup(c => c.Database).Returns(GetMockDatabase().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Borç bu kiracıya ait değil");
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