using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(t => t.ApartmentNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(t => t.LeaseStartDate)
            .IsRequired();
            
        builder.Property(t => t.LeaseEndDate);
            
        builder.Property(t => t.MonthlyRent)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(t => t.IsActive)
            .IsRequired();
            
        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(t => t.CreatedAt)
            .IsRequired();
            
        builder.Property(t => t.UpdatedAt);
        
        // Indexes
        builder.HasIndex(t => t.Email).IsUnique();
        builder.HasIndex(t => t.ApartmentNumber).IsUnique();
        builder.HasIndex(t => t.PhoneNumber);
        builder.HasIndex(t => t.IsActive);
    }
} 