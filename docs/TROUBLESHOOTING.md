# 🔧 Troubleshooting Guide

## 🚨 Common Issues and Solutions

### 1. **HttpClient Disposal Issues**

#### **Problem**: Memory leaks or socket exhaustion
```
System.Net.Sockets.SocketException: Only one usage of each socket address is normally permitted
```

#### **Solution**: Use IHttpClientFactory
```csharp
// ❌ Don't do this
using var httpClient = new HttpClient(); // Creates new HttpClient each time

// ✅ Do this instead
services.AddHttpClient();
var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
var httpClientUtil = new HttpClientUtil(factory);
```

#### **Root Cause**: Creating new `HttpClient` instances frequently can exhaust sockets

---

### 2. **Authentication Failures**

#### **Problem**: 401 Unauthorized responses
```
Microsoft.Azure.DevOps.WebApi.VssUnauthorizedException: VS403403: The request has been denied
```

#### **Solutions**:

**Check Personal Access Token (PAT)**:
```csharp
// Ensure PAT has correct permissions
var client = new CoreApiClient(httpClient, "https://dev.azure.com/yourorg/", "your-pat-token");
```

**Verify Token Permissions**:
- Go to Azure DevOps → User Settings → Personal Access Tokens
- Ensure token has required scopes (e.g., "Work Items (Read)", "Project and Team (Read)")

**Check Token Expiration**:
```csharp
// Add logging to verify token is being sent
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
```

---

### 3. **Timeout Issues**

#### **Problem**: Requests timing out
```
System.Threading.Tasks.TaskCanceledException: The operation was canceled
```

#### **Solutions**:

**Configure Timeout Policies**:
```csharp
var retryOptions = new RetryOptions
{
    MaxRetryAttempts = 3,
    BaseDelay = TimeSpan.FromSeconds(2),
    MaxDelay = TimeSpan.FromSeconds(30)
};

var client = new CoreApiClient(httpClient, baseUrl, pat, retryOptions);
```

**Use CancellationToken**:
```csharp
var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token;
var result = await client.GetTeamsAsync(projectId, cancellationToken);
```

---

### 4. **JSON Serialization Errors**

#### **Problem**: Deserialization failures
```
Newtonsoft.Json.JsonSerializationException: Error converting value
```

#### **Solutions**:

**Check API Response Format**:
```csharp
// Enable detailed logging to see raw responses
services.AddLogging(builder => 
    builder.AddConsole()
           .SetMinimumLevel(LogLevel.Trace));
```

**Handle Null Values**:
```csharp
// The library handles this automatically, but if extending:
[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
public string? OptionalProperty { get; set; }
```

---

### 5. **Circuit Breaker Activation**

#### **Problem**: Circuit breaker preventing requests
```
Polly.CircuitBreaker.CircuitBreakerOpenException: The circuit breaker is now open
```

#### **Solutions**:

**Wait for Circuit Recovery**:
- Circuit breaker opens after consecutive failures
- Waits for recovery period before allowing requests
- This is expected behavior for fault tolerance

**Check Service Health**:
```csharp
// Verify Azure DevOps service status
// https://status.dev.azure.com/
```

**Adjust Circuit Breaker Settings**:
```csharp
var retryOptions = new RetryOptions
{
    CircuitBreakerFailureThreshold = 5, // Increase threshold
    CircuitBreakerSamplingDuration = TimeSpan.FromMinutes(2),
    CircuitBreakerMinimumThroughput = 3
};
```

---

### 6. **Performance Issues**

#### **Problem**: Slow API responses

#### **Solutions**:

**Use Async/Await Properly**:
```csharp
// ✅ Correct
var teams = await client.GetTeamsAsync(projectId);

// ❌ Incorrect - blocks thread
var teams = client.GetTeamsAsync(projectId).Result;
```

**Implement Parallel Processing**:
```csharp
var projectIds = new[] { "proj1", "proj2", "proj3" };
var tasks = projectIds.Select(id => client.GetTeamsAsync(id));
var results = await Task.WhenAll(tasks);
```

**Monitor Performance**:
```csharp
// Add performance logging
using var activity = new Activity("GetTeams");
activity.Start();
var result = await client.GetTeamsAsync(projectId);
activity.Stop();
logger.LogInformation("GetTeams took {Duration}ms", activity.Duration.TotalMilliseconds);
```

---

## 🔍 Diagnostic Tools

### **Enable Detailed Logging**

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole()
           .AddFilter("ApiBase", LogLevel.Debug)
           .AddFilter("AzureDevopsApi", LogLevel.Debug)
           .SetMinimumLevel(LogLevel.Information);
});
```

### **HTTP Request/Response Logging**

```csharp
services.AddHttpClient("AzureDevOps")
        .AddLogger(); // Logs all HTTP requests/responses
```

### **Performance Monitoring**

```csharp
// Add Application Insights or similar
services.AddApplicationInsightsTelemetry();
```

---

## 📊 Health Checks

### **Basic Health Check**

```csharp
public async Task<bool> IsServiceHealthyAsync()
{
    try
    {
        var client = new CoreApiClient(httpClient, baseUrl, pat);
        var projects = await client.GetProjectsAsync();
        return projects != null;
    }
    catch
    {
        return false;
    }
}
```

### **Detailed Health Check**

```csharp
services.AddHealthChecks()
        .AddCheck<AzureDevOpsHealthCheck>("azure-devops");

public class AzureDevOpsHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform actual API call
            var isHealthy = await CheckApiConnectivity();
            return isHealthy 
                ? HealthCheckResult.Healthy("Azure DevOps API is responsive")
                : HealthCheckResult.Unhealthy("Azure DevOps API is not responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Azure DevOps API check failed", ex);
        }
    }
}
```

---

## 🐛 Debugging Tips

### **1. Enable Source Link**
- The library includes source link support
- Step through library code during debugging

### **2. Use Fiddler/Postman**
- Capture HTTP traffic to see actual requests/responses
- Verify headers, authentication, and payload

### **3. Check Azure DevOps Logs**
- Azure DevOps provides audit logs
- Check for rate limiting or permission issues

### **4. Validate Configuration**
```csharp
// Add configuration validation
public void ValidateConfiguration()
{
    if (string.IsNullOrEmpty(baseUrl))
        throw new ArgumentException("Base URL is required");
    
    if (string.IsNullOrEmpty(personalAccessToken))
        throw new ArgumentException("Personal Access Token is required");
    
    if (!Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
        throw new ArgumentException("Base URL must be a valid URI");
}
```

---

## 📞 Getting Help

### **1. Check Documentation**
- [Architecture Guide](./ARCHITECTURE.md)
- [API Reference](./API_REFERENCE.md)
- [Examples](../src/examples/)

### **2. Enable Verbose Logging**
- Set log level to `Trace` or `Debug`
- Check for detailed error messages

### **3. Reproduce with Minimal Example**
```csharp
// Create minimal reproduction case
var services = new ServiceCollection();
services.AddHttpClient();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var serviceProvider = services.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
var client = new CoreApiClient(httpClientFactory, "https://dev.azure.com/yourorg/", "your-pat");

try
{
    var result = await client.GetTeamsAsync("your-project-id");
    Console.WriteLine($"Success: {result?.Count} teams found");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
```

### **4. Common Environment Issues**
- **Proxy Settings**: Check corporate proxy configuration
- **Firewall**: Ensure outbound HTTPS (443) is allowed
- **DNS**: Verify `dev.azure.com` resolves correctly
- **TLS**: Ensure TLS 1.2+ is supported

---

## ⚡ Quick Fixes

| Issue | Quick Fix |
|-------|-----------|
| Socket exhaustion | Use `IHttpClientFactory` |
| 401 Unauthorized | Check PAT permissions and expiration |
| Timeout | Increase timeout or use `CancellationToken` |
| Circuit breaker open | Wait for recovery or check service health |
| JSON errors | Enable debug logging to see raw responses |
| Memory leaks | Ensure proper `using` statements |
| Performance | Use async/await and parallel processing |

---

*For additional support, please check the [examples](../src/examples/) or create an issue with detailed logs and reproduction steps.*
