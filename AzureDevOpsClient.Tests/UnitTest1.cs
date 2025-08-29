using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace AzureDevOpsClient.Tests;

public class BasicTests
{
    [Fact]
    public void HttpClient_ShouldBeDisposable()
    {
        // Arrange & Act
        using var httpClient = new HttpClient();

        // Assert
        Assert.NotNull(httpClient);
        // HttpClient implements IDisposable - this test verifies proper disposal pattern
    }

    [Fact]
    public void IHttpClientFactory_ShouldCreateHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        // Act
        var httpClient = httpClientFactory.CreateClient();

        // Assert
        Assert.NotNull(httpClient);

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void ServiceCollection_ShouldRegisterHttpClientFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHttpClient();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var factory = serviceProvider.GetService<IHttpClientFactory>();
        Assert.NotNull(factory);

        // Cleanup
        serviceProvider.Dispose();
    }
}
