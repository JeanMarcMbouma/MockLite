using BbQ.MockLite;

namespace BbQ.MockLite.Generators.Tests.RegressionA
{
    [GenerateMock]
    public interface IStore
    {
        int Count { get; set; }
        string Find(int id);
        string Find(string key);
    }
}

namespace BbQ.MockLite.Generators.Tests.RegressionB
{
    [GenerateMock]
    public interface IStore
    {
        bool Enabled { get; set; }
    }
}

namespace BbQ.MockLite.Generators.Tests.GenericRegression
{
    [GenerateMock]
    public interface IRepository<T>
    {
        T Find(string key);
    }

    [GenerateMock]
    public interface IConstrainedRepository<T> where T : class, new()
    {
        T Create();
    }
}


namespace BbQ.MockLite.Generators.Tests.ExternalContracts
{
    public sealed record ExternalDto(string Value);
}

namespace BbQ.MockLite.Generators.Tests.ExternalConsumer
{
    [GenerateMock]
    public interface IExternalService
    {
        ExternalContracts.ExternalDto Load(ExternalContracts.ExternalDto value);
    }
}
