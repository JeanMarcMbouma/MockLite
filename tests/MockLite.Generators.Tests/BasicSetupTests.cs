namespace BbQ.MockLite.Generators.Tests;

public class BasicSetupTests
{
    [Fact]
    public void ParameterlessAttribute_GeneratesMockForAnnotatedInterface()
    {
        var service = new MockDirectlyAnnotatedService()
            .GetValueReturns(42);

        Assert.Equal(42, service.GetValue());
    }

    [Fact]
    public async Task TestMethod()
    {

        var svc = new MockUserService()
            .SetupGetCount(category => category.Length)            // behavior directly
            .GetUserAsyncReturns(new User("Jean"));                         // async returns

        svc.GetCount("alpha");               // 5
        var user = await svc.GetUserAsync(42);

        svc.VerifyGetCount(Times.Once);
        svc.VerifyGetUserAsync(Times.Once);

    }
}
