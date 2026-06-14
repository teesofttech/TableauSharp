using System.Text.Json.Serialization;

namespace TableauSharp.Common.Models;

public class AuthToken
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("siteId")]
    public string SiteId { get; set; } = string.Empty;

    [JsonPropertyName("siteContentUrl")]
    public string? SiteContentUrl { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("expiration")]
    public DateTime Expiration { get; set; }
}
