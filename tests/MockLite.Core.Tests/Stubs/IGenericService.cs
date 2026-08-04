namespace BbQ.MockLite.Tests.Stubs;

public interface IGenericService<T>
{
    T GetItem(string id);
    void SetItem(T item);
}
