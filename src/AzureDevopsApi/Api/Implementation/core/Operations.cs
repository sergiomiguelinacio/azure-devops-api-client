using System.Threading.Tasks;
using AzureDevOpsApi.Api.Interface.core;
using ApiBase;
using ApiBase.Utils.Interfaces;
using ApiBase.Utils.Implementations;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AzureDevOpsApi.Api.Implementation.core
{
    /// <summary>
    /// Implementation of Operations operations for Azure DevOps API
    /// </summary>
    public class Operations : BaseApi, IOperations
    {
        private readonly ILogger<Operations> _logger;

        public Operations(IHttpClientUtil httpClient, string baseUrl, ILogger<Operations>? logger = null)
            : base(httpClient, baseUrl)
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Operations>.Instance;
        }

        public Operations(HttpClient httpClient, string baseUrl, RetryOptions? retryOptions = null, ILogger<Operations>? logger = null)
            : base(httpClient, baseUrl, new DefaultPollyPolicyProvider(retryOptions))
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Operations>.Instance;
        }

        /// <summary>
        /// Create a team in a team project. Possible failure scenarios Invalid project name/ID (project doesn't exist) 404 Invalid team name or description 400 Team already ex...
        /// </summary>
        public async Task<T?> CreateAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            object? requestBody = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting POST operation: Create for path: /{organization}/_apis/projects/{projectId}/teams");

                var pathParams = ExtractPathParameters(pathParameters);
                var queryParams = ExtractQueryParameters(queryParameters);
                var headers = CreateHeaders();

                var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams", pathParams, queryParams);

                _logger.LogDebug("Built URL: {Url} for operation: Create", url);

                T? result;
                result = await _httpClient.PostAsync<T>(url, requestBody, headers, cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Completed POST operation: Create in {Duration}ms",
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed POST operation: Create after {Duration}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Get a specific team.
        /// </summary>
        public async Task<T?> GetAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting GET operation: Get for path: /{organization}/_apis/projects/{projectId}/teams/{teamId}");

                var pathParams = ExtractPathParameters(pathParameters);
                var queryParams = ExtractQueryParameters(queryParameters);
                var headers = CreateHeaders();

                var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams/{teamId}", pathParams, queryParams);

                _logger.LogDebug("Built URL: {Url} for operation: Get", url);

                T? result;
                result = await _httpClient.GetAsync<T>(url, headers, cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Completed GET operation: Get in {Duration}ms",
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed GET operation: Get after {Duration}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Delete a team.
        /// </summary>
        public async Task<T?> DeleteAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting DELETE operation: Delete for path: /{organization}/_apis/projects/{projectId}/teams/{teamId}");

                var pathParams = ExtractPathParameters(pathParameters);
                var queryParams = ExtractQueryParameters(queryParameters);
                var headers = CreateHeaders();

                var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams/{teamId}", pathParams, queryParams);

                _logger.LogDebug("Built URL: {Url} for operation: Delete", url);

                T? result;
                result = await _httpClient.DeleteAsync<T>(url, headers, cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Completed DELETE operation: Delete in {Duration}ms",
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed DELETE operation: Delete after {Duration}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Update a team's name and/or description.
        /// </summary>
        public async Task<T?> UpdateAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            object? requestBody = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting PATCH operation: Update for path: /{organization}/_apis/projects/{projectId}/teams/{teamId}");

                var pathParams = ExtractPathParameters(pathParameters);
                var queryParams = ExtractQueryParameters(queryParameters);
                var headers = CreateHeaders();

                var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams/{teamId}", pathParams, queryParams);

                _logger.LogDebug("Built URL: {Url} for operation: Update", url);

                T? result;
                result = await _httpClient.PatchAsync<T>(url, requestBody, headers, cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Completed PATCH operation: Update in {Duration}ms",
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed PATCH operation: Update after {Duration}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

    }
}