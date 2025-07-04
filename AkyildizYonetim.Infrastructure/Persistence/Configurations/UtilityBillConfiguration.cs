using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class UtilityBillConfiguration : IEntityTypeConfiguration<UtilityBill>
{
    public void Configure(EntityTypeBuilder<UtilityBill> builder)
    {
        builder.HasKey(ub => ub.Id);
        
        builder.Property(ub => ub.Type)
            .IsRequired();
            
        builder.Property(ub => ub.PeriodYear)
            .IsRequired();
            
        builder.Property(ub => ub.PeriodMonth)
            .IsRequired();
            
        builder.Property(ub => ub.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(ub => ub.BillDate)
            .IsRequired();
            
        builder.Property(ub => ub.Description)
            .HasMaxLength(500);
            
        builder.Property(ub => ub.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(ub => ub.CreatedAt)
            .IsRequired();
            
        builder.Property(ub => ub.UpdatedAt);
        
        // Indexes
        builder.HasIndex(ub => ub.Type);
        builder.HasIndex(ub => ub.PeriodYear);
        builder.HasIndex(ub => ub.PeriodMonth);
        builder.HasIndex(ub => ub.BillDate);
    }
} 