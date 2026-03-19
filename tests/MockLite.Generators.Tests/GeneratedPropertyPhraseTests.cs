namespace BbQ.MockLite.Generators.Tests;

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
