using Newtonsoft.Json;

namespace ApiBase.Dto.Responses.Authentication
{
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string? TokenType { get; set; }
        [JsonProperty("ext_expires_in")]
        public int ExpiresIn { get; set; }
    }
}
