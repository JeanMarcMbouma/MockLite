using MockLite;

// MockLite Sample - Demonstrating Mock Creation and Verification
// This sample shows how to use MockLite for creating and testing mocks of interfaces.

namespace MockLite.Samples;

// ============================================================================
// Sample 1: Basic Mock Creation and Setup
// ============================================================================

// Define your interfaces with the GenerateMock attribute
[GenerateMock(typeof(IUserRepository))]
public interface IUserRepository
{
    User? GetUser(string userId);
    void SaveUser(User user);
    bool DeleteUser(string userId);
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class BasicMockExample
{
    public static void RunBasicExample()
    {
        Console.WriteLine("=== Basic Mock Creation Example ===\n");

        // Create a mock instance
        var userRepo = new MockUserRepository();
        
        // Setup what the mock should return
        User? testUser = new User { Id = "123", Name = "John Doe", Email = "john@example.com" };
        
        // For generated mocks, you would call methods like:
        userRepo.SetupGetUser(userId => testUser);
        // For now, we'll demonstrate the concept

        var retrievedUser = userRepo.GetUser("123");

        Console.WriteLine($"Mock created for IUserRepository");
        Console.WriteLine($"Type: {userRepo.GetType().Name}");
        Console.WriteLine($"✓ Mock instance created successfully\n");
    }
}

// ============================================================================
// Sample 2: Argument Matchers
// ============================================================================

[GenerateMock(typeof(ILogger))]
public interface ILogger
{
    void Log(string level, string message);
    void LogError(string message);
}

public class ArgumentMatcherExample
{
    public static void RunArgumentMatcherExample()
    {
        Console.WriteLine("=== Argument Matcher Example ===\n");

        var logger = Mock.Of<ILogger>();

        // Use It.IsAny to match any string argument
        var anyString = It.IsAny<string>();
        var anyLevel = It.IsAny<string>();
        var anyMessage = It.IsAny<string>();

        Console.WriteLine("Setting up logger with argument matchers:");
        Console.WriteLine($"  It.IsAny<string>() - matches any string value");
        Console.WriteLine($"  It.Matches<T>(predicate) - matches values satisfying predicate\n");

        // Demonstrate matcher usage concept
        Console.WriteLine("Example: Verify method called with matching arguments:");
        Console.WriteLine("  logger.VerifyLog(It.IsAny<string>(), Times.AtLeast(1));");
        Console.WriteLine("  logger.VerifyLogError(It.Matches<string>(msg => msg.Contains(\"error\")), Times.Once);\n");
    }
}

// ============================================================================
// Sample 3: Times Predicates
// ============================================================================

[GenerateMock(typeof(IEmailService))]
public interface IEmailService
{
    void SendEmail(string to, string subject, string body);
}

public class TimesPredicateExample
{
    public static void RunTimesPredicateExample()
    {
        Console.WriteLine("=== Times Predicates Example ===\n");

        Console.WriteLine("Available Times predicates for verification:\n");
        
        // Demonstrate each Times predicate
        var once = Times.Once;
        var never = Times.Never;
        var exactly3 = Times.Exactly(3);
        var atLeast2 = Times.AtLeast(2);
        var atMost5 = Times.AtMost(5);

        Console.WriteLine($"  Times.Once           - exactly 1 call");
        Console.WriteLine($"  Times.Never          - exactly 0 calls");
        Console.WriteLine($"  Times.Exactly(n)     - exactly n calls");
        Console.WriteLine($"  Times.AtLeast(n)     - at least n calls");
        Console.WriteLine($"  Times.AtMost(n)      - at most n calls\n");

        // Test the predicates with example values
        Console.WriteLine("Testing predicates with example call counts:\n");
        Console.WriteLine("Call count = 0:");
        Console.WriteLine($"  Times.Never(0) = {never(0)}");
        Console.WriteLine($"  Times.Once(0) = {once(0)}");

        Console.WriteLine("\nCall count = 1:");
        Console.WriteLine($"  Times.Once(1) = {once(1)}");
        Console.WriteLine($"  Times.Exactly(3)(1) = {exactly3(1)}");
        Console.WriteLine($"  Times.AtLeast(2)(1) = {atLeast2(1)}");

        Console.WriteLine("\nCall count = 3:");
        Console.WriteLine($"  Times.Exactly(3)(3) = {exactly3(3)}");
        Console.WriteLine($"  Times.AtLeast(2)(3) = {atLeast2(3)}");
        Console.WriteLine($"  Times.AtMost(5)(3) = {atMost5(3)}\n");
    }
}

// ============================================================================
// Sample 4: Verification and Exception Handling
// ============================================================================

[GenerateMock(typeof(IPaymentGateway))]
public interface IPaymentGateway
{
    bool ProcessPayment(string cardNumber, decimal amount);
}

public class VerificationExample
{
    public static void RunVerificationExample()
    {
        Console.WriteLine("=== Verification and Exception Handling ===\n");

        var paymentGateway = Mock.Of<IPaymentGateway>();

        Console.WriteLine("Verification in MockLite:\n");
        Console.WriteLine("Successful verification example:");
        Console.WriteLine("  mock.VerifyProcessPayment(Times.Once);");
        Console.WriteLine("  // Passes if ProcessPayment was called exactly once\n");

        Console.WriteLine("Failed verification example:");
        Console.WriteLine("  mock.VerifyProcessPayment(Times.Exactly(5));");
        Console.WriteLine("  // Throws VerificationException if not called exactly 5 times\n");

        Console.WriteLine("Exception handling:");
        Console.WriteLine("  try");
        Console.WriteLine("  {");
        Console.WriteLine("      mock.VerifyProcessPayment(Times.Never);");
        Console.WriteLine("  }");
        Console.WriteLine("  catch (VerificationException ex)");
        Console.WriteLine("  {");
        Console.WriteLine("      Console.WriteLine($\"Verification failed: {ex.Message}\");");
        Console.WriteLine("  }\n");
    }
}

// ============================================================================
// Sample 5: Async Methods
// ============================================================================

public interface IAsyncRepository
{
    Task<User?> GetUserAsync(string userId);
    Task SaveUserAsync(User user);
}

public class AsyncMockExample
{
    public static async Task RunAsyncExample()
    {
        Console.WriteLine("=== Async Methods Example ===\n");

        var repository = Mock.Of<IAsyncRepository>();

        Console.WriteLine("MockLite supports async methods:\n");
        Console.WriteLine("Setup async return:");
        Console.WriteLine("  repository.SetupGetUserAsync(");
        Console.WriteLine("      userId => Task.FromResult(new User { Id = userId, Name = \"Test\" })");
        Console.WriteLine("  );\n");

        Console.WriteLine("Call and await:");
        Console.WriteLine("  var user = await repository.GetUserAsync(\"123\");");
        Console.WriteLine("  Console.WriteLine(user?.Name);\n");

        Console.WriteLine("Verify async methods:");
        Console.WriteLine("  repository.VerifyGetUserAsync(Times.Once);");
        Console.WriteLine("  repository.VerifySaveUserAsync(Times.Exactly(2));\n");

        Console.WriteLine("✓ Async mock created successfully");
        Console.WriteLine($"  Type: {repository.GetType().Name}\n");
    }
}

// ============================================================================
// Sample 6: Invocation Recording
// ============================================================================

public class InvocationRecordingExample
{
    public static void RunInvocationRecordingExample()
    {
        Console.WriteLine("=== Invocation Recording Example ===\n");

        Console.WriteLine("MockLite records all method invocations:");
        Console.WriteLine("Each call is captured as an Invocation object with:\n");
        
        Console.WriteLine("  1. Method - the MethodInfo of the called method");
        Console.WriteLine("  2. Arguments - the object[] array of arguments passed");
        Console.WriteLine("  3. Timestamp - DateTime.UtcNow when the invocation occurred\n");

        Console.WriteLine("Example Invocation toString():");
        Console.WriteLine("  GetUser(\"123\") @ 2024-01-15T10:30:45.1234567Z\n");

        Console.WriteLine("You can access recorded invocations:");
        Console.WriteLine("  var mock = Mock.Of<IUserRepository>();");
        Console.WriteLine("  mock.GetUser(\"123\");");
        Console.WriteLine("  var invocations = ((dynamic)mock).Invocations;");
        Console.WriteLine("  foreach (var inv in invocations)");
        Console.WriteLine("  {");
        Console.WriteLine("      Console.WriteLine(inv);");
        Console.WriteLine("      Console.WriteLine($\"Called at {inv.Timestamp}\");");
        Console.WriteLine("  }\n");
    }
}

// ============================================================================
// Sample 7: Complete Integration Example
// ============================================================================

public interface IDataStore
{
    T? Get<T>(string key) where T : class;
    void Set<T>(string key, T value) where T : class;
    bool Delete(string key);
}

public class IntegrationExample
{
    public static void RunIntegrationExample()
    {
        Console.WriteLine("=== Complete Integration Example ===\n");

        // This demonstrates how you would use MockLite in a real scenario
        var dataStore = Mock.Of<IDataStore>();

        Console.WriteLine("Real-world usage pattern:");
        Console.WriteLine();
        Console.WriteLine("1. Create the mock");
        Console.WriteLine($"   ✓ Created mock for IDataStore");
        Console.WriteLine($"   Type: {dataStore.GetType().Name}\n");

        Console.WriteLine("2. Setup behavior");
        Console.WriteLine("   dataStore.SetupGet<User>(");
        Console.WriteLine("       key => new User { Id = key, Name = \"Test User\" }");
        Console.WriteLine("   );\n");

        Console.WriteLine("3. Use the mock in test");
        Console.WriteLine("   var user = dataStore.Get<User>(\"user-123\");");
        Console.WriteLine("   Assert.NotNull(user);\n");

        Console.WriteLine("4. Verify interactions");
        Console.WriteLine("   dataStore.VerifyGet<User>(Times.Once);");
        Console.WriteLine("   dataStore.VerifySet<User>(Times.Exactly(0));\n");
    }
}

// ============================================================================
// Main Entry Point
// ============================================================================

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║         MockLite Framework Samples             ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝\n");

        try
        {
            // Run samples
            BasicMockExample.RunBasicExample();
            ArgumentMatcherExample.RunArgumentMatcherExample();
            TimesPredicateExample.RunTimesPredicateExample();
            VerificationExample.RunVerificationExample();
            await AsyncMockExample.RunAsyncExample();
            InvocationRecordingExample.RunInvocationRecordingExample();
            IntegrationExample.RunIntegrationExample();

            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║         All samples completed successfully!    ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running samples: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
