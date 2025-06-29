using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(o => o.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(o => o.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(o => o.ApartmentNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(o => o.MonthlyDues)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(o => o.IsActive)
            .IsRequired();
            
        builder.Property(o => o.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(o => o.CreatedAt)
            .IsRequired();
            
        builder.Property(o => o.UpdatedAt);
        
        // Indexes
        builder.HasIndex(o => o.Email).IsUnique();
        builder.HasIndex(o => o.ApartmentNumber).IsUnique();
        builder.HasIndex(o => o.PhoneNumber);
    }
} 