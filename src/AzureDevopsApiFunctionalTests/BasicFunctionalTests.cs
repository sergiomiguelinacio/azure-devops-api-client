using AzureDevOpsApi.Api;
using FluentAssertions;
using Xunit;

namespace AzureDevOpsApi.FunctionalTests
{
    /// <summary>
    /// Basic functional tests for Azure DevOps API Client
    /// These tests verify the basic functionality without external dependencies
    /// </summary>
    public class BasicFunctionalTests
    {
        [Fact]
        [Trait("Category", "Functional")]
        [Trait("Priority", "High")]
        public void CoreApiClient_Constructor_WithValidBaseUrl_ShouldInitializeSuccessfully()
        {
            // Arrange
            var baseUrl = "https://dev.azure.com";

            // Act
            using var client = new coreApiClient(baseUrl);

            // Assert
            client.Should().NotBeNull("client should be initialized");
            client.Teams.Should().NotBeNull("Teams API should be available");
            client.Operations.Should().NotBeNull("Operations API should be available");
            client.TeamMembersWithExtendedProperties.Should().NotBeNull("TeamMembersWithExtendedProperties API should be available");
        }

        [Theory]
        [Trait("Category", "Functional")]
        [Trait("Priority", "High")]
        [InlineData("https://dev.azure.com")]
        [InlineData("https://dev.azure.com/")]
        [InlineData("https://custom.azure.com")]
        public void CoreApiClient_Constructor_WithVariousValidUrls_ShouldInitializeSuccessfully(string baseUrl)
        {
            // Act & Assert
            using var client = new coreApiClient(baseUrl);
            client.Should().NotBeNull();
        }

        [Theory]
        [Trait("Category", "Functional")]
        [Trait("Priority", "High")]
        [InlineData(null)]
        [InlineData("")]
        public void CoreApiClient_Constructor_WithInvalidBaseUrl_ShouldThrowArgumentException(string? invalidUrl)
        {
            // Act & Assert
            var act = () => new coreApiClient(invalidUrl!);
            act.Should().Throw<ArgumentNullException>("invalid URLs should throw ArgumentNullException");
        }

        [Fact]
        [Trait("Category", "Functional")]
        [Trait("Priority", "Medium")]
        public void CoreApiClient_WithHttpClient_ShouldInitializeSuccessfully()
        {
            // Arrange
            using var httpClient = new HttpClient();
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
        [Trait("Category", "Functional")]
        [Trait("Priority", "Medium")]
        public void CoreApiClient_Dispose_ShouldNotThrow()
        {
            // Arrange
            var client = new coreApiClient("https://dev.azure.com");

            // Act & Assert
            var act = () => client.Dispose();
            act.Should().NotThrow("Dispose should not throw exceptions");
        }

        [Fact]
        [Trait("Category", "Functional")]
        [Trait("Priority", "Medium")]
        public void CoreApiClient_MultipleDispose_ShouldNotThrow()
        {
            // Arrange
            var client = new coreApiClient("https://dev.azure.com");

            // Act & Assert
            var act = () =>
            {
                client.Dispose();
                client.Dispose(); // Second dispose should not throw
            };
            act.Should().NotThrow("Multiple dispose calls should not throw exceptions");
        }

        [Fact]
        [Trait("Category", "Functional")]
        [Trait("Priority", "Low")]
        public void CoreApiClient_Properties_ShouldImplementCorrectInterfaces()
        {
            // Arrange
            using var client = new coreApiClient("https://dev.azure.com");

            // Act & Assert
            client.Teams.Should().BeAssignableTo<AzureDevOpsApi.Api.Interface.core.ITeams>();
            client.Operations.Should().BeAssignableTo<AzureDevOpsApi.Api.Interface.core.IOperations>();
            client.TeamMembersWithExtendedProperties.Should().BeAssignableTo<AzureDevOpsApi.Api.Interface.core.ITeamMembersWithExtendedProperties>();
        }
    }
}
