using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AzureDevOpsApi.Api;
using ApiBase.Utils.Implementations;

namespace AzureDevOpsApi.Examples
{
    /// <summary>
    /// Basic usage examples for the Azure DevOps API Client
    /// </summary>
    public class BasicUsageExample
    {
        private const string ORGANIZATION = "your-organization";
        private const string PROJECT_ID = "your-project-id";
        private const string BASE_URL = "https://dev.azure.com";
        private const string PAT_TOKEN = "your-personal-access-token";

        /// <summary>
        /// Example 1: Simple usage with default retry policies
        /// </summary>
        public static async Task SimpleUsageExample()
        {
            Console.WriteLine("🚀 Example 1: Simple Usage");
            
            // Create API client with default settings
            using var apiClient = new coreApiClient(BASE_URL);

            try
            {
                // Get teams for an organization
                var teams = await apiClient.Teams.GetTeamsAsync(
                    pathParameters: new { organization = ORGANIZATION, projectId = PROJECT_ID }
                );

                Console.WriteLine($"✅ Found {teams.Value?.Count ?? 0} teams");
                
                if (teams.Value != null)
                {
                    foreach (var team in teams.Value)
                    {
                        Console.WriteLine($"   📋 Team: {team.Name} - {team.Description}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Example 2: Usage with authentication (Personal Access Token)
        /// </summary>
        public static async Task AuthenticatedUsageExample()
        {
            Console.WriteLine("\n🔐 Example 2: Authenticated Usage");
            
            // Create HttpClient with authentication
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", PAT_TOKEN);

            // Create API client with authenticated HttpClient
            using var apiClient = new coreApiClient(httpClient, BASE_URL);

            try
            {
                // Get operations
                var operations = await apiClient.Operations.GetOperationsAsync(
                    pathParameters: new { organization = ORGANIZATION }
                );

                Console.WriteLine($"✅ Found {operations.Value?.Count ?? 0} operations");
                
                if (operations.Value != null)
                {
                    foreach (var operation in operations.Value)
                    {
                        Console.WriteLine($"   ⚙️ Operation: {operation.Id} - Status: {operation.Status}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Example 3: Advanced configuration with custom retry policies
        /// </summary>
        public static async Task AdvancedConfigurationExample()
        {
            Console.WriteLine("\n⚙️ Example 3: Advanced Configuration");
            
            // Configure custom retry options
            var retryOptions = new RetryOptions
            {
                MaxRetries = 5,
                BaseDelayMs = 2000,
                UseExponentialBackoff = true
            };

            // Create API client with custom retry policies
            using var apiClient = new coreApiClient(BASE_URL, new DefaultPollyPolicyProvider(retryOptions));

            try
            {
                // Get team members with extended properties
                var teamMembers = await apiClient.TeamMembersWithExtendedProperties.GetTeamMembersWithExtendedPropertiesAsync(
                    pathParameters: new { organization = ORGANIZATION, projectId = PROJECT_ID, teamId = "team-id" }
                );

                Console.WriteLine($"✅ Found {teamMembers.Value?.Count ?? 0} team members");
                
                if (teamMembers.Value != null)
                {
                    foreach (var member in teamMembers.Value)
                    {
                        Console.WriteLine($"   👤 Member: {member.Identity?.DisplayName} - {member.Identity?.UniqueName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Example 4: Error handling and resilience
        /// </summary>
        public static async Task ErrorHandlingExample()
        {
            Console.WriteLine("\n🛡️ Example 4: Error Handling");
            
            using var apiClient = new coreApiClient(BASE_URL);

            try
            {
                // Attempt to get teams with invalid parameters
                var teams = await apiClient.Teams.GetTeamsAsync(
                    pathParameters: new { organization = "invalid-org", projectId = "invalid-project" }
                );
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"🌐 Network Error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"⏱️ Timeout Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Run all examples
        /// </summary>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🎯 Azure DevOps API Client - Usage Examples\n");
            
            await SimpleUsageExample();
            await AuthenticatedUsageExample();
            await AdvancedConfigurationExample();
            await ErrorHandlingExample();
            
            Console.WriteLine("\n✨ All examples completed!");
            Console.WriteLine("📚 For more information, visit: https://github.com/sergio/AzureDevOpsApi");
        }
    }
}
