using System.Text.Json.Serialization;

namespace TableauSharp.Workbooks.Models;

public class TableauView
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("contentUrl")]
    public string ContentUrl { get; set; } = string.Empty;

    [JsonPropertyName("workbookId")]
    public string WorkbookId { get; set; } = string.Empty;

    [JsonPropertyName("ownerId")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("totalViews")]
    public int TotalViews { get; set; }

    [JsonPropertyName("lastViewedAt")]
    public DateTime? LastViewedAt { get; set; }
}
