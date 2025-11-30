using System;

namespace MockLite;

/// <summary>
/// Provides common verification predicates for mock method calls.
/// </summary>
/// <remarks>
/// <see cref="Times"/> offers convenient predicates to verify how many times a mock method
/// should have been called. Use these with <c>Verify*</c> methods in generated mocks or
/// with the <see cref="VerificationException"/> if verification fails.
/// </remarks>
public static class Times
{
    /// <summary>
    /// Verifies that a method was called exactly once.
    /// </summary>
    /// <value>
    /// A predicate that returns <c>true</c> if the call count equals 1.
    /// </value>
    /// <example>
    /// <code>
    /// mock.VerifyGetUser(Times.Once);
    /// </code>
    /// </example>
    public static Func<int, bool> Once => c => c == 1;

    /// <summary>
    /// Verifies that a method was never called.
    /// </summary>
    /// <value>
    /// A predicate that returns <c>true</c> if the call count equals 0.
    /// </value>
    /// <example>
    /// <code>
    /// mock.VerifyDelete(Times.Never);
    /// </code>
    /// </example>
    public static Func<int, bool> Never => c => c == 0;

    /// <summary>
    /// Verifies that a method was called exactly the specified number of times.
    /// </summary>
    /// <param name="n">The exact number of expected calls.</param>
    /// <returns>A predicate that returns <c>true</c> if the call count equals <paramref name="n"/>.</returns>
    /// <example>
    /// <code>
    /// mock.VerifyGetUser(Times.Exactly(3));
    /// </code>
    /// </example>
    public static Func<int, bool> Exactly(int n) => c => c == n;

    /// <summary>
    /// Verifies that a method was called at least the specified number of times.
    /// </summary>
    /// <param name="n">The minimum number of expected calls.</param>
    /// <returns>A predicate that returns <c>true</c> if the call count is greater than or equal to <paramref name="n"/>.</returns>
    /// <example>
    /// <code>
    /// mock.VerifyGetUser(Times.AtLeast(2));
    /// </code>
    /// </example>
    public static Func<int, bool> AtLeast(int n) => c => c >= n;

    /// <summary>
    /// Verifies that a method was called at most the specified number of times.
    /// </summary>
    /// <param name="n">The maximum number of expected calls.</param>
    /// <returns>A predicate that returns <c>true</c> if the call count is less than or equal to <paramref name="n"/>.</returns>
    /// <example>
    /// <code>
    /// mock.VerifyGetUser(Times.AtMost(5));
    /// </code>
    /// </example>
    public static Func<int, bool> AtMost(int n) => c => c <= n;
}

