using System;
using System.Threading.Tasks;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Tests for strongly-typed Setup overloads with compile-time parameter matching.
/// Validates that handlers receive method arguments and compiler enforces parameter types.
/// </summary>
public class StronglyTypedDelegateSetupTests
{
    // ==================== VOID METHOD TESTS ====================

    [Fact]
    public void Test_Setup_VoidMethod_NoParams()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var called = false;
        
        // Act - Setup void method with no parameters
        builder.Setup(
            x => x.Log(),
            () => called = true
        );

        var mock = builder.Object;
        mock.Log();

        // Assert
        Assert.True(called);
    }

    [Fact]
    public void Test_Setup_VoidMethod_TwoParams()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var receivedProc = "";
        var receivedValue = 0;
        
        // Act - Setup void method with 2 parameters - handler receives method arguments
        builder.Setup(
            (IDataService x, string proc, int value) => x.Execute(proc, value),
            (string proc, int value) =>
            {
                receivedProc = proc;
                receivedValue = value;
            }
        );

        var mock = builder.Object;
        mock.Execute("sproc", 42);

        // Assert
        Assert.Equal("sproc", receivedProc);
        Assert.Equal(42, receivedValue);
    }

    [Fact]
    public void Test_Setup_VoidMethod_ThreeParams()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        var sum = 0;
        
        // Act - Setup void method with 3 parameters
        builder.Setup(
            (IDataService x, int a, int b, int c) => x.Calculate(a, b, c),
            (int a, int b, int c) => sum = a + b + c
        );

        var mock = builder.Object;
        mock.Calculate(10, 20, 30);

        // Assert
        Assert.Equal(60, sum);
    }

    // ==================== RETURN VALUE METHOD TESTS ====================

    [Fact]
    public void Test_Setup_ReturnMethod_TwoParams()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method with 2 parameters and return value
        builder.Setup(
            (IDataService x, int a, int b) => x.Add(a, b),
            (int a, int b) => a + b
        );

        var mock = builder.Object;
        var result = mock.Add(15, 27);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Test_Setup_ReturnMethod_ThreeParams()
    {
        // Arrange
        var builder = Mock.Create<IDataService>();
        
        // Act - Setup method with 3 parameters and return value
        builder.Setup(
            (IDataService x, string template, int value1, int value2) => x.Format(template, value1, value2),
            (string template, int value1, int value2) => $"{template}:{value1}+{value2}={value1 + value2}"
        );

        var mock = builder.Object;
        var result = mock.Format("Result", 10, 32);

        // Assert
        Assert.Equal("Result:10+32=42", result);
    }

    // ==================== TEST INTERFACE ====================

    /// <summary>
    /// Test interface with methods taking various parameter counts.
    /// </summary>
    private interface IDataService
    {
        // Void methods
        void Log();
        void Execute(string proc, int value);
        void Calculate(int a, int b, int c);
        
        // Return value methods
        int Add(int a, int b);
        string Format(string template, int value1, int value2);
    }
}
