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
    DbSet<UtilityDebt> UtilityDebts { get; }
    DbSet<AdvanceAccount> AdvanceAccounts { get; }
    DbSet<AidatDefinition> AidatDefinitions { get; }
    DbSet<PaymentDebt> PaymentDebts { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<UserPushSubscription> UserPushSubscriptions { get; }
    
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
} 