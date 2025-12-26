# Strongly-Typed Setup Overloads

## Overview
This feature introduces 10 new `Setup` overloads that provide compile-time type safety for methods returning delegate types (Action and Func). The handler lambda is automatically type-checked against the delegate signature, eliminating runtime errors.

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

### Func Delegates (5 overloads)
- `Setup<TResult>(Expression<Func<T, Func<TResult>>>, Func<TResult>)`
- `Setup<T1, TResult>(Expression<Func<T, Func<T1, TResult>>>, Func<T1, TResult>)`
- `Setup<T1, T2, TResult>(Expression<Func<T, Func<T1, T2, TResult>>>, Func<T1, T2, TResult>)`
- `Setup<T1, T2, T3, TResult>(Expression<Func<T, Func<T1, T2, T3, TResult>>>, Func<T1, T2, T3, TResult>)`
- `Setup<T1, T2, T3, T4, TResult>(Expression<Func<T, Func<T1, T2, T3, T4, TResult>>>, Func<T1, T2, T3, T4, TResult>)`

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
    Func<int, int, int> GetOperation(string operation);
}

var builder = Mock.Create<ICalculatorService>();
builder.Setup(
    x => x.GetOperation("multiply"),
    (int a, int b) => a * b
);

var mock = builder.Object;
var multiply = mock.GetOperation("multiply");
var result = multiply(6, 7); // Returns: 42
```

### Example 3: Chaining multiple setups
```csharp
var builder = Mock.Create<IService>()
    .Setup(x => x.GetAction(), () => Console.WriteLine("Action"))
    .Setup(x => x.GetFunc(), () => 42)
    .Setup(x => x.GetTransform(), (string s) => s.ToUpper());
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

### ❌ Compile Error - Wrong Return Type
```csharp
builder.Setup(
    x => x.GetTransform(),  // Returns Func<string, int>
    (string s) => s.ToUpper()  // ERROR! Returns string, not int
);
// Error: Cannot convert lambda returning string to Func<string, int>
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

### Data Transformations
```csharp
var mapper = Mock.Create<IMapperService>();
mapper.Setup(
    x => x.GetTransform("uppercase"),
    (string input) => input.ToUpper()
);
```

## Test Coverage
- ✅ 13 unit tests covering all delegate arities
- ✅ 4 integration tests demonstrating real-world scenarios
- ✅ Compile-time safety validation
- ✅ All 163 tests passing

## Notes
- Supports Action and Func delegates with 0-4 parameters
- For delegates with more than 4 parameters, use the existing Setup overload
- Works with both generated mocks and runtime proxies
- Fully compatible with existing MockLite features (verification, callbacks, etc.)
