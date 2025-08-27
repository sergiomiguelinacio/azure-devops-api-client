using System.Threading.Tasks;

namespace AzureDevOpsApi.Api.Interface.core
{
    /// <summary>
    /// Interface for Teams operations in Azure DevOps API
    /// </summary>
    public interface ITeams
    {
        /// <summary>
        /// Get a list of teams.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="queryParameters">Query parameters for the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Get a list of teams.</returns>
        Task<T?> GetTeamsAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a list of all teams.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="queryParameters">Query parameters for the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Get a list of all teams.</returns>
        Task<T?> GetAllTeamsAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default);

    }
}