using BbQ.MockLite;

namespace BbQ.MockLite.Generators.Tests;

[GenerateMock<IUserService>]
[GenerateMock<ICompositeService>]
[GenerateMock<ICollectionReturningService>]
[GenerateMock<IGenericMethodService>]
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

// --- Generic method interface for testing ---
public interface IGenericMethodService
{
    TResult Send<TResult>(string command);
    Task<TResult> SendAsync<TResult>(string command);
    void Execute<TItem>(TItem item);
    T Get<T>(string key) where T : class;
    int NonGenericMethod(string input);
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

// ==================== GENERIC METHOD TESTS ====================
public class GenericMethodGeneratorTests
{
    [Fact]
    public void GeneratedMock_GenericMethod_ReturnsDefault()
    {
        var mock = new MockGenericMethodService();

        // Generic method with value-type return should return default
        var result = mock.Send<int>("test");
        Assert.Equal(0, result);
    }

    [Fact]
    public void GeneratedMock_GenericMethod_StringReturn_ReturnsDefault()
    {
        var mock = new MockGenericMethodService();

        var result = mock.Send<string>("test");
        Assert.Null(result);
    }

    [Fact]
    public async Task GeneratedMock_GenericAsyncMethod_ReturnsDefault()
    {
        var mock = new MockGenericMethodService();

        var result = await mock.SendAsync<int>("test");
        Assert.Equal(0, result);
    }

    [Fact]
    public void GeneratedMock_GenericVoidMethod_RecordsInvocation()
    {
        var mock = new MockGenericMethodService();

        mock.Execute("hello");
        mock.Execute(42);

        Assert.Equal(2, mock.Invocations.Count);
    }

    [Fact]
    public void GeneratedMock_ConstrainedGenericMethod_ReturnsDefault()
    {
        var mock = new MockGenericMethodService();

        var result = mock.Get<string>("key");
        Assert.Null(result);
    }

    [Fact]
    public void GeneratedMock_NonGenericMethod_StillWorks()
    {
        // Ensure non-generic methods on the same interface still have setup/returns
        var mock = new MockGenericMethodService()
            .ReturnsNonGenericMethod(42);

        Assert.Equal(42, mock.NonGenericMethod("test"));
    }

    [Fact]
    public void GeneratedMock_GenericMethod_VerifyByName()
    {
        var mock = new MockGenericMethodService();
        mock.Send<int>("cmd1");
        mock.Send<string>("cmd2");

        // Verify counts by method name (generic methods share the name)
        mock.VerifySend(n => n == 2);
    }
}