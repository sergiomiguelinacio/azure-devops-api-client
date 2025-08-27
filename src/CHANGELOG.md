# Changelog

All notable changes to the Azure DevOps API Client Library will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2024-08-19

### 🚀 Major Changes

#### **Upgraded to .NET 9.0**
- **BREAKING**: Updated target framework from .NET 7.0 to .NET 9.0
- Updated all Microsoft.Extensions packages to version 9.0.0
- Improved performance with latest .NET runtime optimizations

#### **Polly Integration Refactoring**
- **BREAKING**: Replaced custom retry implementation with industry-standard **Polly** library
- **NEW**: Full support for Polly 8.4.2 with modern resilience patterns
- **NEW**: Built-in circuit breaker, timeout, and bulkhead isolation policies
- **NEW**: Seamless integration with HttpClientFactory and Dependency Injection

### ✨ New Features

#### **Enhanced API Client Constructors**
```csharp
// 1. Default Polly policies with RetryOptions
new CoreApiClient(httpClient, baseUrl, retryOptions)

// 2. Custom Polly policy provider
new CoreApiClient(httpClient, baseUrl, policyProvider)

// 3. Direct IAsyncPolicy usage
new CoreApiClient(httpClient, baseUrl, asyncPolicy)
```

#### **Dependency Injection Extensions**
```csharp
// Basic registration with default policies
services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl, configureHttpClient, retryOptions);

// Advanced registration with custom policy provider
services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl, policyProvider, configureHttpClient);

// Registration with custom IAsyncPolicy
services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl, customPolicy, configureHttpClient);
```

#### **Advanced Policy Providers**
- **DefaultPollyPolicyProvider**: Basic retry with exponential backoff
- **AdvancedPollyPolicyProvider**: Retry + Circuit Breaker + Timeout
- **IPollyPolicyProvider**: Interface for custom policy implementations

#### **Comprehensive Documentation**
- **NEW**: Complete README.md with usage examples
- **NEW**: Quick Start Guide (QUICKSTART.md)
- **NEW**: Practical examples in Examples/ folder
- **NEW**: Unit testing patterns and best practices

### 🔧 Technical Improvements

#### **Resilience Patterns**
- **Retry Policy**: Exponential backoff with jitter
- **Circuit Breaker**: Prevents cascading failures
- **Timeout Policy**: Configurable operation timeouts
- **Bulkhead Isolation**: Resource isolation patterns

#### **Performance Optimizations**
- HttpClientFactory integration for connection pooling
- Efficient async/await patterns throughout
- Reduced memory allocations in hot paths
- Optimized JSON serialization/deserialization

#### **Error Handling**
- Comprehensive exception handling patterns
- Transient error detection and retry logic
- Detailed logging integration with ILogger
- Graceful degradation strategies

### 📚 Documentation & Examples

#### **New Documentation**
- Complete API reference documentation
- Authentication patterns (PAT, AAD)
- Configuration options and best practices
- Testing strategies and mocking examples

#### **Practical Examples**
- Basic usage without DI
- ASP.NET Core integration
- Advanced Polly configurations
- Real-world scenarios with error handling
- Bulk operations and batch processing

#### **Testing Support**
- Unit testing with Moq examples
- Integration testing patterns
- HttpMessageHandler mocking
- Polly policy testing strategies

### 🔄 Migration Guide

#### **From v1.x to v2.0**

**1. Update Target Framework**
```xml
<!-- Before -->
<TargetFramework>net7.0</TargetFramework>

<!-- After -->
<TargetFramework>net9.0</TargetFramework>
```

**2. Install Polly Packages**
```bash
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

**3. Update API Client Usage**
```csharp
// Before (v1.x)
var apiClient = new CoreApiClient(httpClient, baseUrl);

// After (v2.0) - with default Polly policies
var retryOptions = new RetryOptions { MaxRetries = 3 };
var apiClient = new CoreApiClient(httpClient, baseUrl, retryOptions);
```

**4. Update Dependency Injection**
```csharp
// Before (v1.x)
services.AddHttpClient<CoreApiClient>();

// After (v2.0) - with Polly integration
services.AddAzureDevOpsApiClient<CoreApiClient, CoreApiClient>(
    baseUrl: "https://dev.azure.com",
    configureHttpClient: client => { /* config */ },
    retryOptions: new RetryOptions { MaxRetries = 3 }
);
```

### ⚠️ Breaking Changes

1. **Target Framework**: Requires .NET 9.0 or later
2. **Constructor Changes**: New overloads with Polly integration
3. **Package Dependencies**: Polly packages now required
4. **Interface Changes**: IPollyPolicyProvider replaces custom retry interfaces

### 🐛 Bug Fixes

- Fixed memory leaks in HttpClient usage
- Resolved race conditions in concurrent operations
- Improved error handling for network timeouts
- Fixed JSON deserialization edge cases

### 📦 Dependencies

#### **Updated**
- Microsoft.Extensions.Http: 7.0.0 → 9.0.0
- Microsoft.Extensions.DependencyInjection: 7.0.0 → 9.0.0
- Microsoft.Extensions.Logging: 7.0.0 → 9.0.0

#### **Added**
- Polly: 8.4.2
- Polly.Extensions.Http: 3.0.0
- Microsoft.Extensions.Http.Polly: 9.0.0

### 🙏 Acknowledgments

- **Polly Team**: For providing excellent resilience patterns
- **Microsoft .NET Team**: For HttpClientFactory and DI improvements
- **Community Contributors**: For feedback and suggestions

---

## [1.0.0] - 2024-01-15

### 🎉 Initial Release

- Basic Azure DevOps REST API client
- Support for Projects, Work Items, and Git operations
- Custom retry implementation
- .NET 7.0 support
- Basic dependency injection support

---

**Legend:**
- 🚀 Major Changes
- ✨ New Features  
- 🔧 Technical Improvements
- 📚 Documentation
- 🐛 Bug Fixes
- ⚠️ Breaking Changes
- 🔄 Migration Guide
