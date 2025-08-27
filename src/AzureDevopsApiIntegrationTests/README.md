# Azure DevOps API Integration Tests

This project contains **integration tests** that verify real communication with Azure DevOps APIs. These tests are designed to validate that the generated API clients can successfully connect to and interact with actual Azure DevOps services.

## 🎯 Purpose

Integration tests verify:
- **Real API connectivity** with Azure DevOps services
- **Authentication** using Personal Access Tokens
- **Polly resilience patterns** in real-world scenarios
- **Network timeouts** and error handling
- **End-to-end request/response flow**

## 🔧 Configuration

### Required Settings

Create `appsettings.Development.json` with your test credentials:

```json
{
  "AzureDevOps": {
    "Organization": "your-test-organization",
    "PersonalAccessToken": "your-test-pat-token",
    "TestProject": "your-test-project"
  },
  "IntegrationTests": {
    "Enabled": true,
    "SkipRealApiCalls": false,
    "TimeoutSeconds": 60,
    "MaxRetries": 5
  }
}
```

### Environment Variables (Alternative)

```bash
export AZURE_DEVOPS_PAT="your-personal-access-token"
```

### Test Control

- **`IntegrationTests:Enabled`**: Enable/disable all integration tests
- **`IntegrationTests:SkipRealApiCalls`**: Skip tests that make real API calls
- **`IntegrationTests:TimeoutSeconds`**: HTTP client timeout
- **`IntegrationTests:MaxRetries`**: Retry attempts for failed operations

## 🧪 Test Categories

### 1. Connectivity Tests
- Basic connection to Azure DevOps
- Authentication validation
- Network timeout verification
- Invalid organization handling

### 2. Polly Integration Tests
- Retry policy verification
- Circuit breaker testing
- Timeout policy validation
- Custom policy testing

### 3. API Operation Tests
- Project listing and retrieval
- CRUD operations (when safe)
- Query parameter handling
- Response deserialization

## 🚀 Running Tests

### All Integration Tests
```bash
dotnet test --configuration Release
```

### Specific Test Category
```bash
dotnet test --filter "Category=Connectivity"
dotnet test --filter "Category=Polly"
```

### Skip Real API Calls
Set `IntegrationTests:SkipRealApiCalls` to `true` in configuration.

## 🛡️ Safety Features

### Test Isolation
- Tests use dedicated test organization/project
- No modification of production data
- Read-only operations preferred
- Cleanup after destructive tests

### Conditional Execution
- Tests skip if configuration is missing
- Tests skip if real API calls are disabled
- Tests skip if Azure DevOps is unreachable

### Error Handling
- Graceful handling of network failures
- Proper exception assertions
- Detailed logging for debugging

## 📊 Test Structure

### Base Class: `IntegrationTestBase`
Provides common functionality:
- Configuration management
- Service provider setup
- Logging infrastructure
- Retry mechanisms
- Test lifecycle management

### Test Classes
- **`ConnectivityIntegrationTests`**: Basic connectivity and auth
- **`ProjectsIntegrationTests`**: Project-related operations
- **`PollyIntegrationTests`**: Resilience pattern validation

## 🔍 Debugging

### Enable Detailed Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System.Net.Http.HttpClient": "Debug"
    }
  }
}
```

### Test Output
Tests write detailed information to test output:
- Request/response details
- Timing information
- Error messages
- Configuration values

## ⚠️ Important Notes

### Security
- **Never commit real PAT tokens** to source control
- Use environment variables or user secrets
- Rotate test tokens regularly
- Use minimal permissions for test tokens

### Performance
- Integration tests are slower than unit tests
- Tests may be affected by network conditions
- Azure DevOps rate limiting may apply
- Consider running in CI/CD with appropriate timeouts

### Reliability
- Tests may fail due to network issues
- Azure DevOps service availability affects tests
- Use appropriate retry policies
- Monitor test flakiness

## 🎯 Best Practices

1. **Idempotent Tests**: Tests should not depend on previous test state
2. **Minimal Scope**: Use least privilege for test credentials
3. **Clear Assertions**: Verify specific behaviors, not just "no exceptions"
4. **Proper Cleanup**: Clean up any created resources
5. **Meaningful Names**: Test names should describe the scenario being tested

## 🔄 CI/CD Integration

### GitHub Actions Example
```yaml
- name: Run Integration Tests
  run: dotnet test src/AzureDevopsApi/AzureDevopsApiIntegrationTests
  env:
    AZURE_DEVOPS_PAT: ${{ secrets.AZURE_DEVOPS_PAT }}
    IntegrationTests__Enabled: true
```

### Azure DevOps Pipeline Example
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Integration Tests'
  inputs:
    command: 'test'
    projects: 'src/AzureDevopsApi/AzureDevopsApiIntegrationTests'
  env:
    AZURE_DEVOPS_PAT: $(AzureDevOpsPAT)
```

---

**These integration tests ensure that the generated Azure DevOps API clients work correctly in real-world scenarios with actual Azure DevOps services.**
