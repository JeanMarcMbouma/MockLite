using System;
using System.Threading.Tasks;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Tests for the strongly-typed Setup overload using generic TDelegate parameter.
/// Tests realistic scenarios with methods returning void, Task, Task&lt;T&gt;, or T.
/// Validates that the compiler enforces argument types at compile time.
/// </summary>
public class StronglyTypedDelegateSetupTests
{
    // ==================== VOID-RETURNING DELEGATE TESTS ====================

    [Fact]
    public void Test_Setup_VoidDelegate_NoArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var called = false;
        
        // Act - Setup method returning Action (void, no args)
        builder.Setup(
            x => x.GetLogAction("info"),
            () => called = true
        );

        var mock = builder.Object;
        var action = mock.GetLogAction("info");
        action();

        // Assert
        Assert.True(called);
    }

    [Fact]
    public void Test_Setup_VoidDelegate_WithArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var receivedId = 0;
        var receivedName = "";
        
        // Act - Setup method returning Action<int, string> (void, with args)
        builder.Setup(
            x => x.GetUpdateAction("users"),
            (int id, string name) =>
            {
                receivedId = id;
                receivedName = name;
            }
        );

        var mock = builder.Object;
        var action = mock.GetUpdateAction("users");
        action(42, "John");

        // Assert
        Assert.Equal(42, receivedId);
        Assert.Equal("John", receivedName);
    }

    // ==================== TASK-RETURNING DELEGATE TESTS ====================

    [Fact]
    public async Task Test_Setup_TaskDelegate_NoArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var called = false;
        
        // Act - Setup method returning Func<Task> (async void, no args)
        builder.Setup(
            x => x.GetAsyncOperation("refresh"),
            async () =>
            {
                called = true;
                await Task.CompletedTask;
            }
        );

        var mock = builder.Object;
        var func = mock.GetAsyncOperation("refresh");
        await func();

        // Assert
        Assert.True(called);
    }

    [Fact]
    public async Task Test_Setup_TaskDelegate_WithTwoArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var receivedProc = "";
        var receivedValue = 0;
        
        // Act - Setup method returning Func<string, int, Task> (async void, with args)
        builder.Setup(
            x => x.GetQueryTask("execute"),
            async (string proc, int value) =>
            {
                receivedProc = proc;
                receivedValue = value;
                await Task.CompletedTask;
            }
        );

        var mock = builder.Object;
        var func = mock.GetQueryTask("execute");
        await func("sproc", 100);

        // Assert
        Assert.Equal("sproc", receivedProc);
        Assert.Equal(100, receivedValue);
    }

    [Fact]
    public async Task Test_Setup_TaskDelegate_WithThreeArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var sum = 0;
        
        // Act - Setup method returning Func<int, int, int, Task>
        builder.Setup(
            x => x.GetAsyncCalculation("sum"),
            async (int a, int b, int c) =>
            {
                sum = a + b + c;
                await Task.CompletedTask;
            }
        );

        var mock = builder.Object;
        var func = mock.GetAsyncCalculation("sum");
        await func(10, 20, 30);

        // Assert
        Assert.Equal(60, sum);
    }

    [Fact]
    public async Task Test_Setup_TaskDelegate_WithFourArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var result = "";
        
        // Act - Setup method returning Func<string, int, bool, double, Task>
        builder.Setup(
            x => x.GetComplexTask("process"),
            async (string name, int count, bool flag, double value) =>
            {
                result = $"{name}:{count}:{flag}:{value}";
                await Task.CompletedTask;
            }
        );

        var mock = builder.Object;
        var func = mock.GetComplexTask("process");
        await func("test", 42, true, 3.14);

        // Assert
        Assert.Equal("test:42:True:3.14", result);
    }

    // ==================== TASK<T>-RETURNING DELEGATE TESTS ====================

    [Fact]
    public async Task Test_Setup_TaskTDelegate_NoArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method returning Func<Task<int>> (async int, no args)
        builder.Setup(
            x => x.GetValueFactory("count"),
            async () =>
            {
                await Task.CompletedTask;
                return 42;
            }
        );

        var mock = builder.Object;
        var func = mock.GetValueFactory("count");
        var result = await func();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Test_Setup_TaskTDelegate_WithTwoArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method returning Func<int, int, Task<int>>
        builder.Setup(
            x => x.GetAsyncAdder("add"),
            async (int a, int b) =>
            {
                await Task.CompletedTask;
                return a + b;
            }
        );

        var mock = builder.Object;
        var func = mock.GetAsyncAdder("add");
        var result = await func(15, 27);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Test_Setup_TaskTDelegate_WithThreeArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method returning Func<string, int, int, Task<string>>
        builder.Setup(
            x => x.GetFormatter("format"),
            async (string template, int value1, int value2) =>
            {
                await Task.CompletedTask;
                return $"{template}:{value1}+{value2}={value1 + value2}";
            }
        );

        var mock = builder.Object;
        var func = mock.GetFormatter("format");
        var result = await func("Result", 10, 32);

        // Assert
        Assert.Equal("Result:10+32=42", result);
    }

    [Fact]
    public async Task Test_Setup_TaskTDelegate_WithFourArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method returning Func<int, int, int, int, Task<int>>
        builder.Setup(
            x => x.GetComplexCalculator("calculate"),
            async (int a, int b, int c, int d) =>
            {
                await Task.CompletedTask;
                return (a + b) * (c + d);
            }
        );

        var mock = builder.Object;
        var func = mock.GetComplexCalculator("calculate");
        var result = await func(2, 4, 3, 4);

        // Assert
        Assert.Equal(42, result); // (2+4) * (3+4) = 6 * 7 = 42
    }

    // ==================== T-RETURNING DELEGATE TESTS ====================

    [Fact]
    public void Test_Setup_TDelegate_NoArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method returning Func<string> (returning T, no args)
        builder.Setup(
            x => x.GetStringFactory("hello"),
            () => "Hello, World!"
        );

        var mock = builder.Object;
        var func = mock.GetStringFactory("hello");
        var result = func();

        // Assert
        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void Test_Setup_TDelegate_WithTwoArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method returning Func<string, int, string>
        builder.Setup(
            x => x.GetStringFormatter("concat"),
            (string prefix, int number) => $"{prefix}-{number}"
        );

        var mock = builder.Object;
        var func = mock.GetStringFormatter("concat");
        var result = func("ID", 42);

        // Assert
        Assert.Equal("ID-42", result);
    }

    [Fact]
    public void Test_Setup_TDelegate_WithThreeArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method returning Func<int, int, int, double>
        builder.Setup(
            x => x.GetAverageCalculator("avg"),
            (int a, int b, int c) => (a + b + c) / 3.0
        );

        var mock = builder.Object;
        var func = mock.GetAverageCalculator("avg");
        var result = func(10, 20, 30);

        // Assert
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void Test_Setup_TDelegate_WithFourArgs_CompilerEnforcesTypes()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method returning Func<bool, bool, bool, bool, int>
        builder.Setup(
            x => x.GetBooleanCounter("count"),
            (bool a, bool b, bool c, bool d) => 
                (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0) + (d ? 1 : 0)
        );

        var mock = builder.Object;
        var func = mock.GetBooleanCounter("count");
        var result = func(true, false, true, true);

        // Assert
        Assert.Equal(3, result);
    }

    // ==================== ARGUMENT MATCHING AND CHAINING TESTS ====================

    [Fact]
    public void Test_Setup_MultipleSetups_WithDifferentArgs_BothWork()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        builder
            .Setup(x => x.GetStringFactory("hello"), () => "Hello")
            .Setup(x => x.GetStringFactory("goodbye"), () => "Goodbye");

        var mock = builder.Object;
        
        // Act & Assert
        Assert.Equal("Hello", mock.GetStringFactory("hello")());
        Assert.Equal("Goodbye", mock.GetStringFactory("goodbye")());
    }

    [Fact]
    public void Test_Setup_WithSpecificArgs_OnlyMatchesExactArgs()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        builder.Setup(
            x => x.GetStringFactory("specific"),
            () => "Matched"
        );

        var mock = builder.Object;
        
        // Act & Assert
        Assert.Equal("Matched", mock.GetStringFactory("specific")());
        Assert.Null(mock.GetStringFactory("other")); // Different args return null
    }

    // ==================== TEST INTERFACE ====================

    /// <summary>
    /// Realistic data service interface representing common real-world patterns.
    /// Methods return delegates for various operations with different return types:
    /// - void (Action)
    /// - Task (async void)
    /// - Task&lt;T&gt; (async with result)
    /// - T (sync with result)
    /// </summary>
    private interface IDataService
    {
        // Void-returning delegates
        Action GetLogAction(string level);
        Action<int, string> GetUpdateAction(string table);
        
        // Task-returning delegates (async void)
        Func<Task> GetAsyncOperation(string operation);
        Func<string, int, Task> GetQueryTask(string type);
        Func<int, int, int, Task> GetAsyncCalculation(string operation);
        Func<string, int, bool, double, Task> GetComplexTask(string name);
        
        // Task<T>-returning delegates (async with result)
        Func<Task<int>> GetValueFactory(string name);
        Func<int, int, Task<int>> GetAsyncAdder(string operation);
        Func<string, int, int, Task<string>> GetFormatter(string template);
        Func<int, int, int, int, Task<int>> GetComplexCalculator(string operation);
        
        // T-returning delegates (sync with result)
        Func<string> GetStringFactory(string type);
        Func<string, int, string> GetStringFormatter(string operation);
        Func<int, int, int, double> GetAverageCalculator(string operation);
        Func<bool, bool, bool, bool, int> GetBooleanCounter(string operation);
    }
}
