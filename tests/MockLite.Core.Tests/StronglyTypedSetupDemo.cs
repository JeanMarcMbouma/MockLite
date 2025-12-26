using BbQ.MockLite;

namespace BbQ.MockLite.Demo;

/// <summary>
/// Demonstrates compile-time type safety with strongly-typed Setup overloads for Action delegates.
/// </summary>
public class StronglyTypedSetupDemo
{
    public interface IQueryService
    {
        Action<int, int> Query(string procedure, int param1, int param2);
        Action<string> LogMessage(string level);
    }

    public static void DemoCompileTimeSafety()
    {
        var builder = Mock.Create<IQueryService>();

        // Example 1: Action<int, int> - compile-time enforced parameter types
        builder.Setup(
            x => x.Query("proc", 1, 2),
            (int a, int b) => Console.WriteLine($"Sum: {a + b}")
        );

        // Example 2: Action<string> - compile-time enforced parameter type
        builder.Setup(
            x => x.LogMessage("info"),
            (string message) => Console.WriteLine($"Log: {message}")
        );

        var mock = builder.Object;

        // Use the mock
        var queryAction = mock.Query("proc", 1, 2);
        queryAction(10, 20); // Prints: Sum: 30

        var logAction = mock.LogMessage("info");
        logAction("Operation completed"); // Prints: Log: Operation completed

        // The following would cause COMPILE-TIME errors:
        // builder.Setup(
        //     x => x.Query("proc", 1, 2),
        //     (string a, string b) => Console.WriteLine(a + b)  // ERROR: Wrong parameter types!
        // );
        //
        // builder.Setup(
        //     x => x.LogMessage("info"),
        //     (int level) => Console.WriteLine(level)  // ERROR: Wrong parameter type!
        // );
    }
}
