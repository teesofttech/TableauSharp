using System.Text.Json.Serialization;

namespace TableauSharp.Users.Models;

public class TableauGroup
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("users")]
    public List<TableauUser> Users { get; set; } = new();
}
