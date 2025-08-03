using System.Text.Json.Serialization;

namespace TableauSharp.Users.Models;

public class TableauUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("siteRole")]
    public string SiteRole { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}