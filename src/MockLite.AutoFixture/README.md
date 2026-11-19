# MockLite with AutoFixture

Freeze a mock before creating the service under test. AutoFixture then uses that
same instance whenever it needs the dependency.

```csharp
using AutoFixture;
using BbQ.MockLite;
using BbQ.MockLite.AutoFixture;

var fixture = new Fixture();
var repository = fixture.FreezeMock<IUserRepository, MockUserRepository>();

repository.SetupGetUser(id => id == "123").Returns(expectedUser);
var service = fixture.Create<UserService>();
```

Generate `MockUserRepository` with `[GenerateMock<IUserRepository>]` and
`BbQ.MockLite.Generators`. The extension returns the concrete type so its
generated setup and verification methods are available in IntelliSense.

For an interface without a generated mock, use the runtime overload.

```csharp
var repository = fixture.FreezeMock<IUserRepository>();
repository.Setup(x => x.GetUser("123")).Returns(expectedUser);
var service = fixture.Create<UserService>();
```

Each fixture owns its registrations. Repeating the same freeze returns the
existing mock, including its setups and call history. A different MockLite
registration for either of the same types throws. Use a fresh fixture per
test, and avoid adding fixture customizations that shadow a frozen type.
Freezing affects subsequent requests, including constructor dependencies.
