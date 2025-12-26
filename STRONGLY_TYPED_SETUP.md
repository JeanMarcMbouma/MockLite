# Strongly-Typed Setup Overloads for Action Delegates

## Overview
This feature introduces 5 new `Setup` overloads that provide compile-time type safety for methods returning Action delegate types. The handler lambda is automatically type-checked against the delegate signature, eliminating runtime errors.

## Motivation
When configuring behaviors for methods that return delegates, it's easy to make mistakes:

```csharp
// Before: No compile-time safety
builder.Setup(
    x => x.Query("proc", 1, 2),  // Returns Action<int, int>
    () => (object)((int a, int b) => Console.WriteLine(a + b))  // Runtime cast needed
);
```

Now with strongly-typed Setup:
```csharp
// After: Compile-time enforced
builder.Setup(
    x => x.Query("proc", 1, 2),  // Returns Action<int, int>
    (int a, int b) => Console.WriteLine(a + b)  // Type-safe!
);
```

## Available Overloads

### Action Delegates (5 overloads)
- `Setup(Expression<Func<T, Action>>, Action)`
- `Setup<T1>(Expression<Func<T, Action<T1>>>, Action<T1>)`
- `Setup<T1, T2>(Expression<Func<T, Action<T1, T2>>>, Action<T1, T2>)`
- `Setup<T1, T2, T3>(Expression<Func<T, Action<T1, T2, T3>>>, Action<T1, T2, T3>)`
- `Setup<T1, T2, T3, T4>(Expression<Func<T, Action<T1, T2, T3, T4>>>, Action<T1, T2, T3, T4>)`

### Note on Func Delegates
Func delegates are not supported by these overloads as they would clash with the existing generic `Setup<TResult>` method. For methods returning Func delegates, use the existing `Setup<TResult>(Expression<Func<T, TResult>>, Func<TResult>)` method.

## Examples

### Example 1: Action with two parameters
```csharp
interface IQueryService {
    Action<int, int> Query(string procedure, int param1, int param2);
}

var builder = Mock.Create<IQueryService>();
builder.Setup(
    x => x.Query("sum", 1, 2),
    (int a, int b) => Console.WriteLine($"Sum: {a + b}")
);

var mock = builder.Object;
var action = mock.Query("sum", 1, 2);
action(10, 20); // Prints: Sum: 30
```

### Example 2: Func with parameters and return value
```csharp
interface ICalculatorService {

### Example 2: Action with single parameter
```csharp
interface INotificationService {
    Action<string> SendNotification(string channel);
}

var builder = Mock.Create<INotificationService>();
builder.Setup(
    x => x.SendNotification("email"),
    (string message) => Console.WriteLine($"Email: {message}")
);

var mock = builder.Object;
var notify = mock.SendNotification("email");
notify("Hello World"); // Prints: Email: Hello World
```

### Example 3: Chaining multiple setups
```csharp
var builder = Mock.Create<IService>()
    .Setup(x => x.GetAction(), () => Console.WriteLine("Action"))
    .Setup(x => x.GetSingleParamAction(), (int x) => Console.WriteLine($"Value: {x}"))
    .Setup(x => x.GetTwoParamAction(), (string s, bool b) => Console.WriteLine($"{s}: {b}"));
```

## Compile-Time Safety

### ✅ Valid Setup
```csharp
builder.Setup(
    x => x.Query("proc", 1, 2),  // Returns Action<int, int>
    (int a, int b) => Console.WriteLine(a + b)  // Matches!
);
```

### ❌ Compile Error - Wrong Parameter Types
```csharp
builder.Setup(
    x => x.Query("proc", 1, 2),  // Returns Action<int, int>
    (string a, string b) => Console.WriteLine(a + b)  // ERROR!
);
// Error: Cannot convert lambda to Action<string, string>
```

### ❌ Compile Error - Wrong Parameter Count
```csharp
builder.Setup(
    x => x.Query("proc", 1, 2),  // Returns Action<int, int>
    (int a) => Console.WriteLine(a)  // ERROR!
);
// Error: Cannot convert lambda with 1 parameter to Action<int, int>
```

## Benefits

1. **Compile-Time Safety**: Type mismatches are caught at compile time, not runtime
2. **Better IntelliSense**: Full IDE support with proper type inference and autocomplete
3. **Moq-Like API**: Familiar fluent pattern for developers coming from Moq
4. **Zero Breaking Changes**: Existing Setup overloads remain unchanged
5. **No Reflection**: Type checking happens at compile time, not via reflection

## Real-World Use Cases

### Database Query Callbacks
```csharp
var db = Mock.Create<IDatabaseService>();
db.Setup(
    x => x.ExecuteQuery("SELECT * FROM Users WHERE Age > ?", 18),
    (int age, string name) => Console.WriteLine($"User: {name}, Age: {age}")
);
```

### Event Handlers
```csharp
var events = Mock.Create<IEventService>();
events.Setup(
    x => x.GetClickHandler("submit-button"),
    (string buttonId, int clickCount) => Log($"Button {buttonId} clicked")
);
```

### Logging Callbacks
```csharp
var logger = Mock.Create<ILogService>();
logger.Setup(
    x => x.GetLogger("audit"),
    (string message, string level) => Console.WriteLine($"[{level}] {message}")
);
```

## Test Coverage
- ✅ 5 unit tests covering all Action delegate arities (0-4 parameters)
- ✅ 3 integration tests demonstrating real-world scenarios
- ✅ Compile-time safety validation
- ✅ All tests passing

## Notes
- Supports Action delegates with 0-4 parameters
- For Func delegates, use the existing generic `Setup<TResult>` method
- For delegates with more than 4 parameters, use the existing Setup overload
- Works with both generated mocks and runtime proxies
- Fully compatible with existing MockLite features (verification, callbacks, etc.)
