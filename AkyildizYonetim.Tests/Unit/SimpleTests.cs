using Xunit;

namespace AkyildizYonetim.Tests.Unit;

public class SimpleTests
{
    [Fact]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        var expected = 2;
        var actual = 1 + 1;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AnotherSimpleTest_ShouldPass()
    {
        // Arrange
        var name = "Test";
        
        // Assert
        Assert.NotNull(name);
        Assert.Equal("Test", name);
    }
} 