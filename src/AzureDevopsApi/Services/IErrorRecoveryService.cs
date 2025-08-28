using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureDevopsApi.Exceptions;

namespace AzureDevopsApi.Services
{
    /// <summary>
    /// Service for automatic error recovery and retry logic
    /// </summary>
    public interface IErrorRecoveryService
    {
        /// <summary>
        /// Execute operation with automatic retry and recovery
        /// </summary>
        Task<T> ExecuteWithRecoveryAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            RecoveryPolicy? policy = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute operation with custom recovery strategies
        /// </summary>
        Task<T> ExecuteWithCustomRecoveryAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            IEnumerable<IRecoveryStrategy> recoveryStrategies,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Register a recovery strategy for specific exception types
        /// </summary>
        void RegisterRecoveryStrategy<TException>(IRecoveryStrategy strategy) where TException : Exception;

        /// <summary>
        /// Get recovery statistics
        /// </summary>
        RecoveryStatistics GetRecoveryStatistics();

        /// <summary>
        /// Reset recovery statistics
        /// </summary>
        void ResetStatistics();
    }

    /// <summary>
    /// Implementation of error recovery service
    /// </summary>
    public class ErrorRecoveryService : IErrorRecoveryService
    {
        private readonly Dictionary<Type, List<IRecoveryStrategy>> _recoveryStrategies;
        private readonly RecoveryStatistics _statistics;
        private readonly object _lock = new();

        public ErrorRecoveryService()
        {
            _recoveryStrategies = new Dictionary<Type, List<IRecoveryStrategy>>();
            _statistics = new RecoveryStatistics();
            InitializeDefaultStrategies();
        }

        public async Task<T> ExecuteWithRecoveryAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            RecoveryPolicy? policy = null,
            CancellationToken cancellationToken = default)
        {
            policy ??= RecoveryPolicy.Default;
            var attempts = 0;
            var exceptions = new List<Exception>();

            while (attempts < policy.MaxRetries)
            {
                try
                {
                    var result = await operation(cancellationToken);
                    
                    // Record successful recovery if this wasn't the first attempt
                    if (attempts > 0)
                    {
                        RecordSuccessfulRecovery(attempts, exceptions.LastOrDefault());
                    }
                    
                    return result;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
                {
                    attempts++;
                    exceptions.Add(ex);
                    
                    RecordFailedAttempt(ex);
                    
                    // Check if we should retry
                    if (!ShouldRetry(ex, attempts, policy))
                    {
                        break;
                    }
                    
                    // Try recovery strategies
                    var recovered = await TryRecoveryStrategies(ex, cancellationToken);
                    if (!recovered && attempts < policy.MaxRetries)
                    {
                        // Wait before retry
                        var delay = CalculateDelay(attempts, policy, ex);
                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }

            // All retries exhausted
            RecordFailedRecovery(attempts, exceptions);
            
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }
            
            throw new AggregateException(
                $"Operation failed after {attempts} attempts", 
                exceptions);
        }

        public async Task<T> ExecuteWithCustomRecoveryAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            IEnumerable<IRecoveryStrategy> recoveryStrategies,
            CancellationToken cancellationToken = default)
        {
            var strategies = recoveryStrategies.ToList();
            
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                // Try each recovery strategy
                foreach (var strategy in strategies)
                {
                    try
                    {
                        if (await strategy.CanRecoverAsync(ex, cancellationToken))
                        {
                            await strategy.RecoverAsync(ex, cancellationToken);
                            
                            // Retry the operation after recovery
                            return await operation(cancellationToken);
                        }
                    }
                    catch (Exception recoveryEx)
                    {
                        // Log recovery failure but continue with next strategy
                        RecordRecoveryStrategyFailure(strategy.GetType().Name, recoveryEx);
                    }
                }
                
                // No recovery strategy worked
                throw;
            }
        }

        public void RegisterRecoveryStrategy<TException>(IRecoveryStrategy strategy) where TException : Exception
        {
            var exceptionType = typeof(TException);
            
            lock (_lock)
            {
                if (!_recoveryStrategies.ContainsKey(exceptionType))
                {
                    _recoveryStrategies[exceptionType] = new List<IRecoveryStrategy>();
                }
                
                _recoveryStrategies[exceptionType].Add(strategy);
            }
        }

        public RecoveryStatistics GetRecoveryStatistics()
        {
            lock (_lock)
            {
                return new RecoveryStatistics
                {
                    TotalAttempts = _statistics.TotalAttempts,
                    SuccessfulRecoveries = _statistics.SuccessfulRecoveries,
                    FailedRecoveries = _statistics.FailedRecoveries,
                    AverageAttemptsToSuccess = _statistics.AverageAttemptsToSuccess,
                    MostCommonExceptions = new Dictionary<string, int>(_statistics.MostCommonExceptions),
                    RecoveryStrategySuccesses = new Dictionary<string, int>(_statistics.RecoveryStrategySuccesses),
                    RecoveryStrategyFailures = new Dictionary<string, int>(_statistics.RecoveryStrategyFailures)
                };
            }
        }

        public void ResetStatistics()
        {
            lock (_lock)
            {
                _statistics.Reset();
            }
        }

        private async Task<bool> TryRecoveryStrategies(Exception exception, CancellationToken cancellationToken)
        {
            var strategies = GetRecoveryStrategies(exception);
            
            foreach (var strategy in strategies)
            {
                try
                {
                    if (await strategy.CanRecoverAsync(exception, cancellationToken))
                    {
                        await strategy.RecoverAsync(exception, cancellationToken);
                        RecordRecoveryStrategySuccess(strategy.GetType().Name);
                        return true;
                    }
                }
                catch (Exception recoveryEx)
                {
                    RecordRecoveryStrategyFailure(strategy.GetType().Name, recoveryEx);
                }
            }
            
            return false;
        }

        private IEnumerable<IRecoveryStrategy> GetRecoveryStrategies(Exception exception)
        {
            var strategies = new List<IRecoveryStrategy>();
            var exceptionType = exception.GetType();
            
            lock (_lock)
            {
                // Get strategies for exact type
                if (_recoveryStrategies.TryGetValue(exceptionType, out var exactStrategies))
                {
                    strategies.AddRange(exactStrategies);
                }
                
                // Get strategies for base types
                var baseType = exceptionType.BaseType;
                while (baseType != null && baseType != typeof(object))
                {
                    if (_recoveryStrategies.TryGetValue(baseType, out var baseStrategies))
                    {
                        strategies.AddRange(baseStrategies);
                    }
                    baseType = baseType.BaseType;
                }
            }
            
            return strategies;
        }

        private bool ShouldRetry(Exception exception, int attempts, RecoveryPolicy policy)
        {
            if (attempts >= policy.MaxRetries)
                return false;
                
            // Check if exception is retryable
            if (exception is AzureDevOpsException azureEx)
            {
                return azureEx.IsRetryable;
            }
            
            // Default retryable exceptions
            return exception is TimeoutException ||
                   exception is TaskCanceledException ||
                   (exception is HttpRequestException && !exception.Message.Contains("401")) ||
                   exception.Message.Contains("timeout") ||
                   exception.Message.Contains("network");
        }

        private TimeSpan CalculateDelay(int attempt, RecoveryPolicy policy, Exception exception)
        {
            // Use suggested delay from exception if available
            if (exception is AzureDevOpsException azureEx && azureEx.SuggestedRetryDelay.HasValue)
            {
                return azureEx.SuggestedRetryDelay.Value;
            }
            
            // Calculate exponential backoff
            var baseDelay = policy.BaseDelay.TotalMilliseconds;
            var exponentialDelay = baseDelay * Math.Pow(policy.BackoffMultiplier, attempt - 1);
            var jitteredDelay = exponentialDelay * (0.8 + Random.Shared.NextDouble() * 0.4); // ±20% jitter
            
            var finalDelay = TimeSpan.FromMilliseconds(Math.Min(jitteredDelay, policy.MaxDelay.TotalMilliseconds));
            return finalDelay;
        }

        private void InitializeDefaultStrategies()
        {
            // Register default recovery strategies
            RegisterRecoveryStrategy<ApiCommunicationException>(new NetworkRecoveryStrategy());
            RegisterRecoveryStrategy<RateLimitException>(new RateLimitRecoveryStrategy());
            RegisterRecoveryStrategy<ServiceUnavailableException>(new ServiceRecoveryStrategy());
        }

        private void RecordSuccessfulRecovery(int attempts, Exception? lastException)
        {
            lock (_lock)
            {
                _statistics.SuccessfulRecoveries++;
                _statistics.TotalAttempts += attempts;
                _statistics.UpdateAverageAttemptsToSuccess(attempts);
                
                if (lastException != null)
                {
                    var exceptionName = lastException.GetType().Name;
                    _statistics.MostCommonExceptions[exceptionName] = 
                        _statistics.MostCommonExceptions.GetValueOrDefault(exceptionName) + 1;
                }
            }
        }

        private void RecordFailedAttempt(Exception exception)
        {
            lock (_lock)
            {
                var exceptionName = exception.GetType().Name;
                _statistics.MostCommonExceptions[exceptionName] = 
                    _statistics.MostCommonExceptions.GetValueOrDefault(exceptionName) + 1;
            }
        }

        private void RecordFailedRecovery(int attempts, List<Exception> exceptions)
        {
            lock (_lock)
            {
                _statistics.FailedRecoveries++;
                _statistics.TotalAttempts += attempts;
            }
        }

        private void RecordRecoveryStrategySuccess(string strategyName)
        {
            lock (_lock)
            {
                _statistics.RecoveryStrategySuccesses[strategyName] = 
                    _statistics.RecoveryStrategySuccesses.GetValueOrDefault(strategyName) + 1;
            }
        }

        private void RecordRecoveryStrategyFailure(string strategyName, Exception exception)
        {
            lock (_lock)
            {
                _statistics.RecoveryStrategyFailures[strategyName] = 
                    _statistics.RecoveryStrategyFailures.GetValueOrDefault(strategyName) + 1;
            }
        }
    }

    /// <summary>
    /// Recovery policy configuration
    /// </summary>
    public class RecoveryPolicy
    {
        public int MaxRetries { get; set; } = 3;
        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);
        public double BackoffMultiplier { get; set; } = 2.0;
        
        public static RecoveryPolicy Default => new();
        
        public static RecoveryPolicy Aggressive => new()
        {
            MaxRetries = 5,
            BaseDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromMinutes(2),
            BackoffMultiplier = 1.5
        };
        
        public static RecoveryPolicy Conservative => new()
        {
            MaxRetries = 2,
            BaseDelay = TimeSpan.FromSeconds(5),
            MaxDelay = TimeSpan.FromMinutes(10),
            BackoffMultiplier = 3.0
        };
    }

    /// <summary>
    /// Interface for recovery strategies
    /// </summary>
    public interface IRecoveryStrategy
    {
        Task<bool> CanRecoverAsync(Exception exception, CancellationToken cancellationToken);
        Task RecoverAsync(Exception exception, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Recovery statistics
    /// </summary>
    public class RecoveryStatistics
    {
        public int TotalAttempts { get; set; }
        public int SuccessfulRecoveries { get; set; }
        public int FailedRecoveries { get; set; }
        public double AverageAttemptsToSuccess { get; set; }
        public Dictionary<string, int> MostCommonExceptions { get; set; } = new();
        public Dictionary<string, int> RecoveryStrategySuccesses { get; set; } = new();
        public Dictionary<string, int> RecoveryStrategyFailures { get; set; } = new();
        
        public double SuccessRate => TotalAttempts > 0 ? (double)SuccessfulRecoveries / (SuccessfulRecoveries + FailedRecoveries) : 0;
        
        internal void UpdateAverageAttemptsToSuccess(int attempts)
        {
            if (SuccessfulRecoveries == 1)
            {
                AverageAttemptsToSuccess = attempts;
            }
            else
            {
                AverageAttemptsToSuccess = ((AverageAttemptsToSuccess * (SuccessfulRecoveries - 1)) + attempts) / SuccessfulRecoveries;
            }
        }
        
        internal void Reset()
        {
            TotalAttempts = 0;
            SuccessfulRecoveries = 0;
            FailedRecoveries = 0;
            AverageAttemptsToSuccess = 0;
            MostCommonExceptions.Clear();
            RecoveryStrategySuccesses.Clear();
            RecoveryStrategyFailures.Clear();
        }
    }

    /// <summary>
    /// Network recovery strategy
    /// </summary>
    public class NetworkRecoveryStrategy : IRecoveryStrategy
    {
        public Task<bool> CanRecoverAsync(Exception exception, CancellationToken cancellationToken)
        {
            return Task.FromResult(exception is ApiCommunicationException ||
                                   exception is TimeoutException ||
                                   exception.Message.Contains("network") ||
                                   exception.Message.Contains("timeout"));
        }

        public async Task RecoverAsync(Exception exception, CancellationToken cancellationToken)
        {
            // Wait a bit for network to recover
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    /// <summary>
    /// Rate limit recovery strategy
    /// </summary>
    public class RateLimitRecoveryStrategy : IRecoveryStrategy
    {
        public Task<bool> CanRecoverAsync(Exception exception, CancellationToken cancellationToken)
        {
            return Task.FromResult(exception is RateLimitException);
        }

        public async Task RecoverAsync(Exception exception, CancellationToken cancellationToken)
        {
            if (exception is RateLimitException rateLimitEx && rateLimitEx.RetryAfter.HasValue)
            {
                await Task.Delay(rateLimitEx.RetryAfter.Value, cancellationToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Service recovery strategy
    /// </summary>
    public class ServiceRecoveryStrategy : IRecoveryStrategy
    {
        public Task<bool> CanRecoverAsync(Exception exception, CancellationToken cancellationToken)
        {
            return Task.FromResult(exception is ServiceUnavailableException);
        }

        public async Task RecoverAsync(Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ServiceUnavailableException serviceEx && serviceEx.EstimatedRecoveryTime.HasValue)
            {
                var waitTime = serviceEx.EstimatedRecoveryTime.Value - DateTime.UtcNow;
                if (waitTime > TimeSpan.Zero && waitTime < TimeSpan.FromMinutes(10))
                {
                    await Task.Delay(waitTime, cancellationToken);
                    return;
                }
            }
            
            await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
        }
    }
}
