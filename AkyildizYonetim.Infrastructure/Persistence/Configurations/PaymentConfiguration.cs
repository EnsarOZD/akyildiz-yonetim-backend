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
       .HasMaxLength(500)
       .IsUnicode(false);
            
        builder.Property(p => p.ReceiptNumber).HasMaxLength(100).IsUnicode(false);
            
        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(p => p.CreatedAt)
            .IsRequired();
            
        builder.Property(p => p.UpdatedAt);

        

        builder.ToTable(t => t.HasCheckConstraint("CK_Payments_Amount_Positive", "[Amount] >= 0"));
        
        // Indexes
        builder.HasIndex(p => p.PaymentDate);
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => new { p.TenantId, p.PaymentDate });
        builder.HasIndex(p => new { p.OwnerId,  p.PaymentDate });
        builder.HasIndex(p => p.ReceiptNumber)
       .IsUnique()
       .HasFilter("[IsDeleted] = 0 AND [ReceiptNumber] IS NOT NULL");

        // Relationships
        builder.HasOne(p => p.Owner)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Tenant)
       .WithMany(t => t.Payments)
       .HasForeignKey(p => p.TenantId)
       .OnDelete(DeleteBehavior.SetNull); 
    }
} 