using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Owner> Owners { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<Flat> Flats { get; }
    DbSet<User> Users { get; }
    DbSet<UtilityDebt> UtilityDebts { get; set; }
    DbSet<AdvanceAccount> AdvanceAccounts { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
} 