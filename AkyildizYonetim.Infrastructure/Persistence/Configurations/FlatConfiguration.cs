using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class FlatConfiguration : IEntityTypeConfiguration<Flat>
{
    public void Configure(EntityTypeBuilder<Flat> builder)
    {
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.ApartmentNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(f => f.Floor)
            .IsRequired();
            
        builder.Property(f => f.RoomCount)
            .IsRequired();
            
        builder.Property(f => f.IsActive)
            .IsRequired();
            
        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(f => f.CreatedAt)
            .IsRequired();
            
        builder.Property(f => f.UpdatedAt);
        
        // Indexes
        builder.HasIndex(f => f.ApartmentNumber).IsUnique();
        builder.HasIndex(f => f.OwnerId);
        builder.HasIndex(f => f.TenantId);
        builder.HasIndex(f => f.IsActive);
    }
} 