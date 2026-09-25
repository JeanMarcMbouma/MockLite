namespace BbQ.MockLite.Tests;

public class ReturnsAsyncPartialFactoryRegressionTests
{
    private interface IAsyncLookup
    {
        Task<string> FindAsync(string id, bool includeInactive, int limit);
    }

    [Fact]
    public async Task ReturnsAsync_OneArgumentFactory_AllowsTrailingMethodParameters()
    {
        var mock = Mock.Create<IAsyncLookup>();
        mock.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
            .ReturnsAsync((string id) => $"user:{id}");

        var result = await mock.Object.FindAsync("42", true, 10);

        Assert.Equal("user:42", result);
    }

    [Fact]
    public async Task ReturnsAsync_TwoArgumentFactory_AllowsTrailingMethodParameters()
    {
        var mock = Mock.Create<IAsyncLookup>();
        mock.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
            .ReturnsAsync((string id, bool includeInactive) => $"{id}:{includeInactive}");

        var result = await mock.Object.FindAsync("42", true, 10);

        Assert.Equal("42:True", result);
    }

    [Fact]
    public async Task ReturnsAsync_PartialFactory_PropagatesUserException()
    {
        var mock = Mock.Create<IAsyncLookup>();
        mock.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
            .ReturnsAsync((string id) => id == "bad"
                ? throw new InvalidOperationException("bad id")
                : id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mock.Object.FindAsync("bad", false, 1));

        Assert.Equal("bad id", ex.Message);
    }
}
