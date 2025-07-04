using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class AidatDefinitionConfiguration : IEntityTypeConfiguration<AidatDefinition>
{
    public void Configure(EntityTypeBuilder<AidatDefinition> builder)
    {
        builder.HasKey(ad => ad.Id);
        
        builder.Property(ad => ad.TenantId)
            .IsRequired();
            
        builder.Property(ad => ad.Unit)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(ad => ad.Year)
            .IsRequired();
            
        builder.Property(ad => ad.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(ad => ad.VatIncludedAmount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(ad => ad.IsActive)
            .IsRequired();
            
        builder.Property(ad => ad.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(ad => ad.CreatedAt)
            .IsRequired();
            
        builder.Property(ad => ad.UpdatedAt);
        
        // Indexes
        builder.HasIndex(ad => ad.TenantId);
        builder.HasIndex(ad => ad.Year);
        builder.HasIndex(ad => ad.Unit);
        builder.HasIndex(ad => ad.IsActive);
        
        // Foreign key relationship
        builder.HasOne(ad => ad.Tenant)
            .WithMany()
            .HasForeignKey(ad => ad.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 