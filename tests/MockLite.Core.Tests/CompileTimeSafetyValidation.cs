// This file demonstrates compile-time type safety.
// Uncomment the code blocks to see compilation errors.

using BbQ.MockLite;

namespace BbQ.MockLite.Tests.CompileTimeSafety;

public interface IQueryService
{
    Action<int, int> Query(string procedure, int param1, int param2);
    Func<string, int> GetTransform(string name);
}

public class CompileTimeSafetyValidation
{
    public void ValidSetup()
    {
        var builder = Mock.Create<IQueryService>();
        
        // This is VALID - types match
        builder.Setup(
            x => x.Query("proc", 1, 2),
            (int a, int b) => Console.WriteLine(a + b)
        );
    }

    /* 
    // Uncomment this to see compile-time error
    public void InvalidSetup_WrongParameterTypes()
    {
        var builder = Mock.Create<IQueryService>();
        
        // This would cause COMPILE ERROR - parameter types don't match
        builder.Setup(
            x => x.Query("proc", 1, 2),
            (string a, string b) => Console.WriteLine(a + b)  // ERROR!
        );
    }
    */

    /* 
    // Uncomment this to see compile-time error
    public void InvalidSetup_WrongParameterCount()
    {
        var builder = Mock.Create<IQueryService>();
        
        // This would cause COMPILE ERROR - wrong number of parameters
        builder.Setup(
            x => x.Query("proc", 1, 2),
            (int a) => Console.WriteLine(a)  // ERROR!
        );
    }
    */

    /* 
    // Uncomment this to see compile-time error
    public void InvalidSetup_WrongFuncReturnType()
    {
        var builder = Mock.Create<IQueryService>();
        
        // This would cause COMPILE ERROR - return type doesn't match
        builder.Setup(
            x => x.GetTransform("test"),
            (string s) => s.ToUpper()  // ERROR! Returns string, not int
        );
    }
    */
}
