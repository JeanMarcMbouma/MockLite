using BbQ.MockLite;

namespace BbQ.MockLite.Demo;

/// <summary>
/// Demonstrates compile-time type safety with strongly-typed Setup overloads.
/// </summary>
public class StronglyTypedSetupDemo
{
    public interface IQueryService
    {
        Action<int, int> Query(string procedure, int param1, int param2);
        Func<string, int> GetTransform(string name);
    }

    public static void DemoCompileTimeSafety()
    {
        var builder = Mock.Create<IQueryService>();

        // Example 1: Action<int, int> - compile-time enforced parameter types
        builder.Setup(
            x => x.Query("proc", 1, 2),
            (int a, int b) => Console.WriteLine($"Sum: {a + b}")
        );

        // Example 2: Func<string, int> - compile-time enforced parameter and return types
        builder.Setup(
            x => x.GetTransform("length"),
            (string s) => s.Length
        );

        var mock = builder.Object;

        // Use the mock
        var queryAction = mock.Query("proc", 1, 2);
        queryAction(10, 20); // Prints: Sum: 30

        var transformFunc = mock.GetTransform("length");
        var result = transformFunc("hello");
        Console.WriteLine($"Length: {result}"); // Prints: Length: 5

        // The following would cause COMPILE-TIME errors:
        // builder.Setup(
        //     x => x.Query("proc", 1, 2),
        //     (string a, string b) => Console.WriteLine(a + b)  // ERROR: Wrong parameter types!
        // );
        //
        // builder.Setup(
        //     x => x.GetTransform("length"),
        //     (int i) => i * 2  // ERROR: Wrong parameter type!
        // );
    }
}
