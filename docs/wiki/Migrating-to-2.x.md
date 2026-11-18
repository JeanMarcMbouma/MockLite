# MockLite 2.0 migration guide

MockLite 2.0 tightens interaction semantics so setup, callbacks, and verification describe the same calls.

## Breaking changes

### Verify now matches expression arguments

1.x counted every invocation of the selected method:

```csharp
mock.Verify(x => x.Get("specific"), Times.Once);
```

In 2.0 this verifies the call to `Get("specific")`. Use matchers for flexible assertions:

```csharp
mock.Verify(x => x.Get(It.IsAny<string>()), Times.Exactly(2));
mock.Verify(x => x.Get(It.Matches<string>(s => s.StartsWith("admin"))), Times.Once);
```

For the old method-wide counting behavior, use the explicit API:

```csharp
mock.VerifyAnyArguments(x => x.Get(It.IsAny<string>()), Times.Exactly(2));
```

### Setup callbacks inherit the setup matcher

A callback chained from `Setup(expression)` now fires only when that setup expression matches. Use `OnCall` when you want a separate hook.

### Multiple generated setups coexist

Generated mocks now keep multiple method setups and select the most recently registered matching setup, matching runtime mocks.

### Invocation history is snapshot-based

Runtime and generated public `Invocations` views are stable snapshots. Generated mocks expose `Reset()` instead of requiring mutation of the invocation list.

## Compatibility checklist

- Replace assertions that relied on argument-insensitive `Verify` with `VerifyAnyArguments`.
- Check setup callbacks that intentionally observed non-matching calls; move those to `OnCall`.
- Replace direct `generatedMock.Invocations.Clear()` calls with `generatedMock.Reset()`.
- Re-run tests where several generated setups target the same method; newest matching setup now wins consistently.
