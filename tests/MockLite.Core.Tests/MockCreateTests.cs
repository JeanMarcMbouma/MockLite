using System.Collections.Generic;
using System.Threading.Tasks;

namespace MockLite.Core.Tests;

/// <summary>
/// Unit tests for Mock.Create method that returns MockBuilder instances.
/// Tests the fluent builder API for configuring and verifying mocks.
/// </summary>
public class MockCreateTests
{
    // ==================== BASIC CREATION TESTS ====================

    [Fact]
    public void Test_MockCreate_ReturnsValidMockBuilder()
    {
        // Act
        var builder = Mock.Create<ITestService>();

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<MockBuilder<ITestService>>(builder);
    }

    [Fact]
    public void Test_MockCreate_WithDifferentInterfaces_CreateDistinctBuilders()
    {
        // Act
        var builder1 = Mock.Create<ITestService>();
        var builder2 = Mock.Create<ITestService>();

        // Assert
        Assert.NotNull(builder1);
        Assert.NotNull(builder2);
        Assert.NotSame(builder1, builder2);
    }

    [Fact]
    public void Test_MockCreate_BuilderObject_ReturnsMock()
    {
        // Act
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Assert
        Assert.NotNull(mock);
        Assert.IsAssignableFrom<ITestService>(mock);
    }

    [Fact]
    public void Test_MockCreate_WithGenericInterface_ReturnsMockBuilder()
    {
        // Act
        var builder = Mock.Create<IGenericService<string>>();

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<MockBuilder<IGenericService<string>>>(builder);
    }

    [Fact]
    public void Test_MockCreate_WithPropertyService_ReturnsMockBuilder()
    {
        // Act
        var builder = Mock.Create<IPropertyService>();

        // Assert
        Assert.NotNull(builder);
        Assert.IsAssignableFrom<MockBuilder<IPropertyService>>(builder);
    }

    // ==================== MOCK BUILDER OBJECT TESTS ====================

    [Fact]
    public void Test_MockCreate_Object_ImplementsTargetInterface()
    {
        // Act
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        var interfaces = mock.GetType().GetInterfaces();

        // Assert
        Assert.Contains(typeof(ITestService), interfaces);
    }

    [Fact]
    public void Test_MockCreate_Object_IsClass()
    {
        // Act
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        var type = mock.GetType();

        // Assert
        Assert.True(type.IsClass);
    }

    [Fact]
    public void Test_MockCreate_Object_CanBeCalledMultipleTimes()
    {
        // Act
        var builder = Mock.Create<ITestService>();
        var mock1 = builder.Object;
        var mock2 = builder.Object;

        // Assert
        Assert.NotNull(mock1);
        Assert.NotNull(mock2);
        // Same builder returns same mock instance
        Assert.Same(mock1, mock2);
    }

    [Fact]
    public void Test_MockCreate_Object_DefaultMethodReturnsNull()
    {
        // Act
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        var result = mock.GetValue("test");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Test_MockCreate_Object_DefaultIntMethodReturnsZero()
    {
        // Act
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        var result = mock.GetNumber(5);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Test_MockCreate_Object_VoidMethodExecutes()
    {
        // Act
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Assert - should not throw
        var exception = Record.Exception(() => mock.DoSomething());
        Assert.Null(exception);
    }

    // ==================== INVOCATIONS TESTS ====================

    [Fact]
    public void Test_MockCreate_Invocations_InitiallyEmpty()
    {
        // Act
        var builder = Mock.Create<ITestService>();
        var invocations = builder.Invocations;

        // Assert
        Assert.NotNull(invocations);
        Assert.Empty(invocations);
    }

    [Fact]
    public void Test_MockCreate_Invocations_RecordsMethodCalls()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        mock.GetValue("test");
        var invocations = builder.Invocations;

        // Assert
        Assert.NotNull(invocations);
        Assert.Single(invocations);
    }

    [Fact]
    public void Test_MockCreate_Invocations_RecordsMultipleCalls()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        mock.GetValue("key1");
        mock.GetNumber(42);
        mock.DoSomething();
        var invocations = builder.Invocations;

        // Assert
        Assert.NotNull(invocations);
        Assert.Equal(3, invocations.Count);
    }

    [Fact]
    public void Test_MockCreate_Invocations_IsReadOnly()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        mock.GetValue("test");

        // Act
        var invocations = builder.Invocations;

        // Assert
        Assert.NotNull(invocations);
        Assert.IsAssignableFrom<IReadOnlyList<Invocation>>(invocations);
    }

    [Fact]
    public void Test_MockCreate_Invocations_ContainMethodInfo()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        mock.GetValue("test");

        // Act
        var invocations = builder.Invocations;
        var firstInvocation = invocations[0];

        // Assert
        Assert.NotNull(firstInvocation.Method);
        Assert.Equal("GetValue", firstInvocation.Method.Name);
    }

    [Fact]
    public void Test_MockCreate_Invocations_ContainArguments()
    {
        // Arrange
        const string testArg = "test-argument";
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        mock.GetValue(testArg);
        var invocations = builder.Invocations;
        var firstInvocation = invocations[0];

        // Assert
        Assert.NotNull(firstInvocation.Arguments);
        Assert.Single(firstInvocation.Arguments);
        Assert.Equal(testArg, firstInvocation.Arguments[0]);
    }

    [Fact]
    public void Test_MockCreate_Invocations_PreservesArgumentType()
    {
        // Arrange
        const int testNumber = 42;
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        mock.GetNumber(testNumber);
        var invocations = builder.Invocations;
        var firstInvocation = invocations[0];

        // Assert
        Assert.NotNull(firstInvocation.Arguments);
        Assert.Single(firstInvocation.Arguments);
        Assert.IsType<int>(firstInvocation.Arguments[0]);
        Assert.Equal(testNumber, firstInvocation.Arguments[0]);
    }

    [Fact]
    public void Test_MockCreate_Invocations_VoidMethodHasEmptyArgs()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        mock.DoSomething();
        var invocations = builder.Invocations;
        var firstInvocation = invocations[0];

        // Assert
        Assert.NotNull(firstInvocation.Arguments);
        Assert.Empty(firstInvocation.Arguments);
    }

    [Fact]
    public void Test_MockCreate_Invocations_MaintainsOrder()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        mock.GetValue("first");
        mock.GetValue("second");
        mock.GetValue("third");
        var invocations = builder.Invocations;

        // Assert
        Assert.Equal(3, invocations.Count);
        Assert.Equal("first", invocations[0].Arguments[0]);
        Assert.Equal("second", invocations[1].Arguments[0]);
        Assert.Equal("third", invocations[2].Arguments[0]);
    }

    [Fact]
    public void Test_MockCreate_Invocations_RecordsAsyncMethods()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        var _ = mock.DoSomethingAsync();
        var invocations = builder.Invocations;

        // Assert
        Assert.NotNull(invocations);
        Assert.Single(invocations);
    }

    // ==================== SETUP TESTS ====================

    [Fact]
    public void Test_MockCreate_Setup_ConfiguresReturnValue()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue("test-key"), () => "configured-value");
        var mock = builder.Object;

        // Act
        var result = mock.GetValue("test-key");

        // Assert
        Assert.Equal("configured-value", result);
    }

    [Fact]
    public void Test_MockCreate_Setup_MultipleReturnValues()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetNumber(1), () => 100);
        builder.Setup(x => x.GetNumber(2), () => 200);
        var mock = builder.Object;

        // Act
        var result1 = mock.GetNumber(1);
        var result2 = mock.GetNumber(2);

        // Assert
        Assert.Equal(100, result1);
        Assert.Equal(200, result2);
    }

    [Fact]
    public void Test_MockCreate_Setup_ReturnsSelf()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();

        // Act
        var result = builder.Setup(x => x.GetValue("test"), () => "value");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Test_MockCreate_Setup_EnablesMethodChaining()
    {
        // Arrange & Act
        var builder = Mock.Create<ITestService>()
            .Setup(x => x.GetValue("key1"), () => "value1")
            .Setup(x => x.GetNumber(1), () => 10);

        var mock = builder.Object;

        // Assert
        Assert.Equal("value1", mock.GetValue("key1"));
        Assert.Equal(10, mock.GetNumber(1));
    }

    [Fact]
    public void Test_MockCreate_Setup_WithDynamicBehavior()
    {
        // Arrange
        var callCount = 0;
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetNumber(1), () => ++callCount);
        var mock = builder.Object;

        // Act
        var result1 = mock.GetNumber(1);
        var result2 = mock.GetNumber(1);

        // Assert
        Assert.Equal(1, result1);
        Assert.Equal(2, result2);
    }

    [Fact]
    public void Test_MockCreate_SetupGet_ConfiguresPropertyGetter()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();
        builder.SetupGet(x => x.Name, () => "TestName");
        var mock = builder.Object;

        // Act
        var result = mock.Name;

        // Assert
        Assert.Equal("TestName", result);
    }

    [Fact]
    public void Test_MockCreate_ReturnsGet_ConfiguresPropertyGetter()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();
        builder.ReturnsGet(x => x.Name, "FixedName");
        var mock = builder.Object;

        // Act
        var result = mock.Name;

        // Assert
        Assert.Equal("FixedName", result);
    }

    [Fact]
    public void Test_MockCreate_SetupSet_ConfiguresSetter()
    {
        // Arrange
        var setterCalled = false;
        var builder = Mock.Create<IPropertyService>();
        builder.SetupSet(x => x.Name, (value) => { setterCalled = true; });
        var mock = builder.Object;

        // Act
        mock.Name = "NewValue";

        // Assert
        Assert.True(setterCalled);
    }

    [Fact]
    public void Test_MockCreate_SetupGet_ReturnsSelf()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act
        var result = builder.SetupGet(x => x.Name, () => "test");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Test_MockCreate_ReturnsGet_ReturnsSelf()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act
        var result = builder.ReturnsGet(x => x.Name, "test");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void Test_MockCreate_SetupSet_ReturnsSelf()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act
        var result = builder.SetupSet(x => x.Name, value => { });

        // Assert
        Assert.Same(builder, result);
    }

    // ==================== VERIFICATION TESTS ====================

    [Fact]
    public void Test_MockCreate_Verify_MethodCalledZeroTimes()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();

        // Act & Assert - should not throw
        builder.Verify(x => x.GetValue("test"), times => times == 0);
    }

    [Fact]
    public void Test_MockCreate_Verify_MethodCalledOnce()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        mock.GetValue("test");

        // Act & Assert - should not throw
        builder.Verify(x => x.GetValue("test"), times => times == 1);
    }

    [Fact]
    public void Test_MockCreate_Verify_MethodCalledMultipleTimes()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        mock.GetValue("test");
        mock.GetValue("test");
        mock.GetValue("test");

        // Act & Assert - should not throw
        builder.Verify(x => x.GetValue("test"), times => times == 3);
    }

    [Fact]
    public void Test_MockCreate_Verify_ThrowsOnMismatch()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        mock.GetValue("test");

        // Act & Assert
        var exception = Assert.Throws<VerificationException>(() =>
            builder.Verify(x => x.GetValue("test"), times => times == 2));
        
        Assert.NotNull(exception);
        Assert.Contains("Verification failed", exception.Message);
    }

    [Fact]
    public void Test_MockCreate_Verify_WithMatcher()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        mock.GetValue("admin-123");
        mock.GetValue("user-456");

        // Act & Assert
        builder.Verify(
            x => x.GetValue(It.IsAny<string>()),
            args => args[0] is string s && s.StartsWith("admin"),
            times => times == 1);
    }

    [Fact]
    public void Test_MockCreate_Verify_WithMatcher_ThrowsOnMismatch()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;
        mock.GetValue("user-123");

        // Act & Assert
        var exception = Assert.Throws<VerificationException>(() =>
            builder.Verify(
                x => x.GetValue(It.IsAny<string>()),
                args => args[0] is string s && s.StartsWith("admin"),
                times => times == 1));
        
        Assert.NotNull(exception);
    }

    [Fact]
    public void Test_MockCreate_VerifyGet_PropertyAccessed()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();
        builder.SetupGet(x => x.Name, () => "test");
        var mock = builder.Object;
        var _ = mock.Name;

        // Act & Assert - should not throw
        builder.VerifyGet(x => x.Name, times => times == 1);
    }

    [Fact]
    public void Test_MockCreate_VerifyGet_NotAccessed()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act & Assert - should not throw
        builder.VerifyGet(x => x.Name, times => times == 0);
    }

    [Fact]
    public void Test_MockCreate_VerifyGet_ThrowsOnMismatch()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();
        builder.SetupGet(x => x.Name, () => "test");
        var mock = builder.Object;
        var _ = mock.Name;

        // Act & Assert
        var exception = Assert.Throws<VerificationException>(() =>
            builder.VerifyGet(x => x.Name, times => times == 2));
        
        Assert.NotNull(exception);
        Assert.Contains("Verification failed", exception.Message);
    }

    [Fact]
    public void Test_MockCreate_VerifySet_PropertyModified()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();
        var mock = builder.Object;
        mock.Name = "value1";
        mock.Name = "value2";

        // Act & Assert - should not throw
        builder.VerifySet(x => x.Name, times => times == 2);
    }

    [Fact]
    public void Test_MockCreate_VerifySet_PropertyNotModified()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();

        // Act & Assert - should not throw
        builder.VerifySet(x => x.Name, times => times == 0);
    }

    [Fact]
    public void Test_MockCreate_VerifySet_WithMatcher()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();
        var mock = builder.Object;
        mock.Name = "validName";
        mock.Name = "anotherName";

        // Act & Assert
        builder.VerifySet(x => x.Name, value => value == "validName", times => times == 1);
    }

    // ==================== INTEGRATION TESTS ====================

    [Fact]
    public void Test_MockCreate_CompleteWorkflow()
    {
        // Arrange
        var builder = Mock.Create<ITestService>()
            .Setup(x => x.GetValue("user-123"), () => "John Doe")
            .Setup(x => x.GetNumber(1), () => 100);

        var mock = builder.Object;

        // Act
        var userName = mock.GetValue("user-123");
        var count = mock.GetNumber(1);

        // Assert
        Assert.Equal("John Doe", userName);
        Assert.Equal(100, count);
        Assert.Equal(2, builder.Invocations.Count);

        // Verify the calls
        builder.Verify(x => x.GetValue("user-123"), times => times == 1);
        builder.Verify(x => x.GetNumber(1), times => times == 1);
    }

    [Fact]
    public void Test_MockCreate_WithGenericInterface()
    {
        // Arrange
        var builder = Mock.Create<IGenericService<string>>();
        var mock = builder.Object;

        // Act
        var result = mock.GetItem("id123");
        var invocations = builder.Invocations;

        // Assert
        Assert.Null(result);
        Assert.Single(invocations);
        Assert.Equal("id123", invocations[0].Arguments[0]);
    }

    [Fact]
    public void Test_MockCreate_IndependentBuilders()
    {
        // Arrange
        var builder1 = Mock.Create<ITestService>()
            .Setup(x => x.GetValue("key1"), () => "value1");

        var builder2 = Mock.Create<ITestService>()
            .Setup(x => x.GetValue("key2"), () => "value2");

        var mock1 = builder1.Object;
        var mock2 = builder2.Object;

        // Act
        var result1 = mock1.GetValue("key1");
        var result2 = mock2.GetValue("key2");

        // Assert
        Assert.Equal("value1", result1);
        Assert.Equal("value2", result2);
        Assert.Single(builder1.Invocations);
        Assert.Single(builder2.Invocations);
    }

    [Fact]
    public void Test_MockCreate_PropertyService()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>()
            .ReturnsGet(x => x.Name, "TestName")
            .ReturnsGet(x => x.Count, 42);

        var mock = builder.Object;

        // Act
        var name = mock.Name;
        var count = mock.Count;

        // Assert
        Assert.Equal("TestName", name);
        Assert.Equal(42, count);
    }

    [Fact]
    public void Test_MockCreate_ExceptionMessageForFailedVerification()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        mock.GetValue("test");

        // Assert
        var exception = Assert.Throws<VerificationException>(() =>
            builder.Verify(x => x.GetValue("test"), times => times == 5));

        Assert.Contains("Actual calls: 1", exception.Message);
    }

    [Fact]
    public void Test_MockCreate_AsyncTaskMethod()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        var task = mock.DoSomethingAsync();

        // Assert
        Assert.NotNull(task);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
    }

    [Fact]
    public async Task Test_MockCreate_AsyncTaskGenericMethod()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        var task = mock.GetValueAsync("key");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        Assert.Null(await task);
    }
}

/// <summary>
/// Helper class for argument matching in Mock.Create verifications.
/// </summary>
public static class It
{
    /// <summary>
    /// Matches any argument of type T in verification expressions.
    /// </summary>
    /// <typeparam name="T">The type of the argument to match.</typeparam>
    /// <returns>Default value of T (used as placeholder in lambda expressions).</returns>
    public static T IsAny<T>() => default(T)!;
}
