using System.Threading.Tasks;
using AzureDevOpsApi.Api.Interface.core;
using ApiBase;
using ApiBase.Utils.Interfaces;
using ApiBase.Utils.Implementations;

namespace AzureDevOpsApi.Api.Implementation.core
{
    /// <summary>
    /// Implementation of Operations operations for Azure DevOps API
    /// </summary>
    public class Operations : BaseApi, IOperations
    {
        public Operations(IHttpClientUtil httpClient, string baseUrl)
            : base(httpClient, baseUrl)
        {
        }

        public Operations(HttpClient httpClient, string baseUrl, RetryOptions? retryOptions = null)
            : base(httpClient, baseUrl, new DefaultPollyPolicyProvider(retryOptions))
        {
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
            var pathParams = ExtractPathParameters(pathParameters);
            var queryParams = ExtractQueryParameters(queryParameters);
            var headers = CreateHeaders();

            var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams", pathParams, queryParams);

            return await _httpClient.PostAsync<T>(url, requestBody, headers, cancellationToken);
        }

        /// <summary>
        /// Get a specific team.
        /// </summary>
        public async Task<T?> GetAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var pathParams = ExtractPathParameters(pathParameters);
            var queryParams = ExtractQueryParameters(queryParameters);
            var headers = CreateHeaders();

            var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams/{teamId}", pathParams, queryParams);

            return await _httpClient.GetAsync<T>(url, headers, cancellationToken);
        }

        /// <summary>
        /// Delete a team.
        /// </summary>
        public async Task<T?> DeleteAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var pathParams = ExtractPathParameters(pathParameters);
            var queryParams = ExtractQueryParameters(queryParameters);
            var headers = CreateHeaders();

            var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams/{teamId}", pathParams, queryParams);

            return await _httpClient.DeleteAsync<T>(url, headers, cancellationToken);
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
            var pathParams = ExtractPathParameters(pathParameters);
            var queryParams = ExtractQueryParameters(queryParameters);
            var headers = CreateHeaders();

            var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams/{teamId}", pathParams, queryParams);

            return await _httpClient.PatchAsync<T>(url, requestBody, headers, cancellationToken);
        }

    }
}