namespace BbQ.MockLite.Generators.Tests;

public class GeneratedMethodSetupPhraseTests
{
    // --- Shorthand Returns tests (existing) ---

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

    // --- Phrase struct chaining tests (new) ---

    [Fact]
    public void SetupGetCount_Returns_Constant()
    {
        var mock = new MockUserService();
        mock.SetupGetCount().Returns(42);

        Assert.Equal(42, mock.GetCount("anything"));
    }

    [Fact]
    public void SetupGetCount_Returns_Factory()
    {
        var mock = new MockUserService();
        mock.SetupGetCount().Returns(category => category.Length);

        Assert.Equal(5, mock.GetCount("hello"));
        Assert.Equal(3, mock.GetCount("bye"));
    }

    [Fact]
    public void SetupGetCount_Throws()
    {
        var mock = new MockUserService();
        mock.SetupGetCount().Throws(new InvalidOperationException("no count"));

        var ex = Assert.Throws<InvalidOperationException>(() => mock.GetCount("x"));
        Assert.Equal("no count", ex.Message);
    }

    [Fact]
    public void SetupGetCount_Callback_Then_Returns()
    {
        var log = new List<string>();
        var mock = new MockUserService();
        mock.SetupGetCount()
            .Callback(() => log.Add("called"))
            .Returns(10);

        var result = mock.GetCount("test");
        Assert.Equal(10, result);
        Assert.Single(log);
        Assert.Equal("called", log[0]);
    }

    [Fact]
    public void SetupGetCount_Callback_InvokedOnEveryCall()
    {
        var count = 0;
        var mock = new MockUserService();
        mock.SetupGetCount()
            .Callback(() => count++)
            .Returns(1);

        mock.GetCount("a");
        mock.GetCount("b");
        mock.GetCount("c");

        Assert.Equal(3, count);
    }

    [Fact]
    public void SetupGetCount_Returns_ReturnsMockForFluent()
    {
        // .Returns is terminal and returns the mock for further chaining
        var mock = new MockUserService()
            .SetupGetCount().Returns(42)
            .GetUserAsyncReturns(new User("Jean"));

        Assert.IsType<MockUserService>(mock);
        Assert.Equal(42, mock.GetCount("x"));
    }

    [Fact]
    public async Task SetupGetUserAsync_Returns_Constant()
    {
        var mock = new MockUserService();
        mock.SetupGetUserAsync().Returns(new User("Async"));

        var user = await mock.GetUserAsync(1);
        Assert.Equal("Async", user.Username);
    }

    [Fact]
    public async Task SetupGetUserAsync_Returns_Factory()
    {
        var mock = new MockUserService();
        mock.SetupGetUserAsync().Returns(id => Task.FromResult(new User($"User{id}")));

        var user = await mock.GetUserAsync(7);
        Assert.Equal("User7", user.Username);
    }

    [Fact]
    public async Task SetupGetUserAsync_Throws()
    {
        var mock = new MockUserService();
        mock.SetupGetUserAsync().Throws(new InvalidOperationException("async fail"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => mock.GetUserAsync(1));
        Assert.Equal("async fail", ex.Message);
    }

    [Fact]
    public async Task SetupGetUserAsync_Callback_Then_Returns()
    {
        var called = false;
        var mock = new MockUserService();
        mock.SetupGetUserAsync()
            .Callback(() => called = true)
            .Returns(new User("CB"));

        var user = await mock.GetUserAsync(1);
        Assert.True(called);
        Assert.Equal("CB", user.Username);
    }

    [Fact]
    public void SetupWrite_Throws_VoidMethod()
    {
        var mock = new MockCompositeService();
        mock.SetupWrite().Throws(new InvalidOperationException("readonly"));

        var ex = Assert.Throws<InvalidOperationException>(() => mock.Write("k", "v"));
        Assert.Equal("readonly", ex.Message);
    }

    [Fact]
    public void SetupWrite_Callback_VoidMethod()
    {
        var log = new List<string>();
        var mock = new MockCompositeService();
        mock.SetupWrite()
            .Callback(() => log.Add("write"))
            .Throws(new InvalidOperationException("after-callback"));

        var ex = Assert.Throws<InvalidOperationException>(() => mock.Write("k", "v"));
        Assert.Single(log);
        Assert.Equal("after-callback", ex.Message);
    }

    [Fact]
    public void SetupRead_Returns_Constant()
    {
        var mock = new MockCompositeService();
        mock.SetupRead().Returns("fixed");

        Assert.Equal("fixed", mock.Read("any-key"));
    }

    [Fact]
    public void SetupRead_Returns_Factory()
    {
        var mock = new MockCompositeService();
        mock.SetupRead().Returns(key => $"val-{key}");

        Assert.Equal("val-hello", mock.Read("hello"));
    }

    // --- Task (non-generic) phrase tests ---

    [Fact]
    public async Task SetupDoWorkAsync_Returns_Parameterless()
    {
        var mock = new MockTaskService();
        mock.SetupDoWorkAsync().Returns();

        await mock.DoWorkAsync(); // should complete without error
    }

    [Fact]
    public async Task SetupDoWorkAsync_Returns_Factory()
    {
        var callCount = 0;
        var mock = new MockTaskService();
        mock.SetupDoWorkAsync().Returns(() => { callCount++; return Task.CompletedTask; });

        await mock.DoWorkAsync();
        await mock.DoWorkAsync();
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task SetupDoWorkWithArgAsync_Returns_Factory()
    {
        var captured = "";
        var mock = new MockTaskService();
        mock.SetupDoWorkWithArgAsync().Returns(input => { captured = input; return Task.CompletedTask; });

        await mock.DoWorkWithArgAsync("hello");
        Assert.Equal("hello", captured);
    }

    [Fact]
    public async Task SetupDoWorkAsync_Callback_Then_Returns_Factory()
    {
        var called = false;
        var callCount = 0;
        var mock = new MockTaskService();
        mock.SetupDoWorkAsync()
            .Callback(() => called = true)
            .Returns(() => { callCount++; return Task.CompletedTask; });

        await mock.DoWorkAsync();
        Assert.True(called);
        Assert.Equal(1, callCount);
    }

    // --- ValueTask (non-generic) phrase tests ---

    [Fact]
    public async Task SetupProcessAsync_Returns_Parameterless()
    {
        var mock = new MockTaskService();
        mock.SetupProcessAsync().Returns();

        await mock.ProcessAsync(); // should complete without error
    }

    [Fact]
    public async Task SetupProcessAsync_Returns_Factory()
    {
        var callCount = 0;
        var mock = new MockTaskService();
        mock.SetupProcessAsync().Returns(() => { callCount++; return default; });

        await mock.ProcessAsync();
        await mock.ProcessAsync();
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task SetupProcessWithArgAsync_Returns_Factory()
    {
        var captured = 0;
        var mock = new MockTaskService();
        mock.SetupProcessWithArgAsync().Returns(count => { captured = count; return default; });

        await mock.ProcessWithArgAsync(42);
        Assert.Equal(42, captured);
    }

    [Fact]
    public async Task SetupProcessAsync_Callback_Then_Returns_Factory()
    {
        var called = false;
        var callCount = 0;
        var mock = new MockTaskService();
        mock.SetupProcessAsync()
            .Callback(() => called = true)
            .Returns(() => { callCount++; return default; });

        await mock.ProcessAsync();
        Assert.True(called);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Full_Fluent_Chain_SetupMethod_Returns_Then_SetupAnother()
    {
        // Chain: SetupGetCount().Returns() returns MockUserService,
        // then chain into SetupGetCount() (setup behavior)
        var mock = new MockUserService()
            .SetupGetCount().Returns(42)
            .SetupGetCount(c => c.Length);

        // Last setup wins
        Assert.Equal(5, mock.GetCount("hello"));
    }
}
