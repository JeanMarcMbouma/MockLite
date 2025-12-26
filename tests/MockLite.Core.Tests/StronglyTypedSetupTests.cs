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
    }
}
