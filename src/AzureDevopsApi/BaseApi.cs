using ApiBase.Utils.Interfaces;
using ApiBase.Utils.Implementations;
using System.Web;

namespace ApiBase
{
    public abstract class BaseApi
    {
        protected readonly IHttpClientUtil _httpClient;
        protected readonly string _baseUrl;

        protected BaseApi(IHttpClientUtil httpClient, string baseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        protected BaseApi(HttpClient httpClient, string baseUrl)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            _httpClient = new HttpClientUtil(httpClient);
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        protected BaseApi(HttpClient httpClient, string baseUrl, IPollyPolicyProvider policyProvider)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            _httpClient = new HttpClientUtil(httpClient, policyProvider);
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        /// <summary>
        /// Builds a complete URL by combining base URL, path, and query parameters
        /// </summary>
        /// <param name="path">The API path (e.g., "/{organization}/_apis/projects")</param>
        /// <param name="pathParameters">Path parameters to replace in the path</param>
        /// <param name="queryParameters">Query parameters to append</param>
        /// <returns>The complete URL</returns>
        protected string BuildUrl(string path, Dictionary<string, object>? pathParameters = null, Dictionary<string, object>? queryParameters = null)
        {
            var url = path;

            // Replace path parameters
            if (pathParameters != null)
            {
                foreach (var param in pathParameters)
                {
                    url = url.Replace($"{{{param.Key}}}", HttpUtility.UrlEncode(param.Value?.ToString()));
                }
            }

            // Add query parameters
            if (queryParameters != null && queryParameters.Any())
            {
                var queryString = string.Join("&", 
                    queryParameters
                        .Where(p => p.Value != null)
                        .Select(p => $"{HttpUtility.UrlEncode(p.Key)}={HttpUtility.UrlEncode(p.Value?.ToString())}"));
                
                url += url.Contains('?') ? $"&{queryString}" : $"?{queryString}";
            }

            return $"{_baseUrl}{url}";
        }

        /// <summary>
        /// Creates headers dictionary with authentication and other common headers
        /// </summary>
        /// <param name="additionalHeaders">Additional headers to include</param>
        /// <returns>Headers dictionary</returns>
        protected Dictionary<string, string> CreateHeaders(Dictionary<string, string>? additionalHeaders = null)
        {
            var headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["User-Agent"] = "Azure-DevOps-Generated-Client/1.0"
            };

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    headers[header.Key] = header.Value;
                }
            }

            return headers;
        }

        /// <summary>
        /// Extracts path parameters from a DTO object
        /// </summary>
        /// <param name="pathParametersDto">The path parameters DTO</param>
        /// <returns>Dictionary of path parameters</returns>
        protected Dictionary<string, object> ExtractPathParameters(object? pathParametersDto)
        {
            var parameters = new Dictionary<string, object>();
            
            if (pathParametersDto == null) return parameters;

            var properties = pathParametersDto.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(pathParametersDto);
                if (value != null)
                {
                    parameters[property.Name] = value;
                }
            }

            return parameters;
        }

        /// <summary>
        /// Extracts query parameters from a DTO object
        /// </summary>
        /// <param name="queryParametersDto">The query parameters DTO</param>
        /// <returns>Dictionary of query parameters</returns>
        protected Dictionary<string, object> ExtractQueryParameters(object? queryParametersDto)
        {
            var parameters = new Dictionary<string, object>();
            
            if (queryParametersDto == null) return parameters;

            var properties = queryParametersDto.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(queryParametersDto);
                if (value != null)
                {
                    // Convert property name to kebab-case for API compatibility
                    var paramName = ConvertToKebabCase(property.Name);
                    parameters[paramName] = value;
                }
            }

            return parameters;
        }

        /// <summary>
        /// Converts PascalCase to kebab-case
        /// </summary>
        /// <param name="input">PascalCase string</param>
        /// <returns>kebab-case string</returns>
        private string ConvertToKebabCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Handle special cases for API parameters
            return input switch
            {
                "ApiVersion" => "api-version",
                "ContinuationToken" => "continuationToken",
                "GetDefaultTeamImageUrl" => "getDefaultTeamImageUrl",
                "StateFilter" => "stateFilter",
                "IncludeCapabilities" => "includeCapabilities",
                "IncludeHistory" => "includeHistory",
                _ => char.ToLower(input[0]) + input.Substring(1)
            };
        }
    }
}
