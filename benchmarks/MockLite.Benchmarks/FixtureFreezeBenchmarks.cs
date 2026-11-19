using AutoFixture;
using BbQ.MockLite.AutoFixture;
using BenchmarkDotNet.Attributes;

namespace BbQ.MockLite.Benchmarks;

[MemoryDiagnoser]
public class FixtureFreezeBenchmarks
{
    private Fixture _frozenFixture = null!;

    [GlobalSetup]
    public void Setup()
    {
        _frozenFixture = new Fixture();
        _frozenFixture.FreezeMock<IFixtureClock, MockFixtureClock>();
    }

    [Benchmark(Baseline = true)]
    public MockFixtureClock ManualInjection()
    {
        var fixture = new Fixture();
        var mock = new MockFixtureClock();
        fixture.Inject<IFixtureClock>(mock);
        fixture.Inject(mock);
        return mock;
    }

    [Benchmark]
    public MockFixtureClock FirstFreeze()
        => new Fixture().FreezeMock<IFixtureClock, MockFixtureClock>();

    [Benchmark]
    public MockFixtureClock RepeatedFreeze()
        => _frozenFixture.FreezeMock<IFixtureClock, MockFixtureClock>();
}

[GenerateMock]
public interface IFixtureClock
{
    long Read();
}
