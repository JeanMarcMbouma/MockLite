using BbQ.MockLite.Tests.Stubs;

namespace BbQ.MockLite.Tests;

// ==================== VERIFY WITH MESSAGE TESTS ====================
public class VerifyWithMessageTests
{
    [Fact]
    public void Verify_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<ITestService>();
        mock.Object.GetValue("key");

        var ex = Assert.Throws<VerificationException>(() =>
            mock.Verify(x => x.GetValue("key"), Times.Exactly(2), "Expected two calls"));

        Assert.Contains("Expected two calls", ex.Message);
    }

    [Fact]
    public void Verify_WithoutMessage_WorksAsUsual()
    {
        var mock = Mock.Create<ITestService>();
        mock.Object.GetValue("key");

        // No message — should still work
        mock.Verify(x => x.GetValue("key"), Times.Once);
    }

    [Fact]
    public void VerifyVoid_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<ITestService>();

        var ex = Assert.Throws<VerificationException>(() =>
            mock.Verify(x => x.DoSomething(), Times.Once, "Expected one call"));

        Assert.Contains("Expected one call", ex.Message);
    }

    [Fact]
    public void VerifyGet_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<IPropertyService>();

        var ex = Assert.Throws<VerificationException>(() =>
            mock.VerifyGet(x => x.Name, Times.Once, "Should have read Name"));

        Assert.Contains("Should have read Name", ex.Message);
    }

    [Fact]
    public void VerifySet_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<IPropertyService>();

        var ex = Assert.Throws<VerificationException>(() =>
            mock.VerifySet(x => x.Name, Times.Once, "Should have written Name"));

        Assert.Contains("Should have written Name", ex.Message);
    }

    [Fact]
    public void Verify_WithMatcher_WithMessage_IncludesMessageInException()
    {
        var mock = Mock.Create<ITestService>();
        mock.Object.GetValue("wrong-key");

        var ex = Assert.Throws<VerificationException>(() =>
            mock.Verify(
                x => x.GetValue("specific-key"),
                args => (string?)args[0] == "specific-key",
                Times.Once,
                "Expected call with specific-key"));

        Assert.Contains("Expected call with specific-key", ex.Message);
    }
}
