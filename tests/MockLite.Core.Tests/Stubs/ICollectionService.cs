namespace BbQ.MockLite.Tests.Stubs;

// --- Interfaces for testing new features ---

public interface ICollectionService
{
    IEnumerable<string> GetItems();
    IReadOnlyList<int> GetNumbers();
    IList<string> GetMutableItems();
    IReadOnlyCollection<string> GetReadOnlyCollection();
    ICollection<string> GetCollection();
    Task<IEnumerable<string>> GetItemsAsync();
    Task<IReadOnlyList<int>> GetNumbersAsync();
}
