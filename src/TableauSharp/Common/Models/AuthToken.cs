using System.Text.Json.Serialization;

namespace TableauSharp.Common.Models;

public class AuthToken
{
    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("siteId")]
    public string SiteId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("expiration")]
    public DateTime Expiration { get; set; }
}