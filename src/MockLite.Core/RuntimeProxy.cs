// src/MockLite.Core/Core/Mock.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace MockLite;

/// <summary>
/// Runtime-based mock proxy using DispatchProxy for interfaces without generated mocks.
/// </summary>
/// <remarks>
/// This is an internal helper class that provides fallback mocking functionality
/// when a generated mock is not available. It intercepts method calls and manages
/// behaviors and invocations at runtime.
/// 
/// For better performance, prefer using <see cref="GenerateMockAttribute"/> to
/// generate compile-time mock implementations instead of relying on this runtime proxy.
/// </remarks>
internal class RuntimeProxy<T> : DispatchProxy where T : class
{
    /// <summary>
    /// Gets the list of all method invocations recorded on this mock.
    /// </summary>
    public List<Invocation> Invocations { get; } = [];

    private readonly Dictionary<string, Delegate> _behaviors = [];

    /// <summary>
    /// Intercepts method calls on the proxied interface.
    /// </summary>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        args ??= [];
        Invocations.Add(new Invocation(targetMethod!, args!));

        if (_behaviors.TryGetValue(SignatureKey(targetMethod!), out var del))
            return del.DynamicInvoke(args);

        var ret = targetMethod!.ReturnType;
        if (ret == typeof(void)) return null;
        if (ret == typeof(Task)) return Task.CompletedTask;
        if (ret.IsGenericType && ret.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var tArg = ret.GenericTypeArguments[0];
            return typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(tArg)
                .Invoke(null, new object?[] { GetDefault(tArg) });
        }
        if (ret == typeof(ValueTask)) return default(ValueTask);
        if (ret.IsGenericType && ret.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var tArg = ret.GenericTypeArguments[0];
            return Activator.CreateInstance(typeof(ValueTask<>).MakeGenericType(tArg), GetDefault(tArg));
        }
        return GetDefault(ret);
    }

    /// <summary>
    /// Sets up the behavior for a specific method.
    /// </summary>
    /// <param name="method">The method to set up behavior for.</param>
    /// <param name="behavior">The delegate that implements the method behavior.</param>
    public void Setup(MethodInfo method, Delegate behavior)
        => _behaviors[SignatureKey(method)] = behavior;

    private static string SignatureKey(MethodInfo mi)
    {
        var pars = string.Join(",", mi.GetParameters().Select(p => p.ParameterType.FullName));
        return $"{mi.Name}({pars})";
    }

    private static object? GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
}

/// <summary>
/// Factory for creating runtime proxy mocks.
/// </summary>
internal static class RuntimeProxy
{
    /// <summary>
    /// Creates a runtime proxy mock for the specified interface type.
    /// </summary>
    /// <typeparam name="T">The interface type to mock.</typeparam>
    /// <returns>A dynamic proxy instance implementing the interface.</returns>
    public static T Create<T>() where T : class
        => (T)DispatchProxy.Create<T, RuntimeProxy<T>>();
}
