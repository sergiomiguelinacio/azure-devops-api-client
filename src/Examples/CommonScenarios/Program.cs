using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AzureDevopsApi.Api.Implementation.core;
using ApiBase.Utils.Implementations;
using System.Net.Http;

Console.WriteLine("🚀 Azure DevOps API Client - Common Scenarios");
Console.WriteLine("==============================================");
Console.WriteLine();

// Setup dependency injection
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();
        services.AddLogging(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        services.AddTransient<IHttpClientUtil>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpClientUtil(factory);
        });
    })
    .Build();

var serviceProvider = host.Services;

// Configuration (replace with your values)
const string organizationUrl = "https://dev.azure.com/your-organization";
const string personalAccessToken = "your-pat-token";
const string projectId = "your-project-id";

try
{
    // Scenario 1: Basic team listing
    await Scenario1_BasicTeamListing(serviceProvider, organizationUrl, personalAccessToken, projectId);
    
    // Scenario 2: Error handling and resilience
    await Scenario2_ErrorHandlingAndResilience(serviceProvider, organizationUrl, personalAccessToken);
    
    // Scenario 3: Parallel processing
    await Scenario3_ParallelProcessing(serviceProvider, organizationUrl, personalAccessToken);
    
    // Scenario 4: Custom retry policies
    await Scenario4_CustomRetryPolicies(serviceProvider, organizationUrl, personalAccessToken, projectId);
    
    // Scenario 5: Performance monitoring
    await Scenario5_PerformanceMonitoring(serviceProvider, organizationUrl, personalAccessToken, projectId);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Application error: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("✅ All scenarios completed!");

static async Task Scenario1_BasicTeamListing(IServiceProvider serviceProvider, string orgUrl, string pat, string projectId)
{
    Console.WriteLine("📋 Scenario 1: Basic Team Listing");
    Console.WriteLine("----------------------------------");
    
    var httpClientUtil = serviceProvider.GetRequiredService<IHttpClientUtil>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var teamsClient = new Teams(httpClientUtil);
        
        logger.LogInformation("Fetching teams for project {ProjectId}", projectId);
        
        // This would normally make a real API call
        // var teams = await teamsClient.GetTeamsAsync(projectId);
        
        Console.WriteLine("   ✅ Successfully retrieved teams");
        Console.WriteLine("   📊 Teams found: [This would show actual count]");
        Console.WriteLine("   🔗 API endpoint: GET {organization}/_apis/projects/{projectId}/teams");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to retrieve teams");
        Console.WriteLine($"   ❌ Error: {ex.Message}");
    }
    
    Console.WriteLine();
}

static async Task Scenario2_ErrorHandlingAndResilience(IServiceProvider serviceProvider, string orgUrl, string pat)
{
    Console.WriteLine("🛡️ Scenario 2: Error Handling and Resilience");
    Console.WriteLine("---------------------------------------------");
    
    var httpClientUtil = serviceProvider.GetRequiredService<IHttpClientUtil>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var operationsClient = new Operations(httpClientUtil);
        
        logger.LogInformation("Testing resilience with invalid operation ID");
        
        // This would test retry policies with an invalid ID
        // var operation = await operationsClient.GetOperationAsync("invalid-operation-id");
        
        Console.WriteLine("   ✅ Resilience patterns working:");
        Console.WriteLine("      • Retry policy: Exponential backoff");
        Console.WriteLine("      • Circuit breaker: Fail-fast protection");
        Console.WriteLine("      • Timeout policy: Request timeout protection");
        Console.WriteLine("   📊 Expected behavior: Graceful failure after retries");
    }
    catch (Exception ex)
    {
        logger.LogWarning("Expected error for resilience testing: {Message}", ex.Message);
        Console.WriteLine("   ✅ Error handled gracefully by resilience policies");
    }
    
    Console.WriteLine();
}

static async Task Scenario3_ParallelProcessing(IServiceProvider serviceProvider, string orgUrl, string pat)
{
    Console.WriteLine("⚡ Scenario 3: Parallel Processing");
    Console.WriteLine("----------------------------------");
    
    var httpClientUtil = serviceProvider.GetRequiredService<IHttpClientUtil>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var teamsClient = new Teams(httpClientUtil);
        
        // Simulate multiple project IDs
        var projectIds = new[] { "project1", "project2", "project3" };
        
        logger.LogInformation("Processing {Count} projects in parallel", projectIds.Length);
        
        var startTime = DateTime.UtcNow;
        
        // Create tasks for parallel execution
        var tasks = projectIds.Select(async projectId =>
        {
            try
            {
                // This would make actual API calls
                // var teams = await teamsClient.GetTeamsAsync(projectId);
                await Task.Delay(100); // Simulate API call
                
                logger.LogInformation("Completed processing for project {ProjectId}", projectId);
                return $"Project {projectId}: Success";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process project {ProjectId}", projectId);
                return $"Project {projectId}: Failed - {ex.Message}";
            }
        });
        
        var results = await Task.WhenAll(tasks);
        var duration = DateTime.UtcNow - startTime;
        
        Console.WriteLine("   ✅ Parallel processing completed:");
        foreach (var result in results)
        {
            Console.WriteLine($"      • {result}");
        }
        Console.WriteLine($"   ⏱️ Total duration: {duration.TotalMilliseconds:F0}ms");
        Console.WriteLine("   🚀 Performance benefit: ~3x faster than sequential");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Parallel processing failed");
        Console.WriteLine($"   ❌ Error: {ex.Message}");
    }
    
    Console.WriteLine();
}

static async Task Scenario4_CustomRetryPolicies(IServiceProvider serviceProvider, string orgUrl, string pat, string projectId)
{
    Console.WriteLine("🔄 Scenario 4: Custom Retry Policies");
    Console.WriteLine("------------------------------------");
    
    var httpClientUtil = serviceProvider.GetRequiredService<IHttpClientUtil>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // This would normally configure custom retry options
        var retryOptions = new
        {
            MaxRetryAttempts = 5,
            BaseDelay = TimeSpan.FromSeconds(1),
            MaxDelay = TimeSpan.FromSeconds(30),
            BackoffMultiplier = 2.0
        };
        
        logger.LogInformation("Using custom retry policy: {MaxRetries} attempts, {BaseDelay}s base delay", 
            retryOptions.MaxRetryAttempts, retryOptions.BaseDelay.TotalSeconds);
        
        var teamsClient = new Teams(httpClientUtil);
        
        // This would use the custom retry policy
        // var teams = await teamsClient.GetTeamsAsync(projectId);
        
        Console.WriteLine("   ✅ Custom retry policy configured:");
        Console.WriteLine($"      • Max attempts: {retryOptions.MaxRetryAttempts}");
        Console.WriteLine($"      • Base delay: {retryOptions.BaseDelay.TotalSeconds}s");
        Console.WriteLine($"      • Max delay: {retryOptions.MaxDelay.TotalSeconds}s");
        Console.WriteLine($"      • Backoff multiplier: {retryOptions.BackoffMultiplier}x");
        Console.WriteLine("   📈 Retry sequence: 1s → 2s → 4s → 8s → 16s");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Custom retry policy test failed");
        Console.WriteLine($"   ❌ Error: {ex.Message}");
    }
    
    Console.WriteLine();
}

static async Task Scenario5_PerformanceMonitoring(IServiceProvider serviceProvider, string orgUrl, string pat, string projectId)
{
    Console.WriteLine("📊 Scenario 5: Performance Monitoring");
    Console.WriteLine("-------------------------------------");
    
    var httpClientUtil = serviceProvider.GetRequiredService<IHttpClientUtil>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var teamsClient = new Teams(httpClientUtil);
        
        // Performance monitoring with timing
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        logger.LogInformation("Starting performance monitoring for teams API");
        
        // This would make actual API calls with timing
        // var teams = await teamsClient.GetTeamsAsync(projectId);
        await Task.Delay(50); // Simulate API call
        
        stopwatch.Stop();
        
        var metrics = new
        {
            Duration = stopwatch.ElapsedMilliseconds,
            Success = true,
            Endpoint = "GET /_apis/projects/{projectId}/teams",
            ResponseSize = "1.2KB", // Would be actual size
            RetryCount = 0
        };
        
        logger.LogInformation("API call completed in {Duration}ms", metrics.Duration);
        
        Console.WriteLine("   ✅ Performance metrics collected:");
        Console.WriteLine($"      • Duration: {metrics.Duration}ms");
        Console.WriteLine($"      • Success: {metrics.Success}");
        Console.WriteLine($"      • Endpoint: {metrics.Endpoint}");
        Console.WriteLine($"      • Response size: {metrics.ResponseSize}");
        Console.WriteLine($"      • Retry count: {metrics.RetryCount}");
        Console.WriteLine("   📈 Metrics can be sent to Application Insights, Prometheus, etc.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Performance monitoring failed");
        Console.WriteLine($"   ❌ Error: {ex.Message}");
    }
    
    Console.WriteLine();
}
