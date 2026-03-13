using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AkyildizYonetim.Tests.Unit;

public class SoftDeleteTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public SoftDeleteTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SaveChanges_ShouldSetSoftDeleteMetadata_WhenEntityIsDeleted()
    {
        // Arrange
        var userId = "test-user-id";
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        using var context = new ApplicationDbContext(_options, _currentUserServiceMock.Object);
        
        var owner = new Owner { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Software" };
        context.Owners.Add(owner);
        await context.SaveChangesAsync();

        // Act
        context.Owners.Remove(owner);
        await context.SaveChangesAsync();

        // Assert
        var deletedOwner = await context.Owners.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == owner.Id);
        Assert.NotNull(deletedOwner);
        Assert.True(deletedOwner.IsDeleted);
        Assert.NotNull(deletedOwner.DeletedAt);
        Assert.Equal(userId, deletedOwner.DeletedByUserId);
        Assert.Equal(EntityState.Unchanged, context.Entry(deletedOwner).State);
    }

    [Fact]
    public async Task Query_ShouldExcludesSoftDeletedEntities_ByDefault()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options, _currentUserServiceMock.Object);
        
        var owner1 = new Owner { Id = Guid.NewGuid(), FirstName = "Active", LastName = "User" };
        var owner2 = new Owner { Id = Guid.NewGuid(), FirstName = "Deleted", LastName = "User", IsDeleted = true };
        
        context.Owners.AddRange(owner1, owner2);
        await context.SaveChangesAsync();

        // Act
        var activeOwners = await context.Owners.ToListAsync();

        // Assert
        Assert.Single(activeOwners);
        Assert.Equal("Active", activeOwners[0].FirstName);
    }

    [Fact]
    public async Task SoftDelete_ShouldBeIdempotent_AndPreserveOriginalMetadata()
    {
        // Arrange
        var userId1 = "user-1";
        var userId2 = "user-2";
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId1);
        using var context = new ApplicationDbContext(_options, _currentUserServiceMock.Object);
        
        var owner = new Owner { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Idempotence" };
        context.Owners.Add(owner);
        await context.SaveChangesAsync();

        // First Delete
        context.Owners.Remove(owner);
        await context.SaveChangesAsync();
        
        var firstDeletedAt = (await context.Owners.IgnoreQueryFilters().FirstAsync(o => o.Id == owner.Id)).DeletedAt;

        // Second Delete with different user
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId2);
        context.Owners.Remove(owner);
        await context.SaveChangesAsync();

        // Assert
        var finalOwner = await context.Owners.IgnoreQueryFilters().FirstAsync(o => o.Id == owner.Id);
        Assert.True(finalOwner.IsDeleted);
        Assert.Equal(userId1, finalOwner.DeletedByUserId); // Still user-1
        Assert.Equal(firstDeletedAt, finalOwner.DeletedAt); // Still original timestamp
    }
}
