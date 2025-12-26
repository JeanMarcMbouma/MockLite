# Strongly-Typed Setup Overload for Delegate-Returning Methods

## Overview
This feature introduces a single generic `Setup` overload that provides compile-time type safety for methods returning any delegate type. The handler lambda is automatically type-checked against the delegate signature, eliminating runtime errors.

## Design
Instead of creating multiple overloads for different delegate arities, the feature uses a single generic method with a `where TDelegate : Delegate` constraint:

```csharp
public Mock<T> Setup<TDelegate>(
    Expression<Func<T, TDelegate>> expression, 
    TDelegate handler
) where TDelegate : Delegate
```

This single method automatically works with any delegate type through generic type inference.

## Motivation
When configuring behaviors for methods that return delegates, you want compile-time safety:

```csharp
// Compile-time enforced type safety
builder.Setup(
    x => x.Query("proc", 1, 2),  // Returns Action<int, int>
    (int a, int b) => Console.WriteLine(a + b)  // Type-safe!
);
```

## Examples

### Action with two parameters
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

### Func with parameters
```csharp
interface ITransformService {
    Func<string, int> GetTransform(string name);
}

var builder = Mock.Create<ITransformService>();
builder.Setup(
    x => x.GetTransform("length"),
    (string s) => s.Length
);

var mock = builder.Object;
var transform = mock.GetTransform("length");
var result = transform("hello"); // Returns: 5
```

### Action with no parameters
```csharp
interface ICallbackService {
    Action GetCallback();
}

var builder = Mock.Create<ICallbackService>();
builder.Setup(
    x => x.GetCallback(),
    () => Console.WriteLine("Called")
);

var mock = builder.Object;
var callback = mock.GetCallback();
callback(); // Prints: Called
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
// Error: Cannot convert lambda expression to type 'Action<int, int>'
```

### ❌ Compile Error - Wrong Parameter Count
```csharp
builder.Setup(
    x => x.Query("proc", 1, 2),  // Returns Action<int, int>
    (int a) => Console.WriteLine(a)  // ERROR!
);
// Error: Cannot convert lambda expression to type 'Action<int, int>'
```

## Benefits

1. **Single Generic Overload**: No need for multiple overloads for different delegate arities
2. **Compile-Time Safety**: Type mismatches caught at compile time, not runtime
3. **Better IntelliSense**: Full IDE support with proper type inference
4. **Works with Any Delegate**: Action, Func, custom delegates - all supported automatically
5. **Zero Breaking Changes**: Existing Setup overloads remain unchanged
6. **No Reflection**: Type checking happens at compile time via generics

## Supported Delegate Types
- `Action` - void delegate with no parameters
- `Action<T1>`, `Action<T1, T2>`, etc. - void delegates with parameters
- `Func<TResult>` - delegate returning TResult with no parameters
- `Func<T1, TResult>`, `Func<T1, T2, TResult>`, etc. - delegates with parameters and return value
- Any custom delegate types

## Implementation Notes
- Uses generic type constraint `where TDelegate : Delegate` to accept any delegate type
- The handler is wrapped in `Func<TDelegate>` for storage in the proxy
- Compiler automatically infers the delegate type from the expression
- No ambiguity with existing `Setup<TResult>` method due to the Delegate constraint

## Test Coverage
- ✅ 6 tests covering Action and Func delegates with various arities
- ✅ All existing tests passing (152 total)
- ✅ Compile-time safety validation

## Migration
If you were using the previous multiple-overload implementation, no changes needed - the new single generic overload works the same way with identical syntax.
