using System;
using System.Linq;
using System.Reflection;

namespace MockLite;

/// <summary>
/// Represents a recorded method invocation on a mock object.
/// </summary>
/// <remarks>
/// This class captures details about a method call for verification purposes,
/// including the method that was invoked, the arguments passed, and the timestamp
/// of the invocation.
/// </remarks>
public sealed class Invocation(MethodInfo method, object[] arguments)
{
    /// <summary>
    /// Gets the method that was invoked.
    /// </summary>
    public MethodInfo Method { get; } = method;

    /// <summary>
    /// Gets the arguments that were passed to the method.
    /// </summary>
    public object[] Arguments { get; } = arguments;

    /// <summary>
    /// Gets the UTC timestamp when the method was invoked.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Returns a string representation of the invocation.
    /// </summary>
    /// <returns>
    /// A formatted string showing the method name, arguments, and timestamp.
    /// For example: "GetUser("john") @ 2024-01-15T10:30:45.1234567Z"
    /// </returns>
    public override string ToString()
        => $"{Method.Name}({string.Join(", ", Arguments.Select(FormatArg))}) @ {Timestamp:O}";

    private static string FormatArg(object? a) => a switch
    {
        null => "null",
        string s => $"\"{s}\"",
        _ => a?.ToString() ?? "null"
    };
}
