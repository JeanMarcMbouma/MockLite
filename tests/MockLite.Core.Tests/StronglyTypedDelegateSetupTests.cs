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

    // Test interface
    private interface IQueryService
    {
        Action<int, int> Query(string procedure, int param1, int param2);
        Action<int> Log(string level);
        Action GetCallback();
        Func<string, int> GetTransform(string name);
        Func<int> GetValue();
        Func<int, int, int> GetOperation(string operation);
    }
}
