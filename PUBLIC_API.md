# BbQ.MockLite public API

This reference describes the supported user-facing API in the `BbQ.MockLite` namespace. MockLite has two surfaces:

- Source-generated `MockXxx` types provide member-specific setup and verification methods.
- Runtime mocks, created with `Mock.Create<T>()`, provide the expression-based `Mock<T>` fluent API.

## 2.x interaction semantics

MockLite 2.x makes expression intent explicit and consistent across setup, callback, and verification:

- `Verify(expression, times)` matches the method **and the arguments in the expression**. Exact values use equality, `It.IsAny<T>()` is a wildcard, and `It.Matches<T>()` evaluates its predicate.
- `VerifyAnyArguments(expression, times)` performs an intentional method-wide count when argument values should be ignored.
- A callback chained from `Setup(expression)` inherits the same argument matcher as that setup. Use `OnCall` for an independently registered method hook.
- When multiple runtime or generated setups can match an invocation, the **most recently registered matching setup wins**.
- Runtime and generated invocation collections are exposed as snapshots for verification/inspection; generated mocks provide `Reset()` to clear their concurrent invocation queue.

These are deliberate 2.x behavior changes from 1.x, where ordinary `Verify` ignored expression arguments and fluent setup callbacks were method-wide.


## Packages

```bash
dotnet add package BbQ.MockLite
dotnet add package BbQ.MockLite.Generators
```

The generator package is required only for source-generated mocks. Runtime mocks need `BbQ.MockLite` only.

## Creating mocks

| API | Result | Use when |
|---|---|---|
| `Mock.Of<T>()` | `T` | Resolve a generated implementation when one is registered; otherwise create an unconfigured `DispatchProxy` fallback |
| `Mock.Create<T>()` | `Mock<T>` | Configure and verify a runtime mock with expressions |
| `new Mock<T>()` | `Mock<T>` | Equivalent runtime builder construction |

`T` must be a reference type. In normal use it is an interface.

`Mock.Of<T>()` is statically typed as `T`, so generated setup and verification members are not visible through its return value. Construct the generated class directly when configuring it, or cast a value returned by `Mock.Of<T>()` to the known generated type:

```csharp
var clock = new MockClock().GetUtcNowReturns(DateTime.UnixEpoch);
// Equivalent resolution with an explicit cast:
var resolvedClock = (MockClock)Mock.Of<IClock>();
```

### Source generation attributes

Annotate an interface you own with the non-generic attribute and its interface type:

```csharp
[GenerateMock]
public interface IClock
{
    DateTime UtcNow { get; }
}
```

For an interface you cannot annotate, apply the generic attribute to a class. It allows multiple uses:

```csharp
[GenerateMock<IExternalClock>]
[GenerateMock<IExternalStore>]
public partial class TestMocks { }
```

The generated class is named by removing a conventional leading `I` and adding `Mock`: `IClock` becomes `MockClock`, while `Clock` becomes `MockClock`.

`GeneratedMockAttribute` is a public marker added to generated classes. Applications normally do not apply it themselves. `MockTypeRegistry.Register<TInterface, TMock>()` is public generator infrastructure and normally does not need to be called by application code.

## Runtime fluent API

Start with a builder and use its `Object` in the system under test:

```csharp
var mock = Mock.Create<IUserStore>();
mock.Setup(x => x.Find(It.IsAny<string>()))
    .Returns((string id) => new User(id));

IUserStore store = mock.Object;
```

### Method setup

`Setup` expressions select both a method and an argument pattern. Literal arguments match by equality; `It.IsAny<T>()` and `It.Matches<T>(predicate)` provide wildcard and predicate matching. The newest matching setup wins.

| API family | Purpose |
|---|---|
| `Setup(expression)` | Return a `SetupPhrase<TResult>` |
| `Setup(expression, Func<TResult>)` | Configure a parameterless value factory |
| `Setup(expression, Func<T1, TResult>)` | Configure a factory using the first argument |
| `Setup(expression, Func<T1, T2, TResult>)` | Configure a factory using the first two arguments |
| `Setup(expression, Func<T1, T2, T3, TResult>)` | Configure a factory using the first three arguments |
| `SetupSequence(expression, params values)` | Return each value in order, then repeat the last value |
| `Throws(expression, exception)` | Throw for a return-value or void method |

The partial factories must match the method parameter types in order. They may omit trailing parameters, but they cannot skip a parameter in the middle.

### `SetupPhrase<TResult>`

`Mock<T>.Setup(expression)` returns a phrase with these members:

| Member | Result |
|---|---|
| `Returns(value)` | Use a constant `TResult` |
| `Returns(Func<TResult>)` | Compute the result without arguments |
| `Returns<T1>(Func<T1, TResult>)` | Compute from the first argument |
| `Returns<T1, T2>(Func<T1, T2, TResult>)` | Compute from the first two arguments |
| `Returns<T1, T2, T3>(Func<T1, T2, T3, TResult>)` | Compute from the first three arguments |
| `ReturnsAsync<TInner>(value)` | Wrap a value for a `Task<T>` method, including covariant values |
| `ReturnsAsync<T1, TInner>(Func<T1, TInner>)` | Compute the inner task value from the first argument |
| `ReturnsAsync<T1, T2, TInner>(Func<T1, T2, TInner>)` | Compute the inner task value from the first two arguments |
| `ReturnsAsync<T1, T2, T3, TInner>(Func<T1, T2, T3, TInner>)` | Compute the inner task value from the first three arguments |
| `Throws(exception)` | Throw the exception when the setup matches |
| `Callback(Action)` | Add a parameterless callback and keep the phrase chainable |
| `Callback(Action<object?[]>)` | Add a raw-argument callback and keep the phrase chainable |
| `Callback<T1>(Action<T1>)` | Add a callback using the first argument |
| `Callback<T1, T2>(Action<T1, T2>)` | Add a callback using the first two arguments |
| `Callback<T1, T2, T3>(Action<T1, T2, T3>)` | Add a callback using the first three arguments |

`Returns`, `ReturnsAsync`, and `Throws` are terminal operations that return `Mock<T>` so configuration can continue. `ReturnsAsync` supports `Task<T>` methods. For `ValueTask`, `ValueTask<T>`, non-generic `Task`, or an already-created task, use `Returns` or a `Setup` overload with the method's actual return type.

Phrase callbacks are registered for the selected method and run on every invocation of that method; the setup's literal or matcher arguments do not filter the callback. Use `OnCall(expression, argumentMatcher, callback)` for a conditional callback.

```csharp
mock.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<bool>()))
    .Callback<string>(id => Console.WriteLine(id))
    .ReturnsAsync((string id, bool includeDeleted) =>
        includeDeleted ? FindAny(id) : FindActive(id));
```

### Properties

| API | Purpose |
|---|---|
| `SetupGet(property, getter)` | Configure a property getter factory |
| `ReturnsGet(property, value)` | Configure a constant getter value |
| `SetupGet(property)` | Return `GetSetupPhrase<TProp>` with `Returns`, `Throws`, and chainable `Callback(Action)` |
| `SetupSet(property, setter)` | Configure a typed setter action |
| `SetupSet(property)` | Return `SetSetupPhrase<TProp>` with `Throws`, `Callback(Action)`, and `Callback(Action<TProp>)` |
| `VerifyGet(property, times, message?)` | Verify the getter count |
| `VerifySet(property, times, message?)` | Verify the setter count |
| `VerifySet(property, matcher, times, message?)` | Verify assignments accepted by a typed matcher |

### Callbacks

`OnCall` is available for return-value and void expressions. Each form accepts a parameterless handler, a raw `object?[]` handler, or a typed handler for the first one to three arguments. The expression selects the method; its argument values do not filter the callback. Raw handlers also have an overload with `Func<object?[], bool>` for conditional execution.

Property callback APIs are:

- `OnPropertyAccess(property, callback)` for both get and set.
- `OnGetCallback(property, callback)` for getters.
- `OnSetCallback(property, callback)` for setters.
- `OnSetCallback(property, matcher, callback)` for matching assigned values.

### Verification

| API | What it counts |
|---|---|
| `Verify(returnExpression, times, message?)` | Every call to the selected return-value method |
| `Verify(voidExpression, times, message?)` | Every call to the selected void method |
| `Verify(returnExpression, argumentMatcher, times, message?)` | Calls to the selected return-value method whose `object?[]` arguments match |

The arguments written in the expression identify the overload but are not filtered by the two-argument `Verify(expression, times)` form. Use the explicit argument-matcher overload when values matter:

```csharp
mock.Verify(
    x => x.Find(It.IsAny<string>()),
    args => args[0] is string id && id.StartsWith("user_"),
    Times.Once,
    "Expected one user lookup");
```

A failed verification throws `VerificationException`. The optional message is appended to the generated failure detail.

### State and defaults

| Member | Purpose |
|---|---|
| `Object` | The proxy implementing `T` |
| `Invocations` | Read-only view of recorded `Invocation` objects |
| `Reset()` | Clear invocations while preserving setups and callbacks |
| `SetReturnsDefault<TDefault>(value)` | Override the fallback for unconfigured methods returning exactly `TDefault` |

Built-in defaults include completed `Task`/`Task<T>` values, default `ValueTask`/`ValueTask<T>` values, value-type defaults, and empty arrays for `IEnumerable<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`, `ICollection<T>`, and `IList<T>`.

## Source-generated mock API

For each non-generic interface method named `Method`, a generated mock exposes these stable member patterns where applicable:

| Pattern | Purpose |
|---|---|
| `SetupMethod(behavior)` | Configure a typed behavior using all parameters |
| `SetupMethod(parameterMatcher..., behavior)` | Configure behavior guarded by one typed predicate per parameter |
| `MethodReturns(value)` | Configure a constant sync, `Task<T>`, or `ValueTask<T>` result |
| `MethodReturns()` | Configure completed non-generic `Task` or default `ValueTask` |
| `SetupMethod()` | Return a generated method setup phrase |
| `SetupMethod(parameterMatcher...)` | Return the same typed phrase, filtered by one typed `Predicate<T>` per parameter |
| `SetupMethodMatching(parameterMatcher)` | Collision-safe matcher phrase for a single-parameter method returning `bool` |
| `VerifyMethod(times, message?)` | Verify the method call count |
| `VerifyMethod(parameterMatcher..., times, message?)` | Verify calls accepted by all typed parameter predicates |

Generated method phrases preserve the selected method's complete signature. Their factory `Returns` overload takes the method's full typed parameter list, and methods with parameters expose both `Callback(Action)` and an exact typed `Callback(Action<T1, ...>)`. Matcher-selecting `SetupMethod(parameterMatcher...)` overloads use `Predicate<T>` parameters and return the same phrase, so the matcher is inherited by both callbacks and terminal behaviors. A single-parameter method returning `bool` is the one C# overload-resolution edge case: an untyped lambda is convertible to both its `Func<T, bool>` behavior and a `Predicate<T>` matcher. MockLite therefore generates `SetupMethodMatching(...)` only for that shape; other methods retain `SetupMethod(...)`. Constant returns, factory returns, throws, direct `SetupMethod(behavior)`, and shorthand `MethodReturns(...)` all participate in the same ordered setup registration; the most recently registered matching setup wins. Generic interface methods are implemented and can be verified, but do not receive generated setup/returns helpers.

For example:

```csharp
mock.SetupFormat(
        text => text.StartsWith("item"),
        count => count > 0)
    .Callback((text, count) => log.Add($"{text}:{count}"))
    .Returns((text, count) => $"{text.ToUpperInvariant()}:{count}");
```

Here IntelliSense contextually types `text` as `string` and `count` as `int` in both delegates; incompatible lambda parameter counts or types are rejected by the C# compiler.

For a property named `Name`, applicable generated members are:

- `SetupGetName(behavior)`, `GetNameReturns(value)`, `SetupGetName()`, and `VerifyGetName(times, message?)`.
- `SetupSetName(behavior)`, `SetupSetName(matcher, behavior)`, `SetupSetName()`, `VerifySetName(times, message?)`, and `VerifySetName(matcher, times, message?)`.

Every generated mock exposes `List<Invocation> Invocations`.

## Matchers and verification counts

### `It`

- `It.IsAny<T>()` matches every value in runtime `Setup` expressions.
- `It.Matches<T>(Predicate<T>)` matches values accepted by a predicate in runtime `Setup` expressions.

Generated setup and verification APIs use typed predicate parameters directly rather than `It` markers.

### `Times`

All helpers return `Func<int, bool>` and work with runtime and generated verification:

| Helper | Accepted count |
|---|---|
| `Times.Once` | `1` |
| `Times.Never` | `0` |
| `Times.Exactly(n)` | `n` |
| `Times.AtLeast(n)` | `>= n` |
| `Times.AtMost(n)` | `<= n` |
| `Times.Between(min, max)` | Inclusive range |

`Times.Between` throws `ArgumentOutOfRangeException` when `min > max`.

## Invocation data

Create an invocation record with `new Invocation(MethodInfo method, object[] arguments)`. Each record contains:

- `MethodInfo Method`
- `object[] Arguments`
- `DateTime Timestamp`, captured in UTC

`Invocation.ToString()` formats the method, arguments, and ISO 8601 timestamp.

## Supporting types

- `VerificationException(string message)` is the sealed exception thrown by failed runtime and generated verification.
- `GenerateMockAttribute()` is valid on interfaces; the generator infers the target from the annotated declaration.
- `GenerateMockAttribute(Type interfaceType)` remains available for compatibility, but the explicit type is not required when annotating an interface. Its nullable `Type` property exposes the supplied type.
- `GenerateMockAttribute<T>` is valid on classes and allows multiple instances.
- `GeneratedMockAttribute` marks generated classes and is intended for generator and analyzer infrastructure.
- `MockTypeRegistry.Register<TInterface, TMock>()` registers a generated factory. `TInterface` must be a reference type and `TMock` must implement it and have a public parameterless constructor. Generated module initializers call this automatically.
