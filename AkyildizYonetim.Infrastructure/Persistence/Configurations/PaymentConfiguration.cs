using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(p => p.Type)
            .IsRequired();
            
        builder.Property(p => p.Status)
            .IsRequired();
            
        builder.Property(p => p.PaymentDate)
            .IsRequired();
            
        builder.Property(p => p.Description)
            .HasMaxLength(500);
            
        builder.Property(p => p.ReceiptNumber)
            .HasMaxLength(100);
            
        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(p => p.CreatedAt)
            .IsRequired();
            
        builder.Property(p => p.UpdatedAt);
        
        // Indexes
        builder.HasIndex(p => p.PaymentDate);
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.TenantId);
    }
} 