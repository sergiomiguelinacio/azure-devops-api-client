using ApiBase.Utils.Interfaces;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace ApiBase.Utils.Implementations
{
    /// <summary>
    /// Default implementation of Polly policies for Azure DevOps API operations
    /// Provides sensible defaults for retry, circuit breaker, and timeout policies
    /// </summary>
    public class DefaultPollyPolicyProvider : IPollyPolicyProvider
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;

        public DefaultPollyPolicyProvider(RetryOptions? retryOptions = null)
        {
            var options = retryOptions ?? new RetryOptions();
            _policy = CreateDefaultPolicy(options);
        }

        public IAsyncPolicy<HttpResponseMessage> GetPolicy()
        {
            return _policy;
        }

        private static IAsyncPolicy<HttpResponseMessage> CreateDefaultPolicy(RetryOptions retryOptions)
        {
            return Policy
                .HandleResult<HttpResponseMessage>(response => IsTransientHttpStatusCode(response.StatusCode))
                .Or<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: retryOptions.MaxRetries,
                    sleepDurationProvider: retryAttempt => retryOptions.UseExponentialBackoff
                        ? TimeSpan.FromMilliseconds(retryOptions.BaseDelayMs * Math.Pow(2, retryAttempt - 1))
                        : TimeSpan.FromMilliseconds(retryOptions.BaseDelayMs),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Optional: Add logging here if needed
                    });
        }

        private static bool IsTransientHttpStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.RequestTimeout => true,
                HttpStatusCode.InternalServerError => true,
                HttpStatusCode.BadGateway => true,
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,
                HttpStatusCode.TooManyRequests => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// Advanced Polly policy provider with circuit breaker
    /// Uses Polly 7.x compatible APIs
    /// </summary>
    public class AdvancedPollyPolicyProvider : IPollyPolicyProvider
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;

        public AdvancedPollyPolicyProvider(RetryOptions? retryOptions = null)
        {
            var options = retryOptions ?? new RetryOptions();
            _policy = CreateAdvancedPolicy(options);
        }

        public IAsyncPolicy<HttpResponseMessage> GetPolicy()
        {
            return _policy;
        }

        private static IAsyncPolicy<HttpResponseMessage> CreateAdvancedPolicy(RetryOptions retryOptions)
        {
            // Create a more sophisticated retry policy with circuit breaker
            var retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(response => IsTransientHttpStatusCode(response.StatusCode))
                .Or<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: retryOptions.MaxRetries,
                    sleepDurationProvider: retryAttempt => retryOptions.UseExponentialBackoff
                        ? TimeSpan.FromMilliseconds(retryOptions.BaseDelayMs * Math.Pow(2, retryAttempt - 1))
                        : TimeSpan.FromMilliseconds(retryOptions.BaseDelayMs),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Optional: Add logging here if needed
                    });

            var circuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30));

            // Combine retry and circuit breaker policies
            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }

        private static bool IsTransientHttpStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.RequestTimeout => true,
                HttpStatusCode.InternalServerError => true,
                HttpStatusCode.BadGateway => true,
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,
                HttpStatusCode.TooManyRequests => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// Configuration options for circuit breaker policy
    /// </summary>
    public class CircuitBreakerOptions
    {
        /// <summary>
        /// The failure threshold (0.0 to 1.0) at which the circuit will break
        /// </summary>
        public double FailureThreshold { get; set; } = 0.5;

        /// <summary>
        /// The duration in seconds over which failure statistics are sampled
        /// </summary>
        public int SamplingDurationSeconds { get; set; } = 30;

        /// <summary>
        /// The minimum number of actions that must pass through the circuit in the sampling duration
        /// </summary>
        public int MinimumThroughput { get; set; } = 10;

        /// <summary>
        /// The duration in seconds the circuit will stay open before attempting to close
        /// </summary>
        public int DurationOfBreakSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Configuration options for timeout policy
    /// </summary>
    public class TimeoutOptions
    {
        /// <summary>
        /// Timeout in seconds for individual operations
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300; // 5 minutes
    }
}
