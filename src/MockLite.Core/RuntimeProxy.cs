// src/MockLite.Core/Core/Mock.cs
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Concurrent;

namespace BbQ.MockLite;

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

    // Comparer that uses RuntimeMethodHandle for reliable equality across MethodInfo instances
    // that may originate from different reflection paths (expression trees vs DispatchProxy).
    private static readonly MethodHandleComparer _handleComparer = new();

    // Signature-only fallback behaviors keyed by MethodInfo (no arg matching).
    private readonly Dictionary<MethodInfo, Func<object?[], object?>> _signatureBehaviors = new(_handleComparer);

    // Arg-specific behaviors keyed by MethodInfo; each entry is an ordered list
    // of (matchArgs, isAny flags, compiled invoker).  Later setups win (inserted at front).
    private readonly Dictionary<MethodInfo, List<(object?[] MatchArgs, bool[] IsAny, Func<object?[], object?> Invoker)>> _argBehaviors = new(_handleComparer);

    // Callbacks keyed by MethodInfo to avoid per-call string building.
    private readonly Dictionary<MethodInfo, List<(Func<object?[], bool>? Matcher, Action<object?[]> Callback)>> _callbacks = new(_handleComparer);

    // Cached default return values to avoid repeated Activator.CreateInstance calls.
    private static readonly ConcurrentDictionary<Type, object?> _defaultValues = new();

    // Cached It.IsAny<T>() sentinel instances used to detect IsAny markers.
    private static readonly ConcurrentDictionary<Type, object> _anyMatchers = new();

    /// <summary>
    /// Intercepts method calls on the proxied interface.
    /// </summary>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        args ??= [];
        Invocations.Add(new Invocation(targetMethod!, args!));

        // Execute callbacks for this method.
        ExecuteCallbacks(targetMethod!, args);

        // Try arg-specific behaviors first (supports exact and IsAny matching).
        if (_argBehaviors.TryGetValue(targetMethod!, out var argList))
        {
            for (int i = 0; i < argList.Count; i++)
            {
                var (matchArgs, isAny, invoker) = argList[i];
                if (MatchesArguments(matchArgs, isAny, args))
                    return invoker(args);
            }
        }

        // Fall back to signature-only behavior.
        if (_signatureBehaviors.TryGetValue(targetMethod!, out var sigInvoker))
            return sigInvoker(args);

        // Return the appropriate default for the method's return type.
        return GetDefault(targetMethod!.ReturnType);
    }

    /// <summary>
    /// Sets up the behavior for a specific method (signature-only, no argument matching).
    /// </summary>
    public void Setup(MethodInfo method, Delegate behavior)
        => _signatureBehaviors[method] = CompileInvoker(behavior);

    /// <summary>
    /// Sets up the behavior for a specific method with argument matching (supports It.IsAny).
    /// </summary>
    public void Setup(MethodInfo method, object?[] args, Delegate behavior)
    {
        var isAny = new bool[args.Length];
        for (int i = 0; i < args.Length; i++)
            isAny[i] = IsAnyMatcherInstance(args[i]);

        var invoker = CompileInvoker(behavior);

        if (!_argBehaviors.TryGetValue(method, out var list))
            _argBehaviors[method] = list = [];

        // Insert at the front so that the most recent setup takes priority.
        list.Insert(0, (args, isAny, invoker));
    }

    /// <summary>
    /// Registers a callback that executes when a method is invoked.
    /// </summary>
    public void OnInvocation(MethodInfo method, Action<object?[]> callback)
        => OnInvocation(method, null, callback);

    /// <summary>
    /// Registers a callback that executes when a method is invoked with matching arguments.
    /// </summary>
    public void OnInvocation(MethodInfo method, Func<object?[], bool>? matcher, Action<object?[]> callback)
    {
        if (!_callbacks.TryGetValue(method, out var list))
            _callbacks[method] = list = [];
        list.Add((matcher, callback));
    }

    /// <summary>
    /// Executes all registered callbacks for a method invocation.
    /// </summary>
    private void ExecuteCallbacks(MethodInfo method, object?[] args)
    {
        if (_callbacks.TryGetValue(method, out var callbackList))
        {
            foreach (var (matcher, callback) in callbackList)
            {
                if (matcher == null || matcher(args))
                    callback(args);
            }
        }
    }

    /// <summary>
    /// Checks whether the invocation arguments satisfy an arg-specific setup entry.
    /// Parameters flagged as IsAny match any value; others are compared with Equals.
    /// </summary>
    private static bool MatchesArguments(object?[] matchArgs, bool[] isAny, object?[] invArgs)
    {
        if (isAny.Length != invArgs.Length) return false;
        for (int i = 0; i < isAny.Length; i++)
        {
            if (isAny[i]) continue;
            if (!Equals(matchArgs[i], invArgs[i])) return false;
        }
        return true;
    }

    /// <summary>
    /// Compiles a delegate into a <c>Func&lt;object?[], object?&gt;</c> that can be
    /// invoked without reflection on the hot path.  The compilation cost is paid once
    /// at Setup time rather than on every invocation.
    /// </summary>
    private static Func<object?[], object?> CompileInvoker(Delegate d)
    {
        // Use the delegate type's Invoke method to get the formal parameter list and
        // return type. This is always correct regardless of how the delegate was created
        // (regular method, compiled expression lambda, closures, etc.).
        var invokeMethod = d.GetType().GetMethod("Invoke")!;
        var parms = invokeMethod.GetParameters();
        var returnType = invokeMethod.ReturnType;

        var argsParam = Expression.Parameter(typeof(object?[]), "args");

        Expression[] callArgs = parms.Length == 0
            ? []
            : new Expression[parms.Length];

        for (int i = 0; i < parms.Length; i++)
            callArgs[i] = Expression.Convert(
                Expression.ArrayIndex(argsParam, Expression.Constant(i)),
                parms[i].ParameterType);

        var invoke = Expression.Invoke(Expression.Constant(d), callArgs);

        Expression body = returnType == typeof(void)
            ? Expression.Block(typeof(object), invoke, Expression.Constant(null, typeof(object)))
            : Expression.Convert(invoke, typeof(object));

        return Expression.Lambda<Func<object?[], object?>>(body, argsParam).Compile();
    }

    /// <summary>
    /// Determines if an argument is an It.IsAny marker instance.
    /// </summary>
    private static bool IsAnyMatcherInstance(object? arg)
    {
        if (arg == null) return false;

        if (_anyMatchers.TryGetValue(arg.GetType(), out var cachedMatcher))
            return object.Equals(arg, cachedMatcher);

        var argType = arg.GetType();
        var sentinel = typeof(It).GetMethod(nameof(It.IsAny))!.MakeGenericMethod(argType).Invoke(null, [])!;
        _anyMatchers.TryAdd(argType, sentinel);
        return object.Equals(arg, sentinel);
    }

    /// <summary>
    /// Returns the appropriate default value for <paramref name="t"/>, caching the
    /// result so that <c>Activator.CreateInstance</c> is only called once per type.
    /// </summary>
    private static object? GetDefault(Type t)
    {
        return _defaultValues.GetOrAdd(t, static type =>
        {
            if (type == typeof(void)) return null;
            if (type == typeof(Task)) return Task.CompletedTask;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var tArg = type.GenericTypeArguments[0];
                return typeof(Task).GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(tArg)
                    .Invoke(null, [GetDefault(tArg)]);
            }
            if (type == typeof(ValueTask)) return (object?)default(ValueTask);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var tArg = type.GenericTypeArguments[0];
                return Activator.CreateInstance(
                    typeof(ValueTask<>).MakeGenericType(tArg),
                    GetDefault(tArg));
            }
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        });
    }

    /// <summary>
    /// Equality comparer for MethodInfo that uses RuntimeMethodHandle for reliable
    /// identity comparison across MethodInfo instances that may originate from
    /// different reflection paths (e.g. expression trees vs DispatchProxy).
    /// </summary>
    private sealed class MethodHandleComparer : IEqualityComparer<MethodInfo>
    {
        public bool Equals(MethodInfo? x, MethodInfo? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.MethodHandle == y.MethodHandle;
        }

        public int GetHashCode(MethodInfo m) => m.MethodHandle.GetHashCode();
    }
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
