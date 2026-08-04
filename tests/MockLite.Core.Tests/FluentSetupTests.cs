using BbQ.MockLite.Tests.Stubs;

namespace BbQ.MockLite.Tests;

// ==================== FLUENT SETUP + RETURNS / RETURNSASYNC TESTS ====================
public class FluentSetupTests
{
    [Fact]
    public void Setup_Returns_SetsReturnValue()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue("key")).Returns("value");

        var result = mock.Object.GetValue("key");
        Assert.Equal("value", result);
    }

    [Fact]
    public void Setup_Returns_WithFactory()
    {
        var mock = Mock.Create<ITestService>();
        int callCount = 0;
        mock.Setup(x => x.GetNumber(It.IsAny<int>())).Returns(() => ++callCount);

        Assert.Equal(1, mock.Object.GetNumber(1));
        Assert.Equal(2, mock.Object.GetNumber(2));
    }

    [Fact]
    public void Setup_Returns_WithFactory_ReceivesArguments()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue(It.IsAny<string>()))
            .Returns((string key) => $"value-for-{key}");
        Assert.Equal("value-for-a", mock.Object.GetValue("a"));
        Assert.Equal("value-for-b", mock.Object.GetValue("b"));
    }

    [Fact]
    public void Setup_Returns_WithFactory_ReceivesTwoArguments()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<int>()))
            .Returns((string str, int count) => $"value-for-{count}");
        Assert.Equal("value-for-1", mock.Object.GetValue("x", 1));
        Assert.Equal("value-for-2", mock.Object.GetValue("y", 2));
    }

    [Fact]
    public void Setup_Returns_WithFactory_ReceivesThreeArguments()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns((string str, int count, bool flag) => $"value-for-{count}-{flag}");
        Assert.Equal("value-for-1-True", mock.Object.GetValue("x", 1, true));
        Assert.Equal("value-for-2-False", mock.Object.GetValue("y", 2, false));
    }

    [Fact]
    public async Task Setup_ReturnsAsync_WithFactory_ReceivesArguments()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValueAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => $"async-value-for-{key}");
        Assert.Equal("async-value-for-a", await mock.Object.GetValueAsync("a"));
        Assert.Equal("async-value-for-b", await mock.Object.GetValueAsync("b"));
    }

    [Fact]
    public async Task Setup_ReturnsAsync_WithFactory_ReceivesTwoArguments()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string str, int count) => $"async-value-for-{count}");
        Assert.Equal("async-value-for-1", await mock.Object.GetValueAsync("x", 1));
        Assert.Equal("async-value-for-2", await mock.Object.GetValueAsync("y", 2));
    }

    [Fact]
    public async Task Setup_ReturnsAsync_WithFactory_ReceivesThreeArguments()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((string str, int count, bool flag) => $"async-value-for-{count}-{flag}");
        Assert.Equal("async-value-for-1-True", await mock.Object.GetValueAsync("x", 1, true));
        Assert.Equal("async-value-for-2-False", await mock.Object.GetValueAsync("y", 2, false));
    }

    [Fact]
    public async Task Setup_ReturnsAsync_WrapsValueInTask()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValueAsync("key")).ReturnsAsync("async-value");

        var result = await mock.Object.GetValueAsync("key");
        Assert.Equal("async-value", result);
    }

    [Fact]
    public async Task Setup_ReturnsAsync_HandlesCovariance_ArrayToIEnumerable()
    {
        // This is the key covariance test: method returns Task<IEnumerable<string>>
        // but we pass string[] to ReturnsAsync
        var mock = Mock.Create<ICovariantService>();
        mock.Setup(x => x.GetStringsAsync()).ReturnsAsync(new[] { "a", "b", "c" });

        var result = await mock.Object.GetStringsAsync();
        Assert.Equal(new[] { "a", "b", "c" }, result);
    }

    [Fact]
    public async Task Setup_ReturnsAsync_HandlesCovariance_ListToIReadOnlyList()
    {
        var mock = Mock.Create<ICovariantService>();
        mock.Setup(x => x.GetIntsAsync()).ReturnsAsync(new List<int> { 1, 2, 3 });

        var result = await mock.Object.GetIntsAsync();
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void Setup_Returns_ChainsCorrectly()
    {
        var mock = Mock.Create<ITestService>();
        mock
            .Setup(x => x.GetValue("a")).Returns("A")
            .Setup(x => x.GetValue("b")).Returns("B");

        Assert.Equal("A", mock.Object.GetValue("a"));
        Assert.Equal("B", mock.Object.GetValue("b"));
    }

    [Fact]
    public void Setup_Throws_ThrowsException()
    {
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue("bad")).Throws(new InvalidOperationException("test error"));

        var ex = Assert.Throws<InvalidOperationException>(() => mock.Object.GetValue("bad"));
        Assert.Equal("test error", ex.Message);
    }
}
