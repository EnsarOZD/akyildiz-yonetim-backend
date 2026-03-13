using System.Reflection;
using System.Linq.Expressions;
using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Flat> Flats => Set<Flat>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UtilityDebt> UtilityDebts => Set<UtilityDebt>();
    public DbSet<AdvanceAccount> AdvanceAccounts => Set<AdvanceAccount>();
    public DbSet<AidatDefinition> AidatDefinitions => Set<AidatDefinition>();
    public DbSet<PaymentDebt> PaymentDebts => Set<PaymentDebt>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserPushSubscription> UserPushSubscriptions => Set<UserPushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Global query filter for soft delete using reflection
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) && !entityType.IsOwned() && !entityType.IsKeyless)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var lambda = Expression.Lambda(Expression.Not(property), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
        
        // Ensure all DateTime properties are Utc
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsKeyless) continue;

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
        
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaving()
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
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    if (!entry.Entity.IsDeleted)
                    {
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        entry.Entity.DeletedByUserId = _currentUserService.UserId;
                    }
                    break;
            }
        }
    }
}