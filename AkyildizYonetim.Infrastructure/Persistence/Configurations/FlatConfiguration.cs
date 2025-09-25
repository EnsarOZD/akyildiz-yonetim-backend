using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static AkyildizYonetim.Domain.Entities.Enums.FlatEnums;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class FlatConfiguration : IEntityTypeConfiguration<Flat>
{
    public void Configure(EntityTypeBuilder<Flat> builder)
    {
        builder.ToTable("Flats");

        // PK
        builder.HasKey(f => f.Id);

        // === Yeni alanlar ===
        builder.Property(f => f.Code)
            .IsRequired()
            .HasMaxLength(32);

		
		builder.HasIndex(f => f.Code)
		       .IsUnique()
               .HasFilter("[IsDeleted] = 0") //sql değişirse sil
			   .HasDatabaseName("UX_Flats_Code_ActiveOnly");

		builder.Property(f => f.FloorNumber); // nullable (OTOPARK için)

        builder.Property(f => f.Section)
            .HasMaxLength(4);

        builder.Property(f => f.Type)
            .HasConversion<string>()   // Enum'u string sakla
            .HasMaxLength(16)
            .HasDefaultValue(UnitType.Floor);

        builder.Property(f => f.GroupKey)
            .HasMaxLength(8);

        builder.HasIndex(f => f.GroupKey);

        builder.Property(f => f.GroupStrategy)
            .HasConversion<string>()   // Enum'u string sakla
            .HasMaxLength(24)
            .HasDefaultValue(GroupStrategy.None);

        builder.Property(f => f.IsOccupied)
            .IsRequired()
            .HasDefaultValue(false);

        // === Mevcut (geriye dönük) alanlar ===
        builder.Property(f => f.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.UnitNumber)
            .IsRequired()
            .HasMaxLength(50);

        // DİKKAT: Eskiden Floor vardı; artık FloorNumber kullanıyoruz.
        builder.HasIndex(f => f.FloorNumber);

        builder.Property(f => f.UnitArea)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(f => f.IsActive)
            .IsRequired();

        builder.Property(f => f.ShareCount)
            .IsRequired()
            .HasDefaultValue(1);

        // İş Hanı özel alanları
        builder.Property(f => f.MonthlyRent)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        // BaseEntity alanları (varsayıyorum)
        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // === Indexler ===
        builder.HasIndex(f => f.Number);
        builder.HasIndex(f => f.UnitNumber);
        builder.HasIndex(f => f.OwnerId);
        builder.HasIndex(f => f.TenantId);
        builder.HasIndex(f => f.IsActive);
        builder.HasIndex(f => f.IsOccupied);
        builder.HasIndex(f => f.Type);           // sorgularda faydalı olur
        builder.HasIndex(f => f.GroupStrategy);  // sorgularda faydalı olur

        // === İlişkiler ===
        builder.HasOne(f => f.Owner)
            .WithMany(o => o.Flats)
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Tenant)
            .WithMany(t => t.Flats)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
