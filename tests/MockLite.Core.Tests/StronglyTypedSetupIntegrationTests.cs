using System;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Integration test demonstrating real-world usage of strongly-typed Setup overloads.
/// This test simulates a database query service that returns delegate callbacks.
/// </summary>
public class StronglyTypedSetupIntegrationTests
{
    [Fact]
    public void Test_RealWorldScenario_DatabaseQueryServiceWithCallbacks()
    {
        // Arrange - Create a mock database service
        var builder = Mock.Create<IDatabaseService>();
        var queryResults = new List<string>();

        // Setup: Query returns a callback that processes results
        builder.Setup(
            x => x.ExecuteQuery("SELECT * FROM Users WHERE Age > ?", 18),
            (int age, string name) => queryResults.Add($"User: {name}, Age: {age}")
        );

        // Setup: GetMapper returns a transformation function
        builder.Setup(
            x => x.GetMapper("uppercase"),
            (string input) => input.ToUpper()
        );

        // Setup: GetAggregator returns a multi-parameter aggregation function
        builder.Setup(
            x => x.GetAggregator("sum"),
            (int a, int b, int c) => a + b + c
        );

        var mock = builder.Object;

        // Act - Use the query callback
        var queryCallback = mock.ExecuteQuery("SELECT * FROM Users WHERE Age > ?", 18);
        queryCallback(25, "Alice");
        queryCallback(30, "Bob");
        queryCallback(22, "Charlie");

        // Act - Use the mapper function
        var mapperFunc = mock.GetMapper("uppercase");
        var mappedResult = mapperFunc("hello");

        // Act - Use the aggregator function
        var aggregatorFunc = mock.GetAggregator("sum");
        var aggregatedResult = aggregatorFunc(10, 20, 30);

        // Assert
        Assert.Equal(3, queryResults.Count);
        Assert.Contains("User: Alice, Age: 25", queryResults);
        Assert.Contains("User: Bob, Age: 30", queryResults);
        Assert.Contains("User: Charlie, Age: 22", queryResults);
        Assert.Equal("HELLO", mappedResult);
        Assert.Equal(60, aggregatedResult);

        // Verify the setups were called
        builder.Verify(x => x.ExecuteQuery("SELECT * FROM Users WHERE Age > ?", 18), times => times == 1);
        builder.Verify(x => x.GetMapper("uppercase"), times => times == 1);
        builder.Verify(x => x.GetAggregator("sum"), times => times == 1);
    }

    [Fact]
    public void Test_RealWorldScenario_EventHandlerService()
    {
        // Arrange - Create a mock event handler service
        var builder = Mock.Create<IEventHandlerService>();
        var events = new List<string>();

        // Setup: GetClickHandler returns an event handler for button clicks
        builder.Setup(
            x => x.GetClickHandler("submit-button"),
            (string buttonId, int clickCount) => events.Add($"Button {buttonId} clicked {clickCount} times")
        );

        // Setup: GetValidationHandler returns a validation function
        builder.Setup(
            x => x.GetValidationHandler("email"),
            (string input) => input.Contains("@")
        );

        var mock = builder.Object;

        // Act - Simulate button clicks
        var clickHandler = mock.GetClickHandler("submit-button");
        clickHandler("submit-button", 1);
        clickHandler("submit-button", 2);

        // Act - Validate emails
        var validator = mock.GetValidationHandler("email");
        var validEmail = validator("user@example.com");
        var invalidEmail = validator("invalid-email");

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Contains("Button submit-button clicked 1 times", events);
        Assert.Contains("Button submit-button clicked 2 times", events);
        Assert.True(validEmail);
        Assert.False(invalidEmail);
    }

    [Fact]
    public void Test_RealWorldScenario_CalculatorServiceWithOperations()
    {
        // Arrange - Create a mock calculator service
        var builder = Mock.Create<ICalculatorService>();

        // Setup: Different operations with different arities
        builder.Setup(
            x => x.GetBinaryOperation("add"),
            (double a, double b) => a + b
        );

        builder.Setup(
            x => x.GetUnaryOperation("square"),
            (double x) => x * x
        );

        builder.Setup(
            x => x.GetTernaryOperation("between"),
            (double value, double min, double max) => value >= min && value <= max
        );

        var mock = builder.Object;

        // Act & Assert - Binary operation
        var add = mock.GetBinaryOperation("add");
        Assert.Equal(15.0, add(10.0, 5.0));

        // Act & Assert - Unary operation
        var square = mock.GetUnaryOperation("square");
        Assert.Equal(25.0, square(5.0));

        // Act & Assert - Ternary operation
        var between = mock.GetTernaryOperation("between");
        Assert.True(between(5.0, 0.0, 10.0));
        Assert.False(between(15.0, 0.0, 10.0));
    }

    [Fact]
    public void Test_ChainedSetups_WithDifferentDelegateTypes()
    {
        // Arrange - Create a mock service with multiple delegate types
        var builder = Mock.Create<IMultiDelegateService>();
        var sideEffects = new List<string>();

        // Setup: Chain multiple setups of different types
        builder
            .Setup(x => x.GetAction(), () => sideEffects.Add("Action executed"))
            .Setup(x => x.GetActionWithParam(), (int x) => sideEffects.Add($"Action with param {x}"))
            .Setup(x => x.GetFunc(), () => 42)
            .Setup(x => x.GetFuncWithParam(), (string s) => s.Length);

        var mock = builder.Object;

        // Act
        mock.GetAction()();
        mock.GetActionWithParam()(100);
        var funcResult = mock.GetFunc()();
        var funcWithParamResult = mock.GetFuncWithParam()("hello");

        // Assert
        Assert.Equal(2, sideEffects.Count);
        Assert.Contains("Action executed", sideEffects);
        Assert.Contains("Action with param 100", sideEffects);
        Assert.Equal(42, funcResult);
        Assert.Equal(5, funcWithParamResult);
    }

    // ==================== TEST INTERFACES ====================

    private interface IDatabaseService
    {
        Action<int, string> ExecuteQuery(string query, int parameter);
        Func<string, string> GetMapper(string mapperType);
        Func<int, int, int, int> GetAggregator(string aggregationType);
    }

    private interface IEventHandlerService
    {
        Action<string, int> GetClickHandler(string elementId);
        Func<string, bool> GetValidationHandler(string validationType);
    }

    private interface ICalculatorService
    {
        Func<double, double, double> GetBinaryOperation(string operation);
        Func<double, double> GetUnaryOperation(string operation);
        Func<double, double, double, bool> GetTernaryOperation(string operation);
    }

    private interface IMultiDelegateService
    {
        Action GetAction();
        Action<int> GetActionWithParam();
        Func<int> GetFunc();
        Func<string, int> GetFuncWithParam();
    }
}
