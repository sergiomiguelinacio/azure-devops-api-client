using AzureDevOpsApi.Api.Implementation.core;
using AzureDevOpsApi.Dto.core;
using ApiBase.Utils.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using AutoFixture;
using AutoFixture.Xunit2;

namespace AzureDevOpsApi.UnitTests.Api.Implementation
{
    public class OperationsTests : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly Mock<IHttpClientUtil> _mockHttpClientUtil;
        private readonly string _baseUrl = "https://dev.azure.com";

        public OperationsTests()
        {
            _fixture = new Fixture();
            _mockHttpClientUtil = new Mock<IHttpClientUtil>();
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Act
            var operations = new Operations(_mockHttpClientUtil.Object, _baseUrl);

            // Assert
            operations.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var act = () => new Operations(null!, _baseUrl);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidBaseUrl_ShouldThrowArgumentException(string? baseUrl)
        {
            // Act & Assert
            var act = () => new Operations(_mockHttpClientUtil.Object, baseUrl!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task GetOperationsAsync_WithValidParameters_ShouldCallHttpClient()
        {
            // Arrange
            var operations = new Operations(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization = "test-org" };
            var expectedResponse = new GetOperationsResponse
            {
                Count = 1,
                Value = new List<Operation>
                {
                    new Operation
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = "InProgress",
                        Url = "https://dev.azure.com/test-org/_apis/operations/123"
                    }
                }
            };

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetOperationsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await operations.GetOperationsAsync(pathParameters);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockHttpClientUtil.Verify(
                x => x.GetAsync<GetOperationsResponse>(
                    It.Is<string>(url => url.Contains("test-org")),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOperationsAsync_WithNullPathParameters_ShouldThrowArgumentNullException()
        {
            // Arrange
            var operations = new Operations(_mockHttpClientUtil.Object, _baseUrl);

            // Act & Assert
            var act = async () => await operations.GetOperationsAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetOperationsAsync_WithCancellationToken_ShouldPassTokenToHttpClient()
        {
            // Arrange
            var operations = new Operations(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization = "test-org" };
            var cancellationToken = new CancellationToken();

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetOperationsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetOperationsResponse());

            // Act
            await operations.GetOperationsAsync(pathParameters, cancellationToken: cancellationToken);

            // Assert
            _mockHttpClientUtil.Verify(
                x => x.GetAsync<GetOperationsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task GetOperationsAsync_WithQueryParameters_ShouldIncludeInUrl()
        {
            // Arrange
            var operations = new Operations(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization = "test-org" };
            var queryParameters = new { top = 10, skip = 5 };

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetOperationsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetOperationsResponse());

            // Act
            await operations.GetOperationsAsync(pathParameters, queryParameters);

            // Assert
            _mockHttpClientUtil.Verify(
                x => x.GetAsync<GetOperationsResponse>(
                    It.Is<string>(url => url.Contains("top=10") && url.Contains("skip=5")),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOperationsAsync_WithHeaders_ShouldPassToHttpClient()
        {
            // Arrange
            var operations = new Operations(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization = "test-org" };
            var headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } };

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetOperationsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetOperationsResponse());

            // Act
            await operations.GetOperationsAsync(pathParameters, headers: headers);

            // Assert
            _mockHttpClientUtil.Verify(
                x => x.GetAsync<GetOperationsResponse>(
                    It.IsAny<string>(),
                    It.Is<Dictionary<string, string>>(h => h.ContainsKey("Authorization")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task GetOperationsAsync_WithAutoGeneratedData_ShouldHandleCorrectly(string organization)
        {
            // Arrange
            var operations = new Operations(_mockHttpClientUtil.Object, _baseUrl);
            var pathParameters = new { organization };
            var expectedResponse = _fixture.Create<GetOperationsResponse>();

            _mockHttpClientUtil
                .Setup(x => x.GetAsync<GetOperationsResponse>(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await operations.GetOperationsAsync(pathParameters);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        public void Dispose()
        {
            _mockHttpClientUtil?.Reset();
        }
    }
}
