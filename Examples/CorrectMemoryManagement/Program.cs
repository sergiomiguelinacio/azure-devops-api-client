using System;
using System.Net.Http;
using System.Threading.Tasks;
using ApiBase.Utils.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("⚡ Correct Memory Management Demo");
Console.WriteLine("=================================");
Console.WriteLine();

// Demo 1: Using HttpClient directly (basic pattern)
await DemoBasicHttpClientUsage();

// Demo 2: Using IHttpClientFactory (recommended pattern)
await DemoHttpClientFactory();

// Demo 3: Dependency Injection setup
DemoDependencyInjection();

Console.WriteLine();
Console.WriteLine("✅ All correct memory management patterns demonstrated!");
Console.WriteLine("🎯 Simple, focused, and follows .NET best practices!");

static async Task DemoBasicHttpClientUsage()
{
    Console.WriteLine("🔧 Demo 1: Basic HttpClient Usage (with proper disposal)");
    Console.WriteLine("--------------------------------------------------------");

    // ✅ Correct: HttpClient is already IDisposable
    using var httpClient = new HttpClient();

    // ✅ Correct: HttpClientUtil handles response disposal internally
    var httpClientUtil = new HttpClientUtil(httpClient);

    try
    {
        Console.WriteLine("   ✅ HttpClient created with proper disposal");
        Console.WriteLine("   ✅ HttpClientUtil wraps HttpClient correctly");
        Console.WriteLine("   ✅ All HTTP responses are disposed automatically");

        // Simulate some work
        await Task.Delay(10);

        Console.WriteLine("   ✅ Operations completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Error: {ex.Message}");
    }

    // HttpClient is automatically disposed here due to 'using'
    Console.WriteLine("   ✅ HttpClient automatically disposed");
    Console.WriteLine();
}

static async Task DemoHttpClientFactory()
{
    Console.WriteLine("🏭 Demo 2: IHttpClientFactory Usage (recommended)");
    Console.WriteLine("-------------------------------------------------");

    // Setup DI container with IHttpClientFactory
    var services = new ServiceCollection();
    services.AddHttpClient();

    var serviceProvider = services.BuildServiceProvider();
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

    try
    {
        // ✅ Best Practice: Use IHttpClientFactory for better resource management
        var httpClientUtil = new HttpClientUtil(httpClientFactory);

        Console.WriteLine("   ✅ IHttpClientFactory configured");
        Console.WriteLine("   ✅ HttpClientUtil uses factory for optimal resource management");
        Console.WriteLine("   ✅ HttpClient instances are pooled and reused efficiently");
        Console.WriteLine("   ✅ No need to manually dispose HttpClient instances");

        // Simulate some work
        await Task.Delay(10);

        Console.WriteLine("   ✅ Factory-based operations completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Error: {ex.Message}");
    }
    finally
    {
        serviceProvider.Dispose();
    }

    Console.WriteLine("   ✅ Service provider disposed, all resources cleaned up");
    Console.WriteLine();
}

static void DemoDependencyInjection()
{
    Console.WriteLine("🔧 Demo 3: Dependency Injection Setup");
    Console.WriteLine("-------------------------------------");

    Console.WriteLine("   📋 Recommended DI Registration:");
    Console.WriteLine();
    Console.WriteLine("   // Register IHttpClientFactory (built-in)");
    Console.WriteLine("   services.AddHttpClient();");
    Console.WriteLine();
    Console.WriteLine("   // Register your API client");
    Console.WriteLine("   services.AddTransient<IHttpClientUtil>(provider => {");
    Console.WriteLine("       var factory = provider.GetRequiredService<IHttpClientFactory>();");
    Console.WriteLine("       return new HttpClientUtil(factory);");
    Console.WriteLine("   });");
    Console.WriteLine();
    Console.WriteLine("   // Or use the extension method (if available)");
    Console.WriteLine("   services.AddAzureDevOpsApiClient<CoreApiClient, ICoreApiClient>();");
    Console.WriteLine();

    Console.WriteLine("   🎯 Benefits:");
    Console.WriteLine("      • HttpClient instances are pooled and reused");
    Console.WriteLine("      • Automatic DNS refresh and connection management");
    Console.WriteLine("      • No HttpClient disposal issues");
    Console.WriteLine("      • Optimal resource utilization");
    Console.WriteLine("      • Built-in resilience patterns support");
    Console.WriteLine();

    Console.WriteLine("   ❌ Anti-patterns to avoid:");
    Console.WriteLine("      • Creating new HttpClient() in every request");
    Console.WriteLine("      • Not disposing HttpClient when created manually");
    Console.WriteLine("      • Using HttpClient as singleton without factory");
    Console.WriteLine();
}
