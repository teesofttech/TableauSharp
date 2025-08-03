using System.Text.Json.Serialization;

namespace TableauSharp.Projects.Models;

public class TableauProject
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("parentProjectId")]
    public string ParentProjectId { get; set; }

    [JsonPropertyName("ownerId")]
    public string OwnerId { get; set; }
}