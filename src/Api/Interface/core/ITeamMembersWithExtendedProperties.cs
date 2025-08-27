using System.Threading.Tasks;

namespace AzureDevOpsApi.Api.Interface.core
{
    /// <summary>
    /// Interface for TeamMembersWithExtendedProperties operations in Azure DevOps API
    /// </summary>
    public interface ITeamMembersWithExtendedProperties
    {
        /// <summary>
        /// Get a list of members for a specific team.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="queryParameters">Query parameters for the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Get a list of members for a specific team.</returns>
        Task<T?> GetTeamMembersWithExtendedPropertiesAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default);

    }
}