using System;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Tests for the strongly-typed Setup overload using generic TDelegate parameter.
/// </summary>
public class StronglyTypedDelegateSetupTests
{
    [Fact]
    public void Test_Setup_ActionWithTwoParams_CompileTimeSafe()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        var sum = 0;
        
        // Act - Setup with Action<int, int>
        builder.Setup(
            x => x.Query("proc", 1, 2),
            (int a, int b) => sum = a + b
        );

        var mock = builder.Object;
        var action = mock.Query("proc", 1, 2);
        action(10, 20);

        // Assert
        Assert.Equal(30, sum);
    }

    [Fact]
    public void Test_Setup_ActionWithOneParam_CompileTimeSafe()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        var receivedValue = 0;
        
        // Act - Setup with Action<int>
        builder.Setup(
            x => x.Log("info"),
            (int value) => receivedValue = value
        );

        var mock = builder.Object;
        var action = mock.Log("info");
        action(42);

        // Assert
        Assert.Equal(42, receivedValue);
    }

    [Fact]
    public void Test_Setup_ActionWithNoParams_CompileTimeSafe()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        var called = false;
        
        // Act - Setup with Action
        builder.Setup(
            x => x.GetCallback(),
            () => called = true
        );

        var mock = builder.Object;
        var action = mock.GetCallback();
        action();

        // Assert
        Assert.True(called);
    }

    [Fact]
    public void Test_Setup_FuncWithParams_CompileTimeSafe()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        
        // Act - Setup with Func<string, int>
        builder.Setup(
            x => x.GetTransform("length"),
            (string s) => s.Length
        );

        var mock = builder.Object;
        var func = mock.GetTransform("length");
        var result = func("hello");

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void Test_Setup_FuncWithNoParams_CompileTimeSafe()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        
        // Act - Setup with Func<int>
        builder.Setup(
            x => x.GetValue(),
            () => 42
        );

        var mock = builder.Object;
        var func = mock.GetValue();
        var result = func();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Test_Setup_FuncWithMultipleParams_CompileTimeSafe()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        
        // Act - Setup with Func<int, int, int>
        builder.Setup(
            x => x.GetOperation("add"),
            (int a, int b) => a + b
        );

        var mock = builder.Object;
        var func = mock.GetOperation("add");
        var result = func(10, 32);

        // Assert
        Assert.Equal(42, result);
    }

    // ==================== ARGUMENT MATCHING TESTS ====================

    [Fact]
    public void Test_Setup_WithSpecificArguments_OnlyMatchesExactArgs()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        var setupCalled = false;
        
        builder.Setup(
            x => x.Query("specific", 1, 2),
            (int a, int b) => setupCalled = true
        );

        var mock = builder.Object;
        
        // Act - Call with matching arguments
        var action1 = mock.Query("specific", 1, 2);
        Assert.NotNull(action1);
        action1(10, 20);
        
        // Call with different arguments
        var action2 = mock.Query("other", 1, 2);

        // Assert
        Assert.True(setupCalled);
        Assert.Null(action2); // Different arguments should return null/default
    }

    [Fact]
    public void Test_Setup_MultipleSetupsWithDifferentArgs_BothWork()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        var result1 = 0;
        var result2 = 0;
        
        builder
            .Setup(x => x.Query("proc1", 1, 2), (int a, int b) => result1 = a + b)
            .Setup(x => x.Query("proc2", 3, 4), (int a, int b) => result2 = a * b);

        var mock = builder.Object;
        
        // Act
        var action1 = mock.Query("proc1", 1, 2);
        action1(5, 10);
        
        var action2 = mock.Query("proc2", 3, 4);
        action2(6, 7);

        // Assert
        Assert.Equal(15, result1);
        Assert.Equal(42, result2);
    }

    [Fact]
    public void Test_Setup_MethodChaining_WorksCorrectly()
    {
        // Arrange
        var builder = Mock.Create<IQueryService>();
        var called1 = false;
        var called2 = false;
        
        // Act - Chain multiple setups
        builder
            .Setup(x => x.GetCallback(), () => called1 = true)
            .Setup(x => x.Log("info"), (int value) => called2 = true);

        var mock = builder.Object;
        mock.GetCallback()();
        mock.Log("info")(42);

        // Assert
        Assert.True(called1);
        Assert.True(called2);
    }

    // ==================== CUSTOM DELEGATE TESTS ====================

    [Fact]
    public void Test_Setup_CustomDelegateType_WorksCorrectly()
    {
        // Arrange
        var builder = Mock.Create<ICustomDelegateService>();
        var receivedMessage = "";
        var receivedCode = 0;
        
        builder.Setup(
            x => x.GetHandler("error"),
            (string message, int code) => 
            {
                receivedMessage = message;
                receivedCode = code;
            }
        );

        var mock = builder.Object;
        
        // Act
        var handler = mock.GetHandler("error");
        handler("Test error", 500);

        // Assert
        Assert.Equal("Test error", receivedMessage);
        Assert.Equal(500, receivedCode);
    }

    // ==================== PARAMS ARRAY AND ASYNC TESTS ====================

    [Fact]
    public async Task Test_Setup_AsyncFunc_WithTwoArgs()
    {
        // Arrange
        var builder = Mock.Create<IAsyncQueryService>();
        var callCount = 0;
        
        builder.Setup(
            x => x.GetQueryFunc("proc", 1, 2),
            async (string proc, int a, int b) =>
            {
                callCount++;
                await Task.CompletedTask;
                return a + b;
            }
        );

        var mock = builder.Object;
        
        // Act
        var func = mock.GetQueryFunc("proc", 1, 2);
        var result = await func("proc", 10, 20);

        // Assert
        Assert.Equal(1, callCount);
        Assert.Equal(30, result);
    }

    [Fact]
    public async Task Test_Setup_AsyncFunc_WithThreeArgs()
    {
        // Arrange
        var builder = Mock.Create<IAsyncQueryService>();
        var callCount = 0;
        
        builder.Setup(
            x => x.GetQueryFuncThreeArgs("proc", 1, 2, 3),
            async (string proc, int a, int b, int c) =>
            {
                callCount++;
                await Task.CompletedTask;
                return a + b + c;
            }
        );

        var mock = builder.Object;
        
        // Act
        var func = mock.GetQueryFuncThreeArgs("proc", 1, 2, 3);
        var result = await func("proc", 10, 20, 30);

        // Assert
        Assert.Equal(1, callCount);
        Assert.Equal(60, result);
    }

    [Fact]
    public async Task Test_Setup_AsyncFunc_WithFourArgs()
    {
        // Arrange
        var builder = Mock.Create<IAsyncQueryService>();
        var receivedArgs = new List<int>();
        
        builder.Setup(
            x => x.GetComplexFunc("test"),
            async (string proc, int a, int b, int c, int d) =>
            {
                receivedArgs.AddRange(new[] { a, b, c, d });
                await Task.CompletedTask;
                return a + b + c + d;
            }
        );

        var mock = builder.Object;
        
        // Act
        var func = mock.GetComplexFunc("test");
        var result = await func("test", 1, 2, 3, 4);

        // Assert
        Assert.Equal(4, receivedArgs.Count);
        Assert.Equal(10, result);
        Assert.Equal(new[] { 1, 2, 3, 4 }, receivedArgs);
    }

    // ==================== TEST INTERFACES ====================

    private interface IQueryService
    {
        Action<int, int> Query(string procedure, int param1, int param2);
        Action<int> Log(string level);
        Action GetCallback();
        Func<string, int> GetTransform(string name);
        Func<int> GetValue();
        Func<int, int, int> GetOperation(string operation);
    }

    private delegate void CustomHandler(string message, int code);

    private interface ICustomDelegateService
    {
        CustomHandler GetHandler(string type);
    }

    private interface IAsyncQueryService
    {
        Func<string, int, int, Task<int>> GetQueryFunc(string proc, int p1, int p2);
        Func<string, int, int, int, Task<int>> GetQueryFuncThreeArgs(string proc, int p1, int p2, int p3);
        Func<string, int, int, int, int, Task<int>> GetComplexFunc(string proc);
    }
}
