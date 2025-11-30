using System;

namespace BbQ.MockLite;

/// <summary>
/// Marks an interface for mock code generation.
/// </summary>
/// <remarks>
/// Apply this attribute to interfaces that you want MockLite to generate optimized
/// mock implementations for. The generated mock class will be named <c>Mock</c> followed
/// by the interface name (e.g., <c>IUserService</c> → <c>MockUserService</c>).
/// 
/// Two variations of this attribute are available:
/// <list type="bullet">
/// <item>
///   <term><see cref="GenerateMockAttribute"/></term>
///   <description>
///   Applied directly to interfaces: <c>[GenerateMock] public interface IMyInterface { }</c>
///   </description>
/// </item>
/// <item>
///   <term><see cref="GenerateMockAttribute{T}"/></term>
///   <description>
///   Applied to classes to generate mocks for external interfaces:
///   <c>[GenerateMock&lt;IExternalInterface&gt;] public partial class Mocks { }</c>
///   </description>
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// [GenerateMock]
/// public interface IUserRepository
/// {
///     User GetUser(string id);
///     void SaveUser(User user);
/// }
/// 
/// // In your test:
/// var mock = Mock.Of&lt;IUserRepository&gt;();
/// mock.SetupGetUser(id => new User { Id = id, Name = "Test" });
/// var user = mock.GetUser("123");
/// mock.VerifyGetUser(Times.Once);
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class GenerateMockAttribute(Type interfaceType) : Attribute
{
    /// <summary>
    /// Gets the interface type for which to generate a mock.
    /// </summary>
    public Type Type { get; } = interfaceType;
}

/// <summary>
/// Marks a class to generate mocks for one or more external interfaces.
/// </summary>
/// <remarks>
/// This generic variant of <see cref="GenerateMockAttribute"/> is used when you want to
/// generate mocks for interfaces that are not directly marked with the non-generic attribute.
/// This is useful for third-party interfaces or interfaces defined in external assemblies.
/// </remarks>
/// <example>
/// <code>
/// // Generate mock for an external interface
/// [GenerateMock&lt;IExternalUserService&gt;]
/// public partial class ExternalMocks { }
/// 
/// // Now you can use it:
/// var mock = Mock.Of&lt;IExternalUserService&gt;();
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GenerateMockAttribute<T> : Attribute { }
