# Source generators

Start with [[Getting Started]] to install the packages and select an interface.

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

Every generated mock exposes `IReadOnlyList<Invocation> Invocations` as a snapshot and `Reset()` to clear recorded invocations.


Generated mocks can use reflection metadata to record invocations. For Native AOT, construct the generated class directly to avoid runtime proxy generation. See [[Native AOT|Native-AOT]] for a complete publish-and-run example.

[[Full API reference|API-Reference]]
