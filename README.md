# BbQ.MockLite

A lightweight, high-performance mocking framework for .NET that combines compile-time code generation with runtime flexibility. BbQ.MockLite enables you to create optimized mock implementations at build time while providing fallback runtime proxies for dynamic scenarios.

## Features

- ✨ **Compile-Time Code Generation** - Automatically generates optimized mock classes using source generators
- 🚀 **High Performance** - Generated mocks are as fast as hand-written implementations
- 🔄 **Runtime Fallback** - Seamless fallback to DispatchProxy-based runtime mocks for interfaces without generated mocks
- ⚡ **Async Support** - First-class support for async methods (`Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`)
- 📝 **Invocation Recording** - Automatically records all method invocations with timestamps for verification
- 🎯 **Argument Matching** - Pattern matching for mock setup and verification
- 🔍 **Verification** - Flexible verification with Times predicates (`Once`, `Never`, `Exactly`, `AtLeast`, `AtMost`)
- 🔗 **Callbacks** - Execute custom logic when methods are called or properties are accessed
- 🎓 **Easy API** - Simple, intuitive API inspired by popular mocking frameworks

## Getting Started

### Installation

Add BbQ.MockLite to your project:

```bash
dotnet add package BbQ.MockLite
```

### Basic Usage

```csharp
using BbQ.MockLite;

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

BbQ.MockLite provides a fluent builder API through `Mock.Create<T>()` for advanced mock configuration:

```csharp
using BbQ.MockLite;

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

**Callback Methods:**
- `OnCall(expression, callback)` - Execute logic when method is called
- `OnCall(expression, matcher, callback)` - Execute logic when method is called with matching arguments
- `OnPropertyAccess<T>(property, callback)` - Execute logic on property get or set
- `OnGetCallback<T>(property, callback)` - Execute logic when property getter is accessed
- `OnSetCallback<T>(property, callback)` - Execute logic when property setter is called
- `OnSetCallback<T>(property, matcher, callback)` - Execute logic when property setter is called with matching value

**Properties:**
- `Object` - Get the mock instance
- `Invocations` - Access all recorded invocations for custom verification

## Callbacks for Custom Logic Execution

BbQ.MockLite supports callbacks that execute custom logic when methods are called or properties are accessed. This is useful for audit logging, state management, and complex verification scenarios.

```csharp
using BbQ.MockLite;

// Track method calls with custom logic
var auditLog = new List<string>();
var builder = Mock.Create<IUserRepository>()
    .OnCall(x => x.GetUser(It.IsAny<string>()),
        args => auditLog.Add($"GetUser called with: {args[0]}"))
    .OnCall(x => x.SaveUser(It.IsAny<User>()),
        args => args[0] is User u && u.IsAdmin,
        args => auditLog.Add($"Admin user saved: {((User)args[0]).Name}"));

var mock = builder.Object;

// Use the mock
var user = mock.GetUser("123");
mock.SaveUser(new User { Name = "Admin", IsAdmin = true });

// Verify callbacks were executed
Assert.Equal(2, auditLog.Count);
```

### Callback Patterns

**Track method calls:**
```csharp
var callCount = 0;
mock.OnCall(x => x.Process(It.IsAny<string>()), 
    args => callCount++);
```

**Conditional callbacks:**
```csharp
var adminActions = new List<string>();
mock.OnCall(
    x => x.Execute(It.IsAny<string>()),
    args => args[0] is string s && s.StartsWith("admin"),
    args => adminActions.Add((string)args[0]));
```

**Track property access:**
```csharp
var propertyLog = new List<string>();
mock
    .OnGetCallback(x => x.Status, 
        () => propertyLog.Add("Status read"))
    .OnSetCallback(x => x.Status, 
        value => propertyLog.Add($"Status set to: {value}"));
```

## Two-Tier Mocking Strategy

BbQ.MockLite uses an intelligent two-tier approach:

### Tier 1: Compile-Time Generated Mocks (Recommended)

Mark interfaces with `[GenerateMock]` attribute to generate optimized mock implementations at compile time:

```csharp
[GenerateMock]
public interface IPaymentGateway
{
    bool ProcessPayment(decimal amount);
    Task<bool> ProcessPaymentAsync(decimal amount);
}

// The BbQ.MockLite generator creates: MockPaymentGateway class
var mock = Mock.Of<IPaymentGateway>();
```

**Benefits:**
- Zero runtime overhead
- IntelliSense support for mock-specific methods
- Ahead-of-time compilation compatible
- Type-safe setup and verification

### Tier 2: Runtime Proxies (Fallback)

For interfaces without `[GenerateMock]`, BbQ.MockLite automatically falls back to runtime proxies:

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

BbQ.MockLite fully supports async methods:

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

- **BbQ.MockLite** - Core runtime helpers (DispatchProxy, invocation recording, callbacks)
- **BbQ.MockLite.Generators** - Source generator for compile-time mock generation
- **BbQ.MockLite.Sample** - Comprehensive examples and usage patterns
- **BbQ.MockLite.Tests** - Unit tests for core functionality
- **BbQ.MockLite.Generators.Tests** - Unit tests for source generators

## Architecture

```
BbQ.MockLite Framework
├── Source Generators (Compile-time)
│   └── Generates optimized MockXxx classes from [GenerateMock] interfaces
│
├── Core Runtime (Execution-time)
│   ├── RuntimeProxy<T> - DispatchProxy-based fallback with callback support
│   ├── Invocation - Records method calls with timestamps
│   ├── Mock<T> - Fluent builder for setup, verification, and callbacks
│   └── Mock - Factory for creating mock instances
│
└── Supporting Infrastructure
    ├── GenerateMockAttribute - Marks interfaces for code generation
    ├── It - Argument matchers for flexible matching
    └── Times - Verification count predicates
```

## Best Practices

1. **Use Generated Mocks** - Always apply `[GenerateMock]` to interfaces you control
2. **Setup First** - Configure mock behavior before using in tests
3. **Verify Behaviors** - Use the verification API to assert interactions
4. **Use Argument Matchers** - `It.IsAny<T>()` for flexible matching
5. **Leverage Callbacks** - Use callbacks for audit logging and state tracking
6. **Record Invocations** - Leverage invocation recording for complex verification scenarios

## Examples

See the [BbQ.MockLite.Sample](./src/MockLite.Sample/Program.cs) project for comprehensive examples covering:

- Basic mock creation and setup
- Argument matchers
- Times predicates for verification
- Exception handling
- Async methods
- Invocation recording
- Callback usage
- Complete integration scenarios

## Documentation

For detailed documentation on callbacks and advanced features, see:
- [Callback Feature Guide](./CALLBACK_FEATURE_GUIDE.md) - Complete API reference
- [Callback Quick Reference](./CALLBACK_QUICK_REFERENCE.md) - Quick start and examples
- [Feature Summary](./FEATURE_COMPLETE_SUMMARY.md) - Implementation overview

## Requirements

- .NET 8.0 or later
- C# 14 or later

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues on [GitHub](https://github.com/JeanMarcMbouma/MockLite).

## Support

For questions, issues, or feature requests, please visit the [GitHub repository](https://github.com/JeanMarcMbouma/MockLite).