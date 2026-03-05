using BenchmarkDotNet.Running;
using BbQ.MockLite.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(MockCreationBenchmarks).Assembly).Run(args);
