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
        
        modelBuilder.Entity<Flat>()
            .HasOne(f => f.Owner)
            .WithMany(o => o.Flats)
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Flat>()
            .HasOne(f => f.Tenant)
            .WithMany(t => t.Flats)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<User>()
            .HasOne(u => u.Owner)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<AdvanceAccount>()
            .HasOne(aa => aa.Tenant)
            .WithMany()
            .HasForeignKey(aa => aa.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        
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