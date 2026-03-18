namespace BbQ.MockLite.Tests;

/// <summary>
/// Tests for generic method wildcard matching: using <c>object</c> as a type parameter
/// in Setup/Verify/OnCall should match actual calls that use any type parameter.
/// This covers the case where T is a private nested class inside a handler and the
/// test cannot reference it.
/// </summary>
public class GenericMethodWildcardTests
{
    private interface IQueryService
    {
        TResult Query<TResult>(string sql);
        void Execute<TParam>(TParam param);
        TResult Transform<TParam, TResult>(TParam input);
    }

    // Simulates a private nested class that tests cannot reference.
    private sealed class PrivateResult
    {
        public int Value { get; init; }
    }

    private sealed class AnotherPrivate
    {
        public string Name { get; init; } = "";
    }

    // --- Verify Tests ---

    [Fact]
    public void Verify_GenericMethod_ObjectWildcard_MatchesDifferentTypeArg()
    {
        var builder = Mock.Create<IQueryService>();
        var mock = builder.Object;

        // Actual call uses a type the test "can't reference" in real scenario
        mock.Query<PrivateResult>("SELECT 1");

        // Verify using object as wildcard — should match
        builder.Verify(x => x.Query<object>("SELECT 1"), Times.Once);
    }

    [Fact]
    public void Verify_GenericMethod_ObjectWildcard_MatchesMultipleDifferentTypes()
    {
        var builder = Mock.Create<IQueryService>();
        var mock = builder.Object;

        mock.Query<PrivateResult>("q1");
        mock.Query<AnotherPrivate>("q2");
        mock.Query<int>("q3");

        // object wildcard matches all three
        builder.Verify(x => x.Query<object>(It.IsAny<string>()), times => times == 3);
    }

    [Fact]
    public void Verify_GenericMethod_ExactTypeArg_DoesNotMatchDifferentType()
    {
        var builder = Mock.Create<IQueryService>();
        var mock = builder.Object;

        mock.Query<PrivateResult>("q1");

        // Verifying with string (not object) should NOT match PrivateResult
        Assert.Throws<VerificationException>(() =>
            builder.Verify(x => x.Query<string>("q1"), Times.Once));
    }

    [Fact]
    public void Verify_GenericMethod_ExactTypeArg_StillMatchesExact()
    {
        var builder = Mock.Create<IQueryService>();
        var mock = builder.Object;

        mock.Query<int>("q1");

        // Exact type should still work
        builder.Verify(x => x.Query<int>("q1"), Times.Once);
    }

    [Fact]
    public void Verify_GenericVoidMethod_ObjectWildcard()
    {
        var builder = Mock.Create<IQueryService>();
        var mock = builder.Object;

        mock.Execute(new PrivateResult { Value = 42 });

        // Verify void generic method with object wildcard
        builder.Verify(x => x.Execute<object>(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void Verify_GenericMethod_WithMatcher_ObjectWildcard()
    {
        var builder = Mock.Create<IQueryService>();
        var mock = builder.Object;

        mock.Query<PrivateResult>("SELECT users");
        mock.Query<PrivateResult>("SELECT orders");

        // Verify with matcher + object wildcard
        builder.Verify(
            x => x.Query<object>(It.IsAny<string>()),
            args => args[0] is string s && s.Contains("users"),
            Times.Once);
    }

    [Fact]
    public void Verify_MultiTypeParamMethod_ObjectWildcard_MatchesBothPositions()
    {
        var builder = Mock.Create<IQueryService>();
        var mock = builder.Object;

        mock.Transform<PrivateResult, AnotherPrivate>(new PrivateResult { Value = 1 });

        // Both type args use object wildcard
        builder.Verify(x => x.Transform<object, object>(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void Verify_MultiTypeParamMethod_PartialWildcard()
    {
        var builder = Mock.Create<IQueryService>();
        var mock = builder.Object;

        mock.Transform<string, AnotherPrivate>("hello");

        // Only second type arg is wildcard; first is exact
        builder.Verify(x => x.Transform<string, object>("hello"), Times.Once);
    }

    // --- Setup Tests ---

    [Fact]
    public void Setup_GenericMethod_ObjectWildcard_ReturnsBehavior()
    {
        var builder = Mock.Create<IQueryService>();
        // Setup with object wildcard
        builder.Setup(x => x.Query<object>(It.IsAny<string>()), () => (object)new PrivateResult { Value = 99 });

        var mock = builder.Object;

        // Call with the private type — should use the wildcard setup
        var result = mock.Query<PrivateResult>("anything");

        // The wildcard setup returns an object, so the proxy returns it
        Assert.NotNull(result);
    }

    [Fact]
    public void Setup_GenericMethod_ExactTypeWins_OverWildcard()
    {
        var builder = Mock.Create<IQueryService>();
        // Wildcard setup
        builder.Setup(x => x.Query<object>(It.IsAny<string>()), () => (object)"wildcard-fallback");
        // Exact setup for int
        builder.Setup(x => x.Query<int>(It.IsAny<string>()), () => 42);

        var mock = builder.Object;

        // int call should use exact setup
        var intResult = mock.Query<int>("q");
        Assert.Equal(42, intResult);
    }

    // --- OnCall Tests ---

    [Fact]
    public void OnCall_GenericMethod_ObjectWildcard_FiresCallback()
    {
        var callCount = 0;
        var builder = Mock.Create<IQueryService>();

        builder.OnCall(x => x.Query<object>(It.IsAny<string>()), _ => callCount++);

        var mock = builder.Object;
        mock.Query<PrivateResult>("q1");
        mock.Query<AnotherPrivate>("q2");

        Assert.Equal(2, callCount);
    }

    [Fact]
    public void OnCall_GenericMethod_WithMatcher_ObjectWildcard()
    {
        var captured = new List<string>();
        var builder = Mock.Create<IQueryService>();

        builder.OnCall(
            x => x.Query<object>(It.IsAny<string>()),
            args => args[0] is string s && s.StartsWith("admin"),
            args => captured.Add((string)args[0]!));

        var mock = builder.Object;
        mock.Query<PrivateResult>("admin-query");
        mock.Query<PrivateResult>("user-query");

        Assert.Single(captured);
        Assert.Equal("admin-query", captured[0]);
    }
}
