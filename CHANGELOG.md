# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-01-15

### 🚨 BREAKING CHANGES
- **Package renamed** from `AzureDevOpsApi` to `AzureDevOpsClient`
- **Namespace changed** from `AzureDevOpsApi` to `AzureDevOpsClient`
- **Assembly name changed** to `AzureDevOpsClient`

### ✨ Added
- **Structured Logging**: Full ILogger integration with automatic operation logging
  - Operation start/completion logging with timing
  - Debug-level URL construction logging
  - Error logging with full context and stack traces
  - Support for any ILogger provider (Serilog, Application Insights, etc.)
  
- **Performance Metrics**: Automatic collection of operation metrics
  - Operation duration tracking
  - Success/failure rate monitoring
  - Endpoint performance analysis
  
- **Enhanced Observability**:
  - Correlation ID support for request tracing
  - Structured log output for monitoring tools
  - Integration examples for Application Insights, Elasticsearch
  
- **Improved Error Handling**:
  - Detailed error logging with operation context
  - Timing information for failed operations
  - Better debugging information

### 🔧 Enhanced
- **Constructor Overloads**: All API classes now accept optional ILogger<T> parameter
- **Null Logger Fallback**: Uses NullLogger when no logger is provided
- **Documentation**: Comprehensive logging and observability documentation
- **Examples**: Added logging integration examples

### 📦 Dependencies
- Added `Microsoft.Extensions.Logging.Abstractions` 9.0.0

### 🔄 Migration Guide

#### Package Reference
```xml
<!-- Old -->
<PackageReference Include="AzureDevOpsApi" Version="1.0.0" />

<!-- New -->
<PackageReference Include="AzureDevOpsClient" Version="2.0.0" />
```

#### Namespace Updates
```csharp
// Old
using AzureDevOpsApi.Api.Interface.core;
using AzureDevOpsApi.Api.Implementation.core;

// New
using AzureDevOpsClient.Api.Interface.core;
using AzureDevOpsClient.Api.Implementation.core;
```

#### Logger Integration (Optional)
```csharp
// Basic usage (no breaking changes)
var teamsApi = new Teams(httpClient, baseUrl);

// With logging (new feature)
var logger = loggerFactory.CreateLogger<Teams>();
var teamsApi = new Teams(httpClient, baseUrl, logger: logger);

// With DI (recommended)
services.AddLogging(builder => builder.AddConsole());
services.AddAzureDevOpsApiClient("https://dev.azure.com/yourorg");
```

### 📊 What You Get

After upgrading, every API operation automatically logs:

```
[10:30:15 INF] Starting GET operation: GetTeams for path: /{organization}/_apis/projects/{projectId}/teams
[10:30:15 DBG] Built URL: https://dev.azure.com/myorg/_apis/projects/myproject/teams for operation: GetTeams
[10:30:16 INF] Completed GET operation: GetTeams in 1250ms
```

### 🔗 Integration Examples

**Serilog:**
```csharp
services.AddLogging(builder => builder.AddSerilog());
```

**Application Insights:**
```csharp
services.AddLogging(builder => builder.AddApplicationInsights("key"));
```

**Console + File:**
```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddFile("logs/azuredevops-{Date}.txt");
});
```

---

## [1.0.0] - 2024-12-15

### ✨ Added
- Initial release of Azure DevOps API Client
- **Resilience Patterns**: Built-in retry policies, circuit breakers, and timeout handling using Polly
- **High Performance**: HttpClientFactory integration with connection pooling
- **Dependency Injection**: Full support for .NET DI container
- **Strongly Typed**: Auto-generated DTOs with validation attributes
- **Authentication**: Built-in support for Personal Access Tokens (PAT)
- **Async/Await**: Modern async patterns with cancellation token support

### 🔧 Core Features
- **Core API**: Teams, Operations, TeamMembersWithExtendedProperties
- **Auto-Generated**: Generated from latest Azure DevOps documentation
- **Production Ready**: Comprehensive error handling and resilience patterns

### 📦 Dependencies
- Newtonsoft.Json 13.0.3
- Polly 8.4.2
- Polly.Extensions.Http 3.0.0
- Microsoft.Extensions.Http 9.0.0
- Microsoft.Extensions.Http.Polly 9.0.0
- Microsoft.Extensions.DependencyInjection.Abstractions 9.0.0

### 🎯 Supported Operations
- Get Teams by Project
- Get All Teams
- Get Operations
- Get Team Members with Extended Properties

---

## Links
- [NuGet Package](https://www.nuget.org/packages/AzureDevOpsClient/)
- [GitHub Repository](https://github.com/your-org/azure-devops-api-client)
- [Documentation](./README.md)
- [Examples](./Examples/)
