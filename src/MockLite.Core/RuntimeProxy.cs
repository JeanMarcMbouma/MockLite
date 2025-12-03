// src/MockLite.Core/Core/Mock.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
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

    private readonly Dictionary<string, Delegate> _behaviors = [];

    private readonly Dictionary<string, List<(Func<object?[], bool>? matcher, Action<object?[]> callback)>> _callbacks = [];

    /// <summary>
    /// Stores setup information including whether parameters use It.IsAny
    /// </summary>
    private readonly Dictionary<string, (Delegate behavior, bool[] isAnyMatcher, string signatureKey)> _setupInfo = [];

    private static readonly ConcurrentDictionary<Type, object> _anyMatchers = new();

    /// <summary>
    /// Intercepts method calls on the proxied interface.
    /// </summary>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        args ??= [];
        Invocations.Add(new Invocation(targetMethod!, args!));

        // Execute callbacks for this method
        ExecuteCallbacks(targetMethod!, args);

        // First, try exact argument matching (for setups without IsAny or with specific values)
        var argKey = CreateArgumentKey(targetMethod!, args);
        if (_behaviors.TryGetValue(argKey, out var behavior))
        {
            return behavior.Method.GetParameters().Length == 0 ? 
                behavior.DynamicInvoke() :
                behavior.DynamicInvoke(args);
        }

        // Then, try to find a behavior that matches with IsAny matchers
        var anyMatcherBehavior = FindMatchingBehaviorWithIsAny(targetMethod!, args);
        if (anyMatcherBehavior != null)
        {
            return anyMatcherBehavior.Method.GetParameters().Length == 0 ?
                anyMatcherBehavior.DynamicInvoke() :
                anyMatcherBehavior.DynamicInvoke(args);
        }

        // Fall back to method signature only (for setups without specific arguments)
        var sigKey = SignatureKey(targetMethod!);
        if (_behaviors.TryGetValue(sigKey, out var del))
            return del.Method.GetParameters().Length == 0 ?
                del.DynamicInvoke() : del.DynamicInvoke(args);

        var ret = targetMethod!.ReturnType;
        if (ret == typeof(void)) return null;
        if (ret == typeof(Task)) return Task.CompletedTask;
        if (ret.IsGenericType && ret.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var tArg = ret.GenericTypeArguments[0];
            return typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(tArg)
                .Invoke(null, [GetDefault(tArg)]);
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
    /// Finds a matching behavior for the given method and arguments using IsAny matchers.
    /// </summary>
    private Delegate? FindMatchingBehaviorWithIsAny(MethodInfo method, object?[] args)
    {
        var key = SignatureKey(method);
        // Check all setup information entries for this method looking for IsAny matches
        foreach (var kvp in _setupInfo)
        {
            var (behavior, isAnyMatcher, signatureKey) = kvp.Value;
            
            if (!signatureKey.Equals(key))
                continue;
            // Only check setups that have at least one IsAny
            if (!isAnyMatcher.Any(x => x))
                continue;

            // Check if this setup matches the current invocation
            if (DoesSetupMatch(args, isAnyMatcher))
            {
                return behavior;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines if a setup matches the current invocation arguments.
    /// </summary>
    private static bool DoesSetupMatch(object?[] invocationArgs, bool[] isAnyMatcher)
    {
        // If the setup has different number of parameters than invocation, it doesn't match
        if (isAnyMatcher.Length != invocationArgs.Length)
            return false;

        // For each parameter, check if it matches
        for (int i = 0; i < isAnyMatcher.Length; i++)
        {
            // If this parameter was set up with It.IsAny, it matches anything
            if (isAnyMatcher[i])
                continue;

            // Otherwise, we need to check if the values could match
            // For now, we accept any non-null value as potentially matching an exact setup
            // The actual exact matching is done via the argument key comparison in _behaviors dictionary
        }


        return true;
    }

    /// <summary>
    /// Sets up the behavior for a specific method.
    /// </summary>
    /// <param name="method">The method to set up behavior for.</param>
    /// <param name="behavior">The delegate that implements the method behavior.</param>
    public void Setup(MethodInfo method, Delegate behavior)
        => _behaviors[SignatureKey(method)] = behavior;

    /// <summary>
    /// Sets up the behavior for a specific method with specific argument values.
    /// This now supports It.IsAny matchers in the arguments.
    /// </summary>
    /// <param name="method">The method to set up behavior for.</param>
    /// <param name="args">The specific argument values to match (may include It.IsAny markers).</param>
    /// <param name="behavior">The delegate that implements the method behavior.</param>
    public void Setup(MethodInfo method, object?[] args, Delegate behavior)
    {
        var argKey = CreateArgumentKey(method, args);
        var signatureKey = SignatureKey(method);
        _behaviors[argKey] = behavior;
        
        // Store setup info to track which arguments are IsAny matchers
        var isAnyMatcher = args.Select(arg => IsAnyMatcherInstance(arg)).ToArray();
        _setupInfo[argKey] = (behavior, isAnyMatcher, signatureKey);
    }

    /// <summary>
    /// Determines if an argument is an It.IsAny marker instance.
    /// </summary>
    private static bool IsAnyMatcherInstance(object? arg)
    {
        if (arg == null)
            return false;

        //generate a generic signature for Is.IsAny<T>
        if(_anyMatchers.TryGetValue(arg.GetType(), out var cachedMatcher))
        {
            return object.Equals(arg, cachedMatcher);
        }
        var argType = arg.GetType();
        var signature = typeof(It).GetMethod(nameof(It.IsAny))!.MakeGenericMethod(argType);
        var expectedMatcher = signature.Invoke(null, [])!;
        _anyMatchers.TryAdd(argType, expectedMatcher);
        return object.Equals(arg, expectedMatcher);
    }

    /// <summary>
    /// Registers a callback that executes when a method is invoked.
    /// </summary>
    /// <param name="method">The method to register the callback for.</param>
    /// <param name="callback">The callback action to execute.</param>
    public void OnInvocation(MethodInfo method, Action<object?[]> callback)
        => OnInvocation(method, null, callback);

    /// <summary>
    /// Registers a callback that executes when a method is invoked with matching arguments.
    /// </summary>
    /// <param name="method">The method to register the callback for.</param>
    /// <param name="matcher">Optional predicate to match arguments.</param>
    /// <param name="callback">The callback action to execute.</param>
    public void OnInvocation(MethodInfo method, Func<object?[], bool>? matcher, Action<object?[]> callback)
    {
        var key = SignatureKey(method);
        if (!_callbacks.ContainsKey(key))
            _callbacks[key] = [];
        _callbacks[key].Add((matcher, callback));
    }

    /// <summary>
    /// Executes all registered callbacks for a method invocation.
    /// </summary>
    private void ExecuteCallbacks(MethodInfo method, object?[] args)
    {
        var key = SignatureKey(method);
        if (_callbacks.TryGetValue(key, out var callbackList))
        {
            foreach (var (matcher, callback) in callbackList)
            {
                if (matcher == null || matcher(args))
                {
                    callback(args);
                }
            }
        }
    }

    private static string SignatureKey(MethodInfo mi)
    {
        // take into account generic parameters
        var genericPart = mi.IsGenericMethod ? $"<{string.Join(",", mi.GetGenericArguments().Select(ga => ga.FullName))}>" : "";
        var pars = string.Join(",", mi.GetParameters().Select(p => p.ParameterType.FullName));
        return $"{mi.Name}({pars}){genericPart}";
    }

    private static string CreateArgumentKey(MethodInfo mi, object?[] args)
    {
        var genericPart = mi.IsGenericMethod ? $"<{string.Join(",", mi.GetGenericArguments().Select(ga => ga.FullName))}>" : "";
        var pars = string.Join(",", mi.GetParameters().Select(p => p.ParameterType.FullName));
        var argValues = string.Join(",", args.Select(a => 
        {
            // For AnyMatcher instances, use a special marker in the key
            if (a != null && a.GetType().Name == "AnyMatcher")
                return "IsAny";
            return a?.ToString() ?? "null";
        }));
        return $"{mi.Name}({pars})[{argValues}]{genericPart}";
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
