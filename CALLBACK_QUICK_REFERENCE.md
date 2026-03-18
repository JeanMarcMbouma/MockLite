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

### Moq-style fluent callback (Setup + Callback + Returns)

```csharp
var captured = new List<string>();
builder.Setup(x => x.GetUser(It.IsAny<string>()))
    .Callback<string>(id => captured.Add(id))
    .Returns(new User { Id = "default" });
```

### Fluent callback with ReturnsAsync

```csharp
builder.Setup(x => x.GetUserAsync("123"))
    .Callback(() => callLog.Add("async call"))
    .ReturnsAsync(new User { Id = "123" });
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
| Fluent callback + return | `Setup(expr).Callback(() => ...).Returns(val)` |
| Fluent typed callback | `Setup(expr).Callback<T1>((a) => ...).Returns(val)` |
| Fluent callback + async | `Setup(expr).Callback(() => ...).ReturnsAsync(val)` |
| Fluent callback + throw | `Setup(expr).Callback(() => ...).Throws(ex)` |

---

## See Also

- [Callback Feature Guide](./CALLBACK_FEATURE_GUIDE.md) — complete API reference
- [Feature Summary](./FEATURE_COMPLETE_SUMMARY.md) — full feature overview
- [README](./README.md) — getting started guide
