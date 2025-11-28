using System;
using System.Linq;

namespace MockLite;


using System.Reflection;

public sealed class Invocation(MethodInfo method, object[] arguments)
{
    public MethodInfo Method { get; } = method;
    public object[] Arguments { get; } = arguments;
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public override string ToString()
        => $"{Method.Name}({string.Join(", ", Arguments.Select(FormatArg))}) @ {Timestamp:O}";

    private static string FormatArg(object? a) => a switch
    {
        null => "null",
        string s => $"\"{s}\"",
        _ => a?.ToString() ?? "null"
    };
}
