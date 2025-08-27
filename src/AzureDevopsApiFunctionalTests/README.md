# Azure DevOps API Functional Tests

This project contains **functional tests** that verify the behavior of Azure DevOps API clients using **WireMock** to simulate API responses. These tests validate the complete request/response flow without requiring real Azure DevOps services.

## 🎯 Purpose

Functional tests verify:
- **Request/response serialization** and deserialization
- **HTTP client behavior** with various response scenarios
- **Error handling** for different HTTP status codes
- **Retry mechanisms** with simulated failures
- **Performance** with large responses
- **Authentication** header handling

## 🔧 Technology Stack

- **xUnit**: Test framework
- **WireMock.Net**: HTTP service mocking
- **FluentAssertions**: Readable assertions
- **Moq**: Object mocking (when needed)
- **.NET 9**: Target framework

## 🧪 Test Categories

### 1. Happy Path Tests
- Successful API responses
- Correct deserialization
- Valid request formatting
- Authentication headers

### 2. Error Handling Tests
- HTTP 404 (Not Found)
- HTTP 401 (Unauthorized)
- HTTP 403 (Forbidden)
- HTTP 500 (Internal Server Error)
- Network timeouts

### 3. Resilience Tests
- Retry on transient failures (503, 502)
- Circuit breaker behavior
- Timeout handling
- Exponential backoff verification

### 4. Performance Tests
- Large response handling
- Response time validation
- Memory usage patterns
- Concurrent request handling

### 5. Edge Cases
- Empty responses
- Malformed JSON
- Unexpected response formats
- Missing required fields

## 🚀 Running Tests

### All Functional Tests
```bash
dotnet test --configuration Release
```

### Specific Test Class
```bash
dotnet test --filter "ClassName=AzureDevOpsApiFunctionalTests"
```

### With Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## 🔍 Test Structure

### WireMock Setup
Each test class:
1. **Starts WireMock server** on random port
2. **Configures mock responses** for specific endpoints
3. **Creates HttpClient** pointing to mock server
4. **Executes API calls** through generated clients
5. **Verifies responses** and behavior
6. **Cleans up resources** after tests

### Example Test Pattern
```csharp
[Fact]
public async Task GetProjects_WithValidResponse_ShouldDeserializeCorrectly()
{
    // Arrange - Setup mock response
    _mockServer
        .Given(Request.Create().WithPath("/_apis/projects"))
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(expectedResponse));

    // Act - Execute API call
    var result = await _apiClient.GetProjectsAsync();

    // Assert - Verify behavior
    result.Should().NotBeNull();
    result.Count.Should().Be(2);
}
```

## 🎭 Mock Scenarios

### 1. Success Scenarios
```csharp
// Standard successful response
.RespondWith(Response.Create()
    .WithStatusCode(200)
    .WithHeader("Content-Type", "application/json")
    .WithBodyAsJson(successResponse));
```

### 2. Error Scenarios
```csharp
// Not Found
.RespondWith(Response.Create()
    .WithStatusCode(404)
    .WithBody("Project not found"));

// Unauthorized
.RespondWith(Response.Create()
    .WithStatusCode(401)
    .WithBody("Unauthorized"));
```

### 3. Retry Scenarios
```csharp
// First call fails, second succeeds
_mockServer
    .Given(Request.Create().WithPath("/api/endpoint"))
    .InScenario("Retry Test")
    .WillSetStateTo("FirstCall")
    .RespondWith(Response.Create().WithStatusCode(503));

_mockServer
    .Given(Request.Create().WithPath("/api/endpoint"))
    .InScenario("Retry Test")
    .WhenStateIs("FirstCall")
    .RespondWith(Response.Create().WithStatusCode(200));
```

### 4. Performance Scenarios
```csharp
// Large response with delay
.RespondWith(Response.Create()
    .WithStatusCode(200)
    .WithBodyAsJson(largeResponse)
    .WithDelay(TimeSpan.FromMilliseconds(100)));
```

## 📊 Test Coverage

### API Endpoints Covered
- **Projects**: List, Get, Create, Update, Delete
- **Teams**: List, Get team members
- **Repositories**: List, Get repository details
- **Work Items**: Query, Get, Create, Update
- **Builds**: List, Get build details
- **Releases**: List, Get release details

### HTTP Methods Tested
- **GET**: Retrieve operations
- **POST**: Create operations
- **PUT**: Update operations
- **PATCH**: Partial update operations
- **DELETE**: Delete operations

### Response Types Tested
- **JSON objects**: Single entities
- **JSON arrays**: Collections
- **Paginated responses**: With continuation tokens
- **Empty responses**: No content scenarios
- **Error responses**: Various error formats

## 🛡️ Test Isolation

### Per-Test Isolation
- Each test gets fresh WireMock server
- No shared state between tests
- Independent mock configurations
- Proper resource cleanup

### Deterministic Behavior
- Fixed response data
- Predictable timing
- Controlled error scenarios
- Repeatable test runs

## 🔧 Configuration

### Test Settings
```json
{
  "FunctionalTests": {
    "MockServerPort": 0,  // 0 = random port
    "RequestTimeout": 30,
    "EnableDetailedLogging": true
  }
}
```

### Environment Variables
```bash
FUNCTIONAL_TESTS_ENABLED=true
WIREMOCK_VERBOSE_LOGGING=true
```

## 🎯 Best Practices

### 1. Realistic Mock Data
- Use actual Azure DevOps response formats
- Include all required fields
- Match real API behavior
- Validate against OpenAPI specs

### 2. Comprehensive Error Testing
- Test all relevant HTTP status codes
- Verify error message handling
- Check exception types
- Validate error response parsing

### 3. Performance Validation
- Test with realistic data sizes
- Verify timeout handling
- Check memory usage
- Validate concurrent scenarios

### 4. Maintainable Tests
- Use helper methods for common setups
- Extract constants for repeated values
- Clear test names describing scenarios
- Proper test organization

## 🔍 Debugging

### WireMock Request Logging
```csharp
_mockServer = WireMockServer.Start(new WireMockServerSettings
{
    Logger = new WireMockConsoleLogger(),
    RequestLogExpirationDuration = TimeSpan.FromHours(1)
});
```

### Test Output
Tests provide detailed output:
- Mock server URLs
- Request/response details
- Timing information
- Error messages
- Assertion failures

## 🚀 CI/CD Integration

### Fast Execution
- No external dependencies
- Deterministic results
- Parallel test execution
- Quick feedback loop

### Build Pipeline
```yaml
- name: Run Functional Tests
  run: dotnet test src/AzureDevopsApi/AzureDevopsApiFunctionalTests
  env:
    FUNCTIONAL_TESTS_ENABLED: true
```

## 📈 Benefits

1. **Fast Execution**: No network calls to real services
2. **Reliable**: Not affected by external service availability
3. **Comprehensive**: Can test error scenarios easily
4. **Isolated**: No side effects on real data
5. **Deterministic**: Consistent results across environments

---

**These functional tests ensure that the Azure DevOps API clients handle all scenarios correctly, from happy paths to edge cases, without requiring real Azure DevOps services.**
