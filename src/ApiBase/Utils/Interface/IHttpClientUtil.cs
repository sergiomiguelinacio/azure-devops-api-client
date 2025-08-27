namespace ApiBase.Utils.Interfaces
{
    public interface IHttpClientUtil
    {
        /// <summary>
        /// Executes a GET request with resilience policies
        /// </summary>
        Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a POST request with resilience policies
        /// </summary>
        Task<T?> PostAsync<T>(string endpoint, object? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a PUT request with resilience policies
        /// </summary>
        Task<T?> PutAsync<T>(string endpoint, object? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a DELETE request with resilience policies
        /// </summary>
        Task<T?> DeleteAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a PATCH request with resilience policies
        /// </summary>
        Task<T?> PatchAsync<T>(string endpoint, object? content = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

        // Legacy methods for backward compatibility
        Task<T?> ManagedResult<T>(HttpResponseMessage response);
        Task<T?> HttpPost<T>(string endpoint, HttpContent content, Dictionary<string, string> headers);
        Task<T?> HttpGet<T>(string endpoint, Dictionary<string, string> headers);
    }
}
