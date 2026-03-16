# Callback Quick Reference

Quick-start guide and usage examples for BbQ.MockLite callbacks.

---

## Setup a Callback in 3 Steps

```csharp
// 1. Create a builder
var builder = Mock.Create<IUserRepository>();

// 2. Register a callback
builder.OnCall(x => x.GetUser(It.IsAny<string>()),
    (string id) => Console.WriteLine($"GetUser({id}) called"));

// 3. Use the mock
var user = builder.Object.GetUser("123"); // prints: GetUser(123) called
```

---

## Common Patterns

### Track call count

```csharp
int callCount = 0;
builder.OnCall(x => x.Process(It.IsAny<string>()), () => callCount++);
```

### Collect arguments

```csharp
var received = new List<string>();
builder.OnCall(x => x.Process(It.IsAny<string>()),
    (string item) => received.Add(item));
```

### Audit log

```csharp
var log = new List<string>();
builder
    .OnCall(x => x.GetUser(It.IsAny<string>()),
        (string id) => log.Add($"GET {id}"))
    .OnCall(x => x.SaveUser(It.IsAny<User>()),
        (User u) => log.Add($"SAVE {u.Name}"));
```

### Conditional callback

```csharp
builder.OnCall(
    x => x.Delete(It.IsAny<string>()),
    args => args[0] is string s && s.StartsWith("tmp_"),
    args => Console.WriteLine($"Deleted temp record: {args[0]}"));
```

### Property tracking

```csharp
var reads = 0;
builder
    .OnGetCallback(x => x.IsReady, () => reads++)
    .OnSetCallback(x => x.IsReady, v => Console.WriteLine($"IsReady → {v}"));
```

---

## Callback Cheat Sheet

| Goal | Method |
|---|---|
| Run code on any call | `OnCall(expr, () => ...)` |
| Access raw arguments | `OnCall(expr, args => ...)` |
| Typed first argument | `OnCall<T1>(expr, (T1 a) => ...)` |
| Typed first two arguments | `OnCall<T1,T2>(expr, (T1 a, T2 b) => ...)` |
| Conditional execution | `OnCall(expr, matcher, callback)` |
| Property get | `OnGetCallback(prop, () => ...)` |
| Property set | `OnSetCallback(prop, value => ...)` |
| Conditional property set | `OnSetCallback(prop, matcher, value => ...)` |
| Get or set | `OnPropertyAccess(prop, () => ...)` |

---

## See Also

- [Callback Feature Guide](./CALLBACK_FEATURE_GUIDE.md) — complete API reference
- [Feature Summary](./FEATURE_COMPLETE_SUMMARY.md) — full feature overview
- [README](./README.md) — getting started guide
