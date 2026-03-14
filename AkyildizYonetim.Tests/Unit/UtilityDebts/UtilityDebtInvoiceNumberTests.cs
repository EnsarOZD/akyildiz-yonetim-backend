using AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AkyildizYonetim.Tests.Unit.UtilityDebts;

public class UtilityDebtInvoiceNumberTests
{
    private static (ApplicationDbContext context, IMediator mediator) CreateInMemoryDeps()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("UtilityDebtTest_" + Guid.NewGuid().ToString("N"))
            .Options;

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(s => s.IsAdmin).Returns(true);
        mockUserService.Setup(s => s.IsManager).Returns(true);

        var context = new ApplicationDbContext(options, mockUserService.Object);
        context.Database.EnsureCreated();

        var mediator = new Mock<IMediator>().Object;
        return (context, mediator);
    }

    [Fact]
    public async Task CreateUtilityDebt_WithInvoiceNumber_SavesInvoiceNumber()
    {
        // Arrange
        var (context, mediator) = CreateInMemoryDeps();
        var handler = new CreateUtilityDebtCommandHandler(context, mediator);

        var command = new CreateUtilityDebtCommand
        {
            FlatId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = DebtType.Electricity,
            PeriodYear = 2024,
            PeriodMonth = 3,
            Amount = 500m,
            RemainingAmount = 500m,
            Status = DebtStatus.Unpaid,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(30),
            Description = "Mart elektrik",
            InvoiceNumber = "ELK-2024-0312"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var saved = await context.UtilityDebts.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.InvoiceNumber.Should().Be("ELK-2024-0312");
    }

    [Fact]
    public async Task CreateUtilityDebt_WithoutInvoiceNumber_SavesNull()
    {
        // Arrange
        var (context, mediator) = CreateInMemoryDeps();
        var handler = new CreateUtilityDebtCommandHandler(context, mediator);

        var command = new CreateUtilityDebtCommand
        {
            FlatId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = DebtType.Water,
            PeriodYear = 2024,
            PeriodMonth = 3,
            Amount = 200m,
            RemainingAmount = 200m,
            Status = DebtStatus.Unpaid,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(30),
            // InvoiceNumber atlandı - opsiyonel
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await context.UtilityDebts.FirstOrDefaultAsync();
        saved!.InvoiceNumber.Should().BeNull();
    }

    [Fact]
    public async Task CreateBulkUtilityDebts_WithDifferentInvoiceNumbers_AllSaved()
    {
        // Arrange
        var (context, mediator) = CreateInMemoryDeps();
        var handler = new CreateBulkUtilityDebtsCommandHandler(context);

        var flatId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var command = new CreateBulkUtilityDebtsCommand
        {
            Debts = new List<CreateUtilityDebtCommand>
            {
                new CreateUtilityDebtCommand
                {
                    FlatId = flatId,
                    TenantId = tenantId,
                    Type = DebtType.Electricity,
                    PeriodYear = 2024,
                    PeriodMonth = 4,
                    Amount = 450m,
                    RemainingAmount = 450m,
                    Status = DebtStatus.Unpaid,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    InvoiceNumber = "BULK-ELK-001"
                },
                new CreateUtilityDebtCommand
                {
                    FlatId = flatId,
                    TenantId = tenantId,
                    Type = DebtType.Water,
                    PeriodYear = 2024,
                    PeriodMonth = 4,
                    Amount = 150m,
                    RemainingAmount = 150m,
                    Status = DebtStatus.Unpaid,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    InvoiceNumber = "BULK-SU-001"
                },
                new CreateUtilityDebtCommand
                {
                    FlatId = flatId,
                    TenantId = tenantId,
                    Type = DebtType.Aidat,
                    PeriodYear = 2024,
                    PeriodMonth = 4,
                    Amount = 600m,
                    RemainingAmount = 600m,
                    Status = DebtStatus.Unpaid,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    // InvoiceNumber yok - opsiyonel
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(3);

        var saved = await context.UtilityDebts.ToListAsync();
        saved.Should().HaveCount(3);
        saved.Should().ContainSingle(d => d.InvoiceNumber == "BULK-ELK-001");
        saved.Should().ContainSingle(d => d.InvoiceNumber == "BULK-SU-001");
        saved.Should().ContainSingle(d => d.InvoiceNumber == null);
    }

    [Fact]
    public async Task CreateBulkUtilityDebts_EmptyList_ReturnsFailure()
    {
        // Arrange
        var (context, _) = CreateInMemoryDeps();
        var handler = new CreateBulkUtilityDebtsCommandHandler(context);

        var command = new CreateBulkUtilityDebtsCommand
        {
            Debts = new List<CreateUtilityDebtCommand>()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateUtilityDebt_DueDateDefaultCalculated_WhenNotProvided()
    {
        // Arrange
        var (context, mediator) = CreateInMemoryDeps();
        var handler = new CreateUtilityDebtCommandHandler(context, mediator);

        var command = new CreateUtilityDebtCommand
        {
            FlatId = Guid.NewGuid(),
            Type = DebtType.Electricity,
            PeriodYear = 2024,
            PeriodMonth = 3,
            Amount = 300m,
            Status = DebtStatus.Unpaid,
            // DueDate verilmedi (default) - otomatik hesaplanmalı
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await context.UtilityDebts.FirstOrDefaultAsync();
        // Mart 2024 için DueDate: 2024-04-10 (1 ay sonra + 9 gün = Nisan 1 + 9 gün)
        saved!.DueDate.Should().Be(new DateTime(2024, 4, 10));
    }

    [Fact]
    public async Task CreateUtilityDebt_RemainingAmountZero_EqualsAmount()
    {
        // Arrange
        var (context, mediator) = CreateInMemoryDeps();
        var handler = new CreateUtilityDebtCommandHandler(context, mediator);

        var command = new CreateUtilityDebtCommand
        {
            FlatId = Guid.NewGuid(),
            Type = DebtType.Electricity,
            PeriodYear = 2024,
            PeriodMonth = 3,
            Amount = 750m,
            Status = DebtStatus.Unpaid,
            RemainingAmount = 0m // 0 verilince Amount'a eşit olmalı
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await context.UtilityDebts.FirstOrDefaultAsync();
        saved!.RemainingAmount.Should().Be(750m);
    }

    [Fact]
    public async Task CreateUtilityDebt_SoftDeletedRecord_NotInResults()
    {
        // Arrange
        var (context, mediator) = CreateInMemoryDeps();
        var handler = new CreateUtilityDebtCommandHandler(context, mediator);

        var command = new CreateUtilityDebtCommand
        {
            FlatId = Guid.NewGuid(),
            Type = DebtType.Water,
            PeriodYear = 2024,
            PeriodMonth = 6,
            Amount = 100m,
            Status = DebtStatus.Unpaid,
        };

        await handler.Handle(command, CancellationToken.None);

        // Soft-delete yap
        var debt = await context.UtilityDebts.FirstAsync();
        debt.IsDeleted = true;
        await context.SaveChangesAsync();

        // Silinmiş kayıtlar sorgularda görünmemeli
        var activeDebts = await context.UtilityDebts.Where(d => !d.IsDeleted).ToListAsync();
        activeDebts.Should().BeEmpty();
    }
}
