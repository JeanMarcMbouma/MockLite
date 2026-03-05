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
