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
    /// Implementation of Teams operations for Azure DevOps API
    /// </summary>
    public class Teams : BaseApi, ITeams
    {
        private readonly ILogger<Teams> _logger;

        public Teams(IHttpClientUtil httpClient, string baseUrl, ILogger<Teams>? logger = null)
            : base(httpClient, baseUrl)
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Teams>.Instance;
        }

        public Teams(HttpClient httpClient, string baseUrl, RetryOptions? retryOptions = null, ILogger<Teams>? logger = null)
            : base(httpClient, baseUrl, new DefaultPollyPolicyProvider(retryOptions))
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Teams>.Instance;
        }

        /// <summary>
        /// Get a list of teams.
        /// </summary>
        public async Task<T?> GetTeamsAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting GET operation: GetTeams for path: /{organization}/_apis/projects/{projectId}/teams");

                var pathParams = ExtractPathParameters(pathParameters);
                var queryParams = ExtractQueryParameters(queryParameters);
                var headers = CreateHeaders();

                var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams", pathParams, queryParams);

                _logger.LogDebug("Built URL: {Url} for operation: GetTeams", url);

                T? result;
                result = await _httpClient.GetAsync<T>(url, headers, cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Completed GET operation: GetTeams in {Duration}ms",
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed GET operation: GetTeams after {Duration}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Get a list of all teams.
        /// </summary>
        public async Task<T?> GetAllTeamsAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting GET operation: GetAllTeams for path: /{organization}/_apis/teams");

                var pathParams = ExtractPathParameters(pathParameters);
                var queryParams = ExtractQueryParameters(queryParameters);
                var headers = CreateHeaders();

                var url = BuildUrl("/{organization}/_apis/teams", pathParams, queryParams);

                _logger.LogDebug("Built URL: {Url} for operation: GetAllTeams", url);

                T? result;
                result = await _httpClient.GetAsync<T>(url, headers, cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Completed GET operation: GetAllTeams in {Duration}ms",
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed GET operation: GetAllTeams after {Duration}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

    }
}