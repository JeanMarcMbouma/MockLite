using System;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Integration test demonstrating real-world usage of strongly-typed Setup overloads for Action delegates.
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

        // Setup: LogEvent returns a logging callback
        builder.Setup(
            x => x.LogEvent("query"),
            (string eventName, string details) => queryResults.Add($"Log: {eventName} - {details}")
        );

        var mock = builder.Object;

        // Act - Use the query callback
        var queryCallback = mock.ExecuteQuery("SELECT * FROM Users WHERE Age > ?", 18);
        queryCallback(25, "Alice");
        queryCallback(30, "Bob");
        queryCallback(22, "Charlie");

        // Act - Use the log callback
        var logCallback = mock.LogEvent("query");
        logCallback("query", "Executed successfully");

        // Assert
        Assert.Equal(4, queryResults.Count);
        Assert.Contains("User: Alice, Age: 25", queryResults);
        Assert.Contains("User: Bob, Age: 30", queryResults);
        Assert.Contains("User: Charlie, Age: 22", queryResults);
        Assert.Contains("Log: query - Executed successfully", queryResults);

        // Verify the setups were called
        builder.Verify(x => x.ExecuteQuery("SELECT * FROM Users WHERE Age > ?", 18), times => times == 1);
        builder.Verify(x => x.LogEvent("query"), times => times == 1);
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

        // Setup: GetNotificationHandler returns a notification handler
        builder.Setup(
            x => x.GetNotificationHandler("info"),
            (string message) => events.Add($"Notification: {message}")
        );

        var mock = builder.Object;

        // Act - Simulate button clicks
        var clickHandler = mock.GetClickHandler("submit-button");
        clickHandler("submit-button", 1);
        clickHandler("submit-button", 2);

        // Act - Send notifications
        var notificationHandler = mock.GetNotificationHandler("info");
        notificationHandler("Operation completed");

        // Assert
        Assert.Equal(3, events.Count);
        Assert.Contains("Button submit-button clicked 1 times", events);
        Assert.Contains("Button submit-button clicked 2 times", events);
        Assert.Contains("Notification: Operation completed", events);
    }

    [Fact]
    public void Test_ChainedSetups_WithDifferentActionTypes()
    {
        // Arrange - Create a mock service with multiple Action delegate types
        var builder = Mock.Create<IMultiDelegateService>();
        var sideEffects = new List<string>();

        // Setup: Chain multiple setups of different Action arities
        builder
            .Setup(x => x.GetAction(), () => sideEffects.Add("Action executed"))
            .Setup(x => x.GetActionWithParam(), (int x) => sideEffects.Add($"Action with param {x}"))
            .Setup(x => x.GetActionWithTwoParams(), (string s, bool b) => sideEffects.Add($"Action: {s}, {b}"));

        var mock = builder.Object;

        // Act
        mock.GetAction()();
        mock.GetActionWithParam()(100);
        mock.GetActionWithTwoParams()("test", true);

        // Assert
        Assert.Equal(3, sideEffects.Count);
        Assert.Contains("Action executed", sideEffects);
        Assert.Contains("Action with param 100", sideEffects);
        Assert.Contains("Action: test, True", sideEffects);
    }

    // ==================== TEST INTERFACES ====================

    private interface IDatabaseService
    {
        Action<int, string> ExecuteQuery(string query, int parameter);
        Action<string, string> LogEvent(string eventType);
    }

    private interface IEventHandlerService
    {
        Action<string, int> GetClickHandler(string elementId);
        Action<string> GetNotificationHandler(string notificationType);
    }

    private interface IMultiDelegateService
    {
        Action GetAction();
        Action<int> GetActionWithParam();
        Action<string, bool> GetActionWithTwoParams();
    }
}
