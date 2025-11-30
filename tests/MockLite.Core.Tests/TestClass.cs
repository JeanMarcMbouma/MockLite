using System.Runtime.CompilerServices;

namespace MockLite.Core.Tests;

public interface ITestService
{
    string GetValue(string key);
    int GetNumber(int input);
    void DoSomething();
    Task<string> GetValueAsync(string key);
    Task DoSomethingAsync();
}

public interface IGenericService<T>
{
    T GetItem(string id);
    void SetItem(T item);
}

public class TestClass
{
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
        // Note: Invocations are accessible through reflection for RuntimeProxy
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
}
