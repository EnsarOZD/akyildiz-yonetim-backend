using AkyildizYonetim.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkyildizYonetim.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(e => e.Type)
            .IsRequired();
            
        builder.Property(e => e.ExpenseDate)
            .IsRequired();
            
        builder.Property(e => e.Description)
            .HasMaxLength(500);
            
        builder.Property(e => e.ReceiptNumber)
            .HasMaxLength(100);
            
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        builder.Property(e => e.UpdatedAt);
        
        // Indexes
        builder.HasIndex(e => e.ExpenseDate);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.OwnerId);
    }
} 