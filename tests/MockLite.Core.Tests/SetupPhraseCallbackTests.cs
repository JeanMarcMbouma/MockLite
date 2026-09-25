using BbQ.MockLite.Tests.Stubs;

namespace BbQ.MockLite.Tests;

// ==================== SETUPPHRASE CALLBACK TESTS ====================
public class SetupPhraseCallbackTests
{
    // --- Helper interfaces for multi-param callback tests ---
    private interface ITwoParamService
    {
        string Calculate(string a, int b);
    }

    private interface IThreeParamService
    {
        string Query(string proc, int id, int count);
    }

    // --- Parameterless Callback ---

    [Fact]
    public void Callback_Parameterless_IsInvokedOnCall()
    {
        var called = false;
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue("key"))
            .Callback(() => called = true)
            .Returns("value");

        var result = mock.Object.GetValue("key");
        Assert.True(called);
        Assert.Equal("value", result);
    }

    [Fact]
    public void Callback_Parameterless_WithoutReturns_StillFires()
    {
        var callCount = 0;
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue(It.IsAny<string>()))
            .Callback(() => callCount++);

        mock.Object.GetValue("a");
        mock.Object.GetValue("b");
        Assert.Equal(2, callCount);
    }

    // --- Raw Args Callback ---

    [Fact]
    public void Callback_RawArgs_CapturesArguments()
    {
        var captured = new List<string>();
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue(It.IsAny<string>()))
            .Callback((object?[] args) => captured.Add((string)args[0]!))
            .Returns("ok");

        mock.Object.GetValue("first");
        mock.Object.GetValue("second");

        Assert.Equal(new[] { "first", "second" }, captured);
    }

    // --- Strongly-Typed T1 Callback ---

    [Fact]
    public void Callback_StronglyTyped_T1_ReceivesFirstParam()
    {
        var captured = new List<string>();
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue(It.IsAny<string>()))
            .Callback<string>(key => captured.Add(key))
            .Returns("ok");

        mock.Object.GetValue("alpha");
        mock.Object.GetValue("beta");

        Assert.Equal(new[] { "alpha", "beta" }, captured);
    }

    // --- Strongly-Typed T1, T2 Callback ---

    [Fact]
    public void Callback_StronglyTyped_T1T2_ReceivesParams()
    {
        string? capturedA = null;
        int capturedB = 0;
        var mock = Mock.Create<ITwoParamService>();
        mock.Setup(x => x.Calculate(It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, int>((a, b) => { capturedA = a; capturedB = b; })
            .Returns("result");

        var result = mock.Object.Calculate("hello", 42);

        Assert.Equal("hello", capturedA);
        Assert.Equal(42, capturedB);
        Assert.Equal("result", result);
    }

    // --- Strongly-Typed T1, T2, T3 Callback ---

    [Fact]
    public void Callback_StronglyTyped_T1T2T3_ReceivesParams()
    {
        string? capturedProc = null;
        int capturedId = 0;
        int capturedCount = 0;
        var mock = Mock.Create<IThreeParamService>();
        mock.Setup(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback<string, int, int>((proc, id, count) =>
            {
                capturedProc = proc;
                capturedId = id;
                capturedCount = count;
            })
            .Returns("done");

        var result = mock.Object.Query("sp_test", 7, 100);

        Assert.Equal("sp_test", capturedProc);
        Assert.Equal(7, capturedId);
        Assert.Equal(100, capturedCount);
        Assert.Equal("done", result);
    }

    // --- Chaining Callback with Returns and other chains ---

    [Fact]
    public void Callback_ChainsWithReturns_BothWork()
    {
        var callCount = 0;
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetNumber(It.IsAny<int>()))
            .Callback<int>(n => callCount++)
            .Returns(99);

        Assert.Equal(99, mock.Object.GetNumber(1));
        Assert.Equal(99, mock.Object.GetNumber(2));
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Callback_ChainsWithThrows()
    {
        var called = false;
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValue("bad"))
            .Callback(() => called = true)
            .Throws(new InvalidOperationException("boom"));

        Assert.Throws<InvalidOperationException>(() => mock.Object.GetValue("bad"));
        Assert.True(called);
    }

    [Fact]
    public async Task Callback_ChainsWithReturnsAsync()
    {
        var called = false;
        var mock = Mock.Create<ITestService>();
        mock.Setup(x => x.GetValueAsync("key"))
            .Callback(() => called = true)
            .ReturnsAsync("async-result");

        var result = await mock.Object.GetValueAsync("key");
        Assert.True(called);
        Assert.Equal("async-result", result);
    }

    [Fact]
    public void MultipleCallbacksAndSetup_AllChainCorrectly()
    {
        var mock = Mock.Create<ITestService>();
        var log = new List<string>();

        mock
            .Setup(x => x.GetValue("a"))
                .Callback(() => log.Add("callback-a"))
                .Returns("A");

        mock
            .Setup(x => x.GetValue("b"))
                .Callback(() => log.Add("callback-b"))
                .Returns("B");

        Assert.Equal("A", mock.Object.GetValue("a"));
        Assert.Equal("B", mock.Object.GetValue("b"));
        // v2: callbacks attached to a setup inherit that setup's argument matcher.
        Assert.Equal(["callback-a", "callback-b"], log);
    }
}
