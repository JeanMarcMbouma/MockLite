using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BbQ.MockLite.Tests;

// --- Interfaces for testing new features ---

public interface ICollectionService
{
    IEnumerable<string> GetItems();
    IReadOnlyList<int> GetNumbers();
    IList<string> GetMutableItems();
    IReadOnlyCollection<string> GetReadOnlyCollection();
    ICollection<string> GetCollection();
    Task<IEnumerable<string>> GetItemsAsync();
    Task<IReadOnlyList<int>> GetNumbersAsync();
}

public interface ICovariantService
{
    Task<IEnumerable<string>> GetStringsAsync();
    Task<IReadOnlyList<int>> GetIntsAsync();
}

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

// ==================== VERIFY WITH MESSAGE TESTS ====================
public class VerifyWithMessageTests
{
    [Fact]
    public void Verify_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<ITestService>();
        mock.Object.GetValue("key");

        var ex = Assert.Throws<VerificationException>(() =>
            mock.Verify(x => x.GetValue("key"), Times.Exactly(2), "Expected two calls"));

        Assert.Contains("Expected two calls", ex.Message);
    }

    [Fact]
    public void Verify_WithoutMessage_WorksAsUsual()
    {
        var mock = Mock.Create<ITestService>();
        mock.Object.GetValue("key");

        // No message — should still work
        mock.Verify(x => x.GetValue("key"), Times.Once);
    }

    [Fact]
    public void VerifyVoid_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<ITestService>();

        var ex = Assert.Throws<VerificationException>(() =>
            mock.Verify(x => x.DoSomething(), Times.Once, "Expected one call"));

        Assert.Contains("Expected one call", ex.Message);
    }

    [Fact]
    public void VerifyGet_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<IPropertyService>();

        var ex = Assert.Throws<VerificationException>(() =>
            mock.VerifyGet(x => x.Name, Times.Once, "Should have read Name"));

        Assert.Contains("Should have read Name", ex.Message);
    }

    [Fact]
    public void VerifySet_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<IPropertyService>();

        var ex = Assert.Throws<VerificationException>(() =>
            mock.VerifySet(x => x.Name, Times.Once, "Should have written Name"));

        Assert.Contains("Should have written Name", ex.Message);
    }

    [Fact]
    public void Verify_WithMatcher_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<ITestService>();
        mock.Object.GetValue("wrong-key");

        var ex = Assert.Throws<VerificationException>(() =>
            mock.Verify(
                x => x.GetValue("specific-key"),
                args => (string?)args[0] == "specific-key",
                Times.Once,
                "Expected call with specific-key"));

        Assert.Contains("Expected call with specific-key", ex.Message);
    }
}
