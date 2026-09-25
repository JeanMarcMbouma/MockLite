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
    /// Internal marker type used to indicate that a wildcard matcher was used.
    /// This is used internally by the matching logic to detect It.IsAny usage.
    /// </summary>
    internal sealed class AnyMatcher
    {
        private AnyMatcher() { }
        internal static readonly AnyMatcher Instance = new();
        public override string ToString() => "It.IsAny";
    }

    /// <summary>
    /// Internal marker type used to carry a predicate matcher through the argument pipeline.
    /// Created internally when the framework detects an <c>It.Matches&lt;T&gt;</c> call
    /// in the expression tree.
    /// </summary>
    internal sealed class PredicateMatcher
    {
        private readonly Func<object?, bool> _predicate;
        internal PredicateMatcher(Func<object?, bool> predicate) => _predicate = predicate;
        internal bool Matches(object? value) => _predicate(value);
        public override string ToString() => "It.Matches";
    }

    /// <summary>
    /// Matches any value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of argument to match.</typeparam>
    /// <returns>
    /// The default value of <typeparamref name="T"/>. The matcher is recognized from
    /// the expression tree before this method is evaluated.
    /// </returns>
    /// <remarks>
    /// This method is intended for MockLite expression APIs such as <c>Setup</c>.
    /// Matcher identity is carried by expression parsing rather than by reinterpreting
    /// an internal marker as an arbitrary user type.
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
    /// A marker value that indicates values matching the predicate should match.
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