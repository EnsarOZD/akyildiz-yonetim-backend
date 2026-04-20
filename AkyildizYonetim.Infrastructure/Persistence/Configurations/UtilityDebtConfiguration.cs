using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class UtilityDebtConfiguration : IEntityTypeConfiguration<UtilityDebt>
{
    public void Configure(EntityTypeBuilder<UtilityDebt> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.FlatId)
            .IsRequired();
            
        builder.Property(d => d.Type)
            .IsRequired();
            
        builder.Property(d => d.PeriodYear)
            .IsRequired();
            
        builder.Property(d => d.PeriodMonth)
            .IsRequired();
            
        builder.Property(d => d.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(d => d.Status)
            .IsRequired();
            
        builder.Property(d => d.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(d => d.RemainingAmount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(d => d.PaidDate);
            
        builder.Property(d => d.Description)
            .HasMaxLength(500);
            
        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(d => d.CreatedAt)
            .IsRequired();
            
        builder.Property(d => d.UpdatedAt);
        
        // Single-column indexes
        builder.HasIndex(d => d.FlatId);
        builder.HasIndex(d => d.Type);
        builder.HasIndex(d => d.PeriodYear);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.TenantId);
        builder.HasIndex(d => d.OwnerId);

        // Composite indexes — en sık kullanılan filtre kombinasyonları
        builder.HasIndex(d => new { d.TenantId, d.Status, d.PeriodYear })
            .HasDatabaseName("IX_UtilityDebts_TenantId_Status_PeriodYear");

        builder.HasIndex(d => new { d.OwnerId, d.Status, d.PeriodYear })
            .HasDatabaseName("IX_UtilityDebts_OwnerId_Status_PeriodYear");

        builder.HasIndex(d => new { d.FlatId, d.PeriodYear, d.PeriodMonth, d.Status })
            .HasDatabaseName("IX_UtilityDebts_FlatId_Period_Status");

        builder.HasIndex(d => new { d.Status, d.DueDate })
            .HasDatabaseName("IX_UtilityDebts_Status_DueDate");
    }
} 