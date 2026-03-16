# Callback Feature Guide

Complete API reference for BbQ.MockLite callback and event-hook features.

---

## Overview

Callbacks let you execute custom logic whenever a mock method is called or a property is
accessed, without interfering with any configured return value. They are registered via the
`Mock<T>` builder and are evaluated at invocation time.

---

## OnCall

### Parameterless callback

Fired every time the matched method is called, regardless of arguments.

```csharp
var builder = Mock.Create<IUserRepository>();

builder.OnCall(x => x.GetUser(It.IsAny<string>()),
    () => Console.WriteLine("GetUser called"));
```

### Callback receiving the raw argument array

The `object?[]` overload gives access to all arguments.

```csharp
builder.OnCall(x => x.GetUser(It.IsAny<string>()),
    args => Console.WriteLine($"GetUser called with: {args[0]}"));
```

### Strongly-typed single-parameter callback

```csharp
builder.OnCall(x => x.GetUser(It.IsAny<string>()),
    (string userId) => Console.WriteLine($"Getting user: {userId}"));
```

### Strongly-typed two-parameter callback

```csharp
builder.OnCall(
    x => x.UpdateUser(It.IsAny<string>(), It.IsAny<User>()),
    (string userId, User user) =>
        Console.WriteLine($"Updating {userId} → {user.Name}"));
```

### Strongly-typed three-parameter callback

```csharp
builder.OnCall(
    x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
    (string proc, int id, int count) =>
        Console.WriteLine($"{proc}: id={id}, count={count}"));
```

### Conditional callback with matcher

Fire the callback only when a predicate is satisfied.

```csharp
builder.OnCall(
    x => x.DeleteUser(It.IsAny<string>()),
    args => args[0] is string id && id.StartsWith("admin"),
    args => Console.WriteLine($"Admin user deleted: {args[0]}"));
```

### Void method callbacks

All overloads above work equally for void methods — just use `Expression<Action<T>>`
instead of `Expression<Func<T, object?>>`:

```csharp
builder.OnCall(x => x.SaveUser(It.IsAny<User>()),
    (User user) => Console.WriteLine($"Saving user: {user.Name}"));
```

---

## Property Callbacks

### OnPropertyAccess — fired on both get and set

```csharp
builder.OnPropertyAccess(x => x.Status,
    () => Console.WriteLine("Status accessed"));
```

### OnGetCallback — fired only on property get

```csharp
builder.OnGetCallback(x => x.Status,
    () => Console.WriteLine("Status was read"));
```

### OnSetCallback — fired only on property set

```csharp
builder.OnSetCallback(x => x.Status,
    value => Console.WriteLine($"Status set to: {value}"));
```

### OnSetCallback with matcher — fired only when the set value satisfies a predicate

```csharp
builder.OnSetCallback(
    x => x.Status,
    value => value == "Error",
    value => Console.WriteLine($"Error status set: {value}"));
```

---

## Combining Callbacks with Setup

Callbacks and return-value setups are independent. Both run on the same invocation:

```csharp
var log = new List<string>();

var builder = Mock.Create<IUserRepository>()
    .Setup(x => x.GetUser(It.IsAny<string>()), (string id) => new User { Id = id })
    .OnCall(x => x.GetUser(It.IsAny<string>()),
        (string id) => log.Add($"GetUser({id})"));

var user = builder.Object.GetUser("42");
// user.Id == "42", log == ["GetUser(42)"]
```

---

## Fluent Chaining

All callback methods return `Mock<T>` and can be chained:

```csharp
var builder = Mock.Create<IOrderService>()
    .OnCall(x => x.PlaceOrder(It.IsAny<Order>()), () => orderCount++)
    .OnCall(x => x.CancelOrder(It.IsAny<string>()), () => cancelCount++)
    .OnGetCallback(x => x.IsOpen, () => accessLog.Add("IsOpen read"))
    .OnSetCallback(x => x.IsOpen, v => accessLog.Add($"IsOpen → {v}"));
```

---

## Summary of Callback Signatures

| Method | Expression type | Handler signature |
|---|---|---|
| `OnCall` | `Func` or `Action` | `Action` (parameterless) |
| `OnCall` | `Func` or `Action` | `Action<object?[]>` (raw args) |
| `OnCall<T1>` | `Func` or `Action` | `Action<T1>` |
| `OnCall<T1,T2>` | `Func` or `Action` | `Action<T1,T2>` |
| `OnCall<T1,T2,T3>` | `Func` or `Action` | `Action<T1,T2,T3>` |
| `OnCall` (matcher) | `Func` or `Action` | `Func<object?[],bool>` + `Action<object?[]>` |
| `OnPropertyAccess<T>` | property | `Action` |
| `OnGetCallback<T>` | property | `Action` |
| `OnSetCallback<T>` | property | `Action<T>` |
| `OnSetCallback<T>` (matcher) | property | `Func<T,bool>` + `Action<T>` |
