# 📊 Logging and Observability Guide

This guide covers the comprehensive logging and observability features built into AzureDevOpsClient v2.0.0+.

## 🚀 Quick Start

### Basic Setup with Console Logging

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AzureDevOpsClient.Api.Implementation.core;

// Create logger factory
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create logger for specific API
var logger = loggerFactory.CreateLogger<Teams>();

// Use with API client
var teamsApi = new Teams(httpClient, "https://dev.azure.com/yourorg", logger: logger);

// All operations now include automatic logging
var teams = await teamsApi.GetTeamsAsync<TeamList>();
```

### Dependency Injection Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Register API clients (they'll automatically get loggers injected)
        services.AddTransient<Teams>();
        services.AddTransient<Operations>();
    })
    .Build();

var teamsApi = host.Services.GetRequiredService<Teams>();
```

## 📋 What Gets Logged Automatically

Every API operation logs the following information:

### 1. Operation Start (Information Level)
```
[10:30:15 INF] Starting GET operation: GetTeams for path: /{organization}/_apis/projects/{projectId}/teams
```

### 2. URL Construction (Debug Level)
```
[10:30:15 DBG] Built URL: https://dev.azure.com/myorg/_apis/projects/myproject/teams for operation: GetTeams
```

### 3. Operation Completion (Information Level)
```
[10:30:16 INF] Completed GET operation: GetTeams in 1250ms
```

### 4. Operation Failure (Error Level)
```
[10:30:16 ERR] Failed GET operation: GetTeams after 1250ms
System.Net.Http.HttpRequestException: Response status code does not indicate success: 404 (Not Found).
   at AzureDevOpsClient.Api.Implementation.core.Teams.GetTeamsAsync[T]...
```

## 🔧 Advanced Configuration

### Serilog Integration

```csharp
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyApp")
    .WriteTo.Console()
    .WriteTo.File("logs/azuredevops-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Use with Microsoft.Extensions.Logging
var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog());
var logger = loggerFactory.CreateLogger<Teams>();
```

### Application Insights Integration

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging.ApplicationInsights;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddApplicationInsights("your-instrumentation-key");
    builder.AddFilter<ApplicationInsightsLoggerProvider>("AzureDevOpsClient", LogLevel.Information);
});
```

### Elasticsearch Integration

```csharp
using Serilog.Sinks.Elasticsearch;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        IndexFormat = "azuredevops-logs-{0:yyyy.MM.dd}",
        AutoRegisterTemplate = true
    })
    .CreateLogger();
```

## 📈 Performance Metrics

### Automatic Timing

Every operation is automatically timed:

```csharp
// This operation will log its duration automatically
var teams = await teamsApi.GetTeamsAsync<TeamList>();
// Output: "Completed GET operation: GetTeams in 1250ms"
```

### Custom Metrics Collection

```csharp
// The timing is handled automatically, but you can access it via logs
// Look for log entries with duration information
```

## 🔍 Structured Logging

### Log Structure

All logs include structured data:

```json
{
  "timestamp": "2024-01-15T10:30:15.123Z",
  "level": "Information",
  "message": "Completed GET operation: GetTeams in 1250ms",
  "properties": {
    "operation": "GetTeams",
    "duration": 1250,
    "success": true,
    "path": "/{organization}/_apis/projects/{projectId}/teams"
  }
}
```

### Filtering and Querying

**Filter by operation:**
```csharp
builder.AddFilter("AzureDevOpsClient.Api.Implementation.core.Teams", LogLevel.Debug);
```

**Filter by log level:**
```csharp
builder.SetMinimumLevel(LogLevel.Warning); // Only warnings and errors
```

## 🚨 Error Handling and Debugging

### Exception Logging

When operations fail, detailed information is logged:

```csharp
try
{
    var teams = await teamsApi.GetTeamsAsync<TeamList>();
}
catch (HttpRequestException ex)
{
    // Automatically logged with:
    // - Operation name
    // - Duration before failure  
    // - Full exception details
    // - Request URL (debug level)
}
```

### Debug Information

Enable debug logging to see URL construction:

```csharp
builder.SetMinimumLevel(LogLevel.Debug);
```

This will show:
```
[10:30:15 DBG] Built URL: https://dev.azure.com/myorg/_apis/projects/myproject/teams for operation: GetTeams
```

## 🔗 Integration Examples

### ASP.NET Core Integration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();

// Register API clients
builder.Services.AddTransient<Teams>();
builder.Services.AddTransient<Operations>();

var app = builder.Build();

// Use in controllers
[ApiController]
public class TeamsController : ControllerBase
{
    private readonly Teams _teamsApi;
    
    public TeamsController(Teams teamsApi)
    {
        _teamsApi = teamsApi; // Logger automatically injected
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTeams()
    {
        var teams = await _teamsApi.GetTeamsAsync<TeamList>();
        return Ok(teams);
    }
}
```

### Console Application

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        services.AddTransient<Teams>();
    })
    .Build();

var teamsApi = host.Services.GetRequiredService<Teams>();
var teams = await teamsApi.GetTeamsAsync<TeamList>();
```

### Background Service

```csharp
public class AzureDevOpsBackgroundService : BackgroundService
{
    private readonly Teams _teamsApi;
    private readonly ILogger<AzureDevOpsBackgroundService> _logger;
    
    public AzureDevOpsBackgroundService(Teams teamsApi, ILogger<AzureDevOpsBackgroundService> logger)
    {
        _teamsApi = teamsApi;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Fetching teams data...");
                var teams = await _teamsApi.GetTeamsAsync<TeamList>();
                _logger.LogInformation("Found {TeamCount} teams", teams?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teams data");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## 🎯 Best Practices

### 1. Use Appropriate Log Levels
- **Debug**: URL construction, detailed flow
- **Information**: Operation start/completion, business events
- **Warning**: Retries, recoverable errors
- **Error**: Operation failures, exceptions

### 2. Configure for Environment
```csharp
// Development
builder.SetMinimumLevel(LogLevel.Debug);

// Production
builder.SetMinimumLevel(LogLevel.Information);
```

### 3. Use Structured Logging
The client automatically provides structured logs. Ensure your logging provider supports it.

### 4. Monitor Performance
Watch for operations taking longer than expected:
```
Completed GET operation: GetTeams in 5000ms  // This might be slow
```

### 5. Set Up Alerts
Configure alerts for:
- High error rates
- Slow operations (>5 seconds)
- Authentication failures

## 📊 Monitoring Dashboards

### Application Insights Queries

```kql
// Operations by duration
traces
| where message contains "Completed GET operation"
| extend duration = extract(@"in (\d+)ms", 1, message)
| summarize avg(toint(duration)) by operation_Name

// Error rate
traces  
| where message contains "Failed GET operation"
| summarize errors = count() by bin(timestamp, 1h)
```

### Elasticsearch Queries

```json
{
  "query": {
    "bool": {
      "must": [
        {"term": {"level": "Error"}},
        {"range": {"@timestamp": {"gte": "now-1h"}}}
      ]
    }
  }
}
```

This comprehensive logging system provides full observability into your Azure DevOps API operations, making debugging and monitoring straightforward.
