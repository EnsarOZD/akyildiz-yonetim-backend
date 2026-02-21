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
            
            
        builder.Property(t => t.IdentityNumber)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);
        
        // İletişim Kişisi Bilgileri
        builder.Property(t => t.ContactPersonName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.ContactPersonPhone)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);
            
        builder.Property(t => t.ContactPersonEmail)
            .HasMaxLength(255);
        
        
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
        builder.HasIndex(t => t.IdentityNumber)
        .IsUnique()
        .HasFilter("[IsDeleted] = 0"); // SQL Server: sadece aktif kayıt benzersiz
        builder.HasIndex(t => t.ContactPersonEmail);
        builder.HasIndex(t => t.ContactPersonPhone);
        builder.HasIndex(t => t.IsActive);        
        
    }
} 