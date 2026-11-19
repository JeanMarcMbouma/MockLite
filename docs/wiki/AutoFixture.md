# Use MockLite with AutoFixture

The optional `BbQ.MockLite.AutoFixture` package is part of the upcoming MockLite 2.0.1 release ([PR #28](https://github.com/JeanMarcMbouma/MockLite/pull/28)). Once published, install it in your test project.

```bash
dotnet add package BbQ.MockLite.AutoFixture --version 2.0.1
```

## Freeze a generated mock

Add `BbQ.MockLite.Generators` to the test project and declare `[GenerateMock<IUserRepository>]` on a partial class as shown in [[Getting Started]]. Freeze the generated mock before creating the service. AutoFixture passes the same repository instance to the service constructor, and the returned concrete type exposes its generated setup and verification methods.

```csharp
using AutoFixture;
using BbQ.MockLite;
using BbQ.MockLite.AutoFixture;

var fixture = new Fixture();
var repository = fixture.FreezeMock<IUserRepository, MockUserRepository>();
repository.SetupGetUser(id => id == "123").Returns(expectedUser);

var service = fixture.Create<UserService>();
// Exercise the service before verifying the call.
repository.VerifyGetUser(id => id == "123", Times.Once);
```

## Freeze a runtime mock

Use the single-type overload with an interface when you want expression-based setup. It returns `Mock<IUserRepository>` and registers its `.Object` for requests for `IUserRepository`.

```csharp
var fixture = new Fixture();
var repository = fixture.FreezeMock<IUserRepository>();
repository.Setup(x => x.GetUser("123")).Returns(expectedUser);

var service = fixture.Create<UserService>();
```

Repeating the same freeze on one fixture returns the original mock with its setups and recorded calls. Use a fresh fixture for each test. Freezing affects future requests, and a conflicting MockLite registration for either of the same types throws. Avoid adding a fixture customization that shadows a frozen type.
