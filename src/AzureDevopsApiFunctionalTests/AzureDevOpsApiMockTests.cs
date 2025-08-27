using ApiBase.Exceptions;
using ApiBase.Utils.Implementations;
using FluentAssertions;
using Moq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit.Abstractions;

namespace AzureDevopsApiFunctionalTests;

/// <summary>
/// Functional tests using WireMock to simulate Azure DevOps API responses
/// These tests verify the complete request/response flow without hitting real APIs
/// </summary>
public class AzureDevOpsApiFunctionalTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly WireMockServer _mockServer;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AzureDevOpsApiFunctionalTests(ITestOutputHelper output)
    {
        _output = output;

        // Start WireMock server
        _mockServer = WireMockServer.Start();
        _baseUrl = _mockServer.Urls[0];

        // Create HttpClient pointing to mock server
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "mock-token");

        _output.WriteLine($"WireMock server started at: {_baseUrl}");
    }

    [Fact]
    public async Task GetProjects_WithValidResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var expectedResponse = new
        {
            count = 2,
            value = new[]
            {
                new { id = Guid.NewGuid(), name = "Project 1", state = "wellFormed", visibility = "private" },
                new { id = Guid.NewGuid(), name = "Project 2", state = "wellFormed", visibility = "public" }
            }
        };

        _mockServer
            .Given(Request.Create()
                .WithPath("/_apis/projects")
                .WithParam("api-version", "7.0")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(expectedResponse));

        var policyProvider = new DefaultPollyPolicyProvider();
        var httpUtil = new HttpClientUtil(_httpClient, policyProvider);

        // Act
        var result = await httpUtil.GetAsync<dynamic>("/_apis/projects?api-version=7.0");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, (int)result.count);
        _output.WriteLine($"Successfully retrieved {result.count} projects");
    }

    [Fact]
    public async Task GetProjects_WithRetryOnTransientFailure_ShouldEventuallySucceed()
    {
        // Arrange
        var expectedResponse = new
        {
            count = 1,
            value = new[] { new { id = Guid.NewGuid(), name = "Retry Test Project", state = "wellFormed" } }
        };

        // First call returns 503 (transient failure), second succeeds
        _mockServer
            .Given(Request.Create()
                .WithPath("/_apis/projects")
                .UsingGet())
            .InScenario("Retry Scenario")
            .WillSetStateTo("FirstCall")
            .RespondWith(Response.Create()
                .WithStatusCode(503)
                .WithBody("Service Temporarily Unavailable"));

        _mockServer
            .Given(Request.Create()
                .WithPath("/_apis/projects")
                .UsingGet())
            .InScenario("Retry Scenario")
            .WhenStateIs("FirstCall")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(expectedResponse));

        var retryOptions = new RetryOptions
        {
            MaxRetries = 3,
            BaseDelayMs = 100, // Fast for testing
            UseExponentialBackoff = true
        };

        var policyProvider = new DefaultPollyPolicyProvider(retryOptions);
        var httpUtil = new HttpClientUtil(_httpClient, policyProvider);

        // Act
        var result = await httpUtil.GetAsync<dynamic>("/_apis/projects?api-version=7.0");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, (int)result.count);
        _output.WriteLine("Retry mechanism worked successfully");
    }

    [Fact]
    public async Task PostProject_WithValidPayload_ShouldReturnCreatedProject()
    {
        // Arrange
        var projectRequest = new
        {
            name = "New Test Project",
            description = "A project created via functional test",
            visibility = "private"
        };

        var expectedResponse = new
        {
            id = Guid.NewGuid(),
            name = "New Test Project",
            description = "A project created via functional test",
            state = "wellFormed",
            visibility = "private"
        };

        _mockServer
            .Given(Request.Create()
                .WithPath("/_apis/projects")
                .WithParam("api-version", "7.0")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(expectedResponse));

        var policyProvider = new DefaultPollyPolicyProvider();
        var httpUtil = new HttpClientUtil(_httpClient, policyProvider);

        // Act
        var result = await httpUtil.PostAsync<dynamic>("/_apis/projects?api-version=7.0", projectRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Test Project", (string)result.name);
        Assert.Equal("wellFormed", (string)result.state);
        _output.WriteLine($"Successfully created project: {result.name}");
    }

    [Fact]
    public async Task GetProject_WithNotFound_ShouldThrowHttpRequestException()
    {
        // Arrange
        _mockServer
            .Given(Request.Create()
                .WithPath("/_apis/projects/non-existent-project")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithBody("Project not found"));

        var policyProvider = new DefaultPollyPolicyProvider();
        var httpUtil = new HttpClientUtil(_httpClient, policyProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpFailureResponseException>(async () =>
        {
            await httpUtil.GetAsync<dynamic>("/_apis/projects/non-existent-project?api-version=7.0");
        });

        Assert.Contains("404", exception.Message);
        _output.WriteLine($"Correctly handled 404 error: {exception.Message}");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _mockServer?.Stop();
        _mockServer?.Dispose();
        GC.SuppressFinalize(this);
    }
}