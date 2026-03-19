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
            .GetUserAsyncReturns(new User("Jean"));                         // async returns

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
            .GetVersionReturns(42);

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
            .GetVersionReturns(1);

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
            .NonGenericMethodReturns(42);

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

        // Verify individual invocations recorded the correct generic type arguments
        Assert.Equal(2, mock.Invocations.Count);
        Assert.True(mock.Invocations[0].Method.IsGenericMethod);
        Assert.True(mock.Invocations[1].Method.IsGenericMethod);
        Assert.Equal("cmd1", mock.Invocations[0].Arguments[0]);
        Assert.Equal("cmd2", mock.Invocations[1].Arguments[0]);
    }
}

// ==================== GENERATED METHOD SETUP PHRASE TESTS ====================
public class GeneratedMethodSetupPhraseTests
{
    [Fact]
    public void GetCountReturns_SetsConstantReturnValue()
    {
        var mock = new MockUserService()
            .GetCountReturns(42);

        Assert.Equal(42, mock.GetCount("anything"));
    }

    [Fact]
    public void GetCountReturns_FluentChaining_ReturnsMock()
    {
        var mock = new MockUserService()
            .GetCountReturns(10)
            .SetupGetCount(c => c.Length);

        // Last setup wins
        Assert.Equal(5, mock.GetCount("hello"));
    }

    [Fact]
    public async Task GetUserAsyncReturns_SetsAsyncReturnValue()
    {
        var mock = new MockUserService()
            .GetUserAsyncReturns(new User("Alice"));

        var user = await mock.GetUserAsync(1);
        Assert.Equal("Alice", user.Username);
    }

    [Fact]
    public void GetVersionReturns_WorksOnCompositeInterface()
    {
        var mock = new MockCompositeService()
            .GetVersionReturns(99);

        Assert.Equal(99, mock.GetVersion());
    }

    [Fact]
    public void NonGenericMethodReturns_WorksOnGenericMethodInterface()
    {
        var mock = new MockGenericMethodService()
            .NonGenericMethodReturns(7);

        Assert.Equal(7, mock.NonGenericMethod("input"));
    }

    [Fact]
    public void GetNameReturns_SetsPropertyGetterValue()
    {
        var mock = new MockUserService();
        mock.GetNameReturns("Bob");

        Assert.Equal("Bob", mock.Name);
    }

    [Fact]
    public void GetNameReturns_FluentChaining_ReturnsMock()
    {
        // GetNameReturns returns MockUserService for further chaining
        var mock = new MockUserService()
            .GetNameReturns("Carol")
            .GetCountReturns(5);

        Assert.Equal("Carol", mock.Name);
        Assert.Equal(5, mock.GetCount("x"));
    }
}

// ==================== GENERATED PROPERTY PHRASE CHAINING TESTS ====================
public class GeneratedPropertyPhraseTests
{
    [Fact]
    public void SetupGetName_Returns_Value()
    {
        var mock = new MockUserService();
        mock.SetupGetName().Returns("Alice");

        Assert.Equal("Alice", mock.Name);
    }

    [Fact]
    public void SetupGetName_Returns_Factory()
    {
        var counter = 0;
        var mock = new MockUserService();
        mock.SetupGetName().Returns(() => $"Call{++counter}");

        Assert.Equal("Call1", mock.Name);
        Assert.Equal("Call2", mock.Name);
    }

    [Fact]
    public void SetupGetName_Throws()
    {
        var mock = new MockUserService();
        mock.SetupGetName().Throws(new InvalidOperationException("no access"));

        var ex = Assert.Throws<InvalidOperationException>(() => _ = mock.Name);
        Assert.Equal("no access", ex.Message);
    }

    [Fact]
    public void SetupGetName_Callback_Then_Returns()
    {
        var log = new List<string>();
        var mock = new MockUserService();
        mock.SetupGetName()
            .Callback(() => log.Add("read"))
            .Returns("Bob");

        var value = mock.Name;
        Assert.Equal("Bob", value);
        Assert.Single(log);
        Assert.Equal("read", log[0]);
    }

    [Fact]
    public void SetupSetName_Callback_TypedAction()
    {
        string? captured = null;
        var mock = new MockUserService();
        mock.SetupSetName().Callback((string v) => captured = v);

        mock.Name = "Charlie";
        Assert.Equal("Charlie", captured);
    }

    [Fact]
    public void SetupSetName_Callback_ParameterlessAction()
    {
        var called = false;
        var mock = new MockUserService();
        mock.SetupSetName().Callback(() => called = true);

        mock.Name = "Delta";
        Assert.True(called);
    }

    [Fact]
    public void SetupSetName_Throws()
    {
        var mock = new MockUserService();
        mock.SetupSetName().Throws(new InvalidOperationException("read-only"));

        var ex = Assert.Throws<InvalidOperationException>(() => mock.Name = "oops");
        Assert.Equal("read-only", ex.Message);
    }

    [Fact]
    public void SetupGetName_Returns_ReturnsMockForFluent()
    {
        // .Returns is terminal and returns the mock for further chaining
        var mock = new MockUserService()
            .SetupGetName().Returns("fluent");

        Assert.IsType<MockUserService>(mock);
        Assert.Equal("fluent", mock.Name);
    }

    [Fact]
    public void SetupSetName_Throws_ReturnsMockForFluent()
    {
        // .Throws is terminal and returns the mock for further chaining
        var mock = new MockUserService()
            .SetupSetName().Throws(new InvalidOperationException("x"));

        Assert.IsType<MockUserService>(mock);
    }

    [Fact]
    public void SetupGet_And_SetupSet_Combined_Fluent_Chain()
    {
        var log = new List<string>();
        var mock = new MockUserService()
            .SetupGetName()
                .Callback(() => log.Add("get"))
                .Returns("Evelyn");

        mock.SetupSetName().Callback((string v) => log.Add($"set:{v}"));

        Assert.Equal("Evelyn", mock.Name);
        mock.Name = "Frank";

        Assert.Equal(2, log.Count);
        Assert.Equal("get", log[0]);
        Assert.Equal("set:Frank", log[1]);
    }

    [Fact]
    public void SetupGetName_Callback_InvokedOnEveryAccess()
    {
        var count = 0;
        var mock = new MockUserService();
        mock.SetupGetName()
            .Callback(() => count++)
            .Returns("Always");

        _ = mock.Name;
        _ = mock.Name;
        _ = mock.Name;

        Assert.Equal(3, count);
    }

    [Fact]
    public void Full_Fluent_Chain_SetupGet_Returns_SetupMethod()
    {
        // Chain: SetupGetName().Returns() returns MockUserService,
        // then chain into SetupGetCount() (method setup)
        var mock = new MockUserService()
            .SetupGetName().Returns("chain")
            .SetupGetCount(c => c.Length);

        Assert.Equal("chain", mock.Name);
        Assert.Equal(5, mock.GetCount("hello"));
    }
}