using ApiBase.Extensions;
using ApiBase.Utils.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AzureDevopsApiIntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests with Azure DevOps API
/// Provides common setup, configuration, and utilities
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IConfiguration Configuration;
    protected readonly ILogger Logger;
    protected readonly string Organization;
    protected readonly string? TestProject;
    protected readonly bool IntegrationTestsEnabled;
    protected readonly bool SkipRealApiCalls;

    protected IntegrationTestBase(ITestOutputHelper output)
    {
        Output = output;

        // Build configuration
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // Get logger
        Logger = ServiceProvider.GetRequiredService<ILogger<IntegrationTestBase>>();

        // Get configuration values
        Organization = Configuration["AzureDevOps:Organization"] ?? throw new InvalidOperationException("AzureDevOps:Organization not configured");
        TestProject = Configuration["AzureDevOps:TestProject"];
        IntegrationTestsEnabled = Configuration.GetValue<bool>("IntegrationTests:Enabled");
        SkipRealApiCalls = Configuration.GetValue<bool>("IntegrationTests:SkipRealApiCalls");

        Logger.LogInformation("Integration test initialized for organization: {Organization}", Organization);
        Logger.LogInformation("Integration tests enabled: {Enabled}, Skip real API calls: {Skip}", 
            IntegrationTestsEnabled, SkipRealApiCalls);
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddConfiguration(Configuration.GetSection("Logging"));
        });

        // Add configuration
        services.AddSingleton(Configuration);

        // Add Azure DevOps API client with Polly
        var token = GetPersonalAccessToken();
        services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
            configureHttpClient: client =>
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                client.Timeout = TimeSpan.FromSeconds(Configuration.GetValue<int>("IntegrationTests:TimeoutSeconds", 30));
            },
            retryOptions: new RetryOptions
            {
                MaxRetries = Configuration.GetValue<int>("IntegrationTests:MaxRetries", 3),
                BaseDelayMs = 1000,
                UseExponentialBackoff = true
            }
        );
    }

    protected string GetPersonalAccessToken()
    {
        var token = Configuration["AzureDevOps:PersonalAccessToken"] 
                   ?? Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");

        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException(
                "Personal Access Token not configured. Set AzureDevOps:PersonalAccessToken in appsettings.json or AZURE_DEVOPS_PAT environment variable.");
        }

        return token;
    }

    protected void SkipIfDisabled()
    {
        if (!IntegrationTestsEnabled)
        {
            throw new InvalidOperationException("Integration tests are disabled. Set IntegrationTests:Enabled to true in configuration.");
        }
    }

    protected void SkipIfRealApiCallsDisabled()
    {
        if (SkipRealApiCalls)
        {
            throw new InvalidOperationException("Real API calls are disabled. Set IntegrationTests:SkipRealApiCalls to false in configuration.");
        }
    }

    protected void LogTestStart(string testName)
    {
        Output.WriteLine($"=== Starting Integration Test: {testName} ===");
        Logger.LogInformation("Starting integration test: {TestName}", testName);
    }

    protected void LogTestEnd(string testName, bool success)
    {
        var status = success ? "PASSED" : "FAILED";
        Output.WriteLine($"=== Integration Test {status}: {testName} ===");
        Logger.LogInformation("Integration test {Status}: {TestName}", status, testName);
    }

    protected async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
    {
        var maxRetries = Configuration.GetValue<int>("IntegrationTests:MaxRetries", 3);
        var delay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                Logger.LogDebug("Executing {Operation}, attempt {Attempt}/{MaxRetries}", operationName, attempt, maxRetries);
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                Logger.LogWarning("Attempt {Attempt} failed for {Operation}: {Error}. Retrying in {Delay}ms...", 
                    attempt, operationName, ex.Message, delay.TotalMilliseconds);
                Output.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying...");
                
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
        }

        // Final attempt without catching exception
        Logger.LogDebug("Final attempt {Attempt} for {Operation}", maxRetries, operationName);
        return await operation();
    }

    public virtual void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Configuration options for integration tests
/// </summary>
public class IntegrationTestOptions
{
    public bool Enabled { get; set; }
    public bool SkipRealApiCalls { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}
