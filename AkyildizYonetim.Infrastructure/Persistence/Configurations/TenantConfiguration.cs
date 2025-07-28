using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        
        // İş Yeri Bilgileri
        builder.Property(t => t.CompanyName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(t => t.BusinessType)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.TaxNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        // İletişim Kişisi Bilgileri
        builder.Property(t => t.ContactPersonName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.ContactPersonPhone)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(t => t.ContactPersonEmail)
            .HasMaxLength(255);
        
        // Sözleşme Bilgileri
        builder.Property(t => t.ContractStartDate);
        builder.Property(t => t.ContractEndDate);
        
        // Aidat ve Borç Yönetimi
        builder.Property(t => t.MonthlyAidat)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(t => t.IsActive)
            .IsRequired();
            
        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(t => t.CreatedAt)
            .IsRequired();
            
        builder.Property(t => t.UpdatedAt);
        
        // Indexes
        builder.HasIndex(t => t.CompanyName);
        builder.HasIndex(t => t.TaxNumber).IsUnique();
        builder.HasIndex(t => t.ContactPersonEmail);
        builder.HasIndex(t => t.ContactPersonPhone);
        builder.HasIndex(t => t.IsActive);
        
        // Relationships
        builder.HasMany(t => t.Flats)
            .WithOne(f => f.Tenant)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(t => t.Payments)
            .WithOne(p => p.Tenant)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 