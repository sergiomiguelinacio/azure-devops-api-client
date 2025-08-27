using System.Threading.Tasks;

namespace AzureDevOpsApi.Api.Interface.core
{
    /// <summary>
    /// Interface for Operations operations in Azure DevOps API
    /// </summary>
    public interface IOperations
    {
        /// <summary>
        /// Create a team in a team project. Possible failure scenarios Invalid project name/ID (project doesn't exist) 404 Invalid team name or description 400 Team already ex...
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="queryParameters">Query parameters for the request</param>
        /// <param name="requestBody">Request body content</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Create a team in a team project. Possible failure scenarios Invalid project name/ID (project doesn't exist) 404 Invalid team name or description 400 Team already ex...</returns>
        Task<T?> CreateAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            object? requestBody = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a specific team.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="queryParameters">Query parameters for the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Get a specific team.</returns>
        Task<T?> GetAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a team.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="queryParameters">Query parameters for the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delete a team.</returns>
        Task<T?> DeleteAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a team's name and/or description.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="queryParameters">Query parameters for the request</param>
        /// <param name="requestBody">Request body content</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Update a team's name and/or description.</returns>
        Task<T?> UpdateAsync<T>(
            object? pathParameters = null,
            object? queryParameters = null,
            object? requestBody = null,
            CancellationToken cancellationToken = default);

    }
}