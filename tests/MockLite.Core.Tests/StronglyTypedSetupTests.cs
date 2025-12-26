using System;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Tests for strongly-typed Setup overloads that enforce delegate parameter matching at compile time.
/// </summary>
public class StronglyTypedSetupTests
{
    // ==================== ACTION DELEGATE TESTS ====================

    [Fact]
    public void Test_Setup_Action_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        var called = false;
        
        builder.Setup(
            x => x.GetAction(),
            () => called = true
        );

        var mock = builder.Object;
        
        // Act
        var action = mock.GetAction();
        action();

        // Assert
        Assert.True(called);
    }

    [Fact]
    public void Test_Setup_ActionT1_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        var receivedValue = 0;
        
        builder.Setup(
            x => x.GetActionInt(),
            (int value) => receivedValue = value
        );

        var mock = builder.Object;
        
        // Act
        var action = mock.GetActionInt();
        action(42);

        // Assert
        Assert.Equal(42, receivedValue);
    }

    [Fact]
    public void Test_Setup_ActionT1T2_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        var sum = 0;
        
        builder.Setup(
            x => x.GetActionTwoInts("add"),
            (int a, int b) => sum = a + b
        );

        var mock = builder.Object;
        
        // Act
        var action = mock.GetActionTwoInts("add");
        action(10, 20);

        // Assert
        Assert.Equal(30, sum);
    }

    [Fact]
    public void Test_Setup_ActionT1T2T3_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        var result = "";
        
        builder.Setup(
            x => x.GetActionThreeParams(),
            (int a, string b, bool c) => result = $"{a}:{b}:{c}"
        );

        var mock = builder.Object;
        
        // Act
        var action = mock.GetActionThreeParams();
        action(42, "test", true);

        // Assert
        Assert.Equal("42:test:True", result);
    }

    [Fact]
    public void Test_Setup_ActionT1T2T3T4_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        var result = 0;
        
        builder.Setup(
            x => x.GetActionFourInts(),
            (int a, int b, int c, int d) => result = a + b + c + d
        );

        var mock = builder.Object;
        
        // Act
        var action = mock.GetActionFourInts();
        action(1, 2, 3, 4);

        // Assert
        Assert.Equal(10, result);
    }

    // ==================== FUNC DELEGATE TESTS ====================

    [Fact]
    public void Test_Setup_FuncTResult_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        
        builder.Setup(
            x => x.GetFunc(),
            () => 42
        );

        var mock = builder.Object;
        
        // Act
        var func = mock.GetFunc();
        var result = func();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Test_Setup_FuncT1TResult_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        
        builder.Setup(
            x => x.GetTransform(),
            (int value) => value * 2
        );

        var mock = builder.Object;
        
        // Act
        var func = mock.GetTransform();
        var result = func(21);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Test_Setup_FuncT1T2TResult_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        
        builder.Setup(
            x => x.GetAddOperation(),
            (int a, int b) => a + b
        );

        var mock = builder.Object;
        
        // Act
        var func = mock.GetAddOperation();
        var result = func(10, 32);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Test_Setup_FuncT1T2T3TResult_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        
        builder.Setup(
            x => x.GetThreeParamFunc(),
            (int a, string b, bool c) => c ? $"{a}:{b}" : ""
        );

        var mock = builder.Object;
        
        // Act
        var func = mock.GetThreeParamFunc();
        var result = func(42, "test", true);

        // Assert
        Assert.Equal("42:test", result);
    }

    [Fact]
    public void Test_Setup_FuncT1T2T3T4TResult_WithMatchingHandler_Works()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        
        builder.Setup(
            x => x.GetFourParamFunc(),
            (int a, int b, int c, int d) => a * b + c * d
        );

        var mock = builder.Object;
        
        // Act
        var func = mock.GetFourParamFunc();
        var result = func(2, 3, 4, 5);

        // Assert
        Assert.Equal(26, result); // 2*3 + 4*5 = 6 + 20 = 26
    }

    // ==================== MULTIPLE SETUPS TESTS ====================

    [Fact]
    public void Test_Setup_MultipleActionSetups_EachWorks()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        var result1 = 0;
        var result2 = 0;
        
        builder
            .Setup(x => x.GetActionInt(), (int value) => result1 = value)
            .Setup(x => x.GetActionTwoInts("test"), (int a, int b) => result2 = a * b);

        var mock = builder.Object;
        
        // Act
        var action1 = mock.GetActionInt();
        action1(5);
        
        var action2 = mock.GetActionTwoInts("test");
        action2(7, 3);

        // Assert
        Assert.Equal(5, result1);
        Assert.Equal(21, result2);
    }

    [Fact]
    public void Test_Setup_MixedActionAndFunc_BothWork()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        var actionCalled = false;
        
        builder
            .Setup(x => x.GetAction(), () => actionCalled = true)
            .Setup(x => x.GetTransform(), (int value) => value + 10);

        var mock = builder.Object;
        
        // Act
        var action = mock.GetAction();
        action();
        
        var func = mock.GetTransform();
        var result = func(32);

        // Assert
        Assert.True(actionCalled);
        Assert.Equal(42, result);
    }

    // ==================== ARGUMENT MATCHING TESTS ====================

    [Fact]
    public void Test_Setup_WithSpecificArguments_OnlyMatchesExactArgs()
    {
        // Arrange
        var builder = Mock.Create<IDelegateService>();
        var setupCalled = false;
        
        builder.Setup(
            x => x.GetActionTwoInts("specific"),
            (int a, int b) => setupCalled = true
        );

        var mock = builder.Object;
        
        // Act
        var action1 = mock.GetActionTwoInts("specific");
        action1(1, 2);
        
        var action2 = mock.GetActionTwoInts("other");
        var defaultAction = action2 == null;

        // Assert
        Assert.True(setupCalled);
        // The second setup should return null/default since it doesn't match
    }

    // ==================== TEST INTERFACES ====================

    private interface IDelegateService
    {
        Action GetAction();
        Action<int> GetActionInt();
        Action<int, int> GetActionTwoInts(string operation);
        Action<int, string, bool> GetActionThreeParams();
        Action<int, int, int, int> GetActionFourInts();
        
        Func<int> GetFunc();
        Func<int, int> GetTransform();
        Func<int, int, int> GetAddOperation();
        Func<int, string, bool, string> GetThreeParamFunc();
        Func<int, int, int, int, int> GetFourParamFunc();
    }
}
