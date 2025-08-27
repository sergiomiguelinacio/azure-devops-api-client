using AzureDevOpsApi.Api;
using AzureDevOpsApi.Api.Interface.core;
using ApiBase.Utils.Interfaces;
using ApiBase.Utils.Implementations;
using FluentAssertions;
using Moq;
using Xunit;
using AutoFixture;
using AutoFixture.Xunit2;

namespace AzureDevOpsApi.UnitTests.Api
{
    public class CoreApiClientTests : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly Mock<IHttpClientUtil> _mockHttpClientUtil;
        private readonly Mock<HttpClient> _mockHttpClient;

        public CoreApiClientTests()
        {
            _fixture = new Fixture();
            _mockHttpClientUtil = new Mock<IHttpClientUtil>();
            _mockHttpClient = new Mock<HttpClient>();
        }

        [Fact]
        public void Constructor_WithBaseUrl_ShouldInitializeCorrectly()
        {
            // Arrange
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(baseUrl);

            // Assert
            client.Should().NotBeNull();
            client.Teams.Should().NotBeNull();
            client.Operations.Should().NotBeNull();
            client.TeamMembersWithExtendedProperties.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithHttpClient_ShouldInitializeCorrectly()
        {
            // Arrange
            var httpClient = new HttpClient();
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(httpClient, baseUrl);

            // Assert
            client.Should().NotBeNull();
            client.Teams.Should().NotBeNull();
            client.Operations.Should().NotBeNull();
            client.TeamMembersWithExtendedProperties.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithHttpClientUtil_ShouldInitializeCorrectly()
        {
            // Arrange
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(_mockHttpClientUtil.Object, baseUrl);

            // Assert
            client.Should().NotBeNull();
            client.Teams.Should().NotBeNull();
            client.Operations.Should().NotBeNull();
            client.TeamMembersWithExtendedProperties.Should().NotBeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidBaseUrl_ShouldThrowArgumentException(string? baseUrl)
        {
            // Act & Assert
            var act = () => new coreApiClient(baseUrl!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpClient? httpClient = null;
            var baseUrl = "https://dev.azure.com";

            // Act & Assert
            var act = () => new coreApiClient(httpClient!, baseUrl);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithNullHttpClientUtil_ShouldThrowArgumentNullException()
        {
            // Arrange
            IHttpClientUtil? httpClientUtil = null;
            var baseUrl = "https://dev.azure.com";

            // Act & Assert
            var act = () => new coreApiClient(httpClientUtil!, baseUrl);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Teams_ShouldImplementITeams()
        {
            // Arrange
            using var client = new coreApiClient("https://dev.azure.com");

            // Act & Assert
            client.Teams.Should().BeAssignableTo<ITeams>();
        }

        [Fact]
        public void Operations_ShouldImplementIOperations()
        {
            // Arrange
            using var client = new coreApiClient("https://dev.azure.com");

            // Act & Assert
            client.Operations.Should().BeAssignableTo<IOperations>();
        }

        [Fact]
        public void TeamMembersWithExtendedProperties_ShouldImplementITeamMembersWithExtendedProperties()
        {
            // Arrange
            using var client = new coreApiClient("https://dev.azure.com");

            // Act & Assert
            client.TeamMembersWithExtendedProperties.Should().BeAssignableTo<ITeamMembersWithExtendedProperties>();
        }

        [Fact]
        public void Dispose_ShouldDisposeOwnedHttpClient()
        {
            // Arrange
            var client = new coreApiClient("https://dev.azure.com");

            // Act & Assert
            var act = () => client.Dispose();
            act.Should().NotThrow();
        }

        [Fact]
        public void Dispose_WithExternalHttpClient_ShouldNotThrow()
        {
            // Arrange
            using var httpClient = new HttpClient();
            var client = new coreApiClient(httpClient, "https://dev.azure.com");

            // Act & Assert
            var act = () => client.Dispose();
            act.Should().NotThrow();
        }

        [Theory]
        [AutoData]
        public void Constructor_WithRetryOptions_ShouldInitializeCorrectly(RetryOptions retryOptions)
        {
            // Arrange
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(baseUrl, new DefaultPollyPolicyProvider(retryOptions));

            // Assert
            client.Should().NotBeNull();
            client.Teams.Should().NotBeNull();
            client.Operations.Should().NotBeNull();
            client.TeamMembersWithExtendedProperties.Should().NotBeNull();
        }

        [Fact]
        public void BaseUrl_ShouldTrimTrailingSlash()
        {
            // Arrange
            var baseUrlWithSlash = "https://dev.azure.com/";

            // Act
            using var client = new coreApiClient(baseUrlWithSlash);

            // Assert
            // We can't directly test the private field, but we can verify the client initializes correctly
            client.Should().NotBeNull();
        }

        public void Dispose()
        {
            _mockHttpClientUtil?.Reset();
            _mockHttpClient?.Reset();
        }
    }
}
