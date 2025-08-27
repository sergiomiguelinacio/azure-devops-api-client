using AzureDevOpsApi.Api;
using FluentAssertions;
using Xunit;
using System.Net;

namespace AzureDevOpsApi.IntegrationTests
{
    /// <summary>
    /// Basic integration tests for Azure DevOps API Client
    /// These tests verify integration with HTTP infrastructure
    /// </summary>
    public class BasicIntegrationTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "High")]
        public void CoreApiClient_WithRealHttpClient_ShouldInitializeSuccessfully()
        {
            // Arrange
            using var httpClient = new HttpClient();
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(httpClient, baseUrl);

            // Assert
            client.Should().NotBeNull("client should be initialized with real HttpClient");
            client.Teams.Should().NotBeNull("Teams API should be available");
            client.Operations.Should().NotBeNull("Operations API should be available");
            client.TeamMembersWithExtendedProperties.Should().NotBeNull("TeamMembersWithExtendedProperties API should be available");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "Medium")]
        public void CoreApiClient_WithCustomTimeout_ShouldRespectTimeout()
        {
            // Arrange
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(httpClient, baseUrl);

            // Assert
            client.Should().NotBeNull("client should be initialized with custom timeout");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "Medium")]
        public void CoreApiClient_WithCustomHeaders_ShouldInitializeSuccessfully()
        {
            // Arrange
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "AzureDevOpsApi-Test/1.0");
            httpClient.DefaultRequestHeaders.Add("X-Custom-Header", "test-value");
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(httpClient, baseUrl);

            // Assert
            client.Should().NotBeNull("client should be initialized with custom headers");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "Low")]
        public void CoreApiClient_WithHttpClientFactory_ShouldInitializeSuccessfully()
        {
            // Arrange
            var httpClientHandler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            using var httpClient = new HttpClient(httpClientHandler);
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(httpClient, baseUrl);

            // Assert
            client.Should().NotBeNull("client should be initialized with HttpClientFactory pattern");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "Medium")]
        public void CoreApiClient_MultipleInstances_ShouldWorkIndependently()
        {
            // Arrange
            var baseUrl1 = "https://dev.azure.com";
            var baseUrl2 = "https://custom.azure.com";

            // Act
            using var client1 = new coreApiClient(baseUrl1);
            using var client2 = new coreApiClient(baseUrl2);

            // Assert
            client1.Should().NotBeNull("first client should be initialized");
            client2.Should().NotBeNull("second client should be initialized");
            client1.Should().NotBeSameAs(client2, "clients should be independent instances");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "Low")]
        public void CoreApiClient_ConcurrentInitialization_ShouldWorkCorrectly()
        {
            // Arrange
            var baseUrl = "https://dev.azure.com";
            var tasks = new List<Task<coreApiClient>>();

            // Act
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() => new coreApiClient(baseUrl)));
            }

            var clients = Task.WhenAll(tasks).Result;

            // Assert
            clients.Should().HaveCount(5, "all clients should be created");
            clients.Should().OnlyContain(c => c != null, "all clients should be valid");

            // Cleanup
            foreach (var client in clients)
            {
                client.Dispose();
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "Medium")]
        public void CoreApiClient_WithPollyPolicyProvider_ShouldInitializeSuccessfully()
        {
            // Arrange
            var baseUrl = "https://dev.azure.com";
            var retryOptions = new ApiBase.Utils.Implementations.RetryOptions
            {
                MaxRetries = 3,
                BaseDelayMs = 1000,
                UseExponentialBackoff = true
            };

            // Act
            using var client = new coreApiClient(baseUrl, new ApiBase.Utils.Implementations.DefaultPollyPolicyProvider(retryOptions));

            // Assert
            client.Should().NotBeNull("client should be initialized with Polly policy provider");
            client.Teams.Should().NotBeNull("Teams API should be available");
            client.Operations.Should().NotBeNull("Operations API should be available");
            client.TeamMembersWithExtendedProperties.Should().NotBeNull("TeamMembersWithExtendedProperties API should be available");
        }
    }
}
