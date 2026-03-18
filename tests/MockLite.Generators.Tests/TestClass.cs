using BbQ.MockLite;

namespace BbQ.MockLite.Generators.Tests;

[GenerateMock<IUserService>]
[GenerateMock<ICompositeService>]
[GenerateMock<ICollectionReturningService>]
public partial class Mocks { }

public record User(string Username);
public interface IUserService
{
    string Name { get; set; }
    int GetCount(string category);
    Task<User> GetUserAsync(int id);
}

// --- Composite interface hierarchy for testing ---
public interface IReadable
{
    string Read(string key);
}

public interface IWritable
{
    void Write(string key, string value);
}

public interface ICompositeService : IReadable, IWritable
{
    int GetVersion();
}

// --- Collection-returning interface for smart defaults ---
public interface ICollectionReturningService
{
    IEnumerable<string> GetItems();
    Task<IEnumerable<string>> GetItemsAsync();
    IReadOnlyList<int> GetNumbers();
}

public class TestClass
{
    [Fact]
    public async Task TestMethod()
    {

        var svc = new MockUserService()
            .SetupGetCount(category => category.Length)            // behavior directly
            .ReturnsGetUserAsync(new User("Jean"));                         // async returns

        svc.GetCount("alpha");               // 5
        var user = await svc.GetUserAsync(42);

        svc.VerifyGetCount(Times.Once);
        svc.VerifyGetUserAsync(Times.Once);

    }
}

// ==================== COMPOSITE INTERFACE TESTS ====================
public class CompositeInterfaceTests
{
    [Fact]
    public void CompositeInterface_InheritsAllMethods()
    {
        // MockCompositeService should implement Read (from IReadable),
        // Write (from IWritable), and GetVersion (own)
        var mock = new MockCompositeService();
        Assert.IsAssignableFrom<ICompositeService>(mock);
        Assert.IsAssignableFrom<IReadable>(mock);
        Assert.IsAssignableFrom<IWritable>(mock);
    }

    [Fact]
    public void CompositeInterface_BaseMethodsWork()
    {
        var mock = new MockCompositeService()
            .SetupRead(key => $"value-{key}")
            .ReturnsGetVersion(42);

        Assert.Equal("value-hello", mock.Read("hello"));
        Assert.Equal(42, mock.GetVersion());
    }

    [Fact]
    public void CompositeInterface_VoidBaseMethodRecordsInvocations()
    {
        var mock = new MockCompositeService();
        mock.Write("key", "value");

        Assert.Single(mock.Invocations);
        Assert.Equal("Write", mock.Invocations[0].Method.Name);
    }

    [Fact]
    public void CompositeInterface_VerifyBaseMethod()
    {
        var mock = new MockCompositeService();
        mock.Read("key");

        mock.VerifyRead(Times.Once);
    }

    [Fact]
    public void CompositeInterface_FluentChainsAcrossInheritedMethods()
    {
        // Setup methods from different base interfaces should all return MockCompositeService
        var mock = new MockCompositeService()
            .SetupRead(key => "read-value")
            .ReturnsGetVersion(1);

        Assert.Equal("read-value", mock.Read("anything"));
        Assert.Equal(1, mock.GetVersion());
    }
}

// ==================== GENERATOR SMART DEFAULT TESTS ====================
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