# 📚 Azure DevOps API Client - Examples

This directory contains practical examples demonstrating how to use the Azure DevOps API Client library effectively.

## 🎯 Available Examples

### 1. **CommonScenarios**
**Purpose**: Real-world usage scenarios and patterns

**Key Concepts**:
- ✅ Basic API operations (teams, projects, operations)
- ✅ Error handling and resilience patterns
- ✅ Parallel processing for multiple requests
- ✅ Custom retry policies configuration
- ✅ Performance monitoring and metrics

**Run Example**:
```bash
cd CommonScenarios
dotnet run
```

## 🚀 Quick Start

### **Prerequisites**
1. .NET 9.0 SDK
2. Azure DevOps organization
3. Personal Access Token (PAT) with appropriate permissions

### **Setup**
1. Clone the repository
2. Navigate to an example directory
3. Update configuration values:
   ```csharp
   const string organizationUrl = "https://dev.azure.com/your-organization";
   const string personalAccessToken = "your-pat-token";
   const string projectId = "your-project-id";
   ```
4. Run the example: `dotnet run`

## 📋 Configuration Guide

### **Personal Access Token Setup**
1. Go to Azure DevOps → User Settings → Personal Access Tokens
2. Create new token with required scopes:
   - **Work Items**: Read (for work item operations)
   - **Project and Team**: Read (for project/team operations)
   - **Code**: Read (for repository operations)
3. Copy the token and use it in examples

### **Dependency Injection Setup**
```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register HttpClient factory
        services.AddHttpClient();
        
        // Register logging
        services.AddLogging(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        // Register HttpClientUtil
        services.AddTransient<IHttpClientUtil>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpClientUtil(factory);
        });
        
        // Register API clients
        services.AddTransient<ICoreApiClient, CoreApiClient>();
    })
    .Build();
```

## 🛠️ Example Patterns

### **Pattern 1: Basic API Call**
```csharp
var httpClientUtil = serviceProvider.GetRequiredService<IHttpClientUtil>();
var teamsClient = new Teams(httpClientUtil);

try
{
    var teams = await teamsClient.GetTeamsAsync(projectId);
    Console.WriteLine($"Found {teams.Count} teams");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to retrieve teams");
}
```

### **Pattern 2: Parallel Processing**
```csharp
var projectIds = new[] { "proj1", "proj2", "proj3" };
var tasks = projectIds.Select(id => teamsClient.GetTeamsAsync(id));
var results = await Task.WhenAll(tasks);
```

### **Pattern 3: Custom Retry Policy**
```csharp
var retryOptions = new RetryOptions
{
    MaxRetryAttempts = 5,
    BaseDelay = TimeSpan.FromSeconds(1),
    MaxDelay = TimeSpan.FromSeconds(30)
};

var client = new CoreApiClient(httpClient, baseUrl, pat, retryOptions);
```

### **Pattern 4: Performance Monitoring**
```csharp
var stopwatch = Stopwatch.StartNew();
var result = await client.GetTeamsAsync(projectId);
stopwatch.Stop();

logger.LogInformation("API call took {Duration}ms", stopwatch.ElapsedMilliseconds);
```

## 🔧 Troubleshooting

### **Common Issues**

1. **401 Unauthorized**
   - Check PAT permissions and expiration
   - Verify organization URL format

2. **Socket Exhaustion**
   - Use `IHttpClientFactory` instead of creating new `HttpClient` instances
   - See `CorrectMemoryManagement` example

3. **Timeout Issues**
   - Configure appropriate timeout values
   - Use `CancellationToken` for request cancellation

4. **Performance Issues**
   - Use async/await properly (don't use `.Result`)
   - Implement parallel processing for multiple requests

### **Debug Mode**
Enable detailed logging to troubleshoot issues:
```csharp
services.AddLogging(builder =>
    builder.AddConsole()
           .SetMinimumLevel(LogLevel.Debug)
           .AddFilter("ApiBase", LogLevel.Trace)
           .AddFilter("AzureDevopsApi", LogLevel.Trace));
```

## 📊 Performance Tips

### **1. Use IHttpClientFactory**
- Manages connection pooling automatically
- Handles DNS refresh and connection lifecycle
- Prevents socket exhaustion

### **2. Implement Parallel Processing**
- Use `Task.WhenAll()` for multiple independent requests
- Avoid blocking with `.Result` or `.Wait()`

### **3. Configure Appropriate Timeouts**
- Set reasonable timeout values based on expected response times
- Use `CancellationToken` for user-initiated cancellations

### **4. Monitor Performance**
- Track request duration and success rates
- Implement circuit breaker patterns for fault tolerance
- Use structured logging for observability

## 🔗 Related Documentation

- [Architecture Guide](../../docs/ARCHITECTURE.md)
- [Troubleshooting Guide](../../docs/TROUBLESHOOTING.md)
- [API Reference](../../docs/API_REFERENCE.md)
- [Main README](../../README.md)

## 💡 Contributing Examples

To add a new example:

1. Create a new directory under `src/examples/`
2. Add a console application with clear documentation
3. Include a `.csproj` file with necessary dependencies
4. Update this README with the new example
5. Ensure the example follows established patterns

### **Example Template**
```
src/examples/YourExample/
├── Program.cs              # Main example code
├── YourExample.csproj      # Project file
└── README.md              # Specific example documentation
```

---

*For more detailed information, see the [main documentation](../../docs/) or run the examples to see them in action.*
