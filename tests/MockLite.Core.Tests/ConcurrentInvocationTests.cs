namespace BbQ.MockLite.Tests;

public class ConcurrentInvocationTests
{
    private interface ICounterService
    {
        int Get(int value);
    }

    [Fact]
    public void RuntimeMock_RecordsConcurrentInvocationsWithoutLosingHistory()
    {
        var mock = Mock.Create<ICounterService>()
            .Setup(x => x.Get(It.IsAny<int>()), (int value) => value);

        Parallel.For(0, 1_000, i => Assert.Equal(i, mock.Object.Get(i)));

        Assert.Equal(1_000, mock.Invocations.Count);
        mock.Verify(x => x.Get(It.IsAny<int>()), Times.Exactly(1_000));
    }

    [Fact]
    public void Invocations_ReturnsStableSnapshot()
    {
        var mock = Mock.Create<ICounterService>();
        mock.Object.Get(1);

        var snapshot = mock.Invocations;
        mock.Object.Get(2);

        Assert.Single(snapshot);
        Assert.Equal(2, mock.Invocations.Count);
    }
}
