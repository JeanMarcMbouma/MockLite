using BbQ.MockLite;

namespace BbQ.MockLite.Generators.Tests;

[GenerateMock<IUserService>]
[GenerateMock<ICompositeService>]
[GenerateMock<ICollectionReturningService>]
[GenerateMock<IGenericMethodService>]
public partial class Mocks { }

public record User(string Username);
public interface IUserService
{
    string Name { get; set; }
    int GetCount(string category);
    Task<User> GetUserAsync(int id);
}

// --- Composite interface hierarchy for testing ---
public interface IReadable
{
    string Read(string key);
}

public interface IWritable
{
    void Write(string key, string value);
}

public interface ICompositeService : IReadable, IWritable
{
    int GetVersion();
}

// --- Collection-returning interface for smart defaults ---
public interface ICollectionReturningService
{
    IEnumerable<string> GetItems();
    Task<IEnumerable<string>> GetItemsAsync();
    IReadOnlyList<int> GetNumbers();
}

// --- Generic method interface for testing ---
public interface IGenericMethodService
{
    TResult Send<TResult>(string command);
    Task<TResult> SendAsync<TResult>(string command);
    void Execute<TItem>(TItem item);
    T Get<T>(string key) where T : class;
    int NonGenericMethod(string input);
}
