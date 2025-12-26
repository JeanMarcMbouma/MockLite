using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BbQ.MockLite;

/// <summary>
/// Fluent builder for configuring and verifying mock instances.
/// </summary>
/// <remarks>
/// <c>MockBuilder&lt;T&gt;</c> provides a fluent API for:
/// <list type="bullet">
/// <item><description>Setting up method behaviors and return values</description></item>
/// <item><description>Configuring property getters and setters</description></item>
/// <item><description>Verifying method and property invocations with optional matchers</description></item>
/// <item><description>Tracking all invocations for assertion purposes</description></item>
/// </list>
/// 
/// The builder wraps a <see cref="RuntimeProxy{T}"/> instance which dynamically intercepts
/// all calls to the mocked interface and records them for verification.
/// </remarks>
/// <typeparam name="T">The interface type to mock. Must be a class or interface.</typeparam>
/// <example>
/// <code>
/// var builder = new MockBuilder&lt;IUserRepository&gt;();
/// builder
///     .Setup(x => x.GetUser("123"), () => new User { Id = "123", Name = "John" })
///     .SetupGet(x => x.IsActive, () => true);
/// 
/// var mock = builder.Object;
/// var user = mock.GetUser("123");
/// 
/// builder.Verify(x => x.GetUser("123"), times => times == 1);
/// </code>
/// </example>
public sealed class Mock<T> where T : class
{
    /// <summary>
    /// The underlying runtime proxy that intercepts method and property calls.
    /// </summary>
    private readonly RuntimeProxy<T> _proxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mock{T}"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a new RuntimeProxy instance that will dynamically proxy calls to the interface T.
    /// The proxy intercepts all method and property invocations for setup and verification.
    /// </remarks>
    public Mock()
    {
        _proxy = RuntimeProxy.Create<T>() as RuntimeProxy<T> ?? throw new InvalidOperationException("Failed to create proxy");
    }

    // --- Setup Methods ---

    /// <summary>
    /// Sets up a method with a return value behavior.
    /// </summary>
    /// <typeparam name="TResult">The return type of the method.</typeparam>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="behavior">A function that returns the desired value when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload is used for methods with return values. When the method is invoked,
    /// the behavior function is called to determine the return value. This setup applies
    /// to all invocations of this method regardless of arguments.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Setup(x => x.GetUser("123"), () => new User { Id = "123" });
    /// </code>
    /// </example>
    public Mock<T> Setup<TResult>(Expression<Func<T, TResult>> expression, Func<TResult> behavior)
    {
        var (method, args) = ExtractMethod(expression);
        _proxy.Setup(method, args, behavior);
        return this;
    }

    /// <summary>
    /// Sets up a method with a handler that receives the first parameter.
    /// </summary>
    /// <typeparam name="TResult">The return type of the method.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="handler">A function that receives the first parameter and returns the desired value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The handler receives the first parameter of the method being intercepted.
    /// This enables expressive, type-safe interception without needing to handle all parameters.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.Setup(x => x.Query("proc", 1, 2), (string proc) => GetResult(proc));
    /// </code>
    /// </example>
    public Mock<T> Setup<TResult, T1>(Expression<Func<T, TResult>> expression, Func<T1, TResult> handler)
    {
        var (method, args) = ExtractMethod(expression);
        Func<TResult> behavior = () =>
        {
            var invocation = _proxy.Invocations.LastOrDefault(i => i.Method == method);
            if (invocation == null || invocation.Arguments.Length < 1)
                return handler(default(T1)!);
            return handler((T1)invocation.Arguments[0]);
        };
        _proxy.Setup(method, args, behavior);
        return this;
    }

    /// <summary>
    /// Sets up a method with a handler that receives the first two parameters.
    /// </summary>
    /// <typeparam name="TResult">The return type of the method.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="handler">A function that receives the first two parameters and returns the desired value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The handler receives the first two parameters of the method being intercepted.
    /// This enables expressive, type-safe interception without needing to handle all parameters.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.Setup(x => x.Query("proc", 1, 2), (string proc, int id) => GetResult(proc, id));
    /// </code>
    /// </example>
    public Mock<T> Setup<TResult, T1, T2>(Expression<Func<T, TResult>> expression, Func<T1, T2, TResult> handler)
    {
        var (method, args) = ExtractMethod(expression);
        Func<TResult> behavior = () =>
        {
            var invocation = _proxy.Invocations.LastOrDefault(i => i.Method == method);
            if (invocation == null || invocation.Arguments.Length < 2)
                return handler(default(T1)!, default(T2)!);
            return handler((T1)invocation.Arguments[0], (T2)invocation.Arguments[1]);
        };
        _proxy.Setup(method, args, behavior);
        return this;
    }

    /// <summary>
    /// Sets up a method with a handler that receives the first three parameters.
    /// </summary>
    /// <typeparam name="TResult">The return type of the method.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="handler">A function that receives the first three parameters and returns the desired value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The handler receives the first three parameters of the method being intercepted.
    /// This enables expressive, type-safe interception without needing to handle all parameters.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.Setup(x => x.Query("proc", 1, 2, "extra"), (string proc, int id, int count) => GetResult(proc, id, count));
    /// </code>
    /// </example>
    public Mock<T> Setup<TResult, T1, T2, T3>(Expression<Func<T, TResult>> expression, Func<T1, T2, T3, TResult> handler)
    {
        var (method, args) = ExtractMethod(expression);
        Func<TResult> behavior = () =>
        {
            var invocation = _proxy.Invocations.LastOrDefault(i => i.Method == method);
            if (invocation == null || invocation.Arguments.Length < 3)
                return handler(default(T1)!, default(T2)!, default(T3)!);
            return handler((T1)invocation.Arguments[0], (T2)invocation.Arguments[1], (T3)invocation.Arguments[2]);
        };
        _proxy.Setup(method, args, behavior);
        return this;
    }

    // --- Verification Methods ---

    /// <summary>
    /// Verifies that a method was called a specific number of times.
    /// </summary>
    /// <param name="expression">A lambda expression identifying the method to verify.</param>
    /// <param name="times">A predicate function that validates the invocation count.</param>
    /// <exception cref="VerificationException">Thrown if the verification predicate returns false.</exception>
    /// <remarks>
    /// This method checks that the specified method was invoked exactly as many times
    /// as the <paramref name="times"/> predicate requires. The method is identified by
    /// the expression, and all invocations with any arguments are counted.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Verify(x => x.GetUser("123"), times => times == 1);
    /// </code>
    /// </example>
    public void Verify(Expression<Func<T, object?>> expression, Func<int, bool> times)
    {
        var (method, _) = ExtractMethod(expression);
        var count = _proxy.Invocations.Count(i => i.Method == method);
        if (!times(count))
            throw new VerificationException($"Verification failed for {method.Name}. Actual calls: {count}");
    }

    /// <summary>
    /// Verifies that a method was called with specific arguments a given number of times.
    /// </summary>
    /// <param name="expression">A lambda expression identifying the method to verify.</param>
    /// <param name="matcher">A function that determines if the invocation arguments match expected values.</param>
    /// <param name="times">A predicate function that validates the invocation count.</param>
    /// <exception cref="VerificationException">Thrown if the verification fails.</exception>
    /// <remarks>
    /// This method verifies method invocations with argument matching, allowing validation of
    /// both which method was called and what arguments it received. The matcher function
    /// receives the arguments array and should return true if they match expected criteria.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Verify(
    ///     x => x.GetUser("123"),
    ///     args => args[0] is string id &amp;&amp; id == "123",
    ///     times => times == 1
    /// );
    /// </code>
    /// </example>
    public void Verify(Expression<Func<T, object?>> expression, Func<object?[], bool> matcher, Func<int, bool> times)
    {
        var (method, _) = ExtractMethod(expression);
        var count = _proxy.Invocations.Count(i => i.Method == method && matcher(i.Arguments));
        if (!times(count))
            throw new VerificationException($"Verification failed for {method.Name} with matcher. Actual calls: {count}");
    }

    // --- Property Setup Methods ---

    /// <summary>
    /// Sets up a property getter with a custom behavior.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property to set up.</param>
    /// <param name="getter">A function that returns the desired property value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// When the property getter is accessed, the provided getter function is called
    /// to determine the return value. This allows configuring read-only and read-write
    /// property behaviors independently.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.SetupGet(x => x.IsActive, () => true);
    /// </code>
    /// </example>
    public Mock<T> SetupGet<TProp>(Expression<Func<T, TProp>> property, Func<TProp> getter)
    {
        var pi = ExtractProperty(property);
        if (pi.GetMethod != null)
            _proxy.Setup(pi.GetMethod, getter);
        return this;
    }

    /// <summary>
    /// Sets up a property getter to return a specific value.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property to set up.</param>
    /// <param name="value">The fixed value to return when the property is accessed.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This is a convenience method that wraps <see cref="SetupGet{TProp}(Expression{Func{T, TProp}}, Func{TProp})"/>
    /// for cases where a constant value should be returned. The value is captured at setup time
    /// and returned on every access.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.ReturnsGet(x => x.Count, 42);
    /// </code>
    /// </example>
    public Mock<T> ReturnsGet<TProp>(Expression<Func<T, TProp>> property, TProp value)
        => SetupGet(property, () => value);

    /// <summary>
    /// Sets up a property setter with a custom behavior.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property to set up.</param>
    /// <param name="setter">An action that handles the property assignment.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// When the property is set, the provided setter action is invoked with the assigned value.
    /// This allows tracking and validating property modifications on the mock.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.SetupSet(x => x.Name, (value) => { /* custom logic */ });
    /// </code>
    /// </example>
    public Mock<T> SetupSet<TProp>(Expression<Func<T, TProp>> property, Action<TProp> setter)
    {
        var pi = ExtractProperty(property);
        if (pi.SetMethod != null)
            _proxy.Setup(pi.SetMethod, setter);
        return this;
    }

    // --- Property Verification Methods ---

    /// <summary>
    /// Verifies that a property getter was accessed a specific number of times.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property to verify.</param>
    /// <param name="times">A predicate function that validates the access count.</param>
    /// <exception cref="VerificationException">Thrown if the verification predicate returns false.</exception>
    /// <remarks>
    /// This method checks that the property getter was accessed exactly as many times
    /// as required by the <paramref name="times"/> predicate. Useful for ensuring
    /// properties are read the expected number of times.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.VerifyGet(x => x.IsActive, times => times >= 1);
    /// </code>
    /// </example>
    public void VerifyGet<TProp>(Expression<Func<T, TProp>> property, Func<int, bool> times)
    {
        var pi = ExtractProperty(property);
        var count = _proxy.Invocations.Count(i => i.Method == pi.GetMethod);
        if (!times(count))
            throw new VerificationException($"Verification failed for get_{pi.Name}. Actual calls: {count}");
    }

    /// <summary>
    /// Verifies that a property setter was called a specific number of times.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property to verify.</param>
    /// <param name="times">A predicate function that validates the call count.</param>
    /// <exception cref="VerificationException">Thrown if the verification predicate returns false.</exception>
    /// <remarks>
    /// This method checks that the property setter was invoked exactly as many times
    /// as required by the <paramref name="times"/> predicate. Useful for verifying
    /// that properties are modified the expected number of times.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.VerifySet(x => x.Name, times => times == 1);
    /// </code>
    /// </example>
    public void VerifySet<TProp>(Expression<Func<T, TProp>> property, Func<int, bool> times)
    {
        var pi = ExtractProperty(property);
        var count = _proxy.Invocations.Count(i => i.Method == pi.SetMethod);
        if (!times(count))
            throw new VerificationException($"Verification failed for set_{pi.Name}. Actual calls: {count}");
    }

    /// <summary>
    /// Verifies that a property setter was called with a specific value a given number of times.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property to verify.</param>
    /// <param name="matcher">A function that determines if the assigned value matches expected criteria.</param>
    /// <param name="times">A predicate function that validates the call count.</param>
    /// <exception cref="VerificationException">Thrown if the verification fails.</exception>
    /// <remarks>
    /// This method verifies property setter invocations with value matching, allowing validation
    /// of both which property was set and what value was assigned. The matcher receives the
    /// assigned value and should return true if it matches expected criteria.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.VerifySet(x => x.Name, value => value == "John", times => times == 1);
    /// </code>
    /// </example>
    public void VerifySet<TProp>(Expression<Func<T, TProp>> property, Func<TProp, bool> matcher, Func<int, bool> times)
    {
        var pi = ExtractProperty(property);
        var count = _proxy.Invocations.Count(i => i.Method == pi.SetMethod && matcher((TProp)(i.Arguments.FirstOrDefault() ?? default(TProp)!)));
        if (!times(count))
            throw new VerificationException($"Verification failed for set_{pi.Name} with matcher. Actual calls: {count}");
    }

    // --- Properties ---

    /// <summary>
    /// Gets the mock instance implementing the interface T.
    /// </summary>
    /// <remarks>
    /// Returns the underlying proxy instance that can be used as a mock of the interface T.
    /// This is the object that receives the configured behaviors and whose invocations are tracked.
    /// All method calls and property accesses on this object are intercepted and recorded.
    /// </remarks>
    public T Object => (T)(object)_proxy;

    /// <summary>
    /// Gets a read-only list of all invocations recorded on the mock.
    /// </summary>
    /// <remarks>
    /// Contains all method and property accesses that have occurred on the mock instance,
    /// including their method information and arguments. Useful for manual verification,
    /// debugging, and understanding the call sequence on the mock.
    /// </remarks>
    public IReadOnlyList<Invocation> Invocations => _proxy.Invocations;

    // --- Helper Methods ---

    /// <summary>
    /// Extracts the method information and arguments from a lambda expression.
    /// </summary>
    /// <param name="expr">A lambda expression to analyze.</param>
    /// <returns>A tuple containing the MethodInfo and the evaluated arguments.</returns>
    /// <exception cref="ArgumentException">Thrown if the expression is not a method call.</exception>
    /// <remarks>
    /// This helper parses lambda expressions to identify which method is being targeted
    /// and what arguments are being used. The arguments are evaluated using expression trees,
    /// allowing dynamic determination of method targets at runtime.
    /// </remarks>
    private static (MethodInfo, object?[]) ExtractMethod(LambdaExpression expr)
    {
        var body = expr.Body;
        
        // Handle Convert expressions (e.g., when return type is boxed to object)
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            body = unary.Operand;
        
        if (body is MethodCallExpression call)
            return (call.Method, call.Arguments.Select(a => (object?)Expression.Lambda(a).Compile().DynamicInvoke()).ToArray());
        throw new ArgumentException("Expression must be a method call");
    }

    /// <summary>
    /// Extracts the method information from a void lambda expression.
    /// </summary>
    /// <param name="expr">A void lambda expression to analyze.</param>
    /// <returns>The MethodInfo of the target method.</returns>
    /// <exception cref="ArgumentException">Thrown if the expression is not a method call.</exception>
    /// <remarks>
    /// This helper parses void lambda expressions (Action) to identify which method is being targeted.
    /// </remarks>
    private static MethodInfo ExtractVoidMethod(Expression<Action<T>> expr)
    {
        if (expr.Body is MethodCallExpression call)
            return call.Method;
        throw new ArgumentException("Expression must be a method call");
    }

    private static PropertyInfo ExtractProperty(LambdaExpression expr)
    {
        if (expr.Body is MemberExpression member && member.Member is PropertyInfo pi)
            return pi;
        throw new ArgumentException("Expression must be a property access");
    }

    // --- Callback Methods ---

    /// <summary>
    /// Sets up a callback that executes custom logic when a method is called.
    /// </summary>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="callback">An action that executes when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The callback is executed before the method returns. This is useful for side effects
    /// such as logging, state modifications, or triggering additional actions.
    /// The callback receives the method arguments as an object array.
    /// </remarks>
    /// <example>
    /// <code>
    /// var callCount = 0;
    /// mock.OnCall(x => x.SaveUser(It.IsAny&lt;User&gt;()), 
    ///     args => callCount++);
    /// 
    /// mock.SaveUser(user);
    /// Assert.Equal(1, callCount);
    /// </code>
    /// </example>
    public Mock<T> OnCall(Expression<Func<T, object?>> expression, Action<object?[]> callback)
    {
        var (method, _) = ExtractMethod(expression);
        _proxy.OnInvocation(method, callback);
        return this;
    }

    /// <summary>
    /// Sets up a callback that executes custom logic when a method is called with specific arguments.
    /// </summary>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="matcher">A predicate that determines if the invocation arguments match.</param>
    /// <param name="callback">An action that executes when the method is called with matching arguments.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The callback is only executed if the matcher predicate returns true for the method arguments.
    /// This allows conditional custom logic based on the specific arguments passed to the method.
    /// </remarks>
    /// <example>
    /// <code>
    /// var savedUsers = new List&lt;User&gt;();
    /// mock.OnCall(
    ///     x => x.SaveUser(It.IsAny&lt;User&gt;()),
    ///     args => args[0] is User u && u.Id.StartsWith("admin"),
    ///     args => savedUsers.Add((User)args[0]));
    /// </code>
    /// </example>
    public Mock<T> OnCall(Expression<Func<T, object?>> expression, Func<object?[], bool> matcher, Action<object?[]> callback)
    {
        var (method, _) = ExtractMethod(expression);
        _proxy.OnInvocation(method, matcher, callback);
        return this;
    }

    /// <summary>
    /// Sets up a callback that executes custom logic when a void method is called.
    /// </summary>
    /// <param name="expression">A lambda expression identifying the void method to set up.</param>
    /// <param name="callback">An action that executes when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload is specifically for void methods that don't return a value.
    /// The callback is executed when the method is invoked.
    /// The callback receives the method arguments as an object array.
    /// </remarks>
    /// <example>
    /// <code>
    /// var callCount = 0;
    /// mock.OnCall(x => x.DoSomething(), 
    ///     args => callCount++);
    /// 
    /// mock.DoSomething();
    /// Assert.Equal(1, callCount);
    /// </code>
    /// </example>
    public Mock<T> OnCall(Expression<Action<T>> expression, Action<object?[]> callback)
    {
        var method = ExtractVoidMethod(expression);
        _proxy.OnInvocation(method, callback);
        return this;
    }

    /// <summary>
    /// Sets up a callback that executes custom logic when a void method is called with specific arguments.
    /// </summary>
    /// <param name="expression">A lambda expression identifying the void method to set up.</param>
    /// <param name="matcher">A predicate that determines if the invocation arguments match.</param>
    /// <param name="callback">An action that executes when the method is called with matching arguments.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload is for void methods with argument matching.
    /// The callback is only executed if the matcher predicate returns true for the method arguments.
    /// </remarks>
    /// <example>
    /// <code>
    /// var savedCount = 0;
    /// mock.OnCall(
    ///     x => x.SaveUser(It.IsAny&lt;User&gt;()),
    ///     args => args[0] is User u && u.IsAdmin,
    ///     args => savedCount++);
    /// </code>
    /// </example>
    public Mock<T> OnCall(Expression<Action<T>> expression, Func<object?[], bool> matcher, Action<object?[]> callback)
    {
        var method = ExtractVoidMethod(expression);
        _proxy.OnInvocation(method, matcher, callback);
        return this;
    }

    /// <summary>
    /// Sets up a callback with no parameters when a method is called.
    /// </summary>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="handler">An action that executes when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows registering a parameterless callback for methods.
    /// The handler doesn't receive any parameters from the intercepted method.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.OnCall(x => x.Query("proc", 1, 2), () => Console.WriteLine("Called!"));
    /// </code>
    /// </example>
    public Mock<T> OnCall(Expression<Func<T, object?>> expression, Action handler)
    {
        var (method, _) = ExtractMethod(expression);
        _proxy.OnInvocation(method, _ => handler());
        return this;
    }

    /// <summary>
    /// Sets up a callback that receives the first parameter when a method is called.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="handler">An action that receives the first parameter when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows registering a strongly-typed callback that receives the first parameter.
    /// The handler signature must match the prefix of the method's parameter list.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.OnCall(x => x.Query("proc", 1, 2), (string proc) => Console.WriteLine(proc));
    /// </code>
    /// </example>
    public Mock<T> OnCall<T1>(Expression<Func<T, object?>> expression, Action<T1> handler)
    {
        var (method, _) = ExtractMethod(expression);
        _proxy.OnInvocation(method, args =>
        {
            if (args.Length >= 1)
                handler((T1)args[0]!);
        });
        return this;
    }

    /// <summary>
    /// Sets up a callback that receives the first two parameters when a method is called.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="handler">An action that receives the first two parameters when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows registering a strongly-typed callback that receives the first two parameters.
    /// The handler signature must match the prefix of the method's parameter list.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.OnCall(x => x.Query("proc", 1, 2), (string proc, int id) => Console.WriteLine($"{proc}: {id}"));
    /// </code>
    /// </example>
    public Mock<T> OnCall<T1, T2>(Expression<Func<T, object?>> expression, Action<T1, T2> handler)
    {
        var (method, _) = ExtractMethod(expression);
        _proxy.OnInvocation(method, args =>
        {
            if (args.Length >= 2)
                handler((T1)args[0]!, (T2)args[1]!);
        });
        return this;
    }

    /// <summary>
    /// Sets up a callback that receives the first three parameters when a method is called.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the method to set up.</param>
    /// <param name="handler">An action that receives the first three parameters when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows registering a strongly-typed callback that receives the first three parameters.
    /// The handler signature must match the prefix of the method's parameter list.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.OnCall(x => x.Query("proc", 1, 2, "extra"), (string proc, int id, int count) => 
    ///     Console.WriteLine($"{proc}: {id}, {count}"));
    /// </code>
    /// </example>
    public Mock<T> OnCall<T1, T2, T3>(Expression<Func<T, object?>> expression, Action<T1, T2, T3> handler)
    {
        var (method, _) = ExtractMethod(expression);
        _proxy.OnInvocation(method, args =>
        {
            if (args.Length >= 3)
                handler((T1)args[0]!, (T2)args[1]!, (T3)args[2]!);
        });
        return this;
    }

    /// <summary>
    /// Sets up a callback with no parameters when a void method is called.
    /// </summary>
    /// <param name="expression">A lambda expression identifying the void method to set up.</param>
    /// <param name="handler">An action that executes when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows registering a parameterless callback for void methods.
    /// The handler doesn't receive any parameters from the intercepted method.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.OnCall(x => x.Process(1, 2), () => Console.WriteLine("Called!"));
    /// </code>
    /// </example>
    public Mock<T> OnCall(Expression<Action<T>> expression, Action handler)
    {
        var method = ExtractVoidMethod(expression);
        _proxy.OnInvocation(method, _ => handler());
        return this;
    }

    /// <summary>
    /// Sets up a callback that receives the first parameter when a void method is called.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the void method to set up.</param>
    /// <param name="handler">An action that receives the first parameter when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows registering a strongly-typed callback that receives the first parameter.
    /// The handler signature must match the prefix of the method's parameter list.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.OnCall(x => x.Process("data", 1, 2), (string data) => Console.WriteLine(data));
    /// </code>
    /// </example>
    public Mock<T> OnCall<T1>(Expression<Action<T>> expression, Action<T1> handler)
    {
        var method = ExtractVoidMethod(expression);
        _proxy.OnInvocation(method, args =>
        {
            if (args.Length >= 1)
                handler((T1)args[0]!);
        });
        return this;
    }

    /// <summary>
    /// Sets up a callback that receives the first two parameters when a void method is called.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the void method to set up.</param>
    /// <param name="handler">An action that receives the first two parameters when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows registering a strongly-typed callback that receives the first two parameters.
    /// The handler signature must match the prefix of the method's parameter list.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.OnCall(x => x.Process("data", 1, 2), (string data, int id) => Console.WriteLine($"{data}: {id}"));
    /// </code>
    /// </example>
    public Mock<T> OnCall<T1, T2>(Expression<Action<T>> expression, Action<T1, T2> handler)
    {
        var method = ExtractVoidMethod(expression);
        _proxy.OnInvocation(method, args =>
        {
            if (args.Length >= 2)
                handler((T1)args[0]!, (T2)args[1]!);
        });
        return this;
    }

    /// <summary>
    /// Sets up a callback that receives the first three parameters when a void method is called.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <param name="expression">A lambda expression identifying the void method to set up.</param>
    /// <param name="handler">An action that receives the first three parameters when the method is called.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows registering a strongly-typed callback that receives the first three parameters.
    /// The handler signature must match the prefix of the method's parameter list.
    /// </remarks>
    /// <example>
    /// <code>
    /// mock.OnCall(x => x.Process("data", 1, 2, "extra"), (string data, int id, int count) => 
    ///     Console.WriteLine($"{data}: {id}, {count}"));
    /// </code>
    /// </example>
    public Mock<T> OnCall<T1, T2, T3>(Expression<Action<T>> expression, Action<T1, T2, T3> handler)
    {
        var method = ExtractVoidMethod(expression);
        _proxy.OnInvocation(method, args =>
        {
            if (args.Length >= 3)
                handler((T1)args[0]!, (T2)args[1]!, (T3)args[2]!);
        });
        return this;
    }

    /// <summary>
    /// Sets up a callback that executes custom logic when a property is accessed or modified.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property.</param>
    /// <param name="callback">An action that executes on property access/modification.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This callback is invoked whenever the property getter or setter is accessed.
    /// Use <see cref="OnGetCallback{TProp}"/> or <see cref="OnSetCallback{TProp}"/> 
    /// for more specific control over getter/setter callbacks.
    /// </remarks>
    /// <example>
    /// <code>
    /// var accessLog = new List&lt;string&gt;();
    /// mock.OnPropertyAccess(x => x.Name, 
    ///     () => accessLog.Add("Name accessed"));
    /// 
    /// var name = mock.Name;
    /// Assert.Single(accessLog);
    /// </code>
    /// </example>
    public Mock<T> OnPropertyAccess<TProp>(Expression<Func<T, TProp>> property, Action callback)
    {
        var pi = ExtractProperty(property);
        if (pi.GetMethod != null)
            _proxy.OnInvocation(pi.GetMethod, _ => callback());
        return this;
    }

    /// <summary>
    /// Sets up a callback that executes custom logic when a property getter is accessed.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property.</param>
    /// <param name="callback">An action that executes when the property is read.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The callback is executed before the property value is returned.
    /// This is useful for logging property access or triggering side effects.
    /// </remarks>
    /// <example>
    /// <code>
    /// var getCount = 0;
    /// mock.OnGetCallback(x => x.Count, () => getCount++);
    /// 
    /// var _ = mock.Count;
    /// Assert.Equal(1, getCount);
    /// </code>
    /// </example>
    public Mock<T> OnGetCallback<TProp>(Expression<Func<T, TProp>> property, Action callback)
    {
        var pi = ExtractProperty(property);
        if (pi.GetMethod != null)
            _proxy.OnInvocation(pi.GetMethod, _ => callback());
        return this;
    }

    /// <summary>
    /// Sets up a callback that executes custom logic when a property setter is called.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property.</param>
    /// <param name="callback">An action that receives the assigned value when the property is set.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The callback receives the value being assigned to the property.
    /// This is useful for logging property modifications or maintaining state.
    /// </remarks>
    /// <example>
    /// <code>
    /// var setValues = new List&lt;string&gt;();
    /// mock.OnSetCallback(x => x.Name, value => setValues.Add(value));
    /// 
    /// mock.Name = "Alice";
    /// mock.Name = "Bob";
    /// Assert.Equal(2, setValues.Count);
    /// Assert.Contains("Alice", setValues);
    /// </code>
    /// </example>
    public Mock<T> OnSetCallback<TProp>(Expression<Func<T, TProp>> property, Action<TProp> callback)
    {
        var pi = ExtractProperty(property);
        if (pi.SetMethod != null)
            _proxy.OnInvocation(pi.SetMethod, args => callback((TProp)(args.FirstOrDefault() ?? default(TProp)!)));
        return this;
    }

    /// <summary>
    /// Sets up a callback that executes custom logic when a property setter is called with a specific value.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="property">A lambda expression identifying the property.</param>
    /// <param name="matcher">A predicate that determines if the assigned value matches.</param>
    /// <param name="callback">An action that receives the matching value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The callback is only executed if the matcher predicate returns true for the assigned value.
    /// This allows conditional custom logic based on specific property values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var adminNames = new List&lt;string&gt;();
    /// mock.OnSetCallback(
    ///     x => x.Name, 
    ///     value => value.StartsWith("admin"),
    ///     value => adminNames.Add(value));
    /// 
    /// mock.Name = "user1";
    /// mock.Name = "admin1";
    /// Assert.Single(adminNames);
    /// </code>
    /// </example>
    public Mock<T> OnSetCallback<TProp>(Expression<Func<T, TProp>> property, Func<TProp, bool> matcher, Action<TProp> callback)
    {
        var pi = ExtractProperty(property);
        if (pi.SetMethod != null)
        {
            _proxy.OnInvocation(pi.SetMethod, args =>
            {
                var value = (TProp)(args.FirstOrDefault() ?? default(TProp)!);
                if (matcher(value))
                    callback(value);
            });
        }
        return this;
    }
}

/// <summary>
/// Factory for creating mock instances of interfaces.
/// </summary>
/// <remarks>
/// The <see cref="Mock"/> class provides two strategies for creating mocks:
/// <list type="number">
/// <item>
///   <description>
///   Generated mocks: Uses source-generated mock classes created by the MockLite generator.
///   Mark interfaces with <see cref="GenerateMockAttribute"/> to automatically generate optimized mock implementations.
///   </description>
/// </item>
/// <item>
///   <description>
///   Runtime mocks: Falls back to using DispatchProxy-based proxies for interfaces without generated mocks.
///   Useful for quick testing scenarios but less performant than generated mocks.
///   </description>
/// </item>
/// </list>
/// </remarks>
public static class Mock
{
    /// <summary>
    /// Creates a mock instance of the specified interface type.
    /// </summary>
    /// <typeparam name="T">The interface type to mock. Must be a class or interface.</typeparam>
    /// <returns>
    /// A mock instance that can be configured with <c>Setup</c>, <c>Returns</c>, and verified with <c>Verify</c>.
    /// If a generated mock exists, returns an instance of the generated class; otherwise returns a runtime proxy.
    /// </returns>
    /// <remarks>
    /// Generated mocks are preferred for better performance. Decorate your interfaces with
    /// <see cref="GenerateMockAttribute"/> to generate optimized mock implementations at compile time.
    /// </remarks>
    /// <example>
    /// <code>
    /// [GenerateMock]
    /// public interface IUserRepository
    /// {
    ///     User GetUser(string id);
    /// }
    /// 
    /// var mock = Mock.Of&lt;IUserRepository&gt;();
    /// mock.SetupGetUser(id => new User { Id = id, Name = "Test" });
    /// var user = mock.GetUser("123");
    /// </code>
    /// </example>
    public static T Of<T>() where T : class
    {
        // Try generated type MockXxx in same namespace
        var ifaceName = typeof(T).Name;
        var baseNs = typeof(T).Namespace;
        var mockName = ifaceName.Length > 1 && ifaceName[0] == 'I' && char.IsUpper(ifaceName[1])
            ? $"Mock{ifaceName.Substring(1)}"
            : $"Mock{ifaceName}";
        var fullName = string.IsNullOrEmpty(baseNs) ? mockName : $"{baseNs}.{mockName}";

        var generated = Type.GetType(fullName);
        if (generated is not null) return (T)Activator.CreateInstance(generated)!;

        // Fallback: DispatchProxy-based proxy for quick use
        return RuntimeProxy.Create<T>();
    }


    public static Mock<T> Create<T>() where T : class
        => new();
}
