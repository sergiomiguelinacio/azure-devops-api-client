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
    /// Implementation of TeamMembersWithExtendedProperties operations for Azure DevOps API
    /// </summary>
    public class TeamMembersWithExtendedProperties : BaseApi, ITeamMembersWithExtendedProperties
    {
        private readonly ILogger<TeamMembersWithExtendedProperties> _logger;

        public TeamMembersWithExtendedProperties(IHttpClientUtil httpClient, string baseUrl, ILogger<TeamMembersWithExtendedProperties>? logger = null)
            : base(httpClient, baseUrl)
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TeamMembersWithExtendedProperties>.Instance;
        }

        public TeamMembersWithExtendedProperties(HttpClient httpClient, string baseUrl, RetryOptions? retryOptions = null, ILogger<TeamMembersWithExtendedProperties>? logger = null)
            : base(httpClient, baseUrl, new DefaultPollyPolicyProvider(retryOptions))
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TeamMembersWithExtendedProperties>.Instance;
        }

        /// <summary>
        /// Get a list of members for a specific team.
        /// </summary>
        public async Task<T?> GetTeamMembersWithExtendedPropertiesAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting GET operation: GetTeamMembersWithExtendedProperties for path: /{organization}/_apis/projects/{projectId}/teams/{teamId}/members");

                var pathParams = ExtractPathParameters(pathParameters);
                var queryParams = ExtractQueryParameters(queryParameters);
                var headers = CreateHeaders();

                var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams/{teamId}/members", pathParams, queryParams);

                _logger.LogDebug("Built URL: {Url} for operation: GetTeamMembersWithExtendedProperties", url);

                T? result;
                result = await _httpClient.GetAsync<T>(url, headers, cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Completed GET operation: GetTeamMembersWithExtendedProperties in {Duration}ms",
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed GET operation: GetTeamMembersWithExtendedProperties after {Duration}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

    }
}