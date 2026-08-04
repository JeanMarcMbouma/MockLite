using BbQ.MockLite.Tests.Stubs;

namespace BbQ.MockLite.Tests;

// ==================== SMART DEFAULTS TESTS ====================
public class SmartDefaultTests
{
    [Fact]
    public void IEnumerable_Default_ReturnsEmptyNotNull()
    {
        var mock = Mock.Create<ICollectionService>();
        var result = mock.Object.GetItems();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void IReadOnlyList_Default_ReturnsEmptyNotNull()
    {
        var mock = Mock.Create<ICollectionService>();
        var result = mock.Object.GetNumbers();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void IList_Default_ReturnsEmptyNotNull()
    {
        var mock = Mock.Create<ICollectionService>();
        var result = mock.Object.GetMutableItems();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void IReadOnlyCollection_Default_ReturnsEmptyNotNull()
    {
        var mock = Mock.Create<ICollectionService>();
        var result = mock.Object.GetReadOnlyCollection();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ICollection_Default_ReturnsEmptyNotNull()
    {
        var mock = Mock.Create<ICollectionService>();
        var result = mock.Object.GetCollection();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task TaskOfIEnumerable_Default_ReturnsCompletedTaskWithEmptyCollection()
    {
        var mock = Mock.Create<ICollectionService>();
        var task = mock.Object.GetItemsAsync();

        Assert.NotNull(task);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);

        var result = await task;
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task TaskOfIReadOnlyList_Default_ReturnsCompletedTaskWithEmptyCollection()
    {
        var mock = Mock.Create<ICollectionService>();
        var task = mock.Object.GetNumbersAsync();

        Assert.NotNull(task);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);

        var result = await task;
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExistingDefault_Task_StillReturnsCompletedTask()
    {
        // Ensure Task-returning methods still return completed tasks (existing behavior)
        var mock = Mock.Create<ITestService>();
        var task = mock.Object.DoSomethingAsync();

        Assert.NotNull(task);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        await task; // Should not throw
    }

    [Fact]
    public async Task ExistingDefault_TaskOfString_StillReturnsCompletedTask()
    {
        var mock = Mock.Create<ITestService>();
        var task = mock.Object.GetValueAsync("test");

        Assert.NotNull(task);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        // String default is null (not a collection)
        Assert.Null(await task);
    }
}
