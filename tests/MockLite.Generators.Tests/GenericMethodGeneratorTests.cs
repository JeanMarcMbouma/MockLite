namespace BbQ.MockLite.Generators.Tests;

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
