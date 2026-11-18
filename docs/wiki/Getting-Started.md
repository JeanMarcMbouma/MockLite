# Getting started


Add both packages to the project that will compile your generated mocks:

```bash
dotnet add package BbQ.MockLite
dotnet add package BbQ.MockLite.Generators
```

The core package contains the shared runtime; the generator package supplies source generation, analyzers and code fixes. The repository builds with .NET 8 and .NET 10 SDKs; the generator targets .NET 8. Use a current SDK and C# language version supported by your project.

### 1. Select a contract

Keep the generation declaration in your test project. This also works for interfaces from referenced assemblies, so you can leave application contracts untouched:

```csharp
using BbQ.MockLite;

[GenerateMock<IPriceFeed>]
public partial class TestDoubles { }

// Example contract; normally imported from your application.
public interface IPriceFeed
{
    decimal GetPrice(string sku);
}
```

Build the project. MockLite generates `MockPriceFeed` in the interface's namespace. For an interface you want to annotate directly, `[GenerateMock]` on that interface is also supported.

### 2. Describe the scenario

```csharp
var prices = new MockPriceFeed();

prices.SetupGetPrice(sku => sku == "SKU-42")
    .Callback(sku => Console.WriteLine($"Looked up {sku}"))
    .Returns(19.95m);
```

`sku` is a `string`, inferred from the contract. The predicate filters both the callback and the configured return behavior. The setup API is generated for this method, so incompatible callback signatures and return types receive C# compiler feedback.

### 3. Exercise the behavior and verify it

```csharp
IPriceFeed feed = prices; // Pass this interface to your system under test.
decimal price = feed.GetPrice("SKU-42"); // 19.95m

prices.VerifyGetPrice(sku => sku == "SKU-42", Times.Once);
```

In a real test, call the service that consumes `IPriceFeed`, assert its result with your test framework, then verify the interaction. Construct `MockPriceFeed` directly to access its generated setup and verification members; `Mock.Of<IPriceFeed>()` returns the interface type.


Continue with [[Source Generators]] or the [[API Reference|API-Reference]].
