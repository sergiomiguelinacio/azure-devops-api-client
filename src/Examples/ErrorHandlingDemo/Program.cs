using System.Net;
using AzureDevopsApi.Exceptions;
using AzureDevopsApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("🛡️ Error Handling Improvements Demo");
Console.WriteLine("===================================");
Console.WriteLine();

// Setup services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddTransient<IGracefulDegradationService, GracefulDegradationService>();
services.AddTransient<IHealthCheckService, HealthCheckService>();
services.AddTransient<IErrorRecoveryService, ErrorRecoveryService>();

var serviceProvider = services.BuildServiceProvider();

// Demo 1: Custom Exceptions
await DemoCustomExceptions();

// Demo 2: Graceful Degradation
await DemoGracefulDegradation(serviceProvider);

// Demo 3: Health Checks
await DemoHealthChecks(serviceProvider);

// Demo 4: Error Messages (simulated)
await DemoErrorMessages();

// Demo 5: Error Recovery
await DemoErrorRecovery(serviceProvider);

Console.WriteLine();
Console.WriteLine("✅ All Error Handling demos completed successfully!");
Console.WriteLine("🛡️ Your application now has enterprise-grade error handling!");

static async Task DemoCustomExceptions()
{
    Console.WriteLine("🎯 Demo 1: Custom Exceptions with Rich Context");
    Console.WriteLine("----------------------------------------------");

    try
    {
        // Simulate different types of exceptions
        await SimulateApiCommunicationError();
    }
    catch (ApiCommunicationException ex)
    {
        Console.WriteLine($"✅ Caught custom exception: {ex.ErrorCode}");
        Console.WriteLine($"   Component: {ex.Component}");
        Console.WriteLine($"   Status Code: {ex.StatusCode}");
        Console.WriteLine($"   Retryable: {ex.IsRetryable}");
        Console.WriteLine($"   Suggested Retry Delay: {ex.SuggestedRetryDelay}");
        Console.WriteLine($"   Context: {string.Join(", ", ex.Context.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
    }

    try
    {
        await SimulateRateLimitError();
    }
    catch (RateLimitException ex)
    {
        Console.WriteLine($"✅ Caught rate limit exception: {ex.ErrorCode}");
        Console.WriteLine($"   Remaining Requests: {ex.RemainingRequests}");
        Console.WriteLine($"   Reset Time: {ex.ResetTime}");
        Console.WriteLine($"   Retry After: {ex.RetryAfter}");
    }

    Console.WriteLine();
}

static async Task DemoGracefulDegradation(ServiceProvider serviceProvider)
{
    Console.WriteLine("🔄 Demo 2: Graceful Degradation with Fallbacks");
    Console.WriteLine("----------------------------------------------");

    var degradationService = serviceProvider.GetRequiredService<IGracefulDegradationService>();

    // Demo fallback execution
    try
    {
        var result = await degradationService.ExecuteWithFallbackAsync(
            primaryOperation: async ct =>
            {
                if (Random.Shared.NextDouble() > 0.5)
                    throw new ApiCommunicationException("Primary API failed", HttpStatusCode.InternalServerError);
                return "Primary API Success";
            },
            fallbackOperations: new[]
            {
                async (CancellationToken ct) =>
                {
                    await Task.Delay(50, ct);
                    if (Random.Shared.NextDouble() > 0.3)
                        throw new ApiCommunicationException("Fallback 1 failed", HttpStatusCode.ServiceUnavailable);
                    return "Fallback 1 Success";
                },
                async (CancellationToken ct) =>
                {
                    await Task.Delay(30, ct);
                    return "Fallback 2 Success";
                }
            }
        );

        Console.WriteLine($"✅ Operation completed: {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ All operations failed: {ex.Message}");
    }

    // Demo partial success
    var partialResult = await degradationService.ExecuteWithPartialSuccessAsync(
        operations: Enumerable.Range(1, 5).Select(i => new Func<CancellationToken, Task<string>>(async ct =>
        {
            await Task.Delay(Random.Shared.Next(50, 200), ct);
            if (Random.Shared.NextDouble() > 0.7)
                throw new Exception($"Operation {i} failed");
            return $"Operation {i} success";
        })),
        minimumSuccessRate: 0.6
    );

    Console.WriteLine($"✅ Partial execution completed:");
    Console.WriteLine($"   Success Rate: {partialResult.SuccessRate:P1}");
    Console.WriteLine($"   Successful: {partialResult.SuccessfulOperations}/{partialResult.TotalOperations}");
    Console.WriteLine($"   Results: {string.Join(", ", partialResult.Results)}");

    var status = degradationService.GetDegradationStatus();
    Console.WriteLine($"✅ Overall Health: {status.OverallHealth}");

    Console.WriteLine();
}

static async Task DemoHealthChecks(ServiceProvider serviceProvider)
{
    Console.WriteLine("💓 Demo 3: Health Checks with Monitoring");
    Console.WriteLine("----------------------------------------");

    var healthService = serviceProvider.GetRequiredService<IHealthCheckService>();

    // Register health checks
    healthService.RegisterHealthCheck("Database", async ct =>
    {
        await Task.Delay(100, ct);
        var isHealthy = Random.Shared.NextDouble() > 0.2; // 80% healthy
        return new HealthCheckResult
        {
            Status = isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            Description = isHealthy ? "Database is responsive" : "Database connection timeout",
            Data = new Dictionary<string, object>
            {
                ["ConnectionCount"] = Random.Shared.Next(1, 10),
                ["ResponseTime"] = $"{Random.Shared.Next(50, 200)}ms"
            }
        };
    }, TimeSpan.FromSeconds(5));

    healthService.RegisterHealthCheck("ExternalAPI", async ct =>
    {
        await Task.Delay(150, ct);
        var isHealthy = Random.Shared.NextDouble() > 0.3; // 70% healthy
        return new HealthCheckResult
        {
            Status = isHealthy ? HealthStatus.Healthy : HealthStatus.Degraded,
            Description = isHealthy ? "External API is available" : "External API is slow",
            Data = new Dictionary<string, object>
            {
                ["LastSuccessfulCall"] = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(1, 30)),
                ["ErrorRate"] = $"{Random.Shared.Next(0, 15)}%"
            }
        };
    });

    // Execute health checks
    var overallHealth = await healthService.CheckHealthAsync();

    Console.WriteLine($"✅ Overall Health Status: {overallHealth.Status}");
    Console.WriteLine($"   Total Duration: {overallHealth.TotalDuration:F1}ms");
    Console.WriteLine($"   Checked At: {overallHealth.CheckedAt:HH:mm:ss}");

    foreach (var result in overallHealth.Results)
    {
        Console.WriteLine($"   📊 {result.Key}: {result.Value.Status} ({result.Value.Duration.TotalMilliseconds:F0}ms)");
        Console.WriteLine($"      {result.Value.Description}");
        if (result.Value.Data.Any())
        {
            Console.WriteLine($"      Data: {string.Join(", ", result.Value.Data.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        }
    }

    Console.WriteLine();
}

static async Task DemoErrorMessages()
{
    Console.WriteLine("📝 Demo 4: Informative Error Messages");
    Console.WriteLine("-------------------------------------");

    // Simulate different error scenarios and show improved messages
    var exceptions = new AzureDevOpsException[]
    {
        new AuthenticationException("Invalid API token", "Bearer Token"),
        new ValidationException("Invalid input data", new List<string> { "Email format is invalid", "Password too short" }, "UserInput"),
        new ResourceNotFoundException("Project not found", "Project", "12345"),
        new ConfigurationException("Missing API endpoint", "ApiEndpoint", "https://api.example.com")
    };

    foreach (var ex in exceptions)
    {
        Console.WriteLine($"🔍 Exception Type: {ex.GetType().Name}");
        Console.WriteLine($"   Error Code: {ex.ErrorCode}");
        Console.WriteLine($"   Component: {ex.Component}");
        Console.WriteLine($"   Message: {ex.Message}");
        Console.WriteLine($"   Retryable: {ex.IsRetryable}");
        if (ex.Context.Any())
        {
            Console.WriteLine($"   Context: {string.Join(", ", ex.Context.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        }
        Console.WriteLine();
    }
}

static async Task DemoErrorRecovery(ServiceProvider serviceProvider)
{
    Console.WriteLine("🔧 Demo 5: Automatic Error Recovery");
    Console.WriteLine("-----------------------------------");

    var recoveryService = serviceProvider.GetRequiredService<IErrorRecoveryService>();

    // Demo 1: Basic retry with recovery
    Console.WriteLine("🔄 Testing basic retry with exponential backoff...");
    try
    {
        var result = await recoveryService.ExecuteWithRecoveryAsync(async ct =>
        {
            // Simulate intermittent failure
            if (Random.Shared.NextDouble() > 0.6) // 40% success rate
            {
                return "Operation succeeded!";
            }
            throw new ApiCommunicationException("Temporary service unavailable", HttpStatusCode.ServiceUnavailable);
        }, RecoveryPolicy.Default);

        Console.WriteLine($"✅ Recovery successful: {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Recovery failed after all retries: {ex.Message}");
    }

    // Demo 2: Rate limit recovery
    Console.WriteLine("⏱️ Testing rate limit recovery...");
    try
    {
        var result = await recoveryService.ExecuteWithRecoveryAsync(async ct =>
        {
            // Simulate rate limit on first few attempts
            if (Random.Shared.NextDouble() > 0.8) // 20% success rate initially
            {
                return "Rate limit recovered!";
            }
            throw new RateLimitException("Rate limit exceeded", 0, DateTime.UtcNow.AddMinutes(1), TimeSpan.FromSeconds(2));
        }, RecoveryPolicy.Aggressive);

        Console.WriteLine($"✅ Rate limit recovery successful: {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Rate limit recovery failed: {ex.Message}");
    }

    // Show recovery statistics
    var stats = recoveryService.GetRecoveryStatistics();
    Console.WriteLine($"📊 Recovery Statistics:");
    Console.WriteLine($"   Total Attempts: {stats.TotalAttempts}");
    Console.WriteLine($"   Successful Recoveries: {stats.SuccessfulRecoveries}");
    Console.WriteLine($"   Failed Recoveries: {stats.FailedRecoveries}");
    Console.WriteLine($"   Success Rate: {stats.SuccessRate:P1}");
    Console.WriteLine($"   Average Attempts to Success: {stats.AverageAttemptsToSuccess:F1}");

    if (stats.MostCommonExceptions.Any())
    {
        Console.WriteLine($"   Most Common Exceptions:");
        foreach (var kvp in stats.MostCommonExceptions.Take(3))
        {
            Console.WriteLine($"     • {kvp.Key}: {kvp.Value} times");
        }
    }

    Console.WriteLine();
}

static async Task SimulateApiCommunicationError()
{
    await Task.Delay(50);
    var ex = new ApiCommunicationException(
        "Failed to connect to Azure DevOps API",
        HttpStatusCode.InternalServerError,
        "https://dev.azure.com/myorg/_apis/projects",
        "Internal Server Error");

    ex.AddContext("RequestId", Guid.NewGuid().ToString());
    ex.AddContext("UserAgent", "AzureDevOpsClient/1.0");
    ex.AddContext("Timestamp", DateTime.UtcNow);

    throw ex;
}

static async Task SimulateRateLimitError()
{
    await Task.Delay(30);
    throw new RateLimitException(
        "API rate limit exceeded",
        remainingRequests: 0,
        resetTime: DateTime.UtcNow.AddMinutes(15),
        retryAfter: TimeSpan.FromMinutes(15));
}
