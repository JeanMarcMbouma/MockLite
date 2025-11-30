# MockLite

A lightweight, high-performance mocking framework for .NET that combines compile-time code generation with runtime flexibility. MockLite enables you to create optimized mock implementations at build time while providing fallback runtime proxies for dynamic scenarios.

## Features

- ? **Compile-Time Code Generation** - Automatically generates optimized mock classes using source generators
- ?? **High Performance** - Generated mocks are as fast as hand-written implementations
- ?? **Runtime Fallback** - Seamless fallback to DispatchProxy-based runtime mocks for interfaces without generated mocks
- ? **Async Support** - First-class support for async methods (`Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`)
- ?? **Invocation Recording** - Automatically records all method invocations with timestamps for verification
- ?? **Argument Matching** - Pattern matching for mock setup and verification
- ?? **Verification** - Flexible verification with Times predicates (`Once`, `Never`, `Exactly`, `AtLeast`, `AtMost`)
- ?? **Easy API** - Simple, intuitive API inspired by popular mocking frameworks

## Getting Started

### Installation

Add MockLite to your project:

```bash
dotnet add package MockLite.Core
```

### Basic Usage

```csharp
using MockLite;

// 1. Define your interface
[GenerateMock]
public interface IUserRepository
{
    User? GetUser(string userId);
    void SaveUser(User user);
}

// 2. Create a mock
var userRepo = Mock.Of<IUserRepository>();

// 3. Setup behavior
var testUser = new User { Id = "123", Name = "John Doe" };
// For generated mocks: userRepo.SetupGetUser(userId => testUser);

// 4. Use in tests
var user = userRepo.GetUser("123");

// 5. Verify interactions
// For generated mocks: userRepo.VerifyGetUser(Times.Once);
```

## Advanced Setup and Verification with Fluent API

MockLite provides a fluent builder API through `Mock.Create<T>()` for advanced mock configuration:

```csharp
using MockLite;

// Create a builder instance
var builder = Mock.Create<IUserRepository>();

// Setup method behaviors with specific return values
builder
    .Setup(x => x.GetUser("123"), () => new User { Id = "123", Name = "John Doe" })
    .Setup(x => x.GetUser("456"), () => new User { Id = "456", Name = "Jane Smith" });

// Setup property getters and setters
builder
    .SetupGet(x => x.IsActive, () => true)
    .ReturnsGet(x => x.Count, 42)
    .SetupSet(x => x.Name, (value) => { /* handle set */ });

// Get the configured mock instance
var mock = builder.Object;

// Use the mock in your test
var user = mock.GetUser("123");
var isActive = mock.IsActive;

// Verify method invocations
builder.Verify(x => x.GetUser("123"), times => times == 1);
builder.Verify(x => x.GetUser("456"), times => times == 0);

// Verify property access
builder.VerifyGet(x => x.IsActive, times => times >= 1);
builder.VerifySet(x => x.Name, times => times == 0);

// Access all recorded invocations
foreach (var invocation in builder.Invocations)
{
    Console.WriteLine($"Called: {invocation.Method.Name}");
}
```

### Fluent API Features

**Setup Methods:**
- `Setup<TResult>(expression, behavior)` - Configure method return values
- `SetupGet<TProp>(property, getter)` - Setup property getter behavior
- `ReturnsGet<TProp>(property, value)` - Convenience method for constant property values
- `SetupSet<TProp>(property, setter)` - Setup property setter behavior

**Verification Methods:**
- `Verify(expression, times)` - Verify method was called N times
- `Verify(expression, matcher, times)` - Verify method with argument matching
- `VerifyGet<TProp>(property, times)` - Verify property getter access count
- `VerifySet<TProp>(property, times)` - Verify property setter call count
- `VerifySet<TProp>(property, matcher, times)` - Verify property setter with value matching

**Properties:**
- `Object` - Get the mock instance
- `Invocations` - Access all recorded invocations for custom verification

## Two-Tier Mocking Strategy

MockLite uses an intelligent two-tier approach:

### Tier 1: Compile-Time Generated Mocks (Recommended)

Mark interfaces with `[GenerateMock]` attribute to generate optimized mock implementations at compile time:

```csharp
[GenerateMock]
public interface IPaymentGateway
{
    bool ProcessPayment(decimal amount);
    Task<bool> ProcessPaymentAsync(decimal amount);
}

// The MockLite generator creates: MockPaymentGateway class
var mock = Mock.Of<IPaymentGateway>();
```

**Benefits:**
- Zero runtime overhead
- IntelliSense support for mock-specific methods
- Ahead-of-time compilation compatible
- Type-safe setup and verification

### Tier 2: Runtime Proxies (Fallback)

For interfaces without `[GenerateMock]`, MockLite automatically falls back to runtime proxies:

```csharp
// No [GenerateMock] attribute - uses DispatchProxy at runtime
public interface IQuickMock
{
    string GetValue();
}

var mock = Mock.Of<IQuickMock>(); // Automatically creates runtime proxy
```

**Benefits:**
- Quick testing without code generation
- No attribute decorators needed
- Useful for third-party interfaces

## Core Concepts

### Invocation Recording

All method calls on mocks are automatically recorded:

```csharp
var mock = Mock.Of<IRepository>();
mock.GetUser("123");
mock.GetUser("456");

// Access invocations for custom verification
var invocations = ((dynamic)mock).Invocations;
foreach (var invocation in invocations)
{
    Console.WriteLine($"{invocation.Method.Name} called with {invocation.Arguments}");
}
```

### Times Predicates

Verify method call counts with flexible predicates:

```csharp
var mock = Mock.Of<IService>();

// Example predicates:
Times.Once        // Exactly 1 call
Times.Never       // Exactly 0 calls
Times.Exactly(3)  // Exactly 3 calls
Times.AtLeast(2)  // At least 2 calls
Times.AtMost(5)   // At most 5 calls
```

### Async Support

MockLite fully supports async methods:

```csharp
public interface IAsyncRepository
{
    Task<User?> GetUserAsync(string userId);
    ValueTask SaveUserAsync(User user);
}

var mock = Mock.Of<IAsyncRepository>();
// Setup and verify async methods seamlessly
```

## Project Structure

- **MockLite.Core** - Core runtime helpers (DispatchProxy, invocation recording)
- **MockLite.Generators** - Source generator for compile-time mock generation
- **MockLite.Sample** - Comprehensive examples and usage patterns
- **MockLite.Core.Tests** - Unit tests for core functionality
- **MockLite.Generators.Tests** - Unit tests for source generators

## Architecture

```
MockLite Framework
??? Source Generators (Compile-time)
?   ??? Generates optimized MockXxx classes from [GenerateMock] interfaces
?
??? Core Runtime (Execution-time)
?   ??? RuntimeProxy<T> - DispatchProxy-based fallback implementation
?   ??? Invocation - Records method calls with timestamps
?   ??? MockBuilder<T> - Fluent API for advanced mock creation
?   ??? Mock - Factory for creating mock instances
?
??? Supporting Infrastructure
    ??? GenerateMockAttribute - Marks interfaces for code generation
    ??? It - Argument matchers for flexible matching
    ??? Times - Verification count predicates
```

## Best Practices

1. **Use Generated Mocks** - Always apply `[GenerateMock]` to interfaces you control
2. **Setup First** - Configure mock behavior before using in tests
3. **Verify Behaviors** - Use the verification API to assert interactions
4. **Use Argument Matchers** - `It.IsAny<T>()` for flexible matching
5. **Record Invocations** - Leverage invocation recording for complex verification scenarios

## Examples

See the [MockLite.Sample](./src/MockLite.Sample/Program.cs) project for comprehensive examples covering:

- Basic mock creation and setup
- Argument matchers
- Times predicates for verification
- Exception handling
- Async methods
- Invocation recording
- Complete integration scenarios

## Requirements

- .NET 8.0 or later
- C# 14 or later

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues on [GitHub](https://github.com/JeanMarcMbouma/MockLite).

## Support

For questions, issues, or feature requests, please visit the [GitHub repository](https://github.com/JeanMarcMbouma/MockLite).