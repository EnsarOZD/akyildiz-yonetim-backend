using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.Common.Extensions;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Expenses.Queries.GetExpenses;

public record GetExpensesQuery : IRequest<Result<PagedResult<ExpenseDto>>>
{
    public ExpenseType? Type { get; init; }
    public Guid? OwnerId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, Result<PagedResult<ExpenseDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetExpensesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<ExpenseDto>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        // Defense-in-depth: FinanceRead policy is the primary gate (Admin/Manager).
        // If somehow an external user reaches this handler, we explicitly reject.
        if (DataScopeHelper.IsScopeRestricted(_currentUserService, u => u.IsAdmin, u => u.IsManager))
        {
            return Result<PagedResult<ExpenseDto>>.Success(new PagedResult<ExpenseDto>());
        }

        var query = _context.Expenses
            .AsNoTracking()
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
                (e.Title != null && e.Title.ToLower().Contains(searchTerm)) ||
                (e.Description != null && e.Description.ToLower().Contains(searchTerm)) ||
                (e.ReceiptNumber != null && e.ReceiptNumber.ToLower().Contains(searchTerm)));
        }

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.Id)
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
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
 
        return Result<PagedResult<ExpenseDto>>.Success(expenses);
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