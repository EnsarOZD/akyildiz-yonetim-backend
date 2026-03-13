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

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeCurrentUserService());
    }

    private class DesignTimeCurrentUserService : AkyildizYonetim.Application.Common.Interfaces.ICurrentUserService
    {
        public string? UserId => null;
        public string? Role => null;
        public Guid? TenantId => null;
        public Guid? OwnerId => null;
        public bool IsAdmin => false;
        public bool IsManager => false;
        public bool IsDataEntry => false;
        public bool IsObserver => false;
    }
}