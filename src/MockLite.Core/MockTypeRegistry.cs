using System;
using System.Collections.Concurrent;

namespace BbQ.MockLite;

/// <summary>
/// Compile-time registry mapping interface types to their source-generated mock factory delegates.
/// </summary>
/// <remarks>
/// Each class produced by the MockLite source generator registers itself here via a
/// <c>[ModuleInitializer]</c>, so lookups in <see cref="Mock.Of{T}"/> are O(1) dictionary
/// reads with no string building, <c>Type.GetType</c> reflection, or
/// <c>Activator.CreateInstance</c> overhead.
/// </remarks>
public static class MockTypeRegistry
{
    private static readonly ConcurrentDictionary<Type, Func<object>> _factories = new();

    /// <summary>
    /// Registers a factory delegate for <typeparamref name="TInterface"/>.
    /// Called automatically by the <c>[ModuleInitializer]</c> emitted in each generated mock file.
    /// </summary>
    /// <typeparam name="TInterface">The interface type being mocked.</typeparam>
    /// <typeparam name="TMock">The generated concrete mock class.</typeparam>
    public static void Register<TInterface, TMock>()
        where TInterface : class
        where TMock : TInterface, new()
        => _factories[typeof(TInterface)] = () => new TMock();

    /// <summary>
    /// Attempts to create an instance of the registered mock for <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="interfaceType">The interface type to look up.</param>
    /// <param name="instance">
    /// When this method returns <c>true</c>, contains the newly created mock instance;
    /// otherwise <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if a factory was registered for <paramref name="interfaceType"/>.</returns>
    internal static bool TryCreate(Type interfaceType, out object? instance)
    {
        if (_factories.TryGetValue(interfaceType, out var factory))
        {
            instance = factory();
            return true;
        }

        instance = null;
        return false;
    }
}
