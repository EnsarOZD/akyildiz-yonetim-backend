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

        // DB seviyesinde güvence: negatif ödeme olmasın
        builder.HasCheckConstraint("CK_PaymentDebts_PaidAmount_Positive", "[PaidAmount] >= 0");

        // Sorgu performansı
        builder.HasIndex(pd => pd.PaymentId);
        builder.HasIndex(pd => pd.DebtId);

        // İLİŞKİLER — tek sefer ve tutarlı
        builder.HasOne(pd => pd.Payment)
               .WithMany(p => p.PaymentDebts)
               .HasForeignKey(pd => pd.PaymentId)
               .OnDelete(DeleteBehavior.Restrict);   // veya Cascade, iş kuralına göre

        builder.HasOne(pd => pd.Debt)
               .WithMany()                           // UtilityDebt tarafında navigation yoksa böyle bırak
               .HasForeignKey(pd => pd.DebtId)
               .OnDelete(DeleteBehavior.Restrict);   // veya Cascade, iş kuralına göre

        // İŞ KURALI: Aynı ödeme + aynı borç için tek satır olsun istiyorsan aç:
        // builder.HasIndex(pd => new { pd.PaymentId, pd.DebtId }).IsUnique();

        // Eşzamanlılık istersen:
        // builder.Property(pd => pd.RowVersion).IsRowVersion();
    }
}
