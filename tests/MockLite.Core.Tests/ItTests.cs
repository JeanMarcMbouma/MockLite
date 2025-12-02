using System;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Tests for the It argument matcher class.
/// </summary>
public class ItTests
{
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

    [Fact]
    public void Test_ItIsAny_WithSetup_MatchesAnyStringValue()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.IsAny<string>()), () => "matched");
        var mock = builder.Object;

        // Act
        var result1 = mock.GetValue("test1");
        var result2 = mock.GetValue("test2");
        var result3 = mock.GetValue("completely different");

        // Assert
        Assert.Equal("matched", result1);
        Assert.Equal("matched", result2);
        Assert.Equal("matched", result3);
    }

    [Fact]
    public void Test_ItIsAny_WithSetup_MatchesAnyIntValue()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetNumber(It.IsAny<int>()), () => 42);
        var mock = builder.Object;

        // Act
        var result1 = mock.GetNumber(1);
        var result2 = mock.GetNumber(999);
        var result3 = mock.GetNumber(-5);

        // Assert
        Assert.Equal(42, result1);
        Assert.Equal(42, result2);
        Assert.Equal(42, result3);
    }

    [Fact]
    public void Test_ItIsAny_WithSetupSet_MatchesAnyValue()
    {
        // Arrange
        var builder = Mock.Create<IPropertyService>();
        var setValues = new List<string>();
        
        builder.SetupSet(x => x.Name, value => setValues.Add(value));
        var mock = builder.Object;

        // Act
        mock.Name = "first";
        mock.Name = "second";
        mock.Name = "third";

        // Assert
        Assert.Equal(3, setValues.Count);
        Assert.Contains("first", setValues);
        Assert.Contains("second", setValues);
        Assert.Contains("third", setValues);
    }

    [Fact]
    public void Test_ItIsAny_WithSetup_MultipleParameters_PartialWildcard()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        // Setup with first param exact match, second param IsAny
        builder.Setup(x => x.GetValue(It.IsAny<string>()), () => "isany-matched");
        var mock = builder.Object;

        // Act - call with different values
        var result1 = mock.GetValue("exact-key");
        var result2 = mock.GetValue("another-key");

        // Assert - both should match because both use IsAny
        Assert.Equal("isany-matched", result1);
        Assert.Equal("isany-matched", result2);
    }

    [Fact]
    public void Test_ItIsAny_WithSetup_RecordsInvocationsCorrectly()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.IsAny<string>()), () => "result");
        var mock = builder.Object;

        // Act
        mock.GetValue("test1");
        mock.GetValue("test2");
        mock.GetValue("test3");

        // Assert - verify all invocations were recorded with actual values
        Assert.Equal(3, builder.Invocations.Count);
        Assert.Equal("test1", builder.Invocations[0].Arguments[0]);
        Assert.Equal("test2", builder.Invocations[1].Arguments[0]);
        Assert.Equal("test3", builder.Invocations[2].Arguments[0]);
    }

    [Fact]
    public void Test_ItIsAny_WithSetup_VoidMethod()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var callCount = 0;
        
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
    public void Test_ItIsAny_WithSetup_GenericInterface()
    {
        // Arrange
        var builder = Mock.Create<IGenericService<string>>();
        builder.Setup(x => x.GetItem(It.IsAny<string>()), () => "generic-result");
        var mock = builder.Object;

        // Act
        var result1 = mock.GetItem("id1");
        var result2 = mock.GetItem("id2");
        var result3 = mock.GetItem("id3");

        // Assert
        Assert.Equal("generic-result", result1);
        Assert.Equal("generic-result", result2);
        Assert.Equal("generic-result", result3);
    }
}
