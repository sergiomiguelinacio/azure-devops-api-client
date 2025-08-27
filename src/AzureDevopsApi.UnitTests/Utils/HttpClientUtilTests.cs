using ApiBase.Utils.Implementations;
using ApiBase.Utils.Interfaces;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;
using System.Net;
using AutoFixture;
using AutoFixture.Xunit2;

namespace AzureDevOpsApi.UnitTests.Utils
{
    public class HttpClientUtilTests : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<IPollyPolicyProvider> _mockPolicyProvider;

        public HttpClientUtilTests()
        {
            _fixture = new Fixture();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockPolicyProvider = new Mock<IPollyPolicyProvider>();
        }

        [Fact]
        public void Constructor_WithHttpClient_ShouldInitializeCorrectly()
        {
            // Act
            var httpClientUtil = new HttpClientUtil(_httpClient);

            // Assert
            httpClientUtil.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithHttpClientAndPolicyProvider_ShouldInitializeCorrectly()
        {
            // Act
            var httpClientUtil = new HttpClientUtil(_httpClient, _mockPolicyProvider.Object);

            // Assert
            httpClientUtil.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var act = () => new HttpClientUtil(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task GetAsync_WithValidEndpoint_ShouldReturnResponse()
        {
            // Arrange
            var endpoint = "https://api.example.com/test";
            var expectedResponse = _fixture.Create<TestResponse>();
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(expectedResponse);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClientUtil = new HttpClientUtil(_httpClient);

            // Act
            var result = await httpClientUtil.GetAsync<TestResponse>(endpoint);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedResponse.Id);
            result.Name.Should().Be(expectedResponse.Name);
        }

        [Fact]
        public async Task GetAsync_WithHeaders_ShouldIncludeHeaders()
        {
            // Arrange
            var endpoint = "https://api.example.com/test";
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer token" },
                { "Custom-Header", "custom-value" }
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Headers.Contains("Authorization") && 
                        req.Headers.Contains("Custom-Header")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}")
                });

            var httpClientUtil = new HttpClientUtil(_httpClient);

            // Act
            await httpClientUtil.GetAsync<object>(endpoint, headers);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Headers.Contains("Authorization") && 
                    req.Headers.Contains("Custom-Header")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostAsync_WithValidData_ShouldSendPostRequest()
        {
            // Arrange
            var endpoint = "https://api.example.com/test";
            var data = _fixture.Create<TestRequest>();
            var expectedResponse = _fixture.Create<TestResponse>();
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(expectedResponse);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClientUtil = new HttpClientUtil(_httpClient);

            // Act
            var result = await httpClientUtil.PostAsync<TestResponse>(endpoint, data);

            // Assert
            result.Should().NotBeNull();
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PutAsync_WithValidData_ShouldSendPutRequest()
        {
            // Arrange
            var endpoint = "https://api.example.com/test";
            var data = _fixture.Create<TestRequest>();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}")
                });

            var httpClientUtil = new HttpClientUtil(_httpClient);

            // Act
            await httpClientUtil.PutAsync<object>(endpoint, data);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DeleteAsync_WithValidEndpoint_ShouldSendDeleteRequest()
        {
            // Arrange
            var endpoint = "https://api.example.com/test";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}")
                });

            var httpClientUtil = new HttpClientUtil(_httpClient);

            // Act
            await httpClientUtil.DeleteAsync<object>(endpoint);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PatchAsync_WithValidData_ShouldSendPatchRequest()
        {
            // Arrange
            var endpoint = "https://api.example.com/test";
            var data = _fixture.Create<TestRequest>();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}")
                });

            var httpClientUtil = new HttpClientUtil(_httpClient);

            // Act
            await httpClientUtil.PatchAsync<object>(endpoint, data);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>());
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _mockHttpMessageHandler?.Reset();
            _mockPolicyProvider?.Reset();
        }

        private class TestRequest
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }

        private class TestResponse
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}
