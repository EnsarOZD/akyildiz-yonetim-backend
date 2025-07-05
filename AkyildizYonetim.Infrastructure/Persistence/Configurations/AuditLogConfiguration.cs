using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);
        
        builder.Property(al => al.Action)
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(al => al.EntityType)
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(al => al.EntityId)
            .IsRequired();
        
        builder.Property(al => al.UserId)
            .IsRequired()
            .HasMaxLength(450);
        
        builder.Property(al => al.UserName)
            .HasMaxLength(200);
        
        builder.Property(al => al.Description)
            .HasMaxLength(1000);
        
        builder.Property(al => al.OldValues)
            .HasColumnType("nvarchar(max)");
        
        builder.Property(al => al.NewValues)
            .HasColumnType("nvarchar(max)");
        
        builder.Property(al => al.IpAddress)
            .HasMaxLength(45); // IPv6 için
        
        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);
        
        builder.Property(al => al.Timestamp)
            .IsRequired();
        
        // Indexes for better performance
        builder.HasIndex(al => al.EntityType);
        builder.HasIndex(al => al.EntityId);
        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => al.Timestamp);
        builder.HasIndex(al => new { al.EntityType, al.EntityId });
    }
} 