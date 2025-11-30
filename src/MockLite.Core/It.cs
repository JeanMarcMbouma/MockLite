using System;

namespace BbQ.MockLite;

/// <summary>
/// Provides argument matchers for flexible mock setup and verification.
/// </summary>
/// <remarks>
/// Argument matchers allow you to set up mock behavior or verify calls based on predicates
/// rather than exact values. This is useful when you care about the type or properties
/// of arguments but not the exact values.
/// </remarks>
public static class It
{
    /// <summary>
    /// Matches any value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of argument to match.</typeparam>
    /// <returns>
    /// A default value of type <typeparamref name="T"/> that acts as a wildcard matcher.
    /// </returns>
    /// <remarks>
    /// This is used in <c>Setup</c> and <c>Verify</c> calls to indicate that any value
    /// of the specified type should match, regardless of the actual value.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.SetupGetUser(It.IsAny&lt;string&gt;(), behavior);
    /// mock.VerifyGetUser(It.IsAny&lt;string&gt;(), Times.Once);
    /// </code>
    /// </example>
    public static T IsAny<T>() => default!;

    /// <summary>
    /// Matches values that satisfy the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of argument to match.</typeparam>
    /// <param name="predicate">A function that returns <c>true</c> for matching values.</param>
    /// <returns>
    /// A default value of type <typeparamref name="T"/> that acts as a conditional matcher.
    /// </returns>
    /// <remarks>
    /// This allows fine-grained control over which values match in setup and verification.
    /// The predicate is evaluated at verification time.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match any userId greater than 100
    /// mock.VerifyGetUser(It.Matches&lt;int&gt;(id => id > 100), Times.AtLeast(1));
    /// 
    /// // Match any string that starts with "test"
    /// mock.SetupDelete(It.Matches&lt;string&gt;(s => s.StartsWith("test")), behavior);
    /// </code>
    /// </example>
    public static T Matches<T>(Predicate<T> predicate) => default!;
}
