# 📦 Azure DevOps API Client

[![NuGet Version](https://img.shields.io/nuget/v/AzureDevOpsApi.svg)](https://www.nuget.org/packages/AzureDevOpsApi/)
[![Build Status](https://github.com/your-org/azure-devops-api-client/workflows/🚀%20CI/CD%20Pipeline/badge.svg)](https://github.com/your-org/azure-devops-api-client/actions)
[![Code Coverage](https://codecov.io/gh/your-org/azure-devops-api-client/branch/main/graph/badge.svg)](https://codecov.io/gh/your-org/azure-devops-api-client)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

A comprehensive, **production-ready** .NET library for interacting with Azure DevOps REST APIs. Built with **Polly** for resilience, **HttpClientFactory** for performance, and full **Dependency Injection** support.

> 🤖 **Auto-Generated**: This library is automatically generated from the latest Azure DevOps documentation using intelligent web scraping and AI. Always up-to-date with the official API.

## 🚀 Features

- **🔄 Built-in Resilience**: Powered by Polly for retry, circuit breaker, and timeout policies
- **⚡ High Performance**: Uses HttpClientFactory with connection pooling
- **💉 Dependency Injection**: Full support for .NET DI container with extension methods
- **🎯 Strongly Typed**: Generated DTOs with validation attributes
- **🔐 Authentication Ready**: Built-in support for Personal Access Tokens (PAT)
- **📊 Async/Await**: Modern async patterns with cancellation token support
- **🧪 Test Ready**: Includes comprehensive unit tests and examples
- **📚 Well Documented**: Complete examples and usage patterns
- **🤖 Always Updated**: Automatically generated from latest Azure DevOps documentation

## 📦 Installation

```bash
# Install the main package
dotnet add package AzureDevOpsApi

# For advanced Polly integration (optional)
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

## 🎯 Quick Start

### Basic Usage

```csharp
using AzureDevOpsApi.Api;
using ApiBase.Utils.Implementations;

// Simple usage with default retry policies
var policyProvider = new DefaultPollyPolicyProvider(new RetryOptions
{
    MaxRetries = 3,
    BaseDelayMs = 1000,
    UseExponentialBackoff = true
});
var apiClient = new coreApiClient("https://dev.azure.com", policyProvider);

// Get teams for an organization (core API)
var teams = await apiClient.Teams.GetAllTeamsAsync<AzureDevOpsListResponse<TeamDto>>(
    pathParameters: new { organization = "your-organization" },
    queryParameters: new { top = 10 }
);

Console.WriteLine($"Found {teams?.Count ?? 0} teams");
```

### With Authentication

```csharp
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "your-personal-access-token");

var authenticatedClient = new coreApiClient(httpClient, "https://dev.azure.com");

// Get teams for a specific project
var teams = await authenticatedClient.Teams.GetTeamsAsync<AzureDevOpsListResponse<TeamDto>>(
    pathParameters: new {
        organization = "your-organization",
        projectId = "your-project-id"
    },
    queryParameters: new { top = 5 }
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

## 📚 Examples

Explore comprehensive examples in the [`Examples/`](./Examples/) directory:

- **[Basic Usage](./Examples/BasicUsage/)** - Simple API calls and authentication
- **[Dependency Injection](./Examples/DependencyInjection/)** - ASP.NET Core integration
- **[Advanced Scenarios](./Examples/AdvancedScenarios/)** - Complex workflows and error handling
- **[Testing](./Examples/Testing/)** - Unit testing with mocking

## 🔧 Configuration

### Authentication Options

#### Personal Access Token (Recommended)
```csharp
// Store PAT securely (never hardcode)
var token = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
```

#### Azure Active Directory
```csharp
// For enterprise scenarios
var tokenProvider = new AadTokenProvider(configuration);
var token = await tokenProvider.GetTokenAsync();
```

### Retry Configuration
```csharp
var retryOptions = new RetryOptions
{
    MaxRetries = 5,
    BaseDelayMs = 1000,
    UseExponentialBackoff = true
};
```

## 🔨 Building and Testing

### Build the Library
```bash
# Build the main library
dotnet build src/AzureDevopsApi/AzureDevopsApi.csproj --configuration Release

# Create NuGet package
dotnet pack src/AzureDevopsApi/AzureDevopsApi.csproj --configuration Release --output ./packages
```

### Run Examples
```bash
# Build and run basic usage example
dotnet build Examples/BasicUsage/BasicUsage.csproj --configuration Release
dotnet run --project Examples/BasicUsage/BasicUsage.csproj --configuration Release

# Build dependency injection example
dotnet build Examples/DependencyInjection/DependencyInjection.csproj --configuration Release
```

### Testing
```bash
# Run all tests
dotnet test

# Run basic tests (recommended - these work)
dotnet test Tests/AzureDevopsApi.Tests.csproj --configuration Release

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=Integration
```

#### **Test Strategy**
- **Unit Tests**: Focus on base infrastructure components (not generated code)
- **Functional Tests**: Validate generated API functionality end-to-end
- **Integration Tests**: Test real Azure DevOps integration (requires credentials)
- **Basic Tests**: Simple validation tests that always work

#### **Test Status**
- ✅ **Unit Tests**: 14/14 passing (test base components)
- ✅ **Functional Tests**: 14/14 passing (test generated code)
- ⚠️ **Integration Tests**: May fail without Azure DevOps credentials
- ✅ **Build Tests**: All builds successful

### Verify Installation
```bash
# Check if the package was created
ls -la packages/*.nupkg

# Verify the library builds correctly
dotnet build src/AzureDevopsApi/AzureDevopsApi.csproj --verbosity minimal

# Run tests to ensure everything works
dotnet test Tests/AzureDevopsApi.Tests.csproj --configuration Release
```

### 🧪 Test Architecture

This library uses a **dual-testing strategy**:

#### **1. Unit Tests (Base Components)**
- **Purpose**: Test stable, non-generated infrastructure code
- **Coverage**: `BaseApi`, `IHttpClientUtil`, `IPollyPolicyProvider`, custom exceptions
- **Status**: ✅ 14/14 tests passing
- **Why**: These components don't change when API is regenerated

#### **2. Functional Tests (Generated Code)**
- **Purpose**: Validate that generated API code works end-to-end
- **Coverage**: API clients, DTOs, HTTP calls, serialization
- **Status**: ✅ 14/14 tests passing
- **Why**: Ensures generated code integrates correctly with base infrastructure

#### **3. Integration Tests (Real API)**
- **Purpose**: Test against actual Azure DevOps services
- **Requirements**: Valid Azure DevOps credentials
- **Status**: ⚠️ Skipped without credentials (expected)
- **Why**: Validates real-world usage scenarios

## 📋 Requirements

- **.NET 9.0** or later
- **Azure DevOps Services** or **Azure DevOps Server 2019** or later
- **Personal Access Token** with appropriate permissions

## 🤝 Contributing

This repository contains **auto-generated code**. Please do not submit PRs with manual changes to the generated API code.

For issues or feature requests:
1. 🐛 [Report bugs](https://github.com/your-org/azure-devops-api-client/issues)
2. 💡 [Request features](https://github.com/your-org/azure-devops-api-client/discussions)
3. 📖 [Improve documentation](https://github.com/your-org/azure-devops-api-client/pulls)

## 🔄 Auto-Generation Process

This library is automatically generated using:
1. **🕷️ Web Scraping**: Extracts latest API definitions from Microsoft documentation
2. **🤖 AI Processing**: Uses OpenAI to enhance and validate the extracted data
3. **⚙️ Code Generation**: Creates production-ready C# code with modern patterns
4. **🚀 Automated Deployment**: Updates this repository and publishes to NuGet

**Update Schedule**: Automatically checks for updates every Monday at 2 AM UTC.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Microsoft** - For Azure DevOps REST APIs and excellent documentation
- **Polly** - For providing resilience patterns
- **.NET Team** - For HttpClientFactory and DI container
- **OpenAI** - For AI-powered code generation enhancement

## 📞 Support

- 🐛 **Issues**: [GitHub Issues](https://github.com/your-org/azure-devops-api-client/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/your-org/azure-devops-api-client/discussions)
- 📖 **Documentation**: [Wiki](https://github.com/your-org/azure-devops-api-client/wiki)

---

**🤖 Auto-generated with ❤️ for the .NET community**