using System;

namespace MockLite;

/// <summary>
/// Factory for creating mock instances of interfaces.
/// </summary>
/// <remarks>
/// The <see cref="Mock"/> class provides two strategies for creating mocks:
/// <list type="number">
/// <item>
///   <description>
///   Generated mocks: Uses source-generated mock classes created by the MockLite generator.
///   Mark interfaces with <see cref="GenerateMockAttribute"/> to automatically generate optimized mock implementations.
///   </description>
/// </item>
/// <item>
///   <description>
///   Runtime mocks: Falls back to using DispatchProxy-based proxies for interfaces without generated mocks.
///   Useful for quick testing scenarios but less performant than generated mocks.
///   </description>
/// </item>
/// </list>
/// </remarks>
public static class Mock
{
    /// <summary>
    /// Creates a mock instance of the specified interface type.
    /// </summary>
    /// <typeparam name="T">The interface type to mock. Must be a class or interface.</typeparam>
    /// <returns>
    /// A mock instance that can be configured with <c>Setup</c>, <c>Returns</c>, and verified with <c>Verify</c>.
    /// If a generated mock exists, returns an instance of the generated class; otherwise returns a runtime proxy.
    /// </returns>
    /// <remarks>
    /// Generated mocks are preferred for better performance. Decorate your interfaces with
    /// <see cref="GenerateMockAttribute"/> to generate optimized mock implementations at compile time.
    /// </remarks>
    /// <example>
    /// <code>
    /// [GenerateMock]
    /// public interface IUserRepository
    /// {
    ///     User GetUser(string id);
    /// }
    /// 
    /// var mock = Mock.Of&lt;IUserRepository&gt;();
    /// mock.SetupGetUser(id => new User { Id = id, Name = "Test" });
    /// var user = mock.GetUser("123");
    /// </code>
    /// </example>
    public static T Of<T>() where T : class
    {
        // Try generated type MockXxx in same namespace
        var ifaceName = typeof(T).Name;
        var baseNs = typeof(T).Namespace;
        var mockName = ifaceName.Length > 1 && ifaceName[0] == 'I' && char.IsUpper(ifaceName[1])
            ? $"Mock{ifaceName.Substring(1)}"
            : $"Mock{ifaceName}";
        var fullName = string.IsNullOrEmpty(baseNs) ? mockName : $"{baseNs}.{mockName}";

        var generated = Type.GetType(fullName);
        if (generated is not null) return (T)Activator.CreateInstance(generated)!;

        // Fallback: DispatchProxy-based proxy for quick use
        return RuntimeProxy.Create<T>();
    }
}
