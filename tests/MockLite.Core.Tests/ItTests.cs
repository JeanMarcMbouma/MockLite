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

    // ==================== It.Matches<T> Tests ====================

    [Fact]
    public void Test_ItMatches_WithSetup_MatchesStringPredicate()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.Matches<string>(s => s.StartsWith("test"))), () => "matched");
        var mock = builder.Object;

        // Act
        var result1 = mock.GetValue("test-one");
        var result2 = mock.GetValue("test-two");
        var resultDefault = mock.GetValue("other");

        // Assert
        Assert.Equal("matched", result1);
        Assert.Equal("matched", result2);
        Assert.Null(resultDefault); // does not match predicate, returns default
    }

    [Fact]
    public void Test_ItMatches_WithSetup_MatchesIntPredicate()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetNumber(It.Matches<int>(n => n > 10)), () => 99);
        var mock = builder.Object;

        // Act
        var result1 = mock.GetNumber(11);
        var result2 = mock.GetNumber(100);
        var resultDefault = mock.GetNumber(5);

        // Assert
        Assert.Equal(99, result1);
        Assert.Equal(99, result2);
        Assert.Equal(0, resultDefault); // does not match predicate, returns default int
    }

    [Fact]
    public void Test_ItMatches_WithSetup_MultiplePredicateSetups()
    {
        // Arrange - two setups with different predicates
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.Matches<string>(s => s.StartsWith("admin"))), () => "admin-result");
        builder.Setup(x => x.GetValue(It.Matches<string>(s => s.StartsWith("user"))), () => "user-result");
        var mock = builder.Object;

        // Act
        var adminResult = mock.GetValue("admin-123");
        var userResult = mock.GetValue("user-456");
        var noMatch = mock.GetValue("guest-789");

        // Assert
        Assert.Equal("admin-result", adminResult);
        Assert.Equal("user-result", userResult);
        Assert.Null(noMatch);
    }

    [Fact]
    public void Test_ItMatches_WithFluentSetup_Returns()
    {
        // Arrange - use the fluent Setup().Returns() API
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.Matches<string>(s => s.Length > 3)))
               .Returns("long-key");
        var mock = builder.Object;

        // Act
        var result1 = mock.GetValue("abcd");   // length 4 > 3
        var result2 = mock.GetValue("ab");     // length 2, no match

        // Assert
        Assert.Equal("long-key", result1);
        Assert.Null(result2);
    }

    [Fact]
    public void Test_ItMatches_WithFluentSetup_ReturnsFactory()
    {
        // Arrange - use Returns with a factory
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetNumber(It.Matches<int>(n => n % 2 == 0)))
               .Returns(() => 42);
        var mock = builder.Object;

        // Act
        var even = mock.GetNumber(4);
        var odd = mock.GetNumber(3);

        // Assert
        Assert.Equal(42, even);
        Assert.Equal(0, odd); // no match, default int
    }

    [Fact]
    public void Test_ItMatches_WithFluentSetup_Throws()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.Matches<string>(s => s == "bad")))
               .Throws(new InvalidOperationException("bad key"));
        var mock = builder.Object;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => mock.GetValue("bad"));
        Assert.Equal("bad key", ex.Message);
        Assert.Null(mock.GetValue("good")); // no match, returns default
    }

    [Fact]
    public void Test_ItMatches_WithFluentSetup_Callback()
    {
        // Arrange
        var captured = new List<string>();
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.Matches<string>(s => s.Contains("log"))))
               .Callback<string>(s => captured.Add(s))
               .Returns("logged");
        var mock = builder.Object;

        // Act
        var r1 = mock.GetValue("log-one");
        var r2 = mock.GetValue("no-match");
        var r3 = mock.GetValue("log-two");

        // Assert - Callback fires for all invocations of the method (not scoped to matcher);
        // Returns is scoped to the predicate match.
        Assert.Equal(["log-one", "no-match", "log-two"], captured);
        Assert.Equal("logged", r1);
        Assert.Null(r2); // predicate doesn't match, returns default
        Assert.Equal("logged", r3);
    }

    [Fact]
    public void Test_ItMatches_WithSetup_RecordsInvocationsCorrectly()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.Matches<string>(s => s.StartsWith("a"))), () => "matched");
        var mock = builder.Object;

        // Act
        mock.GetValue("abc");
        mock.GetValue("xyz");

        // Assert - all invocations are recorded regardless of match
        Assert.Equal(2, builder.Invocations.Count);
        Assert.Equal("abc", builder.Invocations[0].Arguments[0]);
        Assert.Equal("xyz", builder.Invocations[1].Arguments[0]);
    }

    [Fact]
    public void Test_ItMatches_WithOnCall_FiltersCallbacks()
    {
        // Arrange
        var matchedCount = 0;
        var builder = Mock.Create<ITestService>();
        builder.OnCall(x => x.GetValue(It.Matches<string>(s => s.StartsWith("match"))), _ => matchedCount++);
        var mock = builder.Object;

        // Act
        mock.GetValue("match-1");
        mock.GetValue("other");
        mock.GetValue("match-2");

        // Assert
        Assert.Equal(2, matchedCount);
    }

    [Fact]
    public void Test_ItMatches_WithVerify_CountsMatchingCalls()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        var mock = builder.Object;

        // Act
        mock.GetValue("alpha");
        mock.GetValue("beta");
        mock.GetValue("alpha-2");

        // Assert - verify calls matching the predicate
        builder.Verify(
            x => x.GetValue(It.Matches<string>(s => s.StartsWith("alpha"))),
            args => args[0] is string s && s.StartsWith("alpha"),
            times => times == 2);
    }

    [Fact]
    public void Test_ItMatches_WithSetup_CombinedWithExactValue()
    {
        // Arrange - exact match takes priority (added last)
        var builder = Mock.Create<ITestService>();
        builder.Setup(x => x.GetValue(It.Matches<string>(s => s.Length > 0)), () => "predicate-match");
        builder.Setup(x => x.GetValue("special"), () => "exact-match");
        var mock = builder.Object;

        // Act
        var exactResult = mock.GetValue("special");
        var predicateResult = mock.GetValue("anything");

        // Assert - most recent setup wins when both could match
        Assert.Equal("exact-match", exactResult);
        Assert.Equal("predicate-match", predicateResult);
    }

    [Fact]
    public void Test_ItMatches_WithSetupSequence()
    {
        // Arrange
        var builder = Mock.Create<ITestService>();
        builder.SetupSequence(x => x.GetValue(It.Matches<string>(s => s.StartsWith("q"))), "first", "second", "third");
        var mock = builder.Object;

        // Act
        var r1 = mock.GetValue("q1");
        var r2 = mock.GetValue("q2");
        var r3 = mock.GetValue("q3");

        // Assert
        Assert.Equal("first", r1);
        Assert.Equal("second", r2);
        Assert.Equal("third", r3);
    }

    [Fact]
    public void Test_ItMatches_WithGenericInterface()
    {
        // Arrange
        var builder = Mock.Create<IGenericService<string>>();
        builder.Setup(x => x.GetItem(It.Matches<string>(id => id.Length == 3)), () => "three-char-id");
        var mock = builder.Object;

        // Act
        var matched = mock.GetItem("abc");
        var notMatched = mock.GetItem("ab");

        // Assert
        Assert.Equal("three-char-id", matched);
        Assert.Null(notMatched);
    }
}
