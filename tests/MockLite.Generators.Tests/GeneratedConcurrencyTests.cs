namespace BbQ.MockLite.Generators.Tests;

public class GeneratedConcurrencyTests
{
    [Fact]
    public void GeneratedMock_RecordsConcurrentInvocations()
    {
        var mock = new MockUserService().GetCountReturns(1);

        Parallel.For(0, 1_000, i => mock.GetCount(i.ToString()));

        Assert.Equal(1_000, mock.Invocations.Count);
        mock.VerifyGetCount(Times.Exactly(1_000));
    }

    [Fact]
    public void GeneratedInvocations_AreStableSnapshots()
    {
        var mock = new MockUserService();
        mock.GetCount("one");

        var snapshot = mock.Invocations;
        mock.GetCount("two");

        Assert.Single(snapshot);
        Assert.Equal(2, mock.Invocations.Count);
    }

    [Fact]
    public void GeneratedReset_ClearsInvocationHistory()
    {
        var mock = new MockUserService();
        mock.GetCount("one");

        mock.Reset();

        Assert.Empty(mock.Invocations);
    }
}
