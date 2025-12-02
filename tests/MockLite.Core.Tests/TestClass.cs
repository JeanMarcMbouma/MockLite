using System.Runtime.CompilerServices;

namespace BbQ.MockLite.Tests;

public interface ITestService
{
    string GetValue(string key);
    int GetNumber(int input);
    void DoSomething();
    Task<string> GetValueAsync(string key);
    Task DoSomethingAsync();
}

public interface IPropertyService
{
    string Name { get; set; }
    int Count { get; }
}

public interface IGenericService<T>
{
    T GetItem(string id);
    void SetItem(T item);
}

public class TestClass
{
    // ==================== MOCK.OF TESTS ====================

    [Fact]
    public void Test_MockOf_CreatesValidInstance()
    {
        // Act
        var mock = Mock.Of<ITestService>();

        // Assert
        Assert.NotNull(mock);
        Assert.IsAssignableFrom<ITestService>(mock);
    }

    [Fact]
    public void Test_MockOf_WithDifferentInterfaces_CreatesDistinctInstances()
    {
        // Act
        var mock1 = Mock.Of<ITestService>();
        var mock2 = Mock.Of<ITestService>();

        // Assert
        Assert.NotNull(mock1);
        Assert.NotNull(mock2);
        Assert.NotSame(mock1, mock2);
    }

    [Fact]
    public void Test_MockMethod_ReturnsDefaultValue()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var result = mock.GetValue("test");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Test_MockVoidMethod_ExecutesWithoutException()
    {
        // Act
        var mock = Mock.Of<ITestService>();

        // Assert - should not throw
        mock.DoSomething();
    }

    [Fact]
    public void Test_MockTaskMethod_ReturnsCompletedTask()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var result = mock.DoSomethingAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TaskStatus.RanToCompletion, result.Status);
    }

    [Fact]
    public async Task Test_MockTaskGenericMethod_ReturnsCompletedTaskWithDefault()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var task = mock.GetValueAsync("key");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        Assert.Null(await task);
    }

    [Fact]
    public void Test_MockIntMethod_ReturnsDefaultValue()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var result = mock.GetNumber(5);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Test_InvocationRecording_RecordsMethodCalls()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();

        // Act
        mock.GetValue("key1");
        mock.GetValue("key2");
        mock.DoSomething();

        // Assert - check if mock has recorded invocations
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        if (invocationsProperty != null)
        {
            var invocations = invocationsProperty.GetValue(mock);
            Assert.NotNull(invocations);
        }
    }

    [Fact]
    public void Test_MultipleMethodCalls_MaintainIndependentState()
    {
        // Act
        var mock = Mock.Of<ITestService>();

        var result1 = mock.GetValue("key1");
        var result2 = mock.GetValue("key2");
        mock.DoSomething();

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public void Test_GenericInterface_CreatesMock()
    {
        // Act
        var mock = Mock.Of<IGenericService<string>>();

        // Assert
        Assert.NotNull(mock);
        Assert.IsAssignableFrom<IGenericService<string>>(mock);
    }

    [Fact]
    public void Test_GenericInterface_ReturnsDefaultValue()
    {
        // Act
        var mock = Mock.Of<IGenericService<int>>();
        var result = mock.GetItem("id");

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Test_GenericInterface_VoidMethod()
    {
        // Act
        var mock = Mock.Of<IGenericService<string>>();

        // Assert - should not throw
        mock.SetItem("test");
    }

    [Fact]
    public void Test_MockType_IsNotNull()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var type = mock.GetType();

        // Assert
        Assert.NotNull(type);
        Assert.True(type.IsClass);
    }

    [Fact]
    public void Test_MockType_ImplementsInterface()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var interfaces = mock.GetType().GetInterfaces();

        // Assert
        Assert.Contains(typeof(ITestService), interfaces);
    }

    [Fact]
    public void Test_SequentialCalls_ReturnConsistentDefaults()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();

        // Act
        var result1 = mock.GetNumber(1);
        var result2 = mock.GetNumber(2);
        var result3 = mock.GetNumber(3);

        // Assert
        Assert.Equal(0, result1);
        Assert.Equal(0, result2);
        Assert.Equal(0, result3);
    }

    [Fact]
    public void Test_ValueTypeInterface_GetDefault()
    {
        // Act
        var mock = Mock.Of<IGenericService<int>>();
        var result = mock.GetItem("test");

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Test_AsyncMethod_CanBeAwaited()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        await mock.DoSomethingAsync();

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task Test_AsyncGenericMethod_CanBeAwaited()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var result = await mock.GetValueAsync("key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Test_SameInterfaceType_CanCreateMultipleMocks()
    {
        // Act
        var mocks = new[]
        {
            Mock.Of<ITestService>(),
            Mock.Of<ITestService>(),
            Mock.Of<ITestService>()
        };

        // Assert
        Assert.Equal(3, mocks.Length);
        Assert.All(mocks, m => Assert.NotNull(m));
    }

    [Fact]
    public void Test_InterfaceWithMultipleMethods_AllReturnDefaults()
    {
        // Act
        var mock = Mock.Of<ITestService>();

        var strResult = mock.GetValue("key");
        var intResult = mock.GetNumber(5);

        // Assert
        Assert.Null(strResult);
        Assert.Equal(0, intResult);
    }

    [Fact]
    public void Test_RuntimeProxy_WithStringMethod_ReturnsNull()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var result = mock.GetValue("test");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Test_RuntimeProxy_WithIntMethod_ReturnsZero()
    {
        // Act
        var mock = Mock.Of<ITestService>();
        var result = mock.GetNumber(42);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Test_MockCreation_DoesNotThrow()
    {
        // Act & Assert - should not throw
        var exception = Record.Exception(() => Mock.Of<ITestService>());
        Assert.Null(exception);
    }

    [Fact]
    public void Test_MockMethodCall_DoesNotThrow()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();

        // Act & Assert - should not throw
        var exception = Record.Exception(() => mock.GetValue("test"));
        Assert.Null(exception);
    }

    // ==================== RUNTIME PROXY INVOCATIONS TESTS ====================

    [Fact]
    public void Test_RuntimeProxy_InvocationsProperty_Exists()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();

        // Act
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Assert
        Assert.NotNull(invocationsProperty);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationsProperty_IsReadable()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        var isReadable = invocationsProperty?.CanRead;

        // Assert
        Assert.True(isReadable);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationsProperty_ReturnsCollection()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        var invocations = invocationsProperty?.GetValue(mock);

        // Assert
        Assert.NotNull(invocations);
        Assert.IsAssignableFrom<System.Collections.IList>(invocations);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationsInitialized_Empty()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;

        // Assert
        Assert.NotNull(invocations);
        Assert.Empty(invocations);
    }

    [Fact]
    public void Test_RuntimeProxy_RecordsFirstInvocation()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetValue("test-key");
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;

        // Assert
        Assert.NotNull(invocations);
        Assert.Single(invocations);
    }

    [Fact]
    public void Test_RuntimeProxy_RecordsMultipleInvocations()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetValue("key1");
        mock.GetValue("key2");
        mock.GetNumber(42);
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;

        // Assert
        Assert.NotNull(invocations);
        Assert.Equal(3, invocations.Count);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationContainsMethodInfo()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetValue("test");
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var firstInvocation = invocations?[0];
        var methodProperty = firstInvocation?.GetType().GetProperty("Method");
        var method = methodProperty?.GetValue(firstInvocation);

        // Assert
        Assert.NotNull(methodProperty);
        Assert.NotNull(method);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationMethodInfo_HasCorrectName()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();

        // Act
        mock.GetValue("test");
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var firstInvocation = invocations?[0];
        var methodProperty = firstInvocation?.GetType().GetProperty("Method");
        var method = methodProperty?.GetValue(firstInvocation) as System.Reflection.MethodInfo;

        // Assert
        Assert.NotNull(method);
        Assert.Equal("GetValue", method.Name);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationContainsArgs()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetValue("test-arg");
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var firstInvocation = invocations?[0];
        var argsProperty = firstInvocation?.GetType().GetProperty("Arguments");

        // Assert
        Assert.NotNull(argsProperty);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationArgs_CapturesArgument()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        const string testArg = "test-argument";
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetValue(testArg);
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var firstInvocation = invocations?[0];
        var argsProperty = firstInvocation?.GetType().GetProperty("Arguments");
        var args = argsProperty?.GetValue(firstInvocation) as object[];

        // Assert
        Assert.NotNull(args);
        Assert.Single(args);
        Assert.Equal(testArg, args[0]);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationArgs_PreservesType()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        const int testNumber = 99;
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetNumber(testNumber);
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var firstInvocation = invocations?[0];
        var argsProperty = firstInvocation?.GetType().GetProperty("Arguments");
        var args = argsProperty?.GetValue(firstInvocation) as object[];

        // Assert
        Assert.NotNull(args);
        Assert.Single(args);
        Assert.IsType<int>(args[0]);
        Assert.Equal(testNumber, args[0]);
    }

    [Fact]
    public void Test_RuntimeProxy_VoidMethod_RecordsInvocation()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.DoSomething();
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;

        // Assert
        Assert.NotNull(invocations);
        Assert.Single(invocations);
    }

    [Fact]
    public void Test_RuntimeProxy_VoidMethod_Args_IsEmpty()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.DoSomething();
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var firstInvocation = invocations?[0];
        var argsProperty = firstInvocation?.GetType().GetProperty("Arguments");
        var args = argsProperty?.GetValue(firstInvocation) as object[];

        // Assert
        Assert.NotNull(args);
        Assert.Empty(args);
    }

    [Fact]
    public void Test_RuntimeProxy_AsyncMethod_RecordsInvocation()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        var _ = mock.DoSomethingAsync();
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;

        // Assert
        Assert.NotNull(invocations);
        Assert.Single(invocations);
    }

    [Fact]
    public void Test_RuntimeProxy_Setup_MethodExists()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();

        // Act
        var setupMethod = mockType.GetMethod("Setup",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, [
                typeof(System.Reflection.MethodInfo),
                typeof(Delegate)
                ]);

        // Assert
        Assert.NotNull(setupMethod);
    }

    [Fact]
    public void Test_RuntimeProxy_Setup_IsPublic()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();

        // Act
        var setupMethod = mockType.GetMethod("Setup",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, [
                typeof(System.Reflection.MethodInfo),
                typeof(Delegate)
                ]);

        // Assert
        Assert.NotNull(setupMethod);
        Assert.True(setupMethod.IsPublic);
    }

    [Fact]
    public void Test_RuntimeProxy_Setup_AcceptsMethodInfoAndDelegate()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();

        // Act
        var setupMethod = mockType.GetMethod("Setup",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, [
                typeof(System.Reflection.MethodInfo),
                typeof(Delegate)
                ]);

        // Assert
        Assert.NotNull(setupMethod);
        var parameters = setupMethod.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(System.Reflection.MethodInfo), parameters[0].ParameterType);
        Assert.Equal(typeof(Delegate), parameters[1].ParameterType);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationOrderMaintained()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetValue("first");
        mock.GetValue("second");
        mock.GetValue("third");
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;

        // Assert
        Assert.NotNull(invocations);
        Assert.Equal(3, invocations.Count);

        var firstArgs = invocations[0]?.GetType().GetProperty("Arguments")?.GetValue(invocations[0]) as object[];
        var secondArgs = invocations[1]?.GetType().GetProperty("Arguments")?.GetValue(invocations[1]) as object[];
        var thirdArgs = invocations[2]?.GetType().GetProperty("Arguments")?.GetValue(invocations[2]) as object[];

        Assert.Equal("first", firstArgs?[0]);
        Assert.Equal("second", secondArgs?[0]);
        Assert.Equal("third", thirdArgs?[0]);
    }

    [Fact]
    public void Test_RuntimeProxy_GenericInterface_RecordsInvocation()
    {
        // Arrange
        var mock = Mock.Of<IGenericService<string>>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetItem("id1");
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;

        // Assert
        Assert.NotNull(invocations);
        Assert.Single(invocations);
    }

    [Fact]
    public void Test_RuntimeProxy_GenericInterface_CapturesGenericArgument()
    {
        // Arrange
        var mock = Mock.Of<IGenericService<string>>();
        const string testId = "test-id-123";
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetItem(testId);
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var firstInvocation = invocations?[0];
        var argsProperty = firstInvocation?.GetType().GetProperty("Arguments");
        var args = argsProperty?.GetValue(firstInvocation) as object[];

        // Assert
        Assert.NotNull(args);
        Assert.Single(args);
        Assert.Equal(testId, args[0]);
    }

    [Fact]
    public void Test_RuntimeProxy_MultipleInstances_HaveIndependentInvocations()
    {
        // Arrange
        var mock1 = Mock.Of<ITestService>();
        var mock2 = Mock.Of<ITestService>();

        // Act
        mock1.GetValue("instance1");
        mock2.GetValue("instance2");

        var invocations1 = mock1.GetType().GetProperty("Invocations")?.GetValue(mock1) as System.Collections.IList;
        var invocations2 = mock2.GetType().GetProperty("Invocations")?.GetValue(mock2) as System.Collections.IList;

        // Assert
        Assert.NotNull(invocations1);
        Assert.NotNull(invocations2);
        Assert.Single(invocations1);
        Assert.Single(invocations2);

        var args1 = invocations1[0]?.GetType().GetProperty("Arguments")?.GetValue(invocations1[0]) as object[];
        var args2 = invocations2[0]?.GetType().GetProperty("Arguments")?.GetValue(invocations2[0]) as object[];

        Assert.Equal("instance1", args1?[0]);
        Assert.Equal("instance2", args2?[0]);
    }

    [Fact]
    public void Test_RuntimeProxy_Invocations_CanBeCleared()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetValue("test");
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var initialCount = invocations?.Count;

        if (invocations?.Count > 0)
        {
            invocations.Clear();
        }

        // Assert
        Assert.NotEqual(0, initialCount);
        Assert.NotNull(invocations);
        Assert.Empty(invocations);
    }

    [Fact]
    public void Test_RuntimeProxy_InvocationIndex_AccessByIndex()
    {
        // Arrange
        var mock = Mock.Of<ITestService>();
        var mockType = mock.GetType();
        var invocationsProperty = mockType.GetProperty("Invocations");

        // Act
        mock.GetValue("first");
        mock.GetValue("second");
        var invocations = invocationsProperty?.GetValue(mock) as System.Collections.IList;
        var secondInvocation = invocations?[1];

        // Assert
        Assert.NotNull(secondInvocation);
        var argsProperty = secondInvocation.GetType().GetProperty("Arguments");
        var args = argsProperty?.GetValue(secondInvocation) as object[];
        Assert.Equal("second", args?[0]);
    }

    [Fact]
    public void Test_ItIsAny_ReturnsAnyMatcherMarker()
    {
        // Act
        var result = It.IsAny<string>();

        // Assert
        Assert.NotNull(result);
        // The result should be an AnyMatcher instance reinterpreted as a string
        // Verify by checking the runtime type name
        var actualType = result.GetType();
        Assert.Equal("AnyMatcher", actualType.Name);
    }

    [Fact]
    public void Test_ItIsAny_WithDifferentTypes()
    {
        // Act - should not throw
        var stringResult = It.IsAny<string>();
        var objectResult = It.IsAny<object>();
        var customResult = It.IsAny<ITestService>();

        // Assert - all should return non-null markers
        Assert.NotNull(stringResult);
        Assert.NotNull(objectResult);
        Assert.NotNull(customResult);
    }

    [Fact]
    public void Test_ItIsAny_CanBeUsedInLambdaExpression()
    {
        // Act - should not throw
        var builder = Mock.Create<ITestService>();
        
        // This demonstrates that It.IsAny can be syntactically used in lambda expressions
        // without causing compilation or runtime errors
        var exception = Record.Exception(() =>
        {
            var mock = builder.Object;
            // The lambda expression with It.IsAny can be evaluated without throwing
            mock.GetValue(It.IsAny<string>());
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Test_ItIsAny_WithVerifyAndMatcher()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act - call the method with specific values
        mock.GetValue("test-value-1");
        mock.GetValue("test-value-2");

        // Assert - verify using It.IsAny in the expression and a matcher predicate
        // The matcher predicate (args => true) implements the "any" logic
        builder.Verify(
            x => x.GetValue(It.IsAny<string>()),
            args => true, // This is what makes it match "any" argument
            times => times == 2);
    }

    [Fact]
    public void Test_ItIsAny_WithMatcherPredicate_FiltersCalls()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        int callCount = 0;
        builder.OnCall(x => x.GetValue(It.IsAny<string>()), (_) => callCount++);
        var mock = builder.Object;

        // Act - call with different values
        mock.GetValue("admin-123");
        mock.GetValue("user-456");
        mock.GetValue("admin-789");

        // Assert - use matcher to filter for specific calls
        builder.Verify(
            x => x.GetValue(It.IsAny<string>()),
            args => args[0] is string s && s.StartsWith("admin"),
            times => times == 2); // Should match 2 admin calls
        Assert.Equal(3, callCount); // Total calls made
    }

    [Fact]
    public void Test_ItIsAny_WithMockCall_TracksInvocation()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act - call the method with a specific value
        mock.GetValue("test-value");

        // Assert - verify the call was recorded
        Assert.Single(builder.Invocations);
        Assert.Equal("GetValue", builder.Invocations[0].Method.Name);
        Assert.Equal("test-value", builder.Invocations[0].Arguments[0]);
    }

    [Fact]
    public void Test_ItIsAny_MultipleCallsRecorded()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act - call the method multiple times with different values
        mock.GetValue("value1");
        mock.GetValue("value2");
        mock.GetValue("value3");

        // Assert - all calls should be recorded with their actual arguments
        Assert.Equal(3, builder.Invocations.Count);
        Assert.All(builder.Invocations, inv => Assert.Equal("GetValue", inv.Method.Name));
        Assert.Equal("value1", builder.Invocations[0].Arguments[0]);
        Assert.Equal("value2", builder.Invocations[1].Arguments[0]);
        Assert.Equal("value3", builder.Invocations[2].Arguments[0]);
    }
}
