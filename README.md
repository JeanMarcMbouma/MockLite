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
- 💎 **Strongly-Typed Handlers** - Type-safe Setup and OnCall with partial parameter signatures
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

// Or use the fluent property setup API
builder
    .SetupGet(x => x.IsActive).Returns(true)
    .SetupGet(x => x.Count).Returns(42)
    .SetupSet<string>(x => x.Name).Callback(value => { /* handle set */ });

// Get the configured mock instance
var mock = builder.Object;

// Use the mock in your test
var user = mock.GetUser("123");
var isActive = mock.IsActive;
mock.SaveUser(new User { Id = "123", Name = "John Doe" });

// Verify method invocations
builder.Verify(x => x.GetUser("123"), times => times == 1);
builder.Verify(x => x.GetUser("456"), times => times == 0);

// Verify void method invocations
builder.Verify(x => x.SaveUser(new User { Id = "123", Name = "John Doe" }), Times.Once);

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
- `Setup<TResult>(expression)` - Begin a fluent setup returning `SetupPhrase<TResult>` for chaining `.Callback()`, `.Returns()`, `.ReturnsAsync()`, or `.Throws()`
- `Setup<TResult, T1>(expression, handler)` - Configure with strongly-typed handler receiving first parameter
- `Setup<TResult, T1, T2>(expression, handler)` - Configure with handler receiving first two parameters
- `Setup<TResult, T1, T2, T3>(expression, handler)` - Configure with handler receiving first three parameters
- `SetupGet<TProp>(property, getter)` - Setup property getter behavior
- `SetupGet<TProp>(property)` - Begin a fluent setup returning `GetSetupPhrase<TProp>` for chaining `.Callback()`, `.Returns()`, or `.Throws()`
- `ReturnsGet<TProp>(property, value)` - Convenience method for constant property values
- `SetupSet<TProp>(property, setter)` - Setup property setter behavior
- `SetupSet<TProp>(property)` - Begin a fluent setup returning `SetSetupPhrase<TProp>` for chaining `.Callback()` or `.Throws()`
- `SetupSequence<TResult>(expression, values)` - Return different values on successive calls (last value repeats)
- `Throws<TResult>(expression, exception)` - Throw an exception when a return-value method is called
- `Throws(expression, exception)` - Throw an exception when a void method is called

**Fluent SetupPhrase Methods** (returned by `Setup(expression)`):
- `.Returns(value)` - Configure a constant return value
- `.Returns(factory)` - Configure a factory-based return value
- `.ReturnsAsync<TInner>(value)` - Configure a `Task<T>` return with covariance support
- `.Throws(exception)` - Configure the method to throw an exception
- `.Callback(callback)` - Register a parameterless callback (returns `SetupPhrase` for further chaining)
- `.Callback(callback)` - Register a callback receiving the raw `object?[]` arguments
- `.Callback<T1>(callback)` - Register a strongly-typed callback for the first parameter
- `.Callback<T1, T2>(callback)` - Register a strongly-typed callback for the first two parameters
- `.Callback<T1, T2, T3>(callback)` - Register a strongly-typed callback for the first three parameters

**Fluent GetSetupPhrase Methods** (returned by `SetupGet(property)`):
- `.Returns(value)` - Configure a constant return value for the property getter
- `.Returns(factory)` - Configure a factory-based return value for the property getter
- `.Throws(exception)` - Configure the property getter to throw an exception
- `.Callback(callback)` - Register a parameterless callback when the getter is accessed (returns `GetSetupPhrase` for further chaining)

**Fluent SetSetupPhrase Methods** (returned by `SetupSet(property)`):
- `.Throws(exception)` - Configure the property setter to throw an exception
- `.Callback(callback)` - Register a parameterless callback when the setter is called (returns `SetSetupPhrase` for further chaining)
- `.Callback(callback)` - Register a strongly-typed callback receiving the assigned value (returns `SetSetupPhrase` for further chaining)

**Verification Methods:**
- `Verify<TResult>(expression, times)` - Verify a return-value method was called N times
- `Verify(voidExpression, times)` - Verify a void method was called N times
- `Verify(expression, matcher, times)` - Verify method with argument matching
- `VerifyGet<TProp>(property, times)` - Verify property getter access count
- `VerifySet<TProp>(property, times)` - Verify property setter call count
- `VerifySet<TProp>(property, matcher, times)` - Verify property setter with value matching

**Callback Methods:**
- `OnCall(expression, callback)` - Execute logic when method is called
- `OnCall(expression, handler)` - Execute logic with no parameters when method is called
- `OnCall<T1>(expression, handler)` - Execute logic with strongly-typed first parameter
- `OnCall<T1, T2>(expression, handler)` - Execute logic with first two parameters
- `OnCall<T1, T2, T3>(expression, handler)` - Execute logic with first three parameters
- `OnCall(expression, matcher, callback)` - Execute logic when method is called with matching arguments
- `OnPropertyAccess<T>(property, callback)` - Execute logic on property get or set
- `OnGetCallback<T>(property, callback)` - Execute logic when property getter is accessed
- `OnSetCallback<T>(property, callback)` - Execute logic when property setter is called
- `OnSetCallback<T>(property, matcher, callback)` - Execute logic when property setter is called with matching value

**Reset Method:**
- `Reset()` - Clear all recorded invocations (setups and callbacks are preserved)

**Properties:**
- `Object` - Get the mock instance
- `Invocations` - Access all recorded invocations for custom verification

## Strongly-Typed Callbacks with Partial Parameters

BbQ.MockLite allows you to configure callbacks and return value handlers that receive only the parameters you care about, while maintaining full type safety.

### Setup with Strongly-Typed Handlers

Configure method return values using handlers that receive a subset of the method's parameters:

```csharp
using BbQ.MockLite;

var builder = Mock.Create<IQueryService>();

// Handler receives only the first parameter (type-safe)
builder.Setup(x => x.Query("proc", 1, 2), 
    (string proc) => $"Result: {proc}");

// Handler receives first two parameters
builder.Setup(x => x.Query("proc", 1, 2), 
    (string proc, int id) => $"{proc}-{id}");

// Handler receives all three parameters
builder.Setup(x => x.Query("proc", 1, 2), 
    (string proc, int id, int count) => $"{proc}:{id}:{count}");

// Parameterless handler (doesn't need any parameters)
builder.Setup(x => x.Query("proc", 1, 2), 
    () => "Fixed result");
```

### OnCall with Strongly-Typed Handlers

Execute custom logic using strongly-typed handlers that receive only the parameters you need:

```csharp
using BbQ.MockLite;

var builder = Mock.Create<IUserRepository>();

// Parameterless callback
builder.OnCall(x => x.GetUser(It.IsAny<string>()), 
    () => Console.WriteLine("GetUser called"));

// Type-safe callback with first parameter
builder.OnCall(x => x.GetUser(It.IsAny<string>()), 
    (string userId) => Console.WriteLine($"Getting user: {userId}"));

// Type-safe callback with multiple parameters
builder.OnCall(x => x.UpdateUser(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<bool>()), 
    (string userId, User user) => 
        Console.WriteLine($"Updating {userId} to {user.Name}"));

// Works with void methods too
builder.OnCall(x => x.DeleteUser(It.IsAny<string>(), It.IsAny<bool>()), 
    (string userId) => Console.WriteLine($"Deleting: {userId}"));
```

### Benefits

- **Type Safety**: Compile-time checking ensures parameter types match
- **Clean Code**: Only handle the parameters you care about, ignore the rest
- **IntelliSense Support**: Full IDE support with parameter names and types
- **Flexible**: Works with methods that have any number of parameters

## Moq-Style Fluent Setup with Callback

BbQ.MockLite supports the familiar Moq-style `Setup(...).Callback(...).Returns(...)` chaining pattern. The `Callback` methods on `SetupPhrase` let you register side-effect logic inline with your setup, and then continue chaining to configure a return value:

```csharp
using BbQ.MockLite;

var builder = Mock.Create<IUserRepository>();
var callLog = new List<string>();

// Parameterless callback — track that the method was called
builder.Setup(x => x.GetUser("123"))
    .Callback(() => callLog.Add("GetUser called"))
    .Returns(new User { Id = "123", Name = "John Doe" });

// Strongly-typed callback — capture the argument
builder.Setup(x => x.GetUser(It.IsAny<string>()))
    .Callback<string>(id => callLog.Add($"GetUser({id})"))
    .Returns(new User { Id = "default" });

// Raw argument array callback
builder.Setup(x => x.GetUser(It.IsAny<string>()))
    .Callback((object?[] args) => callLog.Add($"args: {args[0]}"))
    .Returns(new User { Id = "captured" });

// Callback with ReturnsAsync for async methods
builder.Setup(x => x.GetUserAsync("123"))
    .Callback(() => callLog.Add("async call"))
    .ReturnsAsync(new User { Id = "123" });

// Callback with Throws
builder.Setup(x => x.GetUser("bad-id"))
    .Callback(() => callLog.Add("error path"))
    .Throws(new KeyNotFoundException("User not found"));
```

### Available Callback Overloads on SetupPhrase

| Method | Description | Returns |
|---|---|---|
| `.Callback(Action)` | Parameterless callback | `SetupPhrase` (chainable) |
| `.Callback(Action<object?[]>)` | Raw argument array callback | `SetupPhrase` (chainable) |
| `.Callback<T1>(Action<T1>)` | Strongly-typed first parameter | `SetupPhrase` (chainable) |
| `.Callback<T1,T2>(Action<T1,T2>)` | Strongly-typed first two parameters | `SetupPhrase` (chainable) |
| `.Callback<T1,T2,T3>(Action<T1,T2,T3>)` | Strongly-typed first three parameters | `SetupPhrase` (chainable) |

All `Callback` methods return the `SetupPhrase`, so you can chain further with `.Returns()`, `.ReturnsAsync()`, or `.Throws()`. The terminal methods (`.Returns()`, `.ReturnsAsync()`, `.Throws()`) return `Mock<T>` so you can continue configuring the builder.

## Fluent Property Setup

BbQ.MockLite supports Moq-style fluent property setup through `SetupGet` and `SetupSet` phrases. These allow chaining `.Returns()`, `.Callback()`, and `.Throws()` for property accessors:

```csharp
using BbQ.MockLite;

var builder = Mock.Create<IPropertyService>();

// Fluent getter setup with Returns
builder.SetupGet(x => x.Name).Returns("John");

// Fluent getter setup with factory
builder.SetupGet(x => x.Count).Returns(() => DateTime.Now.Second);

// Fluent getter setup with Callback then Returns
var getLog = new List<string>();
builder.SetupGet(x => x.Name)
    .Callback(() => getLog.Add("Name accessed"))
    .Returns("John");

// Fluent getter setup with Throws
builder.SetupGet(x => x.Name).Throws(new InvalidOperationException("not ready"));

// Fluent setter setup with typed Callback
var setLog = new List<string>();
builder.SetupSet<string>(x => x.Name)
    .Callback(value => setLog.Add($"Name set to: {value}"));

// Fluent setter setup with Throws
builder.SetupSet<string>(x => x.Name)
    .Throws(new InvalidOperationException("read-only property"));

// Chain getter and setter setup together
builder
    .SetupGet(x => x.Name)
        .Callback(() => getLog.Add("read"))
        .Returns("test")
    .SetupSet<string>(x => x.Name)
        .Callback(v => setLog.Add($"wrote: {v}"))
        .Throws(new NotSupportedException());
```

### Available Methods on GetSetupPhrase

| Method | Description | Returns |
|---|---|---|
| `.Returns(value)` | Constant return value | `Mock<T>` (terminal) |
| `.Returns(factory)` | Factory-based return value | `Mock<T>` (terminal) |
| `.Throws(exception)` | Throw on getter access | `Mock<T>` (terminal) |
| `.Callback(Action)` | Parameterless callback | `GetSetupPhrase` (chainable) |

### Available Methods on SetSetupPhrase

| Method | Description | Returns |
|---|---|---|
| `.Throws(exception)` | Throw on setter call | `Mock<T>` (terminal) |
| `.Callback(Action)` | Parameterless callback | `SetSetupPhrase` (chainable) |
| `.Callback(Action<TProp>)` | Typed callback with assigned value | `SetSetupPhrase` (chainable) |

## Callbacks for Custom Logic Execution

BbQ.MockLite supports callbacks that execute custom logic when methods are called or properties are accessed. This is useful for audit logging, state management, and complex verification scenarios.

```csharp
using BbQ.MockLite;

// Track method calls with custom logic using strongly-typed handlers
var auditLog = new List<string>();
var builder = Mock.Create<IUserRepository>()
    // Traditional approach with object array
    .OnCall(x => x.GetUser(It.IsAny<string>()),
        args => auditLog.Add($"GetUser called with: {args[0]}"))
    // Strongly-typed approach (new)
    .OnCall(x => x.SaveUser(It.IsAny<User>()),
        (User user) => auditLog.Add($"SaveUser called with: {user.Name}"))
    // Conditional callback with matcher
    .OnCall(x => x.DeleteUser(It.IsAny<string>()),
        args => args[0] is string id && id.StartsWith("admin"),
        args => auditLog.Add($"Admin user deleted: {args[0]}"));

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
// Parameterless callback
mock.OnCall(x => x.Process(It.IsAny<string>()), 
    () => callCount++);
```

**Type-safe parameter access:**
```csharp
var processedItems = new List<string>();
// Strongly-typed callback
mock.OnCall(x => x.Process(It.IsAny<string>()), 
    (string item) => processedItems.Add(item));
```

**Conditional callbacks:**
```csharp
var adminActions = new List<string>();
// Mix of type-safe handlers and matchers
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

## Exception Throwing

Configure a mock to throw an exception when a method is called:

```csharp
using BbQ.MockLite;

var builder = Mock.Create<IUserRepository>();

// Throw from a return-value method
builder.Throws(x => x.GetUser("bad-id"), new KeyNotFoundException("User not found"));

// Throw from a void method
builder.Throws(x => x.DeleteUser("restricted"), new UnauthorizedAccessException("Cannot delete"));

// Supports argument matchers
builder.Throws(x => x.GetUser(It.IsAny<string>()), new InvalidOperationException("Service unavailable"));

var mock = builder.Object;

try
{
    mock.GetUser("bad-id");
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine(ex.Message); // "User not found"
}
```

## Sequence Setup

Return different values on successive calls with `SetupSequence`. Once all values have been returned, the last value is repeated:

```csharp
using BbQ.MockLite;

var builder = Mock.Create<ICounterService>();

builder.SetupSequence(x => x.GetNext(), 10, 20, 30);

var mock = builder.Object;

Console.WriteLine(mock.GetNext()); // 10
Console.WriteLine(mock.GetNext()); // 20
Console.WriteLine(mock.GetNext()); // 30
Console.WriteLine(mock.GetNext()); // 30 (last value repeated)
```

## Resetting Invocations

Use `Reset()` to clear recorded invocations while keeping all setups and callbacks in place. This is useful when testing multiple phases with the same mock:

```csharp
using BbQ.MockLite;

var builder = Mock.Create<IUserRepository>()
    .Setup(x => x.GetUser("123"), () => new User { Id = "123" });

var mock = builder.Object;

// Phase 1
mock.GetUser("123");
builder.Verify(x => x.GetUser("123"), Times.Once);

// Reset before phase 2
builder.Reset();

// Phase 2 — invocation count starts fresh; setup still works
mock.GetUser("123");
builder.Verify(x => x.GetUser("123"), Times.Once);
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

### Argument Matchers

Use `It` helpers to match arguments by predicate instead of exact value:

```csharp
using BbQ.MockLite;

var builder = Mock.Create<IUserRepository>();

// Match any value of type string
builder.Setup(x => x.GetUser(It.IsAny<string>()), () => new User { Id = "default" });

// Match values that satisfy a custom predicate
builder.Setup(
    x => x.GetUser(It.Matches<string>(id => id.StartsWith("admin_"))),
    (string id) => new User { Id = id, IsAdmin = true });

// Use matchers in verification
builder.Verify(x => x.GetUser(It.IsAny<string>()), Times.AtLeast(1));
builder.Verify(x => x.GetUser(It.Matches<string>(id => id.Length > 5)), Times.Once);
```

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
Times.Once           // Exactly 1 call
Times.Never          // Exactly 0 calls
Times.Exactly(3)     // Exactly 3 calls
Times.AtLeast(2)     // At least 2 calls
Times.AtMost(5)      // At most 5 calls
Times.Between(2, 5)  // At least 2 and at most 5 calls (inclusive)
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
- **BbQ.MockLite.Benchmarks** - BenchmarkDotNet performance comparisons

## Benchmarks

The `benchmarks/MockLite.Benchmarks` project uses [BenchmarkDotNet](https://benchmarkdotnet.org/) to compare
source-generated mocks against runtime-proxy mocks (`Mock.Create<T>`).

### Running the benchmarks

```bash
dotnet run --project benchmarks/MockLite.Benchmarks -c Release
```

To run a specific group only, use the `--filter` option:

```bash
# Creation benchmarks only
dotnet run --project benchmarks/MockLite.Benchmarks -c Release -- --filter '*Creation*'

# Invocation benchmarks only
dotnet run --project benchmarks/MockLite.Benchmarks -c Release -- --filter '*Invocation*'
```

### Results (BenchmarkDotNet v0.15.8, .NET 10.0.2, AMD EPYC 7763, Ubuntu 24.04)

#### MockCreationBenchmarks — instantiating a mock from scratch

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| `new MockCalculator()` (direct) | **14.5 ns** | 0.04× | 88 B | 0.14× |
| `Mock.Of<ICalculator>()` (registry) | **37.5 ns** | 0.11× | 88 B | 0.14× |
| `Mock.Create<ICalculator>()` *(baseline)* | 350 ns | 1.00× | 608 B | 1.00× |

> `Mock.Of<T>()` is **~9× faster** than `Mock.Create<T>()` and allocates **~7× less** memory,  
> thanks to the compile-time `MockTypeRegistry`: an O(1) `ConcurrentDictionary` lookup that  
> replaces the previous per-call `Type.GetType()` + `Activator.CreateInstance()` path.

#### MockInvocationBenchmarks — calling a method on an already-created mock

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| Invoke Add – source-generated | **221 ns** | 0.92× | 128 B | 1.00× |
| Invoke Add – runtime proxy *(baseline)* | 240 ns | 1.00× | 128 B | 1.00× |

> Source-generated and runtime-proxy mocks now have **equivalent per-call performance** (~8%
> difference, within measurement noise). Both record invocations via the same `List.Add`
> path and allocate the same amount of memory per call.

#### MockSetupAndInvokeBenchmarks — configure a return value then call the method

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| Setup + invoke – source-generated | **62.0 ns** | 0.000× | 216 B | 0.03× |
| Setup + invoke – runtime proxy *(baseline)* | 138,937 ns | 1.00× | 7,430 B | 1.00× |

> Source-generated mocks are **~2,240× faster** for the setup+invoke pattern and allocate
> **~34× less** memory.  
> The `Mock.Create<T>` fluent setup uses expression-tree parsing and reflection-based delegate
> construction on every call, which dominates the runtime-proxy cost here.

### What is measured

| Benchmark group | Description |
|---|---|
| `MockCreationBenchmarks` | Time to instantiate a new mock object (three variants) |
| `MockInvocationBenchmarks` | Time to call a method on an already-created mock |
| `MockSetupAndInvokeBenchmarks` | Time to configure a return value and call the method once |

### Why source-generated mocks are faster

`[GenerateMock]` produces a concrete class at **compile time** — its methods are regular C# code
with no reflection or expression-tree overhead. The **setup+invoke** path benefits most: every
`Mock.Create<T>` setup call parses an expression tree and constructs a reflection-based delegate,
while a source-generated mock simply calls a pre-compiled handler directly.

**Per-call invocation** overhead has converged in recent .NET versions: both source-generated and
runtime-proxy mocks now record invocations via the same `List.Add` path and show equivalent
throughput (~221 ns vs ~240 ns, a difference within measurement noise).

`Mock.Of<T>()` is fast to **instantiate** because the source generator also emits a
`[ModuleInitializer]` that registers a pre-compiled constructor delegate into `MockTypeRegistry`
at assembly load time. Every subsequent call to `Mock.Of<T>()` is just an O(1) dictionary
lookup — no `Type.GetType`, no `Activator.CreateInstance`.

Use `[GenerateMock]` on interfaces you control for the best creation and setup performance.  
Use `Mock.Create<T>` when you need the fluent `Setup` / `Verify` / `OnCall` API for fine-grained
test configuration, or for third-party interfaces where source generation is not available.

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
4. **Use Argument Matchers** - `It.IsAny<T>()` for flexible matching, `It.Matches<T>()` for predicate-based matching
5. **Leverage Callbacks** - Use callbacks for audit logging and state tracking
6. **Record Invocations** - Leverage invocation recording for complex verification scenarios
7. **Test Exception Paths** - Use `Throws` to simulate failures and verify error handling
8. **Test Sequential Behavior** - Use `SetupSequence` when order of return values matters
9. **Reset Between Phases** - Use `Reset()` to clear invocation history when testing multiple phases

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

- .NET 8.0 / .NET 10.0 or later
- C# 14 or later

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues on [GitHub](https://github.com/JeanMarcMbouma/MockLite).

## Support

For questions, issues, or feature requests, please visit the [GitHub repository](https://github.com/JeanMarcMbouma/MockLite).