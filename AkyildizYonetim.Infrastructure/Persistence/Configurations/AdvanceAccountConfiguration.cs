using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class AdvanceAccountConfiguration : IEntityTypeConfiguration<AdvanceAccount>
{
    public void Configure(EntityTypeBuilder<AdvanceAccount> builder)
    {
        builder.HasKey(aa => aa.Id);
        
        builder.Property(aa => aa.TenantId)
            .IsRequired();
            
        builder.Property(aa => aa.Balance)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(aa => aa.Description)
            .HasMaxLength(500);
            
        builder.Property(aa => aa.IsActive)
            .IsRequired();
            
        builder.Property(aa => aa.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(aa => aa.CreatedAt)
            .IsRequired();
            
        builder.Property(aa => aa.UpdatedAt);
        
        // Indexes
        builder.HasIndex(aa => aa.TenantId);
        builder.HasIndex(aa => aa.IsActive);
    }
} 