using BenchmarkDotNet.Attributes;
using BbQ.MockLite;

namespace BbQ.MockLite.Benchmarks;

/// <summary>
/// Benchmarks comparing source-generated mocks (Mock.Of&lt;T&gt; with [GenerateMock]) against
/// runtime-proxy-based mocks created via Mock.Create&lt;T&gt;.
///
/// Scenarios measured:
/// <list type="bullet">
///   <item><term>Creation</term><description>Time to instantiate a new mock object.</description></item>
///   <item><term>Method invocation</term><description>Time to call a method on an already-created mock.</description></item>
///   <item><term>Setup + invoke</term><description>Time to configure a return value and call the method once.</description></item>
/// </list>
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class MockCreationBenchmarks
{
    // ── Creation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a source-generated mock instance via Mock.Of&lt;T&gt;.
    /// The generator produces a lightweight MockCalculator class at compile time,
    /// so this is equivalent to <c>new MockCalculator()</c>.
    /// </summary>
    [Benchmark(Description = "Mock.Of (source-generated)")]
    public ICalculator Create_SourceGenerated() => Mock.Of<ICalculator>();

    /// <summary>
    /// Creates a fluent-builder mock backed by a DispatchProxy runtime proxy.
    /// </summary>
    [Benchmark(Description = "Mock.Create (runtime proxy)", Baseline = true)]
    public Mock<ICalculator> Create_RuntimeProxy() => Mock.Create<ICalculator>();
}

/// <summary>
/// Benchmarks measuring the cost of a single method call on each mock type.
/// The mock instances are created once in <see cref="GlobalSetup"/> so that
/// creation overhead is excluded from the measured region.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class MockInvocationBenchmarks
{
    private ICalculator _generatedMock = null!;
    private Mock<ICalculator> _runtimeBuilder = null!;
    private ICalculator _runtimeMock = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _generatedMock = Mock.Of<ICalculator>();
        _runtimeBuilder = Mock.Create<ICalculator>();
        _runtimeMock = _runtimeBuilder.Object;
    }

    /// <summary>Calls Add on a source-generated mock.</summary>
    [Benchmark(Description = "Invoke Add – source-generated")]
    public int Invoke_SourceGenerated() => _generatedMock.Add(3, 4);

    /// <summary>Calls Add on a runtime-proxy mock.</summary>
    [Benchmark(Description = "Invoke Add – runtime proxy", Baseline = true)]
    public int Invoke_RuntimeProxy() => _runtimeMock.Add(3, 4);
}

/// <summary>
/// Benchmarks measuring the overhead of setting up a return value and then
/// invoking the configured method once.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class MockSetupAndInvokeBenchmarks
{
    /// <summary>
    /// Configures a return value on a source-generated mock and calls the method.
    /// Generated mocks expose strongly-typed <c>SetupXxx</c> methods directly.
    /// </summary>
    [Benchmark(Description = "Setup + invoke – source-generated")]
    public int SetupAndInvoke_SourceGenerated()
    {
        var mock = Mock.Of<ICalculator>();
        // The source generator creates a SetupAdd method on MockCalculator.
        ((MockCalculator)mock).SetupAdd((a, b) => a + b);
        return mock.Add(3, 4);
    }

    /// <summary>
    /// Configures a return value via the fluent Mock.Create API (DispatchProxy) and calls the method.
    /// </summary>
    [Benchmark(Description = "Setup + invoke – runtime proxy", Baseline = true)]
    public int SetupAndInvoke_RuntimeProxy()
    {
        var builder = Mock.Create<ICalculator>();
        builder.Setup(x => x.Add(It.IsAny<int>(), It.IsAny<int>()), () => 7);
        return builder.Object.Add(3, 4);
    }
}
