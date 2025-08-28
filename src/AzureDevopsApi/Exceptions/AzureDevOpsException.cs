using System;
using System.Collections.Generic;
using System.Net;

namespace AzureDevopsApi.Exceptions
{
    /// <summary>
    /// Base exception for all Azure DevOps API related errors
    /// </summary>
    public abstract class AzureDevOpsException : Exception
    {
        public string ErrorCode { get; }
        public string Component { get; }
        public Dictionary<string, object> Context { get; }
        public DateTime Timestamp { get; }
        public bool IsRetryable { get; protected set; }
        public TimeSpan? SuggestedRetryDelay { get; protected set; }

        protected AzureDevOpsException(
            string message, 
            string errorCode, 
            string component,
            Exception? innerException = null) : base(message, innerException)
        {
            ErrorCode = errorCode;
            Component = component;
            Context = new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
            IsRetryable = false;
        }

        public void AddContext(string key, object value)
        {
            Context[key] = value;
        }

        public override string ToString()
        {
            var contextInfo = Context.Count > 0 
                ? $"\nContext: {string.Join(", ", Context.Select(kvp => $"{kvp.Key}={kvp.Value}"))}"
                : "";
            
            return $"[{ErrorCode}] {Component}: {Message}" +
                   $"\nTimestamp: {Timestamp:yyyy-MM-dd HH:mm:ss} UTC" +
                   $"\nRetryable: {IsRetryable}" +
                   (SuggestedRetryDelay.HasValue ? $"\nSuggested Retry Delay: {SuggestedRetryDelay}" : "") +
                   contextInfo +
                   (InnerException != null ? $"\nInner Exception: {InnerException}" : "");
        }
    }

    /// <summary>
    /// Exception thrown when API authentication fails
    /// </summary>
    public class AuthenticationException : AzureDevOpsException
    {
        public string AuthenticationMethod { get; }

        public AuthenticationException(string message, string authMethod, Exception? innerException = null)
            : base(message, "AUTH_FAILED", "Authentication", innerException)
        {
            AuthenticationMethod = authMethod;
            IsRetryable = false; // Authentication failures typically require manual intervention
            AddContext("AuthenticationMethod", authMethod);
        }
    }

    /// <summary>
    /// Exception thrown when user lacks required permissions
    /// </summary>
    public class AuthorizationException : AzureDevOpsException
    {
        public string RequiredPermission { get; }
        public string Resource { get; }

        public AuthorizationException(string message, string permission, string resource, Exception? innerException = null)
            : base(message, "AUTH_INSUFFICIENT", "Authorization", innerException)
        {
            RequiredPermission = permission;
            Resource = resource;
            IsRetryable = false;
            AddContext("RequiredPermission", permission);
            AddContext("Resource", resource);
        }
    }

    /// <summary>
    /// Exception thrown for HTTP communication errors
    /// </summary>
    public class ApiCommunicationException : AzureDevOpsException
    {
        public HttpStatusCode? StatusCode { get; }
        public string? RequestUri { get; }
        public string? ResponseContent { get; }

        public ApiCommunicationException(
            string message, 
            HttpStatusCode? statusCode = null, 
            string? requestUri = null,
            string? responseContent = null,
            Exception? innerException = null)
            : base(message, GetErrorCode(statusCode), "ApiCommunication", innerException)
        {
            StatusCode = statusCode;
            RequestUri = requestUri;
            ResponseContent = responseContent;
            
            // Determine if retryable based on status code
            IsRetryable = IsRetryableStatusCode(statusCode);
            SuggestedRetryDelay = GetRetryDelay(statusCode);
            
            if (statusCode.HasValue) AddContext("StatusCode", statusCode.Value);
            if (requestUri != null) AddContext("RequestUri", requestUri);
            if (responseContent != null) AddContext("ResponseContent", responseContent);
        }

        private static string GetErrorCode(HttpStatusCode? statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.Unauthorized => "HTTP_UNAUTHORIZED",
                HttpStatusCode.Forbidden => "HTTP_FORBIDDEN",
                HttpStatusCode.NotFound => "HTTP_NOT_FOUND",
                HttpStatusCode.TooManyRequests => "HTTP_RATE_LIMITED",
                HttpStatusCode.InternalServerError => "HTTP_SERVER_ERROR",
                HttpStatusCode.BadGateway => "HTTP_BAD_GATEWAY",
                HttpStatusCode.ServiceUnavailable => "HTTP_SERVICE_UNAVAILABLE",
                HttpStatusCode.GatewayTimeout => "HTTP_GATEWAY_TIMEOUT",
                _ => "HTTP_COMMUNICATION_ERROR"
            };
        }

        private static bool IsRetryableStatusCode(HttpStatusCode? statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.TooManyRequests => true,
                HttpStatusCode.InternalServerError => true,
                HttpStatusCode.BadGateway => true,
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,
                HttpStatusCode.RequestTimeout => true,
                _ => false
            };
        }

        private static TimeSpan? GetRetryDelay(HttpStatusCode? statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.TooManyRequests => TimeSpan.FromSeconds(60),
                HttpStatusCode.InternalServerError => TimeSpan.FromSeconds(30),
                HttpStatusCode.BadGateway => TimeSpan.FromSeconds(15),
                HttpStatusCode.ServiceUnavailable => TimeSpan.FromSeconds(45),
                HttpStatusCode.GatewayTimeout => TimeSpan.FromSeconds(20),
                _ => null
            };
        }
    }

    /// <summary>
    /// Exception thrown when API rate limits are exceeded
    /// </summary>
    public class RateLimitException : AzureDevOpsException
    {
        public int? RemainingRequests { get; }
        public DateTime? ResetTime { get; }
        public TimeSpan? RetryAfter { get; }

        public RateLimitException(
            string message, 
            int? remainingRequests = null,
            DateTime? resetTime = null,
            TimeSpan? retryAfter = null,
            Exception? innerException = null)
            : base(message, "RATE_LIMIT_EXCEEDED", "RateLimit", innerException)
        {
            RemainingRequests = remainingRequests;
            ResetTime = resetTime;
            RetryAfter = retryAfter;
            IsRetryable = true;
            SuggestedRetryDelay = retryAfter ?? TimeSpan.FromMinutes(1);
            
            if (remainingRequests.HasValue) AddContext("RemainingRequests", remainingRequests.Value);
            if (resetTime.HasValue) AddContext("ResetTime", resetTime.Value);
            if (retryAfter.HasValue) AddContext("RetryAfter", retryAfter.Value);
        }
    }

    /// <summary>
    /// Exception thrown for data validation errors
    /// </summary>
    public class ValidationException : AzureDevOpsException
    {
        public List<string> ValidationErrors { get; }
        public string? FieldName { get; }

        public ValidationException(
            string message, 
            List<string>? validationErrors = null,
            string? fieldName = null,
            Exception? innerException = null)
            : base(message, "VALIDATION_FAILED", "Validation", innerException)
        {
            ValidationErrors = validationErrors ?? new List<string>();
            FieldName = fieldName;
            IsRetryable = false; // Validation errors require data correction
            
            if (fieldName != null) AddContext("FieldName", fieldName);
            if (ValidationErrors.Any()) AddContext("ValidationErrors", string.Join("; ", ValidationErrors));
        }
    }

    /// <summary>
    /// Exception thrown when requested resource is not found
    /// </summary>
    public class ResourceNotFoundException : AzureDevOpsException
    {
        public string ResourceType { get; }
        public string ResourceId { get; }

        public ResourceNotFoundException(
            string message, 
            string resourceType, 
            string resourceId,
            Exception? innerException = null)
            : base(message, "RESOURCE_NOT_FOUND", "ResourceAccess", innerException)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
            IsRetryable = false; // Resource not found typically doesn't resolve with retry
            
            AddContext("ResourceType", resourceType);
            AddContext("ResourceId", resourceId);
        }
    }

    /// <summary>
    /// Exception thrown for configuration-related errors
    /// </summary>
    public class ConfigurationException : AzureDevOpsException
    {
        public string ConfigurationKey { get; }
        public string? ExpectedFormat { get; }

        public ConfigurationException(
            string message, 
            string configurationKey,
            string? expectedFormat = null,
            Exception? innerException = null)
            : base(message, "CONFIGURATION_ERROR", "Configuration", innerException)
        {
            ConfigurationKey = configurationKey;
            ExpectedFormat = expectedFormat;
            IsRetryable = false; // Configuration errors require manual fix
            
            AddContext("ConfigurationKey", configurationKey);
            if (expectedFormat != null) AddContext("ExpectedFormat", expectedFormat);
        }
    }

    /// <summary>
    /// Exception thrown for temporary service unavailability
    /// </summary>
    public class ServiceUnavailableException : AzureDevOpsException
    {
        public string ServiceName { get; }
        public DateTime? EstimatedRecoveryTime { get; }

        public ServiceUnavailableException(
            string message, 
            string serviceName,
            DateTime? estimatedRecoveryTime = null,
            Exception? innerException = null)
            : base(message, "SERVICE_UNAVAILABLE", "ServiceHealth", innerException)
        {
            ServiceName = serviceName;
            EstimatedRecoveryTime = estimatedRecoveryTime;
            IsRetryable = true;
            SuggestedRetryDelay = TimeSpan.FromMinutes(5);
            
            AddContext("ServiceName", serviceName);
            if (estimatedRecoveryTime.HasValue) AddContext("EstimatedRecoveryTime", estimatedRecoveryTime.Value);
        }
    }
}
