using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Expenses.Queries.GetExpenseById;

public record GetExpenseByIdQuery : IRequest<Result<ExpenseDto>>
{
    public Guid Id { get; init; }
}

public class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, Result<ExpenseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExpenseByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExpenseDto>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
            .Where(e => e.Id == request.Id && !e.IsDeleted)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Title = e.Title,
                Amount = e.Amount,
                Type = e.Type,
                ExpenseDate = e.ExpenseDate,
                Description = e.Description,
                ReceiptNumber = e.ReceiptNumber,
                OwnerId = e.OwnerId,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (expense == null)
            return Result<ExpenseDto>.Failure("Gider bulunamadı.");

        return Result<ExpenseDto>.Success(expense);
    }
}

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseType Type { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNumber { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 