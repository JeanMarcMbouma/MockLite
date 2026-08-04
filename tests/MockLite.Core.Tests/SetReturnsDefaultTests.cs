using BbQ.MockLite.Tests.Stubs;

namespace BbQ.MockLite.Tests;

// ==================== SETRETURNSDEFAULT TESTS ====================
public class SetReturnsDefaultTests
{
    [Fact]
    public void SetReturnsDefault_OverridesDefaultForType()
    {
        var mock = Mock.Create<ITestService>();
        mock.SetReturnsDefault<string>("default-string");

        // All string-returning methods should now return "default-string" by default
        Assert.Equal("default-string", mock.Object.GetValue("any-key"));
    }

    [Fact]
    public void SetReturnsDefault_ExplicitSetupStillWins()
    {
        var mock = Mock.Create<ITestService>();
        mock.SetReturnsDefault<string>("blanket-default");
        mock.Setup(x => x.GetValue("specific"), () => "specific-value");

        Assert.Equal("specific-value", mock.Object.GetValue("specific"));
    }

    [Fact]
    public void SetReturnsDefault_WorksWithCollectionTypes()
    {
        var items = new List<string> { "a", "b" };
        var mock = Mock.Create<ICollectionService>();
        mock.SetReturnsDefault<IEnumerable<string>>(items);

        Assert.Same(items, mock.Object.GetItems());
    }

    [Fact]
    public void SetReturnsDefault_ChainsWithOtherSetup()
    {
        var mock = Mock.Create<ITestService>();
        mock
            .SetReturnsDefault<string>("default")
            .Setup(x => x.GetNumber(It.IsAny<int>()), () => 42);

        Assert.Equal("default", mock.Object.GetValue("key"));
        Assert.Equal(42, mock.Object.GetNumber(1));
    }
}
