using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Flat> Flats => Set<Flat>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UtilityDebt> UtilityDebts { get; set; }
    public DbSet<AdvanceAccount> AdvanceAccounts => Set<AdvanceAccount>();
    public DbSet<AidatDefinition> AidatDefinitions => Set<AidatDefinition>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<UtilityBill> UtilityBills => Set<UtilityBill>();
    public DbSet<PaymentDebt> PaymentDebts => Set<PaymentDebt>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UtilityPricingConfiguration> UtilityPricingConfigurations => Set<UtilityPricingConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Global query filter for soft delete
        modelBuilder.Entity<Owner>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Flat>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UtilityDebt>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AdvanceAccount>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AidatDefinition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MeterReading>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UtilityBill>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PaymentDebt>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UtilityPricingConfiguration>().HasQueryFilter(e => !e.IsDeleted);
        
        
        
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
} 