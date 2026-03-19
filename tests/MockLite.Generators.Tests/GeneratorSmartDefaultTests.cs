namespace BbQ.MockLite.Generators.Tests;

public class GeneratorSmartDefaultTests
{
    [Fact]
    public void GeneratedMock_IEnumerable_Default_ReturnsEmptyNotNull()
    {
        var mock = new MockCollectionReturningService();
        var result = mock.GetItems();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GeneratedMock_TaskOfIEnumerable_Default_ReturnsEmptyNotNull()
    {
        var mock = new MockCollectionReturningService();
        var result = await mock.GetItemsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GeneratedMock_IReadOnlyList_Default_ReturnsEmptyNotNull()
    {
        var mock = new MockCollectionReturningService();
        var result = mock.GetNumbers();

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
