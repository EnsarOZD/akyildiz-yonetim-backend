using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class PaymentDebtConfiguration : IEntityTypeConfiguration<PaymentDebt>
{
    public void Configure(EntityTypeBuilder<PaymentDebt> builder)
    {
        builder.HasKey(pd => pd.Id);
        
        builder.Property(pd => pd.PaidAmount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.HasOne(pd => pd.Payment)
            .WithMany(p => p.PaymentDebts)
            .HasForeignKey(pd => pd.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(pd => pd.Debt)
            .WithMany()
            .HasForeignKey(pd => pd.DebtId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 