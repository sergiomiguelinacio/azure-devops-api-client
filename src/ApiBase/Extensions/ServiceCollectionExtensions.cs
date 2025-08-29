using ApiBase.Utils.Implementations;
using ApiBase.Utils.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace ApiBase.Extensions
{
    /// <summary>
    /// Extension methods for configuring Azure DevOps API clients with Polly policies
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Default base URL for Azure DevOps Services
        /// </summary>
        private const string DEFAULT_AZURE_DEVOPS_BASE_URL = "https://dev.azure.com";
        /// <summary>
        /// Adds Azure DevOps API client with default Polly policies
        /// </summary>
        public static IServiceCollection AddAzureDevOpsApiClient<TClient, TInterface>(
            this IServiceCollection services,
            string? baseUrl = null,
            Action<HttpClient>? configureHttpClient = null,
            RetryOptions? retryOptions = null)
            where TClient : class, TInterface
            where TInterface : class
        {
            // Register HttpClient with default configuration
            services.AddHttpClient<TInterface, TClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl ?? DEFAULT_AZURE_DEVOPS_BASE_URL);
                client.DefaultRequestHeaders.Add("User-Agent", "AzureDevOpsApiClient/1.0");
                configureHttpClient?.Invoke(client);
            })
            .AddPolicyHandler(CreateDefaultPollyPolicy(retryOptions));

            // Register the policy provider
            services.AddSingleton<IPollyPolicyProvider>(provider =>
                new DefaultPollyPolicyProvider(retryOptions));

            return services;
        }

        /// <summary>
        /// Adds Azure DevOps API client with custom Polly policy provider
        /// </summary>
        public static IServiceCollection AddAzureDevOpsApiClient<TClient, TInterface>(
            this IServiceCollection services,
            string? baseUrl = null,
            Action<HttpClient>? configureHttpClient = null,
            IPollyPolicyProvider? policyProvider = null)
            where TClient : class, TInterface
            where TInterface : class
        {
            var effectiveBaseUrl = baseUrl ?? DEFAULT_AZURE_DEVOPS_BASE_URL;
            policyProvider ??= new DefaultPollyPolicyProvider();

            services.AddHttpClient<TInterface, TClient>(client =>
            {
                client.BaseAddress = new Uri(effectiveBaseUrl);
                client.DefaultRequestHeaders.Add("User-Agent", "AzureDevOpsApiClient/1.0");
                configureHttpClient?.Invoke(client);
            })
            .AddPolicyHandler(policyProvider.GetPolicy());

            services.AddSingleton(policyProvider);

            return services;
        }

        /// <summary>
        /// Adds Azure DevOps API client with custom IAsyncPolicy
        /// </summary>
        public static IServiceCollection AddAzureDevOpsApiClient<TClient, TInterface>(
            this IServiceCollection services,
            string? baseUrl = null,
            IAsyncPolicy<HttpResponseMessage>? policy = null,
            Action<HttpClient>? configureHttpClient = null)
            where TClient : class, TInterface
            where TInterface : class
        {
            var effectiveBaseUrl = baseUrl ?? DEFAULT_AZURE_DEVOPS_BASE_URL;

            services.AddHttpClient<TInterface, TClient>(client =>
            {
                client.BaseAddress = new Uri(effectiveBaseUrl!);
                client.DefaultRequestHeaders.Add("User-Agent", "AzureDevOpsApiClient/1.0");
                configureHttpClient?.Invoke(client);
            });

            if (policy != null)
            {
                services.AddHttpClient<TInterface, TClient>().AddPolicyHandler(policy);
            }

            return services;
        }

        /// <summary>
        /// Creates a default Polly policy for Azure DevOps API operations
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> CreateDefaultPollyPolicy(RetryOptions? retryOptions = null)
        {
            var options = retryOptions ?? new RetryOptions();

            return Policy
                .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode && IsTransientHttpStatusCode(response.StatusCode))
                .Or<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: options.MaxRetries,
                    sleepDurationProvider: retryAttempt => options.UseExponentialBackoff
                        ? TimeSpan.FromMilliseconds(options.BaseDelayMs * Math.Pow(2, retryAttempt - 1))
                        : TimeSpan.FromMilliseconds(options.BaseDelayMs),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Optional: Add logging here
                        Console.WriteLine($"Retry {retryCount} after {timespan} delay");
                    });
        }

        private static bool IsTransientHttpStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.RequestTimeout => true,
                HttpStatusCode.InternalServerError => true,
                HttpStatusCode.BadGateway => true,
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,
                HttpStatusCode.TooManyRequests => true,
                _ => false
            };
        }
    }
}
