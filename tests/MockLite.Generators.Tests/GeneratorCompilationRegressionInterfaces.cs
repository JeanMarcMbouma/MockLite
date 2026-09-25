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
        T? Find(string key);
    }
}
