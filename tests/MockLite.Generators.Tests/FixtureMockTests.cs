using AutoFixture;
using BbQ.MockLite.AutoFixture;

namespace BbQ.MockLite.Generators.Tests;

[GenerateMock]
public interface IFixtureRepository
{
    string Find(string id);
}

public sealed class FixtureConsumer(IFixtureRepository repository)
{
    public IFixtureRepository Repository { get; } = repository;
}

public sealed class FixtureMockTests
{
    [Fact]
    public void GeneratedMockKeepsItsTypedSetupAndIsInjectedByReference()
    {
        var fixture = new Fixture();
        var mock = fixture.FreezeMock<IFixtureRepository, MockFixtureRepository>();
        mock.SetupFind(id => id == "123").Returns("Alice");
        int customizationCount = fixture.Customizations.Count;

        var consumer = fixture.Create<FixtureConsumer>();

        Assert.Same(mock, consumer.Repository);
        Assert.Equal("Alice", consumer.Repository.Find("123"));
        mock.VerifyFind(id => id == "123", Times.Once);
        Assert.Same(mock, fixture.Create<MockFixtureRepository>());
        Assert.Same(mock, fixture.FreezeMock<IFixtureRepository, MockFixtureRepository>());
        Assert.Equal(customizationCount, fixture.Customizations.Count);
        Assert.Throws<InvalidOperationException>(() => fixture.FreezeMock<IFixtureRepository>());
    }

    [Fact]
    public void RuntimeMockSharesItsProxyWithinOneFixture()
    {
        var fixture = new Fixture();
        var mock = fixture.FreezeMock<IFixtureRepository>();
        mock.Setup(x => x.Find("123")).Returns("Alice");
        int customizationCount = fixture.Customizations.Count;

        var consumer = fixture.Create<FixtureConsumer>();

        Assert.Same(mock.Object, consumer.Repository);
        Assert.Equal("Alice", consumer.Repository.Find("123"));
        Assert.Same(mock, fixture.Create<Mock<IFixtureRepository>>());
        Assert.Same(mock, fixture.FreezeMock<IFixtureRepository>());
        Assert.Equal(customizationCount, fixture.Customizations.Count);
        Assert.NotSame(mock, new Fixture().FreezeMock<IFixtureRepository>());
        Assert.Throws<InvalidOperationException>(
            () => fixture.FreezeMock<IFixtureRepository, MockFixtureRepository>());
    }
}
