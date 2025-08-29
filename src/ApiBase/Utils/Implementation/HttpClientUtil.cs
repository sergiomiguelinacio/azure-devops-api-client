using ApiBase.Exceptions;
using ApiBase.Utils.Interfaces;
using Newtonsoft.Json;
using Polly;
using System.Net;
using System.Text;

namespace ApiBase.Utils.Implementations
{
    /// <summary>
    /// HTTP client utility with Polly-based resilience policies
    /// Supports retry, circuit breaker, timeout, and custom user-defined policies
    /// </summary>
    public class HttpClientUtil : IHttpClientUtil
    {
        private readonly HttpClient? _httpClient;
        private readonly IHttpClientFactory? _httpClientFactory;
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;
        private readonly string? _httpClientName;

        /// <summary>
        /// Initializes with HttpClient that already has Polly policies configured (via DI)
        /// Uses a pass-through policy that doesn't add additional retry logic
        /// </summary>
        public HttpClientUtil(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            // Use pass-through policy since HttpClient already has Polly configured
            _policy = Policy.NoOpAsync<HttpResponseMessage>();
        }

        /// <summary>
        /// Initializes with custom Polly policy provider
        /// </summary>
        public HttpClientUtil(HttpClient httpClient, IPollyPolicyProvider policyProvider)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (policyProvider == null) throw new ArgumentNullException(nameof(policyProvider));
            _policy = policyProvider.GetPolicy();
        }

        /// <summary>
        /// Initializes with custom IAsyncPolicy directly
        /// </summary>
        public HttpClientUtil(HttpClient httpClient, IAsyncPolicy<HttpResponseMessage> policy)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        /// <summary>
        /// Initializes with IHttpClientFactory for better resource management
        /// </summary>
        public HttpClientUtil(IHttpClientFactory httpClientFactory, string? httpClientName = null)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClientName = httpClientName;
            _policy = Policy.NoOpAsync<HttpResponseMessage>();
        }

        /// <summary>
        /// Initializes with IHttpClientFactory and custom policy provider
        /// </summary>
        public HttpClientUtil(IHttpClientFactory httpClientFactory, IPollyPolicyProvider policyProvider, string? httpClientName = null)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClientName = httpClientName;
            if (policyProvider == null) throw new ArgumentNullException(nameof(policyProvider));
            _policy = policyProvider.GetPolicy();
        }

        /// <summary>
        /// Gets HttpClient instance - either from factory or direct reference
        /// </summary>
        private HttpClient GetHttpClient()
        {
            return _httpClient ?? _httpClientFactory!.CreateClient(_httpClientName ?? string.Empty);
        }

        public async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            using var response = await _policy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                AddHeaders(request, headers);

                var httpClient = GetHttpClient();
                return await httpClient.SendAsync(request, cancellationToken);
            });

            return await ProcessResponseAsync<T>(response);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            using var response = await _policy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                AddHeaders(request, headers);

                if (data != null)
                {
                    var json = JsonConvert.SerializeObject(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var httpClient = GetHttpClient();
                return await httpClient.SendAsync(request, cancellationToken);
            });

            return await ProcessResponseAsync<T>(response);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            using var response = await _policy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
                AddHeaders(request, headers);

                if (data != null)
                {
                    var json = JsonConvert.SerializeObject(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var httpClient = GetHttpClient();
                return await httpClient.SendAsync(request, cancellationToken);
            });

            return await ProcessResponseAsync<T>(response);
        }

        public async Task<T?> DeleteAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            using var response = await _policy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                AddHeaders(request, headers);

                var httpClient = GetHttpClient();
                return await httpClient.SendAsync(request, cancellationToken);
            });

            return await ProcessResponseAsync<T>(response);
        }

        public async Task<T?> PatchAsync<T>(string endpoint, object? data = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            using var response = await _policy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
                AddHeaders(request, headers);

                if (data != null)
                {
                    var json = JsonConvert.SerializeObject(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var httpClient = GetHttpClient();
                return await httpClient.SendAsync(request, cancellationToken);
            });

            return await ProcessResponseAsync<T>(response);
        }

        // Legacy methods for backward compatibility
        public async Task<T?> ManagedResult<T>(HttpResponseMessage response)
        {
            using (response)
            {
                return await ProcessResponseAsync<T>(response);
            }
        }

        public async Task<T?> HttpPost<T>(string endpoint, HttpContent content, Dictionary<string, string> headers)
        {
            using var response = await _policy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = content;
                AddHeaders(request, headers);

                var httpClient = GetHttpClient();
                return await httpClient.SendAsync(request);
            });

            return await ProcessResponseAsync<T>(response);
        }

        public async Task<T?> HttpGet<T>(string endpoint, Dictionary<string, string> headers)
        {
            return await GetAsync<T>(endpoint, headers);
        }

        private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
        {
            if (headers == null) return;

            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        private static async Task<T?> ProcessResponseAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)content;
                }
                
                if (string.IsNullOrEmpty(content))
                {
                    return default(T);
                }

                return JsonConvert.DeserializeObject<T>(content);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpFailureResponseException(
                $"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorContent}");
        }
    }

    public class RetryOptions
    {
        public int MaxRetries { get; set; } = 3;
        public int BaseDelayMs { get; set; } = 1000;
        public bool UseExponentialBackoff { get; set; } = true;
    }
}
