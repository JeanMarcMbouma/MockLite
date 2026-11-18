# Benchmarks

These are historical results recorded in the repository, not a new benchmark run. Re-run on your own hardware and workload before drawing performance conclusions.

The `benchmarks/MockLite.Benchmarks` project uses [BenchmarkDotNet](https://benchmarkdotnet.org/) to compare
source-generated mocks against runtime-proxy mocks (`Mock.Create<T>`).

### Running the benchmarks

```bash
dotnet run --project benchmarks/MockLite.Benchmarks -c Release
```

To run a specific group only, use the `--filter` option:

```bash
# Creation benchmarks only
dotnet run --project benchmarks/MockLite.Benchmarks -c Release -- --filter '*Creation*'

# Invocation benchmarks only
dotnet run --project benchmarks/MockLite.Benchmarks -c Release -- --filter '*Invocation*'
```

### Results (BenchmarkDotNet v0.15.8, .NET 10.0.2, AMD EPYC 7763, Ubuntu 24.04)

#### MockCreationBenchmarks — instantiating a mock from scratch

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| `new MockCalculator()` (direct) | **14.5 ns** | 0.04× | 88 B | 0.14× |
| `Mock.Of<ICalculator>()` (registry) | **37.5 ns** | 0.11× | 88 B | 0.14× |
| `Mock.Create<ICalculator>()` *(baseline)* | 350 ns | 1.00× | 608 B | 1.00× |

> `Mock.Of<T>()` is **~9× faster** than `Mock.Create<T>()` and allocates **~7× less** memory,  
> thanks to the compile-time `MockTypeRegistry`: an O(1) `ConcurrentDictionary` lookup that  
> replaces the previous per-call `Type.GetType()` + `Activator.CreateInstance()` path.

#### MockInvocationBenchmarks — calling a method on an already-created mock

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| Invoke Add – source-generated | **221 ns** | 0.92× | 128 B | 1.00× |
| Invoke Add – runtime proxy *(baseline)* | 240 ns | 1.00× | 128 B | 1.00× |

> Source-generated and runtime-proxy mocks now have **equivalent per-call performance** (~8%
> difference, within measurement noise). Both record invocations via the invocation-recording
> path and allocate the same amount of memory per call.

#### MockSetupAndInvokeBenchmarks — configure a return value then call the method

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| Setup + invoke – source-generated | **62.0 ns** | 0.000× | 216 B | 0.03× |
| Setup + invoke – runtime proxy *(baseline)* | 138,937 ns | 1.00× | 7,430 B | 1.00× |

> Source-generated mocks are **~2,240× faster** for the setup+invoke pattern and allocate
> **~34× less** memory.  
> The `Mock.Create<T>` fluent setup uses expression-tree parsing and reflection-based delegate
> construction on every call, which dominates the runtime-proxy cost here.

### What is measured

| Benchmark group | Description |
|---|---|
| `MockCreationBenchmarks` | Time to instantiate a new mock object (three variants) |
| `MockInvocationBenchmarks` | Time to call a method on an already-created mock |
| `MockSetupAndInvokeBenchmarks` | Time to configure a return value and call the method once |

### Why source-generated mocks are faster

`[GenerateMock]` produces a concrete class at **compile time** — its methods are regular C# code
with direct typed behavior calls rather than expression-based setup. The **setup+invoke** path benefits most: every
`Mock.Create<T>` setup call parses an expression tree and constructs a reflection-based delegate,
while a source-generated mock simply calls a pre-compiled handler directly.

**Per-call invocation** overhead has converged in recent .NET versions: both source-generated and
runtime-proxy mocks now record invocations via the invocation-recording path and show equivalent
throughput (~221 ns vs ~240 ns, a difference within measurement noise).

`Mock.Of<T>()` is fast to **instantiate** because the source generator also emits a
`[ModuleInitializer]` that registers a pre-compiled constructor delegate into `MockTypeRegistry`
at assembly load time. Every subsequent call to `Mock.Of<T>()` is just an O(1) dictionary
lookup — no `Type.GetType`, no `Activator.CreateInstance`.

Use `[GenerateMock]` on interfaces you control for the best creation and setup performance.  
Use `Mock.Create<T>` when you need the fluent `Setup` / `Verify` / `OnCall` API for fine-grained
test configuration, or for third-party interfaces where source generation is not available.

