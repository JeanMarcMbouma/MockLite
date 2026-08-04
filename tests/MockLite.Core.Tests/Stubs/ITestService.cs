namespace BbQ.MockLite.Tests.Stubs;

public interface ITestService
{
    string GetValue(string key);
    string GetValue(string key, int count);
    string GetValue(string key, int count = 1, bool flag = true);
    int GetNumber(int input);
    void DoSomething();
    Task<string> GetValueAsync(string key);
    Task<string> GetValueAsync(string key, int count);
    Task<string> GetValueAsync(string key, int count = 1, bool flag = true);
    Task DoSomethingAsync();
}
