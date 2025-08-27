using System.Threading.Tasks;
using AzureDevOpsApi.Api.Interface.core;
using ApiBase;
using ApiBase.Utils.Interfaces;
using ApiBase.Utils.Implementations;

namespace AzureDevOpsApi.Api.Implementation.core
{
    /// <summary>
    /// Implementation of TeamMembersWithExtendedProperties operations for Azure DevOps API
    /// </summary>
    public class TeamMembersWithExtendedProperties : BaseApi, ITeamMembersWithExtendedProperties
    {
        public TeamMembersWithExtendedProperties(IHttpClientUtil httpClient, string baseUrl)
            : base(httpClient, baseUrl)
        {
        }

        public TeamMembersWithExtendedProperties(HttpClient httpClient, string baseUrl, RetryOptions? retryOptions = null)
            : base(httpClient, baseUrl, new DefaultPollyPolicyProvider(retryOptions))
        {
        }

        /// <summary>
        /// Get a list of members for a specific team.
        /// </summary>
        public async Task<T?> GetTeamMembersWithExtendedPropertiesAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var pathParams = ExtractPathParameters(pathParameters);
            var queryParams = ExtractQueryParameters(queryParameters);
            var headers = CreateHeaders();

            var url = BuildUrl("/{organization}/_apis/projects/{projectId}/teams/{teamId}/members", pathParams, queryParams);

            return await _httpClient.GetAsync<T>(url, headers, cancellationToken);
        }

    }
}