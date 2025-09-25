using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class UtilityPricingConfigurationConfiguration : IEntityTypeConfiguration<UtilityPricingConfiguration>
{
    public void Configure(EntityTypeBuilder<UtilityPricingConfiguration> builder)
    {
        builder.HasKey(upc => upc.Id);
        
        builder.Property(upc => upc.MeterType)
            .IsRequired();
            
        builder.Property(upc => upc.Year)
            .IsRequired();
            
        builder.Property(upc => upc.Month)
            .IsRequired();
            
        builder.Property(upc => upc.UnitPrice)
            .HasPrecision(18, 4)
            .IsRequired();
            
        builder.Property(upc => upc.VatRate)
            .HasPrecision(5, 2)
            .IsRequired();
            
        builder.Property(upc => upc.BtvRate)
            .HasPrecision(5, 2)
            .IsRequired();
            
        builder.Property(upc => upc.EffectiveDate)
            .IsRequired();
            
        builder.Property(upc => upc.ExpiryDate);
            
        builder.Property(upc => upc.Description)
            .HasMaxLength(200);
            
        builder.Property(upc => upc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(upc => upc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(upc => upc.CreatedAt)
            .IsRequired();
            
        builder.Property(upc => upc.UpdatedAt);
        
        // Indexes
        builder.HasIndex(upc => new { upc.MeterType, upc.Year, upc.Month })
            .HasDatabaseName("IX_UtilityPricing_Type_Year_Month");
            
        builder.HasIndex(upc => new { upc.EffectiveDate, upc.ExpiryDate })
            .HasDatabaseName("IX_UtilityPricing_EffectiveDate");
            
        builder.HasIndex(upc => upc.IsActive)
            .HasDatabaseName("IX_UtilityPricing_IsActive");
    }
}
