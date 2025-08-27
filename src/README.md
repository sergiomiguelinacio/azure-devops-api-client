# Azure DevOps API Client Library

[![NuGet Version](https://img.shields.io/nuget/v/AzureDevOpsApi.svg)](https://www.nuget.org/packages/AzureDevOpsApi/)
[![Build Status](https://github.com/sergio/AzureDevOpsApi/workflows/🚀%20CI/CD%20Pipeline/badge.svg)](https://github.com/sergio/AzureDevOpsApi/actions)
[![Code Coverage](https://codecov.io/gh/sergio/AzureDevOpsApi/branch/main/graph/badge.svg)](https://codecov.io/gh/sergio/AzureDevOpsApi)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Semantic Versioning](https://img.shields.io/badge/semver-2.0.0-blue.svg)](https://semver.org/)

A comprehensive, production-ready .NET library for interacting with Azure DevOps REST APIs. Built with **Polly** for resilience, **HttpClientFactory** for performance, and full **Dependency Injection** support.

## 🚀 Features

- **🔄 Built-in Resilience**: Powered by Polly for retry, circuit breaker, and timeout policies
- **⚡ High Performance**: Uses HttpClientFactory with connection pooling
- **💉 Dependency Injection**: Full support for .NET DI container with extension methods
- **🎯 Strongly Typed**: Generated DTOs with validation attributes
- **🔐 Authentication Ready**: Built-in support for Personal Access Tokens (PAT)
- **📊 Async/Await**: Modern async patterns with cancellation token support
- **🧪 Test Ready**: Includes comprehensive unit tests and examples
- **📚 Well Documented**: Complete examples and usage patterns

## 📦 Installation

```bash
# Install the main package
dotnet add package AzureDevOpsApi

# For advanced Polly integration
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

## 🎯 Quick Start

### Basic Usage

```csharp
using AzureDevOpsApi.Api;
using ApiBase.Utils.Implementations;

// Simple usage with default retry policies
var apiClient = new CoreApiClient("https://dev.azure.com", new RetryOptions 
{ 
    MaxRetries = 3,
    BaseDelayMs = 1000,
    UseExponentialBackoff = true 
});

// Get projects for an organization
var projects = await apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
    pathParameters: new { organization = "your-organization" },
    queryParameters: new { stateFilter = "wellFormed", top = 10 }
);

Console.WriteLine($"Found {projects?.Count ?? 0} projects");
```

### With Authentication

```csharp
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "your-personal-access-token");

var apiClient = new CoreApiClient(httpClient, "https://dev.azure.com");

var project = await apiClient.Projects.GetProjectAsync<TeamProjectDto>(
    pathParameters: new { 
        organization = "your-organization",
        projectId = "project-id"
    }
);
```

## 💉 Dependency Injection

### ASP.NET Core Setup

```csharp
// Program.cs
using ApiBase.Extensions;
using ApiBase.Utils.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Register Azure DevOps API client with default Polly policies
builder.Services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl: "https://dev.azure.com",
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
```

### Using in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly CoreApiClient _azureDevOpsClient;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(CoreApiClient azureDevOpsClient, ILogger<ProjectsController> logger)
    {
        _azureDevOpsClient = azureDevOpsClient;
        _logger = logger;
    }

    [HttpGet("{organization}")]
    public async Task<IActionResult> GetProjects(string organization, CancellationToken cancellationToken)
    {
        try
        {
            var projects = await _azureDevOpsClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                pathParameters: new { organization },
                queryParameters: new { stateFilter = "wellFormed" },
                cancellationToken: cancellationToken
            );

            return Ok(projects?.Value ?? new List<TeamProjectDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve projects for organization {Organization}", organization);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{organization}")]
    public async Task<IActionResult> CreateProject(string organization, [FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var projectDto = new TeamProjectDto
            {
                Name = request.Name,
                Description = request.Description,
                Visibility = request.IsPrivate ? ProjectVisibility.Private : ProjectVisibility.Public
            };

            var result = await _azureDevOpsClient.Projects.CreateProjectAsync<TeamProjectDto>(
                pathParameters: new { organization },
                requestBody: projectDto,
                cancellationToken: cancellationToken
            );

            return CreatedAtAction(nameof(GetProject), new { organization, projectId = result?.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create project {ProjectName} in organization {Organization}", request.Name, organization);
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## 🔄 Advanced Polly Integration

### Custom Polly Policies

```csharp
using Polly;
using Polly.Extensions.Http;

// Create custom retry policy
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timespan} delay");
        });

// Create circuit breaker policy
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (exception, duration) =>
        {
            Console.WriteLine($"Circuit breaker opened for {duration}");
        },
        onReset: () =>
        {
            Console.WriteLine("Circuit breaker reset");
        });

// Combine policies
var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

// Use with API client
var apiClient = new CoreApiClient(httpClient, "https://dev.azure.com", combinedPolicy);
```

### Advanced DI Registration with Custom Policies

```csharp
// Register with custom Polly policy
builder.Services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl: "https://dev.azure.com",
    policy: combinedPolicy,
    configureHttpClient: client =>
    {
        var token = builder.Configuration["AzureDevOps:PersonalAccessToken"];
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
);
```

### Using Policy Provider

```csharp
using ApiBase.Utils.Implementations;
using ApiBase.Utils.Interfaces;

// Create custom policy provider
public class CustomPollyPolicyProvider : IPollyPolicyProvider
{
    public IAsyncPolicy<HttpResponseMessage> GetPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt * 2),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Custom logging or metrics
                });
    }
}

// Register with DI
builder.Services.AddSingleton<IPollyPolicyProvider, CustomPollyPolicyProvider>();
builder.Services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl: "https://dev.azure.com",
    policyProvider: serviceProvider.GetRequiredService<IPollyPolicyProvider>()
);
```

## 🔐 Authentication

### Personal Access Token (PAT)

The recommended way to authenticate with Azure DevOps APIs:

```csharp
// Store PAT securely (never hardcode in source)
// appsettings.json
{
  "AzureDevOps": {
    "Organization": "your-organization-name",
    "PersonalAccessToken": "your-pat-token-here"
    // BaseUrl defaults to "https://dev.azure.com"
    // Only specify for Azure DevOps Server: "BaseUrl": "https://tfs.company.com:8080/tfs"
  }
}

// Secure token provider
public class AzureDevOpsTokenProvider
{
    private readonly IConfiguration _configuration;

    public AzureDevOpsTokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetToken()
    {
        return _configuration["AzureDevOps:PersonalAccessToken"]
            ?? Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT")
            ?? throw new InvalidOperationException("Azure DevOps PAT not configured");
    }
}

// Register in DI
builder.Services.AddSingleton<AzureDevOpsTokenProvider>();
builder.Services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl: builder.Configuration["AzureDevOps:BaseUrl"],
    configureHttpClient: (serviceProvider, client) =>
    {
        var tokenProvider = serviceProvider.GetRequiredService<AzureDevOpsTokenProvider>();
        var token = tokenProvider.GetToken();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
);
```

### Azure Active Directory (AAD)

For enterprise scenarios with AAD integration:

```csharp
using Microsoft.Identity.Client;

public class AadTokenProvider
{
    private readonly IConfidentialClientApplication _app;

    public AadTokenProvider(IConfiguration configuration)
    {
        _app = ConfidentialClientApplicationBuilder
            .Create(configuration["AzureAd:ClientId"])
            .WithClientSecret(configuration["AzureAd:ClientSecret"])
            .WithAuthority(configuration["AzureAd:Authority"])
            .Build();
    }

    public async Task<string> GetTokenAsync()
    {
        var scopes = new[] { "https://app.vssps.visualstudio.com/.default" };
        var result = await _app.AcquireTokenForClient(scopes).ExecuteAsync();
        return result.AccessToken;
    }
}
```

## 📊 Configuration Options

### RetryOptions

```csharp
public class RetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts (default: 3)
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay between retries in milliseconds (default: 1000)
    /// </summary>
    public int BaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// Use exponential backoff (default: true)
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
}

// Usage examples
var conservativeRetry = new RetryOptions
{
    MaxRetries = 2,
    BaseDelayMs = 2000,
    UseExponentialBackoff = false // Linear backoff
};

var aggressiveRetry = new RetryOptions
{
    MaxRetries = 10,
    BaseDelayMs = 500,
    UseExponentialBackoff = true // 500ms, 1s, 2s, 4s, 8s...
};
```

### HttpClient Configuration

```csharp
builder.Services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl: "https://dev.azure.com",
    configureHttpClient: client =>
    {
        // Authentication
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Timeouts
        client.Timeout = TimeSpan.FromMinutes(10);

        // Custom headers
        client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
        client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value");

        // Accept headers
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
);
```

## 🧪 Testing

### Unit Testing with Mocking

```csharp
using Moq;
using Moq.Protected;
using Xunit;

public class ProjectServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly CoreApiClient _apiClient;

    public ProjectServiceTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object);
        _apiClient = new CoreApiClient(_httpClient, "https://dev.azure.com");
    }

    [Fact]
    public async Task GetProjects_Success_ReturnsExpectedProjects()
    {
        // Arrange
        var expectedResponse = new AzureDevOpsListResponse<TeamProjectDto>
        {
            Count = 2,
            Value = new List<TeamProjectDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Project 1", State = ProjectState.WellFormed },
                new() { Id = Guid.NewGuid(), Name = "Project 2", State = ProjectState.WellFormed }
            }
        };

        var responseJson = JsonConvert.SerializeObject(expectedResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
            pathParameters: new { organization = "test-org" }
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Project 1", result.Value[0].Name);
    }
}
```

## 📚 Common Usage Patterns

### Working with Projects

```csharp
// List all projects in an organization
var projects = await apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
    pathParameters: new { organization = "myorg" },
    queryParameters: new {
        stateFilter = "wellFormed",
        top = 100,
        skip = 0
    }
);

// Get specific project details
var project = await apiClient.Projects.GetProjectAsync<TeamProjectDto>(
    pathParameters: new {
        organization = "myorg",
        projectId = "project-guid-or-name"
    },
    queryParameters: new {
        includeCapabilities = true,
        includeHistory = false
    }
);

// Create a new project
var newProject = new TeamProjectDto
{
    Name = "My New Project",
    Description = "Project created via API",
    Visibility = ProjectVisibility.Private,
    Capabilities = new Dictionary<string, object>
    {
        ["versioncontrol"] = new { sourceControlType = "Git" },
        ["processTemplate"] = new { templateTypeId = "adcc42ab-9882-485e-a3ed-7678f01f66bc" } // Agile
    }
};

var createdProject = await apiClient.Projects.CreateProjectAsync<TeamProjectDto>(
    pathParameters: new { organization = "myorg" },
    requestBody: newProject
);
```

### Working with Work Items

```csharp
// Get work item by ID
var workItem = await apiClient.WorkItems.GetWorkItemAsync<WorkItemDto>(
    pathParameters: new {
        organization = "myorg",
        project = "myproject",
        id = 123
    },
    queryParameters: new {
        fields = "System.Title,System.State,System.AssignedTo",
        expand = "relations"
    }
);

// Query work items using WIQL
var wiqlQuery = new WiqlQueryDto
{
    Query = @"
        SELECT [System.Id], [System.Title], [System.State]
        FROM WorkItems
        WHERE [System.TeamProject] = 'MyProject'
        AND [System.State] <> 'Closed'
        ORDER BY [System.ChangedDate] DESC"
};

var queryResult = await apiClient.WorkItems.QueryByWiqlAsync<WorkItemQueryResultDto>(
    pathParameters: new { organization = "myorg", project = "myproject" },
    requestBody: wiqlQuery
);

// Create a new work item
var newWorkItem = new WorkItemDto
{
    Fields = new Dictionary<string, object>
    {
        ["System.Title"] = "New bug found",
        ["System.WorkItemType"] = "Bug",
        ["System.Description"] = "Description of the bug",
        ["System.AssignedTo"] = "user@company.com",
        ["Microsoft.VSTS.Common.Priority"] = 2,
        ["Microsoft.VSTS.Common.Severity"] = "3 - Medium"
    }
};

var createdWorkItem = await apiClient.WorkItems.CreateWorkItemAsync<WorkItemDto>(
    pathParameters: new {
        organization = "myorg",
        project = "myproject",
        type = "Bug"
    },
    requestBody: newWorkItem
);
```

### Working with Git Repositories

```csharp
// List repositories in a project
var repositories = await apiClient.Git.GetRepositoriesAsync<AzureDevOpsListResponse<GitRepositoryDto>>(
    pathParameters: new { organization = "myorg", project = "myproject" }
);

// Get repository details
var repository = await apiClient.Git.GetRepositoryAsync<GitRepositoryDto>(
    pathParameters: new {
        organization = "myorg",
        project = "myproject",
        repositoryId = "repo-guid-or-name"
    }
);

// Create a new repository
var newRepo = new GitRepositoryDto
{
    Name = "my-new-repo",
    Project = new TeamProjectReferenceDto { Id = projectId }
};

var createdRepo = await apiClient.Git.CreateRepositoryAsync<GitRepositoryDto>(
    pathParameters: new { organization = "myorg", project = "myproject" },
    requestBody: newRepo
);
```

## ⚠️ Error Handling

### Best Practices

```csharp
public class AzureDevOpsService
{
    private readonly CoreApiClient _apiClient;
    private readonly ILogger<AzureDevOpsService> _logger;

    public AzureDevOpsService(CoreApiClient apiClient, ILogger<AzureDevOpsService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<TeamProjectDto?> GetProjectSafelyAsync(string organization, string projectId)
    {
        try
        {
            return await _apiClient.Projects.GetProjectAsync<TeamProjectDto>(
                pathParameters: new { organization, projectId }
            );
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("Project {ProjectId} not found in organization {Organization}",
                projectId, organization);
            return null;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("403"))
        {
            _logger.LogError("Authentication failed for organization {Organization}: {Error}",
                organization, ex.Message);
            throw new UnauthorizedAccessException("Invalid credentials or insufficient permissions", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("Request timeout for project {ProjectId} in organization {Organization}",
                projectId, organization);
            throw new TimeoutException("Request timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting project {ProjectId} from organization {Organization}",
                projectId, organization);
            throw;
        }
    }
}
```

## 🔧 Troubleshooting

### Common Issues

#### 1. Authentication Errors (401/403)

```csharp
// Check PAT permissions and expiration
// Ensure PAT has required scopes:
// - Project and team (read/write)
// - Work items (read/write)
// - Code (read/write) for Git operations
```

#### 2. Rate Limiting (429)

```csharp
// Increase retry delays and implement exponential backoff
var retryOptions = new RetryOptions
{
    MaxRetries = 5,
    BaseDelayMs = 2000, // Start with 2 seconds
    UseExponentialBackoff = true
};
```

#### 3. Timeout Issues

```csharp
// Increase HttpClient timeout for large operations
builder.Services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl: "https://dev.azure.com",
    configureHttpClient: client =>
    {
        client.Timeout = TimeSpan.FromMinutes(10); // Increase timeout
    }
);
```

## 📋 Requirements

- **.NET 9.0** or later
- **Azure DevOps Services** or **Azure DevOps Server 2019** or later
- **Personal Access Token** with appropriate permissions

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Polly** - For providing excellent resilience patterns
- **Microsoft** - For Azure DevOps REST APIs
- **.NET Team** - For HttpClientFactory and DI container

## 📞 Support

- 📧 **Email**: support@yourcompany.com
- 🐛 **Issues**: [GitHub Issues](https://github.com/yourorg/azure-devops-api/issues)
- 📖 **Documentation**: [Wiki](https://github.com/yourorg/azure-devops-api/wiki)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/yourorg/azure-devops-api/discussions)

---

**Made with ❤️ for the .NET community**
