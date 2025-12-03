using System;
using System.Collections.Generic;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Unit tests for Mock callback functionality.
/// Tests the OnCall, OnCallWithMatcher, and property callback methods.
/// </summary>
public class MockCallbackTests
{
    // ==================== ONCALL BASIC TESTS ====================

    [Fact]
    public void Test_OnCall_ExecutesCallbackWhenMethodCalled()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(x => x.DoSomething(), args => callbackExecuted = true);
        var mock = builder.Object;

        // Act
        mock.DoSomething();

        // Assert
        Assert.True(callbackExecuted);
    }

    [Fact]
    public void Test_OnCall_DoesNotExecuteCallbackWhenMethodNotCalled()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(x => x.DoSomething(), args => callbackExecuted = true);
        var mock = builder.Object;

        // Act
        // Method not called

        // Assert
        Assert.False(callbackExecuted);
    }

    [Fact]
    public void Test_OnCall_ExecutesMultipleTimesForMultipleCalls()
    {
        // Arrange
        var callCount = 0;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(x => x.DoSomething(), args => callCount++);
        var mock = builder.Object;

        // Act
        mock.DoSomething();
        mock.DoSomething();
        mock.DoSomething();

        // Assert
        Assert.Equal(3, callCount);
    }

    [Fact]
    public void Test_OnCall_ReceivesMethodArguments()
    {
        // Arrange
        object?[]? capturedArgs = null;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(x => x.GetValue(It.IsAny<string>()), args => capturedArgs = args);
        var mock = builder.Object;

        // Act
        mock.GetValue("test-key");

        // Assert
        Assert.NotNull(capturedArgs);
        Assert.Single(capturedArgs);
        Assert.Equal("test-key", capturedArgs[0]);
    }

    [Fact]
    public void Test_OnCall_WithMultipleArguments()
    {
        // Arrange
        object?[]? capturedArgs = null;
        var builder = Mock.Create<ITestService>();
        // Using a method that would take multiple args if we had one
        // For now, we'll test with the available GetValue method
        builder.OnCall(x => x.GetValue(It.IsAny<string>()), args => capturedArgs = args);
        var mock = builder.Object;

        // Act
        mock.GetValue("argument1");

        // Assert
        Assert.NotNull(capturedArgs);
        Assert.Single(capturedArgs);
        Assert.Equal("argument1", capturedArgs[0]);
    }

    [Fact]
    public void Test_OnCall_ReturnsSelfForChaining()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();

        // Act
        var result = builder.OnCall(x => x.DoSomething(), args => { });

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Test_OnCall_EnablesMethodChaining()
    {
        // Arrange
        var callback1Executed = false;
        var callback2Executed = false;
        var builder = Mock.Create<ITestService>();

        // Act
        builder
            .OnCall(x => x.DoSomething(), args => callback1Executed = true)
            .OnCall(x => x.GetNumber(It.IsAny<int>()), args => callback2Executed = true);

        var mock = builder.Object;
        mock.DoSomething();
        mock.GetNumber(42);

        // Assert
        Assert.True(callback1Executed);
        Assert.True(callback2Executed);
    }

    [Fact]
    public void Test_OnCall_MultipleCallbacksSameMethod()
    {
        // Arrange
        var count1 = 0;
        var count2 = 0;
        var builder = Mock.Create<ITestService>();
        builder
            .OnCall(x => x.DoSomething(), args => count1++)
            .OnCall(x => x.DoSomething(), args => count2++);

        var mock = builder.Object;

        // Act
        mock.DoSomething();

        // Assert
        Assert.Equal(1, count1);
        Assert.Equal(1, count2);
    }

    [Fact]
    public void Test_OnCall_WithReturnValue_CallbackExecutesBeforeReturn()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<ITestService>()
            .Setup(x => x.GetNumber(1), () => 100)
            .OnCall(x => x.GetNumber(It.IsAny<int>()), args => callbackExecuted = true);

        var mock = builder.Object;

        // Act
        var result = mock.GetNumber(1);

        // Assert
        Assert.Equal(100, result);
        Assert.True(callbackExecuted);
    }

    // ==================== ONCALL WITH MATCHER TESTS ====================

    [Fact]
    public void Test_OnCall_WithMatcher_ExecutesWhenArgumentsMatch()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(
            x => x.GetValue(It.IsAny<string>()),
            args => args[0] is string s && s.StartsWith("admin"),
            args => callbackExecuted = true);

        var mock = builder.Object;

        // Act
        mock.GetValue("admin-user");

        // Assert
        Assert.True(callbackExecuted);
    }

    [Fact]
    public void Test_OnCall_WithMatcher_DoesNotExecuteWhenArgumentsDoNotMatch()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(
            x => x.GetValue(It.IsAny<string>()),
            args => args[0] is string s && s.StartsWith("admin"),
            args => callbackExecuted = true);

        var mock = builder.Object;

        // Act
        mock.GetValue("user-123");

        // Assert
        Assert.False(callbackExecuted);
    }

    [Fact]
    public void Test_OnCall_WithMatcher_ExecutesOnlyForMatchingCalls()
    {
        // Arrange
        var callCount = 0;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(
            x => x.GetValue(It.IsAny<string>()),
            args => args[0] is string s && s.Contains("target"),
            args => callCount++);

        var mock = builder.Object;

        // Act
        mock.GetValue("target-1");
        mock.GetValue("other-value");
        mock.GetValue("target-2");
        mock.GetValue("another");

        // Assert
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Test_OnCall_WithMatcher_ReceivesArguments()
    {
        // Arrange
        object?[]? capturedArgs = null;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(
            x => x.GetValue(It.IsAny<string>()),
            args => args[0] is string s && s == "specific",
            args => capturedArgs = args);

        var mock = builder.Object;

        // Act
        mock.GetValue("specific");

        // Assert
        Assert.NotNull(capturedArgs);
        Assert.Single(capturedArgs);
        Assert.Equal("specific", capturedArgs[0]);
    }

    [Fact]
    public void Test_OnCall_WithMatcher_ReturnsSelfForChaining()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();

        // Act
        var result = builder.OnCall(
            x => x.GetValue(It.IsAny<string>()),
            args => true,
            args => { });

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Test_OnCall_WithMatcher_MultipleCallbacks()
    {
        // Arrange
        var adminCount = 0;
        var userCount = 0;
        var builder = Mock.Create<ITestService>();
        builder
            .OnCall(
                x => x.GetValue(It.IsAny<string>()),
                args => args[0] is string s && s.StartsWith("admin"),
                args => adminCount++)
            .OnCall(
                x => x.GetValue(It.IsAny<string>()),
                args => args[0] is string s && s.StartsWith("user"),
                args => userCount++);

        var mock = builder.Object;

        // Act
        mock.GetValue("admin-1");
        mock.GetValue("user-1");
        mock.GetValue("admin-2");
        mock.GetValue("other");

        // Assert
        Assert.Equal(2, adminCount);
        Assert.Equal(1, userCount);
    }

    // ==================== PROPERTY CALLBACK TESTS ====================

    [Fact]
    public void Test_OnPropertyAccess_CallbackExecutesOnRead()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<IPropertyService>();
        builder.OnPropertyAccess(x => x.Name, () => callbackExecuted = true);
        var mock = builder.Object;

        // Act
        var _ = mock.Name;

        // Assert
        Assert.True(callbackExecuted);
    }

    [Fact]
    public void Test_OnPropertyAccess_ReturnsSelfForChaining()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act
        var result = builder.OnPropertyAccess(x => x.Name, () => { });

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Test_OnGetCallback_CallbackExecutesOnPropertyRead()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<IPropertyService>();
        builder.OnGetCallback(x => x.Name, () => callbackExecuted = true);
        var mock = builder.Object;

        // Act
        var _ = mock.Name;

        // Assert
        Assert.True(callbackExecuted);
    }

    [Fact]
    public void Test_OnGetCallback_ExecutesMultipleTimes()
    {
        // Arrange
        var callCount = 0;
        var builder = Mock.Create<IPropertyService>();
        builder.OnGetCallback(x => x.Name, () => callCount++);
        var mock = builder.Object;

        // Act
        var _ = mock.Name;
        var __ = mock.Name;
        var ___ = mock.Name;

        // Assert
        Assert.Equal(3, callCount);
    }

    [Fact]
    public void Test_OnGetCallback_ReturnsSelfForChaining()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act
        var result = builder.OnGetCallback(x => x.Name, () => { });

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Test_OnSetCallback_CallbackExecutesOnPropertySet()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<IPropertyService>();
        builder.OnSetCallback(x => x.Name, value => callbackExecuted = true);
        var mock = builder.Object;

        // Act
        mock.Name = "NewValue";

        // Assert
        Assert.True(callbackExecuted);
    }

    [Fact]
    public void Test_OnSetCallback_ReceivesAssignedValue()
    {
        // Arrange
        string? capturedValue = null;
        var builder = Mock.Create<IPropertyService>();
        builder.OnSetCallback(x => x.Name, value => capturedValue = value);
        var mock = builder.Object;

        // Act
        mock.Name = "TestValue";

        // Assert
        Assert.Equal("TestValue", capturedValue);
    }

    [Fact]
    public void Test_OnSetCallback_ExecutesMultipleTimes()
    {
        // Arrange
        var values = new List<string>();
        var builder = Mock.Create<IPropertyService>();
        builder.OnSetCallback(x => x.Name, value => values.Add(value));
        var mock = builder.Object;

        // Act
        mock.Name = "Value1";
        mock.Name = "Value2";
        mock.Name = "Value3";

        // Assert
        Assert.Equal(3, values.Count);
        Assert.Contains("Value1", values);
        Assert.Contains("Value2", values);
        Assert.Contains("Value3", values);
    }

    [Fact]
    public void Test_OnSetCallback_ReturnsSelfForChaining()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act
        var result = builder.OnSetCallback(x => x.Name, value => { });

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Test_OnSetCallback_WithMatcher()
    {
        // Arrange
        var matchedValues = new List<string>();
        var builder = Mock.Create<IPropertyService>();
        builder.OnSetCallback(
            x => x.Name,
            value => value.StartsWith("admin"),
            value => matchedValues.Add(value));

        var mock = builder.Object;

        // Act
        mock.Name = "admin-user";
        mock.Name = "regular-user";
        mock.Name = "admin-superuser";
        mock.Name = "guest";

        // Assert
        Assert.Equal(2, matchedValues.Count);
        Assert.Contains("admin-user", matchedValues);
        Assert.Contains("admin-superuser", matchedValues);
    }

    [Fact]
    public void Test_OnSetCallback_WithMatcher_DoesNotExecuteWhenValueDoesNotMatch()
    {
        // Arrange
        var callCount = 0;
        var builder = Mock.Create<IPropertyService>();
        builder.OnSetCallback(
            x => x.Name,
            value => value.StartsWith("admin"),
            value => callCount++);

        var mock = builder.Object;

        // Act
        mock.Name = "user-123";

        // Assert
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Test_OnSetCallback_WithMatcher_ReturnsSelfForChaining()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act
        var result = builder.OnSetCallback(
            x => x.Name,
            value => true,
            value => { });

        // Assert
        Assert.Same(builder, result);
    }

    // ==================== INTEGRATION TESTS ====================

    [Fact]
    public void Test_Callback_WithSetup_AndCallback()
    {
        // Arrange
        var callbackExecuted = false;
        var builder = Mock.Create<ITestService>()
            .Setup(x => x.GetNumber(1), () => 100)
            .OnCall(x => x.GetNumber(It.IsAny<int>()), args => callbackExecuted = true);

        var mock = builder.Object;

        // Act
        var result = mock.GetNumber(1);

        // Assert
        Assert.Equal(100, result);
        Assert.True(callbackExecuted);
    }

    [Fact]
    public void Test_Callback_WithVerification()
    {
        // Arrange
        var callCount = 0;
        var builder = Mock.Create<ITestService>()
            .OnCall(x => x.GetValue(It.IsAny<string>()), args => callCount++);

        var mock = builder.Object;
        mock.GetValue("test");

        // Act & Assert
        Assert.Equal(1, callCount);
        builder.Verify(x => x.GetValue("test"), times => times == 1);
    }

    [Fact]
    public void Test_Callback_WithPropertySetupAndCallback()
    {
        // Arrange
        var getCallCount = 0;
        var setCallCount = 0;
        var builder = Mock.Create<IPropertyService>()
            .ReturnsGet(x => x.Name, "InitialValue")
            .OnGetCallback(x => x.Name, () => getCallCount++)
            .OnSetCallback(x => x.Name, value => setCallCount++);

        var mock = builder.Object;

        // Act
        var value1 = mock.Name;
        mock.Name = "NewValue";
        var value2 = mock.Name;

        // Assert
        Assert.Equal(2, getCallCount);
        Assert.Equal(1, setCallCount);
        Assert.Equal("InitialValue", value1);
    }

    [Fact]
    public void Test_Callback_CompleteWorkflow()
    {
        // Arrange
        var methodCalls = new List<string>();
        var builder = Mock.Create<ITestService>()
            .Setup(x => x.GetValue("admin-123"), () => "Admin User")
            .OnCall(x => x.GetValue(It.IsAny<string>()), 
                args => args[0] is string s && s.StartsWith("admin"),
                args => methodCalls.Add($"Admin query: {args[0]}"))
            .OnCall(x => x.GetValue(It.IsAny<string>()), 
                args => args[0] is string s && !s.StartsWith("admin"),
                args => methodCalls.Add($"User query: {args[0]}"));

        var mock = builder.Object;

        // Act
        var adminResult = mock.GetValue("admin-123");
        var userResult = mock.GetValue("user-456");

        // Assert
        Assert.Equal("Admin User", adminResult);
        Assert.Null(userResult);
        Assert.Equal(2, methodCalls.Count);
        Assert.Contains("Admin query: admin-123", methodCalls);
        Assert.Contains("User query: user-456", methodCalls);
    }

    [Fact]
    public void Test_Callback_SideEffectTracking()
    {
        // Arrange
        var auditLog = new List<string>();
        var builder = Mock.Create<IPropertyService>()
            .OnGetCallback(x => x.Name, () => auditLog.Add("Property 'Name' was read"))
            .OnSetCallback(x => x.Name, value => auditLog.Add($"Property 'Name' was set to '{value}'"));

        var mock = builder.Object;

        // Act
        var _ = mock.Name;
        mock.Name = "Alice";
        var __ = mock.Name;

        // Assert
        Assert.Equal(3, auditLog.Count);
        Assert.Equal("Property 'Name' was read", auditLog[0]);
        Assert.Equal("Property 'Name' was set to 'Alice'", auditLog[1]);
        Assert.Equal("Property 'Name' was read", auditLog[2]);
    }

    [Fact]
    public void Test_Callback_ComplexArgumentMatching()
    {
        // Arrange

        var builder = Mock.Create<ITestQueryService>()
            .Setup(x => x.Get<IEnumerable<int>>(It.IsAny<string>()), () => [1, 2, 3])
            .Setup(x => x.Get<IEnumerable<string>>(It.IsAny<string>()), () => ["one", "two", "three"]);

        var mock = builder.Object;
        // Act
        var nums = mock.Get<IEnumerable<int>>("my-key-123");
        var strs = mock.Get<IEnumerable<string>>("anotherkey");
        // Assert
      
        Assert.Contains(2, nums);
        Assert.Contains("three", strs);
    }

    private interface ITestQueryService
    {
        T Get<T>(string key);
    }
}
