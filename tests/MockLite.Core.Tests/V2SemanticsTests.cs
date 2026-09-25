using BbQ.MockLite;

namespace BbQ.MockLite.Tests;

public class V2SemanticsTests
{
    private interface IService
    {
        string Get(string key);
        void Save(string key);
    }

    [Fact]
    public void Verify_UsesExactExpressionArguments()
    {
        var mock = Mock.Create<IService>();
        mock.Object.Get("actual");

        Assert.Throws<VerificationException>(() => mock.Verify(x => x.Get("expected"), Times.Once));
        mock.Verify(x => x.Get("actual"), Times.Once);
    }

    [Fact]
    public void Verify_UsesItMatchersFromExpression()
    {
        var mock = Mock.Create<IService>();
        mock.Object.Get("admin-1");
        mock.Object.Get("user-1");

        mock.Verify(x => x.Get(It.Matches<string>(x => x.StartsWith("admin"))), Times.Once);
        mock.Verify(x => x.Get(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void VerifyVoid_UsesExpressionArguments()
    {
        var mock = Mock.Create<IService>();
        mock.Object.Save("one");
        mock.Object.Save("two");

        mock.Verify(x => x.Save("one"), Times.Once);
        mock.Verify(x => x.Save(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void VerifyAnyArguments_IsExplicitMethodWideCount()
    {
        var mock = Mock.Create<IService>();
        mock.Object.Get("one");
        mock.Object.Get("two");

        mock.VerifyAnyArguments(x => x.Get("ignored"), Times.Exactly(2));
    }

    [Fact]
    public void ConcurrentSetupRegistration_AndInvocation_DoesNotCorruptState()
    {
        var mock = Mock.Create<IService>();

        Parallel.For(0, 100, i =>
            mock.Setup(x => x.Get(i.ToString()), () => i.ToString()));

        Parallel.For(0, 100, i =>
            Assert.Equal(i.ToString(), mock.Object.Get(i.ToString())));

        Assert.Equal(100, mock.Invocations.Count);
    }

    [Fact]
    public void SetupPhraseCallback_UsesSetupArgumentMatcher()
    {
        var calls = new List<string>();
        var mock = Mock.Create<IService>();
        mock.Setup(x => x.Get("match"))
            .Callback<string>(calls.Add)
            .Returns("ok");

        mock.Object.Get("other");
        mock.Object.Get("match");

        Assert.Equal(new[] { "match" }, calls);
    }
}
