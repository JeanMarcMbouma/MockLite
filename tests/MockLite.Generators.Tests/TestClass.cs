using MockLite;

namespace MockLite.Generators.Tests;

[GenerateMock<IUserService>]
public partial class Mocks { }

public record User(string Username);
public interface IUserService
{
    string Name { get; set; }
    int GetCount(string category);
    Task<User> GetUserAsync(int id);
}

public class TestClass
{
    [Fact]
    public async Task TestMethod()
    {

        var svc = new MockUserService()
            .SetupGetCount(category => category.Length)            // behavior directly
            .ReturnsGetUserAsync(new User("Jean"));                         // async returns

        svc.GetCount("alpha");               // 5
        var user = await svc.GetUserAsync(42);

        svc.VerifyGetCount(Times.Once);
        svc.VerifyGetUserAsync(Times.Once);

    }
}