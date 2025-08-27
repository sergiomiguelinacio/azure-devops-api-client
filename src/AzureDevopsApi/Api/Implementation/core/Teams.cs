using System.Threading.Tasks;
using AzureDevOpsApi.Api.Interface.core;
using ApiBase;
using ApiBase.Utils.Interfaces;
using ApiBase.Utils.Implementations;

namespace AzureDevOpsApi.Api.Implementation.core
{
    /// <summary>
    /// Implementation of Teams operations for Azure DevOps API
    /// </summary>
    public class Teams : BaseApi, ITeams
    {
        public Teams(IHttpClientUtil httpClient, string baseUrl)
            : base(httpClient, baseUrl)
        {
        }

        public Teams(HttpClient httpClient, string baseUrl, RetryOptions? retryOptions = null)
            : base(httpClient, baseUrl, new DefaultPollyPolicyProvider(retryOptions))
        {
        }

        /// <summary>
        /// Get a list of teams.
        /// </summary>
        public async Task<T?> GetTeamsAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var pathParams = ExtractPathParameters(pathParameters);
            var queryParams = ExtractQueryParameters(queryParameters);
            var headers = CreateHeaders();

            var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams", pathParams, queryParams);

            return await _httpClient.GetAsync<T>(url, headers, cancellationToken);
        }

        /// <summary>
        /// Get a list of all teams.
        /// </summary>
        public async Task<T?> GetAllTeamsAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var pathParams = ExtractPathParameters(pathParameters);
            var queryParams = ExtractQueryParameters(queryParameters);
            var headers = CreateHeaders();

            var url = BuildUrl("/{organization}/_apis/teams", pathParams, queryParams);

            return await _httpClient.GetAsync<T>(url, headers, cancellationToken);
        }

    }
}