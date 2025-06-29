using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Expenses.Queries.GetExpenses;

public record GetExpensesQuery : IRequest<Result<List<ExpenseDto>>>
{
    public ExpenseType? Type { get; init; }
    public Guid? OwnerId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? SearchTerm { get; init; }
}

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, Result<List<ExpenseDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetExpensesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ExpenseDto>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Expenses
            .Where(e => !e.IsDeleted)
            .AsQueryable();

        if (request.Type.HasValue)
            query = query.Where(e => e.Type == request.Type.Value);

        if (request.OwnerId.HasValue)
            query = query.Where(e => e.OwnerId == request.OwnerId.Value);

        if (request.StartDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= request.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(searchTerm) ||
                e.Description.ToLower().Contains(searchTerm) ||
                e.ReceiptNumber.ToLower().Contains(searchTerm));
        }

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
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
            .ToListAsync(cancellationToken);

        return Result<List<ExpenseDto>>.Success(expenses);
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