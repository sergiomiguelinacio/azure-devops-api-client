using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevopsApi.Services
{
    /// <summary>
    /// Service for monitoring component health
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Register a health check for a component
        /// </summary>
        void RegisterHealthCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> healthCheck, TimeSpan? timeout = null);

        /// <summary>
        /// Execute all health checks
        /// </summary>
        Task<OverallHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute specific health check
        /// </summary>
        Task<HealthCheckResult> CheckComponentHealthAsync(string componentName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get cached health status
        /// </summary>
        OverallHealthStatus GetCachedHealthStatus();

        /// <summary>
        /// Start continuous health monitoring
        /// </summary>
        Task StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop health monitoring
        /// </summary>
        Task StopMonitoringAsync();
    }

    /// <summary>
    /// Implementation of health check service
    /// </summary>
    public class HealthCheckService : IHealthCheckService, IDisposable
    {
        private readonly Dictionary<string, HealthCheckRegistration> _healthChecks;
        private readonly object _lock = new();
        private OverallHealthStatus? _cachedStatus;
        private Timer? _monitoringTimer;
        private bool _disposed;

        public HealthCheckService()
        {
            _healthChecks = new Dictionary<string, HealthCheckRegistration>();
        }

        public void RegisterHealthCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> healthCheck, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Health check name cannot be null or empty", nameof(name));

            if (healthCheck == null)
                throw new ArgumentNullException(nameof(healthCheck));

            lock (_lock)
            {
                _healthChecks[name] = new HealthCheckRegistration
                {
                    Name = name,
                    HealthCheck = healthCheck,
                    Timeout = timeout ?? TimeSpan.FromSeconds(30),
                    LastResult = null,
                    LastExecuted = null
                };
            }
        }

        public async Task<OverallHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, HealthCheckResult>();
            var tasks = new List<Task>();

            Dictionary<string, HealthCheckRegistration> healthChecksSnapshot;
            lock (_lock)
            {
                healthChecksSnapshot = new Dictionary<string, HealthCheckRegistration>(_healthChecks);
            }

            foreach (var kvp in healthChecksSnapshot)
            {
                tasks.Add(ExecuteHealthCheckAsync(kvp.Key, kvp.Value, results, cancellationToken));
            }

            await Task.WhenAll(tasks);

            var overallStatus = DetermineOverallStatus(results.Values);
            var healthStatus = new OverallHealthStatus
            {
                Status = overallStatus,
                Results = results,
                CheckedAt = DateTime.UtcNow,
                TotalDuration = results.Values.Sum(r => r.Duration.TotalMilliseconds)
            };

            lock (_lock)
            {
                _cachedStatus = healthStatus;
            }

            return healthStatus;
        }

        public async Task<HealthCheckResult> CheckComponentHealthAsync(string componentName, CancellationToken cancellationToken = default)
        {
            HealthCheckRegistration? registration;
            lock (_lock)
            {
                if (!_healthChecks.TryGetValue(componentName, out registration))
                {
                    return new HealthCheckResult
                    {
                        Status = HealthStatus.Unhealthy,
                        Description = $"Component '{componentName}' not registered",
                        Exception = new InvalidOperationException($"Health check for '{componentName}' is not registered"),
                        Duration = TimeSpan.Zero
                    };
                }
            }

            return await ExecuteHealthCheckInternalAsync(componentName, registration, cancellationToken);
        }

        public OverallHealthStatus GetCachedHealthStatus()
        {
            lock (_lock)
            {
                return _cachedStatus ?? new OverallHealthStatus
                {
                    Status = HealthStatus.Unknown,
                    Results = new Dictionary<string, HealthCheckResult>(),
                    CheckedAt = DateTime.MinValue,
                    TotalDuration = 0
                };
            }
        }

        public async Task StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default)
        {
            if (_monitoringTimer != null)
            {
                await StopMonitoringAsync();
            }

            _monitoringTimer = new Timer(async _ =>
            {
                try
                {
                    await CheckHealthAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error but don't throw - monitoring should continue
                    Console.WriteLine($"Health check monitoring error: {ex.Message}");
                }
            }, null, TimeSpan.Zero, interval);
        }

        public async Task StopMonitoringAsync()
        {
            if (_monitoringTimer != null)
            {
                await _monitoringTimer.DisposeAsync();
                _monitoringTimer = null;
            }
        }

        private async Task ExecuteHealthCheckAsync(
            string name, 
            HealthCheckRegistration registration, 
            Dictionary<string, HealthCheckResult> results,
            CancellationToken cancellationToken)
        {
            var result = await ExecuteHealthCheckInternalAsync(name, registration, cancellationToken);
            
            lock (_lock)
            {
                results[name] = result;
                registration.LastResult = result;
                registration.LastExecuted = DateTime.UtcNow;
            }
        }

        private async Task<HealthCheckResult> ExecuteHealthCheckInternalAsync(
            string name,
            HealthCheckRegistration registration,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(registration.Timeout);

                var result = await registration.HealthCheck(timeoutCts.Token);
                stopwatch.Stop();

                return new HealthCheckResult
                {
                    Status = result?.Status ?? HealthStatus.Unhealthy,
                    Description = result?.Description ?? "Health check returned null",
                    Data = result?.Data ?? new Dictionary<string, object>(),
                    Duration = stopwatch.Elapsed,
                    Exception = result?.Exception
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Description = "Health check was cancelled",
                    Duration = stopwatch.Elapsed,
                    Exception = new OperationCanceledException("Health check cancelled by user")
                };
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Description = $"Health check timed out after {registration.Timeout.TotalSeconds} seconds",
                    Duration = stopwatch.Elapsed,
                    Exception = new TimeoutException($"Health check for '{name}' timed out")
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Description = $"Health check failed: {ex.Message}",
                    Duration = stopwatch.Elapsed,
                    Exception = ex
                };
            }
        }

        private static HealthStatus DetermineOverallStatus(IEnumerable<HealthCheckResult> results)
        {
            var resultList = results.ToList();
            
            if (!resultList.Any())
                return HealthStatus.Unknown;

            if (resultList.All(r => r.Status == HealthStatus.Healthy))
                return HealthStatus.Healthy;

            if (resultList.All(r => r.Status == HealthStatus.Unhealthy))
                return HealthStatus.Unhealthy;

            return HealthStatus.Degraded;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _monitoringTimer?.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Health check registration information
    /// </summary>
    internal class HealthCheckRegistration
    {
        public string Name { get; set; } = string.Empty;
        public Func<CancellationToken, Task<HealthCheckResult>> HealthCheck { get; set; } = null!;
        public TimeSpan Timeout { get; set; }
        public HealthCheckResult? LastResult { get; set; }
        public DateTime? LastExecuted { get; set; }
    }

    /// <summary>
    /// Result of a health check
    /// </summary>
    public class HealthCheckResult
    {
        public HealthStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public TimeSpan Duration { get; set; }
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Overall health status of all components
    /// </summary>
    public class OverallHealthStatus
    {
        public HealthStatus Status { get; set; }
        public Dictionary<string, HealthCheckResult> Results { get; set; } = new();
        public DateTime CheckedAt { get; set; }
        public double TotalDuration { get; set; }
    }

    /// <summary>
    /// Health status enumeration
    /// </summary>
    public enum HealthStatus
    {
        Unknown,
        Healthy,
        Degraded,
        Unhealthy
    }
}
