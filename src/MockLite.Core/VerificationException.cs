using System;

namespace MockLite;

/// <summary>
/// Exception thrown when mock verification fails.
/// </summary>
/// <remarks>
/// This exception is raised by <c>Verify*</c> methods in generated mocks when
/// the expected number of invocations does not match the actual invocations.
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     mock.VerifyGetUser(Times.Once);
/// }
/// catch (VerificationException ex)
/// {
///     Console.WriteLine($"Verification failed: {ex.Message}");
/// }
/// </code>
/// </example>
public sealed class VerificationException(string message) : Exception(message);