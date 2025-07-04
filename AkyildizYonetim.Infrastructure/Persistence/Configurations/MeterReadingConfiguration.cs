using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> builder)
    {
        builder.HasKey(mr => mr.Id);
        
        builder.Property(mr => mr.FlatId)
            .IsRequired();
            
        builder.Property(mr => mr.Type)
            .IsRequired();
            
        builder.Property(mr => mr.ReadingDate)
            .IsRequired();
            
        builder.Property(mr => mr.PeriodYear)
            .IsRequired();
            
        builder.Property(mr => mr.PeriodMonth)
            .IsRequired();
            
        builder.Property(mr => mr.ReadingValue)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(mr => mr.Consumption)
            .HasPrecision(18, 2);
            
        builder.Property(mr => mr.ReadingDate)
            .IsRequired();
            
        builder.Property(mr => mr.Note)
            .HasMaxLength(500);
            
        builder.Property(mr => mr.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(mr => mr.CreatedAt)
            .IsRequired();
            
        builder.Property(mr => mr.UpdatedAt);
        
        // Indexes
        builder.HasIndex(mr => mr.FlatId);
        builder.HasIndex(mr => mr.Type);
        builder.HasIndex(mr => mr.ReadingDate);
        
        // Foreign key relationship
        builder.HasOne(mr => mr.Flat)
            .WithMany()
            .HasForeignKey(mr => mr.FlatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 