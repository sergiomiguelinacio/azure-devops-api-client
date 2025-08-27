using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsApi.Dto.core
{
    /// <summary>
    /// Path parameters for GetTeams operation
    /// </summary>
    public class GetTeamsPathParametersDto
    {
        /// <summary>
        /// The name of the Azure DevOps organization.
        /// </summary>
        [JsonProperty("organization")]
        [Required]
        public string Organization { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("projectId")]
        [Required]
        public string ProjectId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Query parameters for GetTeams operation
    /// </summary>
    public class GetTeamsQueryParametersDto
    {
        /// <summary>
        /// Version of the API to use. This should be set to '7.2-preview.3' to use this version of the api.
        /// </summary>
        [JsonProperty("api-version")]
        [Required]
        public string? Apiversion { get; set; }
        /// <summary>
        /// A value indicating whether or not to expand Identity information in the result WebApiTeam object.
        /// </summary>
        [JsonProperty("$expandIdentity")]
        public string? ExpandIdentity { get; set; }
        /// <summary>
        /// If true return all the teams requesting user is member, otherwise return all the teams user has read access.
        /// </summary>
        [JsonProperty("$mine")]
        public string? Mine { get; set; }
        /// <summary>
        /// Number of teams to skip.
        /// </summary>
        [JsonProperty("$skip")]
        public string? Skip { get; set; }
        /// <summary>
        /// Maximum number of teams to return.
        /// </summary>
        [JsonProperty("$top")]
        public string? Top { get; set; }
    }


    /// <summary>
    /// Path parameters for Create operation
    /// </summary>
    public class CreatePathParametersDto
    {
        /// <summary>
        /// The name of the Azure DevOps organization.
        /// </summary>
        [JsonProperty("organization")]
        [Required]
        public string Organization { get; set; } = string.Empty;
        /// <summary>
        /// The name or ID (GUID) of the team project in which to create the team.
        /// </summary>
        [JsonProperty("projectId")]
        [Required]
        public string ProjectId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Query parameters for Create operation
    /// </summary>
    public class CreateQueryParametersDto
    {
        /// <summary>
        /// Version of the API to use. This should be set to '7.2-preview.3' to use this version of the api.
        /// </summary>
        [JsonProperty("api-version")]
        [Required]
        public string? Apiversion { get; set; }
    }


    /// <summary>
    /// Path parameters for Get operation
    /// </summary>
    public class GetPathParametersDto
    {
        /// <summary>
        /// The name of the Azure DevOps organization.
        /// </summary>
        [JsonProperty("organization")]
        [Required]
        public string Organization { get; set; } = string.Empty;
        /// <summary>
        /// The name or ID (GUID) of the team project containing the team.
        /// </summary>
        [JsonProperty("projectId")]
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        /// <summary>
        /// The name or ID (GUID) of the team.
        /// </summary>
        [JsonProperty("teamId")]
        [Required]
        public string TeamId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Query parameters for Get operation
    /// </summary>
    public class GetQueryParametersDto
    {
        /// <summary>
        /// Version of the API to use. This should be set to '7.2-preview.3' to use this version of the api.
        /// </summary>
        [JsonProperty("api-version")]
        [Required]
        public string? Apiversion { get; set; }
        /// <summary>
        /// A value indicating whether or not to expand Identity information in the result WebApiTeam object.
        /// </summary>
        [JsonProperty("$expandIdentity")]
        public string? ExpandIdentity { get; set; }
    }


    /// <summary>
    /// Path parameters for Delete operation
    /// </summary>
    public class DeletePathParametersDto
    {
        /// <summary>
        /// The name of the Azure DevOps organization.
        /// </summary>
        [JsonProperty("organization")]
        [Required]
        public string Organization { get; set; } = string.Empty;
        /// <summary>
        /// The name or ID (GUID) of the team project containing the team to delete.
        /// </summary>
        [JsonProperty("projectId")]
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        /// <summary>
        /// The name or ID of the team to delete.
        /// </summary>
        [JsonProperty("teamId")]
        [Required]
        public string TeamId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Query parameters for Delete operation
    /// </summary>
    public class DeleteQueryParametersDto
    {
        /// <summary>
        /// Version of the API to use. This should be set to '7.2-preview.3' to use this version of the api.
        /// </summary>
        [JsonProperty("api-version")]
        [Required]
        public string? Apiversion { get; set; }
    }


    /// <summary>
    /// Path parameters for Update operation
    /// </summary>
    public class UpdatePathParametersDto
    {
        /// <summary>
        /// The name of the Azure DevOps organization.
        /// </summary>
        [JsonProperty("organization")]
        [Required]
        public string Organization { get; set; } = string.Empty;
        /// <summary>
        /// The name or ID (GUID) of the team project containing the team to update.
        /// </summary>
        [JsonProperty("projectId")]
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        /// <summary>
        /// The name of ID of the team to update.
        /// </summary>
        [JsonProperty("teamId")]
        [Required]
        public string TeamId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Query parameters for Update operation
    /// </summary>
    public class UpdateQueryParametersDto
    {
        /// <summary>
        /// Version of the API to use. This should be set to '7.2-preview.3' to use this version of the api.
        /// </summary>
        [JsonProperty("api-version")]
        [Required]
        public string? Apiversion { get; set; }
    }


    /// <summary>
    /// Path parameters for GetAllTeams operation
    /// </summary>
    public class GetAllTeamsPathParametersDto
    {
        /// <summary>
        /// The name of the Azure DevOps organization.
        /// </summary>
        [JsonProperty("organization")]
        [Required]
        public string Organization { get; set; } = string.Empty;
    }

    /// <summary>
    /// Query parameters for GetAllTeams operation
    /// </summary>
    public class GetAllTeamsQueryParametersDto
    {
        /// <summary>
        /// Version of the API to use. This should be set to '7.2-preview.3' to use this version of the api.
        /// </summary>
        [JsonProperty("api-version")]
        [Required]
        public string? Apiversion { get; set; }
        /// <summary>
        /// A value indicating whether or not to expand Identity information in the result WebApiTeam object.
        /// </summary>
        [JsonProperty("$expandIdentity")]
        public string? ExpandIdentity { get; set; }
        /// <summary>
        /// If true, then return all teams requesting user is member. Otherwise return all teams user has read access.
        /// </summary>
        [JsonProperty("$mine")]
        public string? Mine { get; set; }
        /// <summary>
        /// Number of teams to skip.
        /// </summary>
        [JsonProperty("$skip")]
        public string? Skip { get; set; }
        /// <summary>
        /// Maximum number of teams to return.
        /// </summary>
        [JsonProperty("$top")]
        public string? Top { get; set; }
    }


    /// <summary>
    /// Path parameters for GetTeamMembersWithExtendedProperties operation
    /// </summary>
    public class GetTeamMembersWithExtendedPropertiesPathParametersDto
    {
        /// <summary>
        /// The name of the Azure DevOps organization.
        /// </summary>
        [JsonProperty("organization")]
        [Required]
        public string Organization { get; set; } = string.Empty;
        /// <summary>
        /// The name or ID (GUID) of the team project the team belongs to.
        /// </summary>
        [JsonProperty("projectId")]
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        /// <summary>
        /// The name or ID (GUID) of the team .
        /// </summary>
        [JsonProperty("teamId")]
        [Required]
        public string TeamId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Query parameters for GetTeamMembersWithExtendedProperties operation
    /// </summary>
    public class GetTeamMembersWithExtendedPropertiesQueryParametersDto
    {
        /// <summary>
        /// Version of the API to use. This should be set to '7.2-preview.2' to use this version of the api.
        /// </summary>
        [JsonProperty("api-version")]
        [Required]
        public string? Apiversion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("$skip")]
        public string? Skip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("$top")]
        public string? Top { get; set; }
    }



    // Common response DTOs
    /// <summary>
    /// Standard list response for Azure DevOps API
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class AzureDevOpsListResponse<T>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("value")]
        public List<T> Value { get; set; } = new List<T>();
    }

    /// <summary>
    /// Team Project DTO
    /// </summary>
    public class TeamProjectDto
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("state")]
        public ProjectState State { get; set; }

        [JsonProperty("visibility")]
        public ProjectVisibility Visibility { get; set; }

        [JsonProperty("revision")]
        public long? Revision { get; set; }

        [JsonProperty("lastUpdateTime")]
        public DateTime? LastUpdateTime { get; set; }

        [JsonProperty("abbreviation")]
        public string? Abbreviation { get; set; }

        [JsonProperty("defaultTeam")]
        public WebApiTeamRefDto? DefaultTeam { get; set; }

        [JsonProperty("_links")]
        public ReferenceLinksDto? Links { get; set; }

        [JsonProperty("capabilities")]
        public Dictionary<string, object>? Capabilities { get; set; }
    }

    /// <summary>
    /// Web API Team Reference DTO
    /// </summary>
    public class WebApiTeamRefDto
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string? Url { get; set; }
    }

    /// <summary>
    /// Reference Links DTO
    /// </summary>
    public class ReferenceLinksDto
    {
        [JsonProperty("self")]
        public ReferenceLinkDto? Self { get; set; }

        [JsonProperty("web")]
        public ReferenceLinkDto? Web { get; set; }
    }

    /// <summary>
    /// Reference Link DTO
    /// </summary>
    public class ReferenceLinkDto
    {
        [JsonProperty("href")]
        public string? Href { get; set; }
    }

    /// <summary>
    /// Project State enumeration
    /// </summary>
    public enum ProjectState
    {
        CreatePending,
        Deleted,
        Deleting,
        New,
        Unchanged,
        WellFormed
    }

    /// <summary>
    /// Project Visibility enumeration
    /// </summary>
    public enum ProjectVisibility
    {
        Private,
        Public
    }

    /// <summary>
    /// JSON Patch Operation DTO
    /// </summary>
    public class JsonPatchOperationDto
    {
        [JsonProperty("op")]
        [Required]
        public string Op { get; set; } = string.Empty;

        [JsonProperty("path")]
        [Required]
        public string Path { get; set; } = string.Empty;

        [JsonProperty("value")]
        public object? Value { get; set; }

        [JsonProperty("from")]
        public string? From { get; set; }
    }
}