# 🚀 Quick Start Guide

Get up and running with the Azure DevOps API Client Library in minutes!

## 📋 Prerequisites

- **.NET 9.0** SDK or later
- **Azure DevOps** account with a project
- **Personal Access Token (PAT)** with appropriate permissions

## 🔑 Step 1: Create Personal Access Token

1. Go to your Azure DevOps organization: `https://dev.azure.com/{your-organization}`
2. Click on **User Settings** → **Personal Access Tokens**
3. Click **New Token**
4. Configure your token:
   - **Name**: `API Client Token`
   - **Expiration**: Choose appropriate duration
   - **Scopes**: Select required permissions:
     - ✅ **Project and team** (read/write)
     - ✅ **Work items** (read/write)
     - ✅ **Code** (read) - for Git operations
5. Click **Create** and **copy the token** (you won't see it again!)

## 📦 Step 2: Install Package

```bash
# Create new console application
dotnet new console -n MyAzureDevOpsApp
cd MyAzureDevOpsApp

# Install the Azure DevOps API package
dotnet add package AzureDevOpsApi

# Install Polly for resilience (optional but recommended)
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

## 🎯 Step 3: Basic Usage

Create a simple console application:

```csharp
using AzureDevOpsApi.Api;
using ApiBase.Utils.Implementations;

// Replace with your values
const string organization = "your-organization";
const string personalAccessToken = "your-pat-token";

// Create HTTP client with authentication
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", personalAccessToken);

// Create API client with retry policies
var retryOptions = new RetryOptions
{
    MaxRetries = 3,
    BaseDelayMs = 1000,
    UseExponentialBackoff = true
};

var apiClient = new CoreApiClient(httpClient, "https://dev.azure.com", retryOptions);

// Get projects
try
{
    var projects = await apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
        pathParameters: new { organization },
        queryParameters: new { stateFilter = "wellFormed", top = 10 }
    );

    Console.WriteLine($"Found {projects?.Count ?? 0} projects:");
    
    if (projects?.Value != null)
    {
        foreach (var project in projects.Value)
        {
            Console.WriteLine($"  • {project.Name} ({project.State})");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## 🏗️ Step 4: ASP.NET Core Integration

For web applications, use dependency injection:

### Program.cs

```csharp
using ApiBase.Extensions;
using ApiBase.Utils.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Register Azure DevOps API client (baseUrl defaults to https://dev.azure.com)
builder.Services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    configureHttpClient: client =>
    {
        var token = builder.Configuration["AzureDevOps:PersonalAccessToken"];
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

var app = builder.Build();

// Configure pipeline
app.UseRouting();
app.MapControllers();

app.Run();
```

### appsettings.json

```json
{
  "AzureDevOps": {
    "PersonalAccessToken": "your-pat-token-here"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly CoreApiClient _azureDevOpsClient;

    public ProjectsController(CoreApiClient azureDevOpsClient)
    {
        _azureDevOpsClient = azureDevOpsClient;
    }

    [HttpGet("{organization}")]
    public async Task<IActionResult> GetProjects(string organization)
    {
        try
        {
            var projects = await _azureDevOpsClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                pathParameters: new { organization },
                queryParameters: new { stateFilter = "wellFormed" }
            );

            return Ok(projects?.Value ?? new List<TeamProjectDto>());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
```

## 🔄 Step 5: Advanced Polly Configuration

For production applications, configure advanced resilience policies:

```csharp
using Polly;
using Polly.Extensions.Http;

// Create advanced policies
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timespan} delay");
        });

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30));

var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

// Use with API client
var apiClient = new CoreApiClient(httpClient, "https://dev.azure.com", combinedPolicy);
```

## 🧪 Step 6: Testing

Create unit tests for your Azure DevOps integrations:

```csharp
using Moq;
using Moq.Protected;
using Xunit;

public class AzureDevOpsServiceTests
{
    [Fact]
    public async Task GetProjects_Success_ReturnsProjects()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var expectedResponse = new AzureDevOpsListResponse<TeamProjectDto>
        {
            Count = 1,
            Value = new List<TeamProjectDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Test Project" }
            }
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var apiClient = new CoreApiClient(httpClient, "https://dev.azure.com");

        // Act
        var result = await apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
            pathParameters: new { organization = "test-org" }
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count);
        Assert.Equal("Test Project", result.Value[0].Name);
    }
}
```

## 🔒 Step 7: Security Best Practices

### Environment Variables

```bash
# Set environment variables (recommended for production)
export AZURE_DEVOPS_PAT="your-pat-token"
export AZURE_DEVOPS_ORG="your-organization"
```

### Azure Key Vault (Production)

```csharp
// For production applications
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());

// Use in configuration
var token = builder.Configuration["AzureDevOps--PersonalAccessToken"];
```

## 🎉 You're Ready!

You now have a fully functional Azure DevOps API client with:

- ✅ **Resilience**: Built-in retry and circuit breaker policies
- ✅ **Performance**: HttpClientFactory with connection pooling
- ✅ **Testability**: Easy to mock and unit test
- ✅ **Security**: Secure token management
- ✅ **Scalability**: Dependency injection ready

## 📚 Next Steps

- Explore the [full documentation](README.md)
- Check out [advanced examples](Examples/)
- Learn about [error handling patterns](README.md#error-handling)
- Implement [custom Polly policies](README.md#advanced-polly-integration)

## 🆘 Need Help?

- 📖 [Full Documentation](README.md)
- 🐛 [Report Issues](https://github.com/yourorg/azure-devops-api/issues)
- 💬 [Discussions](https://github.com/yourorg/azure-devops-api/discussions)
