// src/MockLite.Core/Core/Mock.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace MockLite;



internal sealed class RuntimeProxy<T> : DispatchProxy where T : class
{
    public List<Invocation> Invocations { get; } = [];
    private readonly Dictionary<string, Delegate> _behaviors = [];

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

    public void Setup(MethodInfo method, Delegate behavior)
        => _behaviors[SignatureKey(method)] = behavior;

    private static string SignatureKey(MethodInfo mi)
    {
        var pars = string.Join(",", mi.GetParameters().Select(p => p.ParameterType.FullName));
        return $"{mi.Name}({pars})";
    }

    private static object? GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
}

internal static class RuntimeProxy
{
    public static T Create<T>() where T : class
        => (T)DispatchProxy.Create<T, RuntimeProxy<T>>();
}
