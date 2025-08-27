using AzureDevOpsApi.Api.Implementation.core;
using AzureDevOpsApi.Dto.core;
using ApiBase.Utils.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using AutoFixture;
using AutoFixture.Xunit2;
using System.Text.Json;

namespace AzureDevOpsApi.UnitTests.Api.Implementation
{
    public class TeamsTests : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly Mock<IHttpClientUtil> _mockHttpClientUtil;
        private readonly string _baseUrl = "https://dev.azure.com";

        public TeamsTests()
        {
            _fixture = new Fixture();
            _mockHttpClientUtil = new Mock<IHttpClientUtil>();
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Act
            var teams = new Teams(_mockHttpClientUtil.Object, _baseUrl);

            // Assert
            teams.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var act = () => new Teams(null!, _baseUrl);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidBaseUrl_ShouldThrowArgumentException(string? baseUrl)
        {
            // Act & Assert
            var act = () => new Teams(_mockHttpClientUtil.Object, baseUrl!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task GetTeamsAsync_WithValidParameters_ShouldCallHttpClient()
        {
            // Arrange
            var teams = new Teams(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization = "test-org", projectId = "test-project" };
            var expectedResponse = new GetTeamsResponse
            {
                Count = 1,
                Value = new List<Team>
                {
                    new Team
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Test Team",
                        Description = "Test Description"
                    }
                }
            };

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetTeamsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await teams.GetTeamsAsync(pathParameters);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockHttpClientUtil.Verify(
                x => x.GetAsync<GetTeamsResponse>(
                    It.Is<string>(url => url.Contains("test-org") && url.Contains("test-project")),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamsAsync_WithNullPathParameters_ShouldThrowArgumentNullException()
        {
            // Arrange
            var teams = new Teams(_mockHttpClientUtil.Object, _baseUrl);

            // Act & Assert
            var act = async () => await teams.GetTeamsAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetTeamsAsync_WithCancellationToken_ShouldPassTokenToHttpClient()
        {
            // Arrange
            var teams = new Teams(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization = "test-org", projectId = "test-project" };
            var cancellationToken = new CancellationToken();

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetTeamsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTeamsResponse());

            // Act
            await teams.GetTeamsAsync(pathParameters, cancellationToken: cancellationToken);

            // Assert
            _mockHttpClientUtil.Verify(
                x => x.GetAsync<GetTeamsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamsAsync_WithQueryParameters_ShouldIncludeInUrl()
        {
            // Arrange
            var teams = new Teams(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization = "test-org", projectId = "test-project" };
            var queryParameters = new { top = 10, skip = 5 };

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetTeamsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTeamsResponse());

            // Act
            await teams.GetTeamsAsync(pathParameters, queryParameters);

            // Assert
            _mockHttpClientUtil.Verify(
                x => x.GetAsync<GetTeamsResponse>(
                    It.Is<string>(url => url.Contains("top=10") && url.Contains("skip=5")),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamsAsync_WithHeaders_ShouldPassToHttpClient()
        {
            // Arrange
            var teams = new Teams(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization = "test-org", projectId = "test-project" };
            var headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } };

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetTeamsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTeamsResponse());

            // Act
            await teams.GetTeamsAsync(pathParameters, headers: headers);

            // Assert
            _mockHttpClientUtil.Verify(
                x => x.GetAsync<GetTeamsResponse>(
                    It.IsAny<string>(),
                    It.Is<Dictionary<string, string>>(h => h.ContainsKey("Authorization")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task GetTeamsAsync_WithAutoGeneratedData_ShouldHandleCorrectly(
            string organization, 
            string projectId)
        {
            // Arrange
            var teams = new Teams(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization, projectId };
            var expectedResponse = _fixture.Create<GetTeamsResponse>();

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetTeamsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await teams.GetTeamsAsync(pathParameters);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        public void Dispose()
        {
            _mockHttpClientUtil?.Reset();
        }
    }
}
