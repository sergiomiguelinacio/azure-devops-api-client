using AzureDevOpsApi.Api;
using ApiBase.Utils.Implementations;
using System.Net.Http.Headers;

namespace BasicUsage;

/// <summary>
/// Basic usage examples for Azure DevOps API Client
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Azure DevOps API Client - Basic Usage Examples");
        Console.WriteLine("=================================================");

        // Get configuration from environment variables
        var organization = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG") ?? "your-organization";
        var personalAccessToken = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");

        if (string.IsNullOrEmpty(personalAccessToken))
        {
            Console.WriteLine("❌ Please set AZURE_DEVOPS_PAT environment variable");
            Console.WriteLine("💡 Example: export AZURE_DEVOPS_PAT=your-token-here");
            return;
        }

        try
        {
            await RunBasicExamples(organization, personalAccessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    static async Task RunBasicExamples(string organization, string personalAccessToken)
    {
        // Example 1: Simple API client with default retry policies
        Console.WriteLine("\n📋 Example 1: List Projects");
        Console.WriteLine("----------------------------");

        var policyProvider = new DefaultPollyPolicyProvider(new RetryOptions
        {
            MaxRetries = 3,
            BaseDelayMs = 1000,
            UseExponentialBackoff = true
        });
        var apiClient = new coreApiClient("https://dev.azure.com", policyProvider);

        // Configure authentication
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", personalAccessToken);

        var authenticatedClient = new coreApiClient(httpClient, "https://dev.azure.com");

        // Get teams (since Projects is not available in core API)
        var teams = await authenticatedClient.Teams.GetTeamsAsync<AzureDevOpsListResponse<TeamDto>>(
            pathParameters: new { organization, projectId = "sample-project" },
            queryParameters: new { top = 10 }
        );

        Console.WriteLine($"✅ Found {teams?.Count ?? 0} teams:");
        if (teams?.Value != null)
        {
            foreach (var team in teams.Value.Take(5))
            {
                Console.WriteLine($"   👥 {team.Name}");
            }
        }

        // Example 2: Get all teams
        Console.WriteLine("\n📋 Example 2: All Teams");
        Console.WriteLine("------------------------");

        try
        {
            var allTeams = await authenticatedClient.Teams.GetAllTeamsAsync<AzureDevOpsListResponse<TeamDto>>(
                pathParameters: new
                {
                    organization
                },
                queryParameters: new
                {
                    top = 5
                }
            );

            Console.WriteLine($"✅ All Teams:");
            if (allTeams?.Value != null)
            {
                foreach (var team in allTeams.Value)
                {
                    Console.WriteLine($"   👥 {team.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ℹ️ Could not fetch all teams: {ex.Message}");
        }

        // Example 3: Error handling
        Console.WriteLine("\n📋 Example 3: Error Handling");
        Console.WriteLine("-----------------------------");

        try
        {
            // Try to get a specific team (this might fail if team doesn't exist)
            await authenticatedClient.Operations.GetAsync<TeamDto>(
                pathParameters: new
                {
                    organization,
                    projectId = "non-existent-project",
                    teamId = "non-existent-team"
                }
            );
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            Console.WriteLine("✅ Correctly handled 404 error for non-existent team");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Unexpected error: {ex.Message}");
        }

        Console.WriteLine("\n🎉 Basic examples completed!");
    }
}

/// <summary>
/// Example DTO classes (these would be auto-generated)
/// </summary>
public class AzureDevOpsListResponse<T>
{
    public int Count { get; set; }
    public List<T> Value { get; set; } = new();
}

public class TeamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class OperationDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
}
