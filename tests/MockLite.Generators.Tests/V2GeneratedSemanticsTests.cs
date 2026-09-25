namespace BbQ.MockLite.Generators.Tests;

public class V2GeneratedSemanticsTests
{
    [Fact]
    public void MatcherSetups_Coexist_AndNewestMatchingSetupWins()
    {
        var mock = new MockUserService();

        mock.SetupLookup(key => true, key => "fallback");
        mock.SetupLookup(key => key.StartsWith("admin"), key => "admin");

        Assert.Equal("fallback", mock.Lookup("user-1"));
        Assert.Equal("admin", mock.Lookup("admin-1"));
    }

    [Fact]
    public void ConcurrentSetupRegistration_DoesNotCorruptGeneratedMock()
    {
        var mock = new MockUserService();

        Parallel.For(0, 100, i =>
            mock.SetupLookup(key => key == i.ToString(), key => key));

        Parallel.For(0, 100, i =>
            Assert.Equal(i.ToString(), mock.Lookup(i.ToString())));
    }

    [Fact]
    public void LaterBroadSetup_WinsWhenBothMatch()
    {
        var mock = new MockUserService();

        mock.SetupLookup(key => key.StartsWith("admin"), key => "specific");
        mock.SetupLookup(key => true, key => "latest");

        Assert.Equal("latest", mock.Lookup("admin-1"));
    }
}
