using AzureDevOpsApi.Api;
using ApiBase.Utils.Implementations;
using ApiBase.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace AzureDevOpsApi.Examples;

/// <summary>
/// Comprehensive examples demonstrating Azure DevOps API usage with Polly integration
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Azure DevOps API Client Examples");
        Console.WriteLine("====================================\n");

        // Example 1: Basic usage without DI
        await BasicUsageExample();

        // Example 2: Using Dependency Injection
        await DependencyInjectionExample();

        // Example 3: Advanced Polly configuration
        await AdvancedPollyExample();

        // Example 4: Real-world scenarios
        await RealWorldScenariosExample();

        Console.WriteLine("\n✅ All examples completed successfully!");
    }

    /// <summary>
    /// Example 1: Basic usage without dependency injection
    /// </summary>
    private static async Task BasicUsageExample()
    {
        Console.WriteLine("📝 Example 1: Basic Usage");
        Console.WriteLine("-------------------------");

        try
        {
            // Simple configuration with default retry policies
            var retryOptions = new RetryOptions
            {
                MaxRetries = 3,
                BaseDelayMs = 1000,
                UseExponentialBackoff = true
            };

            // Create API client with authentication
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetPersonalAccessToken());

            var apiClient = new CoreApiClient(httpClient, "https://dev.azure.com", retryOptions);

            // Example: List projects
            Console.WriteLine("🔍 Fetching projects...");
            var projects = await apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                pathParameters: new { organization = GetOrganization() },
                queryParameters: new { stateFilter = "wellFormed", top = 5 }
            );

            Console.WriteLine($"✅ Found {projects?.Count ?? 0} projects:");
            if (projects?.Value != null)
            {
                foreach (var project in projects.Value.Take(3))
                {
                    Console.WriteLine($"   • {project.Name} ({project.State})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Example 2: Using Dependency Injection with ASP.NET Core patterns
    /// </summary>
    private static async Task DependencyInjectionExample()
    {
        Console.WriteLine("📝 Example 2: Dependency Injection");
        Console.WriteLine("----------------------------------");

        try
        {
            // Setup dependency injection container
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

            // Add configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AzureDevOps:BaseUrl"] = "https://dev.azure.com",
                    ["AzureDevOps:Organization"] = GetOrganization(),
                    ["AzureDevOps:PersonalAccessToken"] = GetPersonalAccessToken()
                })
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Register Azure DevOps API client with Polly
            services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
                baseUrl: configuration["AzureDevOps:BaseUrl"] ?? "https://dev.azure.com",
                configureHttpClient: client =>
                {
                    var token = configuration["AzureDevOps:PersonalAccessToken"];
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    client.Timeout = TimeSpan.FromMinutes(5);
                },
                retryOptions: new RetryOptions
                {
                    MaxRetries = 3,
                    BaseDelayMs = 1000,
                    UseExponentialBackoff = true
                }
            );

            // Register application service
            services.AddScoped<ProjectService>();

            // Build service provider
            using var serviceProvider = services.BuildServiceProvider();

            // Use the service
            var projectService = serviceProvider.GetRequiredService<ProjectService>();
            await projectService.DemonstrateProjectOperations();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Advanced Polly configuration with circuit breaker
    /// </summary>
    private static async Task AdvancedPollyExample()
    {
        Console.WriteLine("📝 Example 3: Advanced Polly Configuration");
        Console.WriteLine("------------------------------------------");

        try
        {
            // Create advanced Polly policy with circuit breaker
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"🔄 Retry {retryCount} after {timespan.TotalSeconds:F1}s delay");
                    });

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(10),
                    onBreak: (exception, duration) =>
                    {
                        Console.WriteLine($"🔴 Circuit breaker opened for {duration.TotalSeconds}s");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("🟢 Circuit breaker reset");
                    });

            var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

            // Use with API client
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetPersonalAccessToken());

            var apiClient = new CoreApiClient(httpClient, "https://dev.azure.com", combinedPolicy);

            Console.WriteLine("🔍 Testing advanced Polly policies...");
            var projects = await apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                pathParameters: new { organization = GetOrganization() },
                queryParameters: new { top = 1 }
            );

            Console.WriteLine($"✅ Successfully retrieved {projects?.Count ?? 0} projects with advanced policies");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: Real-world scenarios with error handling
    /// </summary>
    private static async Task RealWorldScenariosExample()
    {
        Console.WriteLine("📝 Example 4: Real-World Scenarios");
        Console.WriteLine("----------------------------------");

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetPersonalAccessToken());

            var apiClient = new CoreApiClient(httpClient, "https://dev.azure.com");

            // Scenario 1: Safe project retrieval with error handling
            Console.WriteLine("🔍 Scenario 1: Safe project retrieval");
            var project = await GetProjectSafelyAsync(apiClient, GetOrganization(), "non-existent-project");
            if (project != null)
            {
                Console.WriteLine($"   ✅ Found project: {project.Name}");
            }
            else
            {
                Console.WriteLine("   ℹ️ Project not found (handled gracefully)");
            }

            // Scenario 2: Bulk operations with progress tracking
            Console.WriteLine("\n🔍 Scenario 2: Bulk operations");
            await DemonstrateBulkOperations(apiClient);

            // Scenario 3: Working with different API endpoints
            Console.WriteLine("\n🔍 Scenario 3: Multiple API endpoints");
            await DemonstrateMultipleEndpoints(apiClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Safe project retrieval with proper error handling
    /// </summary>
    private static async Task<TeamProjectDto?> GetProjectSafelyAsync(CoreApiClient apiClient, string organization, string projectId)
    {
        try
        {
            return await apiClient.Projects.GetProjectAsync<TeamProjectDto>(
                pathParameters: new { organization, projectId }
            );
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            Console.WriteLine($"   ⚠️ Project '{projectId}' not found");
            return null;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("403"))
        {
            Console.WriteLine($"   🔒 Authentication failed: {ex.Message}");
            throw new UnauthorizedAccessException("Invalid credentials", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            Console.WriteLine($"   ⏱️ Request timeout for project '{projectId}'");
            throw new TimeoutException("Request timed out", ex);
        }
    }

    /// <summary>
    /// Demonstrate bulk operations with progress tracking
    /// </summary>
    private static async Task DemonstrateBulkOperations(CoreApiClient apiClient)
    {
        var organization = GetOrganization();
        
        // Get all projects first
        var allProjects = await apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
            pathParameters: new { organization },
            queryParameters: new { stateFilter = "wellFormed" }
        );

        if (allProjects?.Value != null && allProjects.Value.Any())
        {
            Console.WriteLine($"   📊 Processing {allProjects.Value.Count} projects...");
            
            var processedCount = 0;
            foreach (var project in allProjects.Value.Take(3)) // Limit for demo
            {
                try
                {
                    // Get detailed project info
                    var detailedProject = await apiClient.Projects.GetProjectAsync<TeamProjectDto>(
                        pathParameters: new { organization, projectId = project.Id }
                    );
                    
                    processedCount++;
                    Console.WriteLine($"   ✅ [{processedCount}] {project.Name} - {detailedProject?.State}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ [{processedCount + 1}] Failed to process {project.Name}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Demonstrate working with multiple API endpoints
    /// </summary>
    private static async Task DemonstrateMultipleEndpoints(CoreApiClient apiClient)
    {
        var organization = GetOrganization();
        
        // Get first project
        var projects = await apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
            pathParameters: new { organization },
            queryParameters: new { top = 1 }
        );

        if (projects?.Value?.FirstOrDefault() is { } firstProject)
        {
            Console.WriteLine($"   🎯 Working with project: {firstProject.Name}");
            
            // Example: Get repositories (if Git endpoints are available)
            try
            {
                // This would work if Git API endpoints are generated
                Console.WriteLine($"   📁 Project ID: {firstProject.Id}");
                Console.WriteLine($"   📊 Project State: {firstProject.State}");
                Console.WriteLine($"   👁️ Visibility: {firstProject.Visibility}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ℹ️ Additional endpoints not available: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Get Personal Access Token from environment or configuration
    /// </summary>
    private static string GetPersonalAccessToken()
    {
        return Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT") 
            ?? "your-personal-access-token-here";
    }

    /// <summary>
    /// Get organization name from environment or configuration
    /// </summary>
    private static string GetOrganization()
    {
        return Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG") 
            ?? "your-organization-name";
    }
}

/// <summary>
/// Example service demonstrating dependency injection usage
/// </summary>
public class ProjectService
{
    private readonly CoreApiClient _apiClient;
    private readonly ILogger<ProjectService> _logger;
    private readonly IConfiguration _configuration;

    public ProjectService(CoreApiClient apiClient, ILogger<ProjectService> logger, IConfiguration configuration)
    {
        _apiClient = apiClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task DemonstrateProjectOperations()
    {
        var organization = _configuration["AzureDevOps:Organization"];
        
        _logger.LogInformation("🔍 Starting project operations for organization: {Organization}", organization);

        try
        {
            // Get projects with logging
            var projects = await _apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                pathParameters: new { organization },
                queryParameters: new { stateFilter = "wellFormed", top = 3 }
            );

            _logger.LogInformation("✅ Retrieved {Count} projects", projects?.Count ?? 0);
            
            if (projects?.Value != null)
            {
                foreach (var project in projects.Value)
                {
                    _logger.LogInformation("   • Project: {Name} (ID: {Id})", project.Name, project.Id);
                }
            }

            Console.WriteLine($"✅ DI Example: Successfully processed {projects?.Count ?? 0} projects");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during project operations");
            throw;
        }
    }
}
