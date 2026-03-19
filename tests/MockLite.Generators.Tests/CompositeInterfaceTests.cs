namespace BbQ.MockLite.Generators.Tests;

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
