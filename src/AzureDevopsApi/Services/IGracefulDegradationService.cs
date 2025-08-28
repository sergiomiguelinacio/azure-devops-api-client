using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureDevopsApi.Exceptions;

namespace AzureDevopsApi.Services
{
    /// <summary>
    /// Service for handling graceful degradation when components fail
    /// </summary>
    public interface IGracefulDegradationService
    {
        /// <summary>
        /// Execute operation with graceful degradation fallbacks
        /// </summary>
        Task<TResult> ExecuteWithFallbackAsync<TResult>(
            Func<CancellationToken, Task<TResult>> primaryOperation,
            IEnumerable<Func<CancellationToken, Task<TResult>>> fallbackOperations,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute operation with partial success tolerance
        /// </summary>
        Task<PartialResult<TResult>> ExecuteWithPartialSuccessAsync<TResult>(
            IEnumerable<Func<CancellationToken, Task<TResult>>> operations,
            double minimumSuccessRate = 0.5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get degraded service status
        /// </summary>
        DegradationStatus GetDegradationStatus();

        /// <summary>
        /// Register a component for degradation monitoring
        /// </summary>
        void RegisterComponent(string componentName, ComponentHealthCheck healthCheck);

        /// <summary>
        /// Get available fallback options for a component
        /// </summary>
        IEnumerable<string> GetAvailableFallbacks(string componentName);
    }

    /// <summary>
    /// Implementation of graceful degradation service
    /// </summary>
    public class GracefulDegradationService : IGracefulDegradationService
    {
        private readonly Dictionary<string, ComponentInfo> _components;
        private readonly Dictionary<string, List<Exception>> _recentFailures;
        private readonly object _lock = new();

        public GracefulDegradationService()
        {
            _components = new Dictionary<string, ComponentInfo>();
            _recentFailures = new Dictionary<string, List<Exception>>();
        }

        public async Task<TResult> ExecuteWithFallbackAsync<TResult>(
            Func<CancellationToken, Task<TResult>> primaryOperation,
            IEnumerable<Func<CancellationToken, Task<TResult>>> fallbackOperations,
            CancellationToken cancellationToken = default)
        {
            var operations = new[] { primaryOperation }.Concat(fallbackOperations).ToList();
            var exceptions = new List<Exception>();

            for (int i = 0; i < operations.Count; i++)
            {
                try
                {
                    var result = await operations[i](cancellationToken);
                    
                    // Log successful fallback usage
                    if (i > 0)
                    {
                        RecordFallbackUsage($"Operation_{i}", exceptions.LastOrDefault());
                    }
                    
                    return result;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    exceptions.Add(ex);
                    RecordFailure($"Operation_{i}", ex);
                    
                    // If this is the last operation, throw aggregate exception
                    if (i == operations.Count - 1)
                    {
                        throw new AggregateException(
                            "All operations failed including fallbacks", 
                            exceptions);
                    }
                }
            }

            throw new InvalidOperationException("No operations provided");
        }

        public async Task<PartialResult<TResult>> ExecuteWithPartialSuccessAsync<TResult>(
            IEnumerable<Func<CancellationToken, Task<TResult>>> operations,
            double minimumSuccessRate = 0.5,
            CancellationToken cancellationToken = default)
        {
            var operationList = operations.ToList();
            var results = new List<TResult>();
            var failures = new List<Exception>();
            var tasks = new List<Task>();

            foreach (var operation in operationList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await operation(cancellationToken);
                        lock (_lock)
                        {
                            results.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (_lock)
                        {
                            failures.Add(ex);
                        }
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            var successRate = (double)results.Count / operationList.Count;
            var isSuccess = successRate >= minimumSuccessRate;

            return new PartialResult<TResult>
            {
                Results = results,
                Failures = failures,
                SuccessRate = successRate,
                IsSuccess = isSuccess,
                TotalOperations = operationList.Count,
                SuccessfulOperations = results.Count,
                FailedOperations = failures.Count
            };
        }

        public DegradationStatus GetDegradationStatus()
        {
            lock (_lock)
            {
                var componentStatuses = _components.Select(kvp => new ComponentStatus
                {
                    Name = kvp.Key,
                    IsHealthy = kvp.Value.IsHealthy,
                    LastCheck = kvp.Value.LastHealthCheck,
                    FailureCount = _recentFailures.GetValueOrDefault(kvp.Key)?.Count ?? 0,
                    AvailableFallbacks = kvp.Value.FallbackOptions.ToList()
                }).ToList();

                var overallHealth = componentStatuses.All(c => c.IsHealthy)
                    ? DegradationHealthStatus.Healthy
                    : componentStatuses.Any(c => c.IsHealthy)
                        ? DegradationHealthStatus.Degraded
                        : DegradationHealthStatus.Unhealthy;

                return new DegradationStatus
                {
                    OverallHealth = overallHealth,
                    Components = componentStatuses,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }

        public void RegisterComponent(string componentName, ComponentHealthCheck healthCheck)
        {
            lock (_lock)
            {
                _components[componentName] = new ComponentInfo
                {
                    Name = componentName,
                    HealthCheck = healthCheck,
                    IsHealthy = true,
                    LastHealthCheck = DateTime.UtcNow,
                    FallbackOptions = new List<string>()
                };
            }
        }

        public IEnumerable<string> GetAvailableFallbacks(string componentName)
        {
            lock (_lock)
            {
                return _components.GetValueOrDefault(componentName)?.FallbackOptions ?? Enumerable.Empty<string>();
            }
        }

        private void RecordFailure(string componentName, Exception exception)
        {
            lock (_lock)
            {
                if (!_recentFailures.ContainsKey(componentName))
                {
                    _recentFailures[componentName] = new List<Exception>();
                }

                _recentFailures[componentName].Add(exception);

                // Keep only recent failures (last 10)
                if (_recentFailures[componentName].Count > 10)
                {
                    _recentFailures[componentName].RemoveAt(0);
                }

                // Update component health
                if (_components.ContainsKey(componentName))
                {
                    _components[componentName].IsHealthy = false;
                    _components[componentName].LastHealthCheck = DateTime.UtcNow;
                }
            }
        }

        private void RecordFallbackUsage(string componentName, Exception? originalException)
        {
            // Log fallback usage for monitoring
            // This could be extended to send metrics to monitoring systems
        }
    }

    /// <summary>
    /// Result of partial execution
    /// </summary>
    public class PartialResult<T>
    {
        public List<T> Results { get; set; } = new();
        public List<Exception> Failures { get; set; } = new();
        public double SuccessRate { get; set; }
        public bool IsSuccess { get; set; }
        public int TotalOperations { get; set; }
        public int SuccessfulOperations { get; set; }
        public int FailedOperations { get; set; }
    }

    /// <summary>
    /// Overall degradation status
    /// </summary>
    public class DegradationStatus
    {
        public DegradationHealthStatus OverallHealth { get; set; }
        public List<ComponentStatus> Components { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Status of individual component
    /// </summary>
    public class ComponentStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public DateTime LastCheck { get; set; }
        public int FailureCount { get; set; }
        public List<string> AvailableFallbacks { get; set; } = new();
    }

    /// <summary>
    /// Component information for degradation tracking
    /// </summary>
    internal class ComponentInfo
    {
        public string Name { get; set; } = string.Empty;
        public ComponentHealthCheck HealthCheck { get; set; } = null!;
        public bool IsHealthy { get; set; }
        public DateTime LastHealthCheck { get; set; }
        public List<string> FallbackOptions { get; set; } = new();
    }

    /// <summary>
    /// Health check delegate
    /// </summary>
    public delegate Task<bool> ComponentHealthCheck(CancellationToken cancellationToken);

    /// <summary>
    /// Health status enumeration for degradation service
    /// </summary>
    public enum DegradationHealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }
}
