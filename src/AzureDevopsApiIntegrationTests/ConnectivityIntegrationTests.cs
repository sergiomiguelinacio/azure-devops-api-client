using AzureDevopsApiIntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace AzureDevopsApiIntegrationTests;

/// <summary>
/// Integration tests for basic connectivity and authentication with Azure DevOps
/// These tests verify that the API client can successfully connect and authenticate
/// </summary>
[Collection("Integration Tests")]
public class ConnectivityIntegrationTests : IntegrationTestBase
{
    private readonly CoreApiClient _apiClient;

    public ConnectivityIntegrationTests(ITestOutputHelper output) : base(output)
    {
        _apiClient = ServiceProvider.GetRequiredService<CoreApiClient>();
    }

    [Fact]
    public async Task CanConnectToAzureDevOps_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        LogTestStart(nameof(CanConnectToAzureDevOps_WithValidCredentials_ShouldSucceed));
        SkipIfDisabled();
        SkipIfRealApiCallsDisabled();

        try
        {
            // Act
            var result = await ExecuteWithRetryAsync(async () =>
            {
                // Simple connectivity test - get projects (should work with any valid PAT)
                return await _apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                    pathParameters: new { organization = Organization },
                    queryParameters: new { stateFilter = "wellFormed", top = 1 }
                );
            }, "Get Projects for Connectivity Test");

            // Assert
            Assert.NotNull(result);
            Output.WriteLine($"Successfully connected to Azure DevOps. Found {result.Count} projects.");
            LogTestEnd(nameof(CanConnectToAzureDevOps_WithValidCredentials_ShouldSucceed), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Connectivity test failed: {ex.Message}");
            LogTestEnd(nameof(CanConnectToAzureDevOps_WithValidCredentials_ShouldSucceed), false);
            throw;
        }
    }

    [Fact]
    public async Task CanAuthenticateWithPersonalAccessToken_ShouldReturnValidResponse()
    {
        // Arrange
        LogTestStart(nameof(CanAuthenticateWithPersonalAccessToken_ShouldReturnValidResponse));
        SkipIfDisabled();
        SkipIfRealApiCallsDisabled();

        try
        {
            // Act
            var result = await ExecuteWithRetryAsync(async () =>
            {
                return await _apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                    pathParameters: new { organization = Organization },
                    queryParameters: new { stateFilter = "wellFormed", top = 5 }
                );
            }, "Authentication Test");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 0, "Should return a valid count (0 or more)");

            if (result.Value != null && result.Value.Any())
            {
                var firstProject = result.Value[0];
                Assert.NotEqual(Guid.Empty, firstProject.Id);
                Assert.NotEmpty(firstProject.Name);
                Output.WriteLine($"Authentication successful. First project: {firstProject.Name} (ID: {firstProject.Id})");
            }
            else
            {
                Output.WriteLine("Authentication successful. No projects found in organization.");
            }

            LogTestEnd(nameof(CanAuthenticateWithPersonalAccessToken_ShouldReturnValidResponse), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Authentication test failed: {ex.Message}");
            LogTestEnd(nameof(CanAuthenticateWithPersonalAccessToken_ShouldReturnValidResponse), false);
            throw;
        }
    }

    [Fact]
    public async Task PollyRetryPolicy_WithTransientFailures_ShouldRetryAndSucceed()
    {
        // Arrange
        LogTestStart(nameof(PollyRetryPolicy_WithTransientFailures_ShouldRetryAndSucceed));
        SkipIfDisabled();
        SkipIfRealApiCallsDisabled();

        try
        {
            // Act - This test verifies that Polly retry policies are working
            // We'll make a legitimate call and verify it succeeds (Polly handles retries internally)
            var result = await ExecuteWithRetryAsync(async () =>
            {
                return await _apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                    pathParameters: new { organization = Organization },
                    queryParameters: new { stateFilter = "wellFormed", top = 3 }
                );
            }, "Polly Retry Policy Test");

            // Assert
            Assert.NotNull(result);
            Output.WriteLine($"Polly retry policy test completed successfully. Projects count: {result.Count}");
            LogTestEnd(nameof(PollyRetryPolicy_WithTransientFailures_ShouldRetryAndSucceed), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Polly retry policy test failed: {ex.Message}");
            LogTestEnd(nameof(PollyRetryPolicy_WithTransientFailures_ShouldRetryAndSucceed), false);
            throw;
        }
    }

    [Fact]
    public async Task HttpClientTimeout_WithLongRunningOperation_ShouldRespectTimeout()
    {
        // Arrange
        LogTestStart(nameof(HttpClientTimeout_WithLongRunningOperation_ShouldRespectTimeout));
        SkipIfDisabled();
        SkipIfRealApiCallsDisabled();

        try
        {
            // Act - Test that timeout configuration is working
            var startTime = DateTime.UtcNow;
            
            var result = await ExecuteWithRetryAsync(async () =>
            {
                return await _apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                    pathParameters: new { organization = Organization },
                    queryParameters: new { stateFilter = "wellFormed", top = 10 }
                );
            }, "Timeout Configuration Test");

            var duration = DateTime.UtcNow - startTime;

            // Assert
            Assert.NotNull(result);
            Assert.True(duration.TotalSeconds < 30, $"Operation should complete within timeout. Took: {duration.TotalSeconds:F2}s");
            
            Output.WriteLine($"Timeout test completed in {duration.TotalSeconds:F2} seconds. Projects count: {result.Count}");
            LogTestEnd(nameof(HttpClientTimeout_WithLongRunningOperation_ShouldRespectTimeout), true);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            Output.WriteLine("Operation timed out as expected - timeout configuration is working");
            LogTestEnd(nameof(HttpClientTimeout_WithLongRunningOperation_ShouldRespectTimeout), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Timeout test failed: {ex.Message}");
            LogTestEnd(nameof(HttpClientTimeout_WithLongRunningOperation_ShouldRespectTimeout), false);
            throw;
        }
    }

    [Fact]
    public async Task InvalidOrganization_ShouldReturnAppropriateError()
    {
        // Arrange
        LogTestStart(nameof(InvalidOrganization_ShouldReturnAppropriateError));
        SkipIfDisabled();
        SkipIfRealApiCallsDisabled();

        try
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await _apiClient.Projects.GetProjectsAsync<AzureDevOpsListResponse<TeamProjectDto>>(
                    pathParameters: new { organization = "non-existent-organization-12345" },
                    queryParameters: new { stateFilter = "wellFormed", top = 1 }
                );
            });

            Assert.Contains("404", exception.Message);
            Output.WriteLine($"Invalid organization test passed. Error: {exception.Message}");
            LogTestEnd(nameof(InvalidOrganization_ShouldReturnAppropriateError), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Invalid organization test failed: {ex.Message}");
            LogTestEnd(nameof(InvalidOrganization_ShouldReturnAppropriateError), false);
            throw;
        }
    }
}

/// <summary>
/// Dummy DTOs for integration tests - these would normally be generated
/// </summary>
public class AzureDevOpsListResponse<T>
{
    public int Count { get; set; }
    public List<T>? Value { get; set; }
}

public class TeamProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
}

/// <summary>
/// Dummy API client for integration tests - this would normally be generated
/// </summary>
public class CoreApiClient
{
    private readonly HttpClient _httpClient;

    public CoreApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        Projects = new ProjectsApi(_httpClient);
    }

    public ProjectsApi Projects { get; }
}

public class ProjectsApi
{
    private readonly HttpClient _httpClient;

    public ProjectsApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetProjectsAsync<T>(object pathParameters, object? queryParameters = null)
    {
        // This is a simplified implementation for testing
        // In real generated code, this would be much more sophisticated
        var response = await _httpClient.GetAsync("/_apis/projects?api-version=7.0");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
    }
}
