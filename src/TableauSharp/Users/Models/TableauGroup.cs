using System.Text.Json.Serialization;

namespace TableauSharp.Users.Models;

public class TableauGroup
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("users")]
    public List<TableauUser> Users { get; set; } = new();
}