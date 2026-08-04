namespace BbQ.MockLite.Tests.Stubs;

public interface ICovariantService
{
    Task<IEnumerable<string>> GetStringsAsync();
    Task<IReadOnlyList<int>> GetIntsAsync();
}
