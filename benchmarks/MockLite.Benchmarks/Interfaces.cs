using BbQ.MockLite;

namespace BbQ.MockLite.Benchmarks;

/// <summary>
/// Simple interface used in benchmark comparisons between source-generated mocks and Mock.Create.
/// The [GenerateMock] attribute causes the source generator to produce a MockCalculator class
/// at compile time, which is used by Mock.Of&lt;ICalculator&gt;().
/// </summary>
[GenerateMock(typeof(ICalculator))]
public interface ICalculator
{
    int Add(int a, int b);
    double Divide(double numerator, double denominator);
    string Describe(string operation, int a, int b);
    void Clear(string reason);
}


/// <summary>Hand-written baseline used to ground generated-mock performance claims.</summary>
public sealed class HandWrittenCalculator : ICalculator
{
    public int Add(int a, int b) => default;
    public double Divide(double numerator, double denominator) => default;
    public string Describe(string operation, int a, int b) => string.Empty;
    public void Clear(string reason) { }
}
