using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class FlatConfiguration : IEntityTypeConfiguration<Flat>
{
    public void Configure(EntityTypeBuilder<Flat> builder)
    {
        builder.HasKey(f => f.Id);
        
        // Temel Bilgiler
        builder.Property(f => f.Number)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(f => f.UnitNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(f => f.Floor)
            .IsRequired();
            
        builder.Property(f => f.UnitArea)
            .HasPrecision(10, 2)
            .IsRequired();
            
        builder.Property(f => f.RoomCount)
            .IsRequired();
            
        builder.Property(f => f.IsActive)
            .IsRequired();
            
        builder.Property(f => f.IsOccupied)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(f => f.Category)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Normal");
            
        builder.Property(f => f.ShareCount)
            .IsRequired()
            .HasDefaultValue(1);
        
        // İş Hanı Özel Alanları
        builder.Property(f => f.BusinessType)
            .HasMaxLength(100);
            
        builder.Property(f => f.MonthlyRent)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
            
        builder.Property(f => f.Description)
            .HasMaxLength(500);
            
        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(f => f.CreatedAt)
            .IsRequired();
            
        builder.Property(f => f.UpdatedAt);
        
        // Indexes
        builder.HasIndex(f => f.Number);
        builder.HasIndex(f => f.UnitNumber);
        builder.HasIndex(f => f.Floor);
        builder.HasIndex(f => f.OwnerId);
        builder.HasIndex(f => f.TenantId);
        builder.HasIndex(f => f.IsActive);
        builder.HasIndex(f => f.IsOccupied);
        builder.HasIndex(f => f.Category);
        
        // Relationships
        builder.HasOne(f => f.Owner)
            .WithMany()
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(f => f.Tenant)
            .WithMany(t => t.Flats)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 