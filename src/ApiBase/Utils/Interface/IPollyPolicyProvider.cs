using Polly;

namespace ApiBase.Utils.Interfaces
{
    /// <summary>
    /// Interface for providing Polly policies for HTTP operations
    /// </summary>
    public interface IPollyPolicyProvider
    {
        /// <summary>
        /// Gets the async policy for HTTP operations
        /// </summary>
        IAsyncPolicy<HttpResponseMessage> GetPolicy();
    }
}
