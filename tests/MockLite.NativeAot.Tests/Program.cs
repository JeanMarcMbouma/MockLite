using BbQ.MockLite;

var mock = new MockPriceFeed();
mock.SetupGetPrice(sku => sku == "A").Returns(19.95m);
IPriceFeed feed = mock;
if (feed.GetPrice("A") != 19.95m)
    throw new Exception("Generated mock returned the wrong price.");
mock.VerifyGetPrice(sku => sku == "A", Times.Once);

if (Mock.CreateGenerated<IPriceFeed>() is not MockPriceFeed)
    throw new Exception("Generated factory did not resolve the registered implementation.");

try
{
    Mock.CreateGenerated<IUnregistered>();
}
catch (InvalidOperationException)
{
    Console.WriteLine("Native AOT smoke test passed.");
    return;
}

throw new Exception("An unregistered contract should fail explicitly.");

[GenerateMock<IPriceFeed>]
public partial class TestDoubles { }

public interface IPriceFeed
{
    decimal GetPrice(string sku);
}

public interface IUnregistered { }
