using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255).IsUnicode(false);
            
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(u => u.Role)
            .IsRequired();
            
        builder.Property(u => u.IsActive)
            .IsRequired();
            
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(u => u.CreatedAt)
            .IsRequired();
            
        builder.Property(u => u.UpdatedAt);
        
        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(100);
            
        builder.Property(u => u.ResetTokenExpires);
        
        // Indexes
        builder.HasIndex(u => u.Email)
       .IsUnique()
       .HasFilter("[IsDeleted] = 0"); // SQL Server: sadece aktif kayıt benzersiz
        builder.HasIndex(u => u.Role);
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.OwnerId);
        builder.HasIndex(u => u.TenantId);

        // Relationships
        builder.HasOne(u => u.Owner)
       .WithMany(o => o.Users)
       .HasForeignKey(u => u.OwnerId)
       .OnDelete(DeleteBehavior.SetNull); 

        builder.HasOne(u => u.Tenant)
       .WithMany(t => t.Users)
       .HasForeignKey(u => u.TenantId)
       .OnDelete(DeleteBehavior.SetNull); 
    }
} 