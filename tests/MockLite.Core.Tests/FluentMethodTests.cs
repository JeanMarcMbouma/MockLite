using System;
using System.Collections.Generic;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Tests for new fluent methods: Throws, SetupSequence, Reset, Verify (void), and Times.Between.
/// </summary>
public class FluentMethodTests
{
    // ==================== THROWS TESTS ====================

    [Fact]
    public void Test_Throws_ThrowsExceptionOnMatchingCall()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .Throws(x => x.GetValue("bad-key"), new KeyNotFoundException("not found"));

        // Act & Assert
        var ex = Assert.Throws<KeyNotFoundException>(() => mock.Object.GetValue("bad-key"));
        Assert.Equal("not found", ex.Message);
    }

    [Fact]
    public void Test_Throws_DoesNotThrowOnNonMatchingCall()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .Throws(x => x.GetValue("bad-key"), new KeyNotFoundException("not found"));

        // Act - different arg should not throw
        var result = mock.Object.GetValue("good-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Test_Throws_WithIsAny_ThrowsOnEveryCall()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .Throws(x => x.GetValue(It.IsAny<string>()), new InvalidOperationException("service down"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mock.Object.GetValue("any-key"));
        Assert.Throws<InvalidOperationException>(() => mock.Object.GetValue("another-key"));
    }

    [Fact]
    public void Test_Throws_VoidMethod_ThrowsException()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .Throws(x => x.DoSomething(), new InvalidOperationException("cannot do"));

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => mock.Object.DoSomething());
        Assert.Equal("cannot do", ex.Message);
    }

    [Fact]
    public void Test_Throws_ReturnsSelfForChaining()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();

        // Act
        var result = mock.Throws(x => x.GetValue("key"), new Exception());

        // Assert
        Assert.Same(mock, result);
    }

    [Fact]
    public void Test_Throws_VoidMethod_ReturnsSelfForChaining()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();

        // Act
        var result = mock.Throws(x => x.DoSomething(), new Exception());

        // Assert
        Assert.Same(mock, result);
    }

    [Fact]
    public void Test_Throws_RecordsInvocationBeforeThrowing()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .Throws(x => x.GetValue("key"), new Exception());

        // Act
        try { mock.Object.GetValue("key"); } catch { }

        // Assert - invocation should still be recorded
        Assert.Single(mock.Invocations);
    }

    [Fact]
    public void Test_Throws_CombinedWithSetup()
    {
        // Arrange - setup a normal return, then throw for specific arg
        var mock = Mock.Create<ITestService>()
            .Setup(x => x.GetValue(It.IsAny<string>()), () => "default")
            .Throws(x => x.GetValue("bad"), new InvalidOperationException("bad key"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mock.Object.GetValue("bad"));
        Assert.Equal("default", mock.Object.GetValue("good"));
    }

    // ==================== SETUP SEQUENCE TESTS ====================

    [Fact]
    public void Test_SetupSequence_ReturnsDifferentValuesOnSuccessiveCalls()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .SetupSequence(x => x.GetNumber(1), 10, 20, 30);

        // Act & Assert
        Assert.Equal(10, mock.Object.GetNumber(1));
        Assert.Equal(20, mock.Object.GetNumber(1));
        Assert.Equal(30, mock.Object.GetNumber(1));
    }

    [Fact]
    public void Test_SetupSequence_RepeatsLastValueWhenExhausted()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .SetupSequence(x => x.GetNumber(1), 10, 20);

        // Act - exhaust the sequence
        mock.Object.GetNumber(1); // 10
        mock.Object.GetNumber(1); // 20

        // Assert - should repeat last value
        Assert.Equal(20, mock.Object.GetNumber(1));
        Assert.Equal(20, mock.Object.GetNumber(1));
    }

    [Fact]
    public void Test_SetupSequence_SingleValue()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .SetupSequence(x => x.GetNumber(1), 42);

        // Act & Assert
        Assert.Equal(42, mock.Object.GetNumber(1));
        Assert.Equal(42, mock.Object.GetNumber(1));
    }

    [Fact]
    public void Test_SetupSequence_WithIsAny()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .SetupSequence(x => x.GetValue(It.IsAny<string>()), "first", "second", "third");

        // Act & Assert
        Assert.Equal("first", mock.Object.GetValue("a"));
        Assert.Equal("second", mock.Object.GetValue("b"));
        Assert.Equal("third", mock.Object.GetValue("c"));
    }

    [Fact]
    public void Test_SetupSequence_ReturnsSelfForChaining()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();

        // Act
        var result = mock.SetupSequence(x => x.GetNumber(1), 1, 2, 3);

        // Assert
        Assert.Same(mock, result);
    }

    [Fact]
    public void Test_SetupSequence_EmptyValuesThrows()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            mock.SetupSequence(x => x.GetNumber(1), Array.Empty<int>()));
    }

    // ==================== RESET TESTS ====================

    [Fact]
    public void Test_Reset_ClearsInvocations()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();
        mock.Object.GetValue("test");
        mock.Object.GetNumber(42);
        Assert.Equal(2, mock.Invocations.Count);

        // Act
        mock.Reset();

        // Assert
        Assert.Empty(mock.Invocations);
    }

    [Fact]
    public void Test_Reset_PreservesSetups()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .Setup(x => x.GetNumber(1), () => 42);
        mock.Object.GetNumber(1);

        // Act
        mock.Reset();

        // Assert - setup should still work
        Assert.Equal(42, mock.Object.GetNumber(1));
    }

    [Fact]
    public void Test_Reset_PreservesCallbacks()
    {
        // Arrange
        var callCount = 0;
        var mock = Mock.Create<ITestService>()
            .OnCall(x => x.DoSomething(), _ => callCount++);
        mock.Object.DoSomething();
        Assert.Equal(1, callCount);

        // Act
        mock.Reset();
        mock.Object.DoSomething();

        // Assert - callback should still fire
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Test_Reset_ReturnsSelfForChaining()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();

        // Act
        var result = mock.Reset();

        // Assert
        Assert.Same(mock, result);
    }

    [Fact]
    public void Test_Reset_AllowsFreshVerification()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();
        mock.Object.GetValue("test");
        mock.Verify(x => x.GetValue(It.IsAny<string>()), Times.Once);

        // Act
        mock.Reset();

        // Assert - now verifying zero calls should pass
        mock.Verify(x => x.GetValue(It.IsAny<string>()), Times.Never);
    }

    // ==================== VERIFY VOID METHOD TESTS ====================

    [Fact]
    public void Test_Verify_VoidMethod_VerifiesCallCount()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();
        mock.Object.DoSomething();
        mock.Object.DoSomething();

        // Act & Assert
        mock.Verify(x => x.DoSomething(), Times.Exactly(2));
    }

    [Fact]
    public void Test_Verify_VoidMethod_ThrowsWhenNotMet()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();
        mock.Object.DoSomething();

        // Act & Assert
        Assert.Throws<VerificationException>(() =>
            mock.Verify(x => x.DoSomething(), Times.Exactly(2)));
    }

    [Fact]
    public void Test_Verify_VoidMethod_TimesNever()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();

        // Act & Assert - should not throw
        mock.Verify(x => x.DoSomething(), Times.Never);
    }

    [Fact]
    public void Test_Verify_VoidMethod_TimesOnce()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();
        mock.Object.DoSomething();

        // Act & Assert
        mock.Verify(x => x.DoSomething(), Times.Once);
    }

    // ==================== TIMES.BETWEEN TESTS ====================

    [Fact]
    public void Test_TimesBetween_WithinRange()
    {
        // Act
        var predicate = Times.Between(2, 5);

        // Assert
        Assert.False(predicate(1));
        Assert.True(predicate(2));
        Assert.True(predicate(3));
        Assert.True(predicate(4));
        Assert.True(predicate(5));
        Assert.False(predicate(6));
    }

    [Fact]
    public void Test_TimesBetween_WithMock()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();
        mock.Object.DoSomething();
        mock.Object.DoSomething();
        mock.Object.DoSomething();

        // Act & Assert - 3 calls is between 2 and 5
        mock.Verify(x => x.DoSomething(), Times.Between(2, 5));
    }

    [Fact]
    public void Test_TimesBetween_FailsOutsideRange()
    {
        // Arrange
        var mock = Mock.Create<ITestService>();
        mock.Object.DoSomething();

        // Act & Assert - 1 call is not between 2 and 5
        Assert.Throws<VerificationException>(() =>
            mock.Verify(x => x.DoSomething(), Times.Between(2, 5)));
    }

    [Fact]
    public void Test_TimesBetween_ThrowsWhenMinGreaterThanMax()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Times.Between(5, 2));
    }

    // ==================== INTEGRATION TESTS ====================

    [Fact]
    public void Test_FluentChaining_AllNewMethods()
    {
        // Arrange & Act - chain all new methods
        var mock = Mock.Create<ITestService>()
            .Setup(x => x.GetNumber(1), () => 100)
            .Throws(x => x.GetValue("error"), new InvalidOperationException())
            .SetupSequence(x => x.GetNumber(2), 10, 20, 30)
            .OnCall(x => x.DoSomething(), _ => { });

        // Assert
        Assert.NotNull(mock);
        Assert.Equal(100, mock.Object.GetNumber(1));
        Assert.Throws<InvalidOperationException>(() => mock.Object.GetValue("error"));
        Assert.Equal(10, mock.Object.GetNumber(2));
        Assert.Equal(20, mock.Object.GetNumber(2));
    }

    [Fact]
    public void Test_SetupSequence_ThenReset_ThenVerify()
    {
        // Arrange
        var mock = Mock.Create<ITestService>()
            .SetupSequence(x => x.GetNumber(1), 10, 20, 30);

        // Act - consume some values
        mock.Object.GetNumber(1);
        mock.Object.GetNumber(1);

        // Reset invocations
        mock.Reset();

        // Verify fresh state
        mock.Verify(x => x.GetNumber(It.IsAny<int>()), Times.Never);
    }
}
