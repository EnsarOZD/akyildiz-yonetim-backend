using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AkyildizYonetim.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Development ortamı için connection string
        var connectionString = "Server=ENSAROZD\\SQLEXPRESS2025;Database=AkyildizYonetimDB;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true;";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
} 