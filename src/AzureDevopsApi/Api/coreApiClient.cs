using System.Threading.Tasks;
using AzureDevOpsApi.Api.Interface.core;
using AzureDevOpsApi.Api.Implementation.core;
using ApiBase;
using ApiBase.Utils.Interfaces;
using ApiBase.Utils.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace AzureDevOpsApi.Api
{
    /// <summary>
    /// Main API client for Azure DevOps core operations
    /// Provides access to all core related operations with built-in retry logic
    /// </summary>
    public class coreApiClient : IDisposable
    {
        private readonly HttpClient? _ownedHttpClient;
        private readonly IHttpClientUtil _httpClientUtil;
        private readonly string _baseUrl;
        private readonly ILogger<coreApiClient> _logger;
        private bool _disposed = false;

        /// <summary>
        /// Teams operations
        /// </summary>
        public ITeams Teams { get; }
        /// <summary>
        /// Operations operations
        /// </summary>
        public IOperations Operations { get; }
        /// <summary>
        /// TeamMembersWithExtendedProperties operations
        /// </summary>
        public ITeamMembersWithExtendedProperties TeamMembersWithExtendedProperties { get; }

        /// <summary>
        /// Initializes a new instance of the coreApiClient with a custom HttpClient
        /// </summary>
        /// <param name="httpClient">Custom HttpClient instance (caller is responsible for disposal)</param>
        /// <param name="baseUrl">Base URL for the Azure DevOps API (e.g., "https://dev.azure.com")</param>
        /// <param name="policyProvider">Optional Polly policy provider for resilience patterns</param>
        /// <param name="logger">Optional logger instance</param>
        public coreApiClient(HttpClient httpClient, string baseUrl, IPollyPolicyProvider? policyProvider = null, ILogger<coreApiClient>? logger = null)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

            _httpClientUtil = new HttpClientUtil(httpClient, policyProvider ?? new DefaultPollyPolicyProvider());
            _baseUrl = baseUrl.TrimEnd('/');
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<coreApiClient>.Instance;

            _logger.LogInformation("Initializing coreApiClient with base URL: {BaseUrl}", _baseUrl);

            // Initialize all operation clients
            Teams = new Implementation.core.Teams(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.Teams>);
            Operations = new Implementation.core.Operations(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.Operations>);
            TeamMembersWithExtendedProperties = new Implementation.core.TeamMembersWithExtendedProperties(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.TeamMembersWithExtendedProperties>);
        }

        /// <summary>
        /// Initializes a new instance of the coreApiClient with default HttpClient and Polly policies
        /// </summary>
        /// <param name="baseUrl">Base URL for the Azure DevOps API (e.g., "https://dev.azure.com")</param>
        /// <param name="policyProvider">Optional Polly policy provider for resilience patterns</param>
        /// <param name="logger">Optional logger instance</param>
        public coreApiClient(string baseUrl, IPollyPolicyProvider? policyProvider = null, ILogger<coreApiClient>? logger = null)
        {
            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

            _ownedHttpClient = new HttpClient();
            _httpClientUtil = new HttpClientUtil(_ownedHttpClient, policyProvider ?? new DefaultPollyPolicyProvider());
            _baseUrl = baseUrl.TrimEnd('/');
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<coreApiClient>.Instance;

            _logger.LogInformation("Initializing coreApiClient with base URL: {BaseUrl}", _baseUrl);

            // Initialize all operation clients
            Teams = new Implementation.core.Teams(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.Teams>);
            Operations = new Implementation.core.Operations(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.Operations>);
            TeamMembersWithExtendedProperties = new Implementation.core.TeamMembersWithExtendedProperties(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.TeamMembersWithExtendedProperties>);
        }

        /// <summary>
        /// Initializes a new instance of the coreApiClient with IHttpClientUtil
        /// </summary>
        /// <param name="httpClientUtil">HTTP client utility with retry logic</param>
        /// <param name="baseUrl">Base URL for the Azure DevOps API</param>
        /// <param name="logger">Optional logger instance</param>
        public coreApiClient(IHttpClientUtil httpClientUtil, string baseUrl, ILogger<coreApiClient>? logger = null)
        {
            _httpClientUtil = httpClientUtil ?? throw new ArgumentNullException(nameof(httpClientUtil));
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<coreApiClient>.Instance;

            _logger.LogInformation("Initializing coreApiClient with base URL: {BaseUrl}", _baseUrl);

            // Initialize all operation clients
            Teams = new Implementation.core.Teams(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.Teams>);
            Operations = new Implementation.core.Operations(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.Operations>);
            TeamMembersWithExtendedProperties = new Implementation.core.TeamMembersWithExtendedProperties(_httpClientUtil, _baseUrl, _logger as ILogger<Implementation.core.TeamMembersWithExtendedProperties>);
        }





        /// <summary>
        /// Sets authentication header for all requests
        /// </summary>
        /// <param name="token">Personal Access Token or Bearer token</param>
        /// <param name="tokenType">Type of token (default: "Bearer")</param>
        public void SetAuthentication(string token, string tokenType = "Bearer")
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            // Note: This would require extending the HttpClientUtil to support default headers
            // For now, authentication should be set on the HttpClient before passing it to the constructor
        }

        /// <summary>
        /// Disposes the API client and its resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _ownedHttpClient?.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Extension methods for dependency injection
    /// </summary>
    public static class coreApiClientExtensions
    {
        /// <summary>
        /// Adds coreApiClient to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="baseUrl">Base URL for the Azure DevOps API</param>
        /// <param name="retryOptions">Optional retry configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddcoreApiClient(
            this IServiceCollection services, 
            string baseUrl, 
            RetryOptions? retryOptions = null)
        {
            services.AddHttpClient<coreApiClient>();
            services.AddTransient<IHttpClientUtil, HttpClientUtil>();
            
            services.AddTransient<coreApiClient>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                var logger = provider.GetService<ILogger<coreApiClient>>();
                return new coreApiClient(httpClient, baseUrl, new DefaultPollyPolicyProvider(retryOptions), logger);
            });

            // Register individual operation interfaces
            services.AddTransient<ITeams>(provider =>
            {
                var client = provider.GetRequiredService<coreApiClient>();
                return client.Teams;
            });
            services.AddTransient<IOperations>(provider =>
            {
                var client = provider.GetRequiredService<coreApiClient>();
                return client.Operations;
            });
            services.AddTransient<ITeamMembersWithExtendedProperties>(provider =>
            {
                var client = provider.GetRequiredService<coreApiClient>();
                return client.TeamMembersWithExtendedProperties;
            });

            return services;
        }

        /// <summary>
        /// Adds coreApiClient to the service collection with custom HttpClient configuration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="baseUrl">Base URL for the Azure DevOps API</param>
        /// <param name="configureHttpClient">Action to configure the HttpClient</param>
        /// <param name="retryOptions">Optional retry configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddcoreApiClient(
            this IServiceCollection services,
            string baseUrl,
            Action<HttpClient> configureHttpClient,
            RetryOptions? retryOptions = null)
        {
            services.AddHttpClient<coreApiClient>(configureHttpClient);
            services.AddTransient<IHttpClientUtil, HttpClientUtil>();
            
            services.AddTransient<coreApiClient>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(nameof(coreApiClient));
                var logger = provider.GetService<ILogger<coreApiClient>>();
                return new coreApiClient(httpClient, baseUrl, new DefaultPollyPolicyProvider(retryOptions), logger);
            });

            // Register individual operation interfaces
            services.AddTransient<ITeams>(provider =>
            {
                var client = provider.GetRequiredService<coreApiClient>();
                return client.Teams;
            });
            services.AddTransient<IOperations>(provider =>
            {
                var client = provider.GetRequiredService<coreApiClient>();
                return client.Operations;
            });
            services.AddTransient<ITeamMembersWithExtendedProperties>(provider =>
            {
                var client = provider.GetRequiredService<coreApiClient>();
                return client.TeamMembersWithExtendedProperties;
            });

            return services;
        }
    }
}
