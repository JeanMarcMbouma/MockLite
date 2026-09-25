# MockLite 2.x semantics note

For 2.x, ordinary `Verify(expression, times)` is argument-aware, setup callbacks inherit their setup matcher, and newest matching setup wins consistently. Use `VerifyAnyArguments` for intentional method-wide counts. See `MIGRATION_2.0.md`.

# Feature Summary

High-level overview of all BbQ.MockLite features.

---

## Mock Creation

| API | Description |
|---|---|
| `Mock.Of<T>()` | Fast creation via the compile-time `MockTypeRegistry` (O(1) lookup). Requires `[GenerateMock]` on the interface, or falls back to a runtime proxy. |
| `Mock.Create<T>()` | Creates a `Mock<T>` fluent builder backed by a `RuntimeProxy<T>`. |

---

## Setup

| API | Description |
|---|---|
| `Setup(expr, Func<TResult>)` | Configure a constant or computed return value. |
| `Setup(expr)` | Begin a fluent setup returning `SetupPhrase<TResult>` for `.Callback()`, `.Returns()`, `.ReturnsAsync()`, `.Throws()` chaining. |
| `Setup<TResult,T1>(expr, Func<T1,TResult>)` | Return value computed from the first argument. |
| `Setup<TResult,T1,T2>(expr, ...)` | Return value computed from the first two arguments. |
| `Setup<TResult,T1,T2,T3>(expr, ...)` | Return value computed from the first three arguments. |
| `SetupGet(prop, Func<TProp>)` | Configure a property getter. |
| `SetupGet(prop)` | Begin a fluent setup returning `GetSetupPhrase<TProp>` for `.Callback()`, `.Returns()`, `.Throws()` chaining. |
| `ReturnsGet(prop, value)` | Shorthand for a constant property getter. |
| `SetupSet(prop, Action<TProp>)` | Configure a property setter. |
| `SetupSet(prop)` | Begin a fluent setup returning `SetSetupPhrase<TProp>` for `.Callback()`, `.Throws()` chaining. |
| `SetupSequence(expr, values)` | Return successive values on each call; last value repeats. |
| `Throws(expr, exception)` | Throw an exception when a return-value method is called. |
| `Throws(voidExpr, exception)` | Throw an exception when a void method is called. |

---

## SetupPhrase (Moq-Style Fluent Chaining)

Returned by `Setup(expression)`, the `SetupPhrase<TResult>` struct supports:

| API | Returns | Description |
|---|---|---|
| `.Returns(value)` | `Mock<T>` | Configure a constant return value. |
| `.Returns(Func<TResult>)` | `Mock<T>` | Configure a factory-based return value. |
| `.Returns<T1>(Func<T1,TResult>)` | `Mock<T>` | Compute the result from the first argument. |
| `.Returns<T1,T2>(Func<T1,T2,TResult>)` | `Mock<T>` | Compute the result from the first two arguments. |
| `.Returns<T1,T2,T3>(Func<T1,T2,T3,TResult>)` | `Mock<T>` | Compute the result from the first three arguments. |
| `.ReturnsAsync<TInner>(value)` | `Mock<T>` | Configure a `Task<T>` return with covariance support. |
| `.ReturnsAsync<T1,TInner>(factory)` | `Mock<T>` | Compute the inner task value from the first argument. |
| `.ReturnsAsync<T1,T2,TInner>(factory)` | `Mock<T>` | Compute the inner task value from the first two arguments. |
| `.ReturnsAsync<T1,T2,T3,TInner>(factory)` | `Mock<T>` | Compute the inner task value from the first three arguments. |
| `.Throws(exception)` | `Mock<T>` | Configure the method to throw. |
| `.Callback(Action)` | `SetupPhrase` | Parameterless callback (chainable). |
| `.Callback(Action<object?[]>)` | `SetupPhrase` | Raw argument array callback (chainable). |
| `.Callback<T1>(Action<T1>)` | `SetupPhrase` | Strongly-typed callback for first parameter (chainable). |
| `.Callback<T1,T2>(Action<T1,T2>)` | `SetupPhrase` | Strongly-typed callback for first two parameters (chainable). |
| `.Callback<T1,T2,T3>(Action<T1,T2,T3>)` | `SetupPhrase` | Strongly-typed callback for first three parameters (chainable). |

---

## GetSetupPhrase (Fluent Property Getter Setup)

Returned by `SetupGet(property)`, the `GetSetupPhrase<TProp>` struct supports:

| API | Returns | Description |
|---|---|---|
| `.Returns(value)` | `Mock<T>` | Configure a constant return value for the getter. |
| `.Returns(Func<TProp>)` | `Mock<T>` | Configure a factory-based return value for the getter. |
| `.Throws(exception)` | `Mock<T>` | Configure the getter to throw. |
| `.Callback(Action)` | `GetSetupPhrase` | Parameterless callback on getter access (chainable). |

## SetSetupPhrase (Fluent Property Setter Setup)

Returned by `SetupSet(property)`, the `SetSetupPhrase<TProp>` struct supports:

| API | Returns | Description |
|---|---|---|
| `.Throws(exception)` | `Mock<T>` | Configure the setter to throw. |
| `.Callback(Action)` | `SetSetupPhrase` | Parameterless callback on setter call (chainable). |
| `.Callback(Action<TProp>)` | `SetSetupPhrase` | Typed callback receiving the assigned value (chainable). |

---

## Verification

| API | Description |
|---|---|
| `Verify(expr, times, message?)` | Verify a return-value method call count. In 2.x, expression arguments filter calls using exact values and It matchers. |
| `Verify(voidExpr, times, message?)` | Verify a void method call count. In 2.x, expression arguments filter calls using exact values and It matchers. |
| `Verify(expr, matcher, times, message?)` | Verify a return-value method with `object?[]` argument matching. |
| `VerifyGet(prop, times, message?)` | Verify property getter access count. |
| `VerifySet(prop, times, message?)` | Verify property setter call count. |
| `VerifySet(prop, matcher, times, message?)` | Verify property setter with value matching. |

---

## Callbacks

| API | Description |
|---|---|
| `OnCall(expr, Action)` | Parameterless callback on any call. |
| `OnCall(expr, Action<object?[]>)` | Callback receiving the raw argument array. |
| `OnCall<T1>(expr, Action<T1>)` | Typed callback receiving the first argument. |
| `OnCall<T1,T2>(expr, Action<T1,T2>)` | Typed callback receiving the first two arguments. |
| `OnCall<T1,T2,T3>(expr, Action<T1,T2,T3>)` | Typed callback receiving the first three arguments. |
| `OnCall(expr, matcher, Action<object?[]>)` | Conditional callback with argument matcher. |
| `OnPropertyAccess(prop, Action)` | Callback fired on both get and set. |
| `OnGetCallback(prop, Action)` | Callback fired only on property get. |
| `OnSetCallback(prop, Action<TProp>)` | Callback fired only on property set. |
| `OnSetCallback(prop, matcher, Action<TProp>)` | Conditional callback on property set. |

> All callback methods listed above also have `Expression<Action<T>>` overloads for void methods.

---

## Argument Matchers

| API | Description |
|---|---|
| `It.IsAny<T>()` | Matches any value of type `T`. |
| `It.Matches<T>(predicate)` | Matches values that satisfy the predicate. |

---

## Times Predicates

| API | Description |
|---|---|
| `Times.Once` | Exactly 1 call. |
| `Times.Never` | Exactly 0 calls. |
| `Times.Exactly(n)` | Exactly `n` calls. |
| `Times.AtLeast(n)` | At least `n` calls. |
| `Times.AtMost(n)` | At most `n` calls. |
| `Times.Between(min, max)` | Between `min` and `max` calls (inclusive). |

---

## Invocation Recording

Every method call on a mock is automatically recorded in `Mock<T>.Invocations`
(type `IReadOnlyList<Invocation>`). Each `Invocation` exposes:

| Property | Type | Description |
|---|---|---|
| `Method` | `MethodInfo` | The method that was invoked. |
| `Arguments` | `object[]` | The arguments passed. |
| `Timestamp` | `DateTime` | UTC time of the invocation. |

---

## Reset

`Mock<T>.Reset()` clears all recorded invocations while preserving setups and callbacks.
Useful when testing multiple phases with the same mock instance.

`Mock<T>.SetReturnsDefault<TDefault>(value)` overrides the fallback used by unconfigured methods returning exactly `TDefault`.

---

## Two-Tier Architecture

| Tier | Description |
|---|---|
| **Compile-time (Tier 1)** | `[GenerateMock]` interfaces get optimized mock classes generated by the source generator. `Mock.Of<T>()` resolves them via an O(1) registry lookup. |
| **Runtime (Tier 2)** | Interfaces without `[GenerateMock]` fall back to `DispatchProxy`-based `RuntimeProxy<T>` automatically. |

---

## Generated Mock Convenience Methods

For each non-generic method and gettable property on a `[GenerateMock]` interface, the source generator emits convenience methods on the mock class using `{Name}Returns` naming:

| Generated Method | Description |
|---|---|
| `{Method}Returns(value)` | Set a constant return value for a non-void method. Async methods are automatically wrapped (e.g. `Task.FromResult`). |
| `Get{Property}Returns(value)` | Set a constant return value for a property getter. |
| `Setup{Method}(behavior)` | Configure a method with a custom behavior delegate. |
| `SetupGet{Property}(behavior)` | Configure a property getter with a custom factory. |
| `Verify{Method}(times)` | Verify method call count. |
| `VerifyGet{Property}(times)` | Verify property getter access count. |

---

## See Also

- [Public API Reference](./PUBLIC_API.md) — full runtime and generated API
- [README](./README.md) — getting started and examples
- [Callback Feature Guide](./CALLBACK_FEATURE_GUIDE.md) — detailed callback reference
- [Callback Quick Reference](./CALLBACK_QUICK_REFERENCE.md) — quick-start patterns
