using System.Text.Json.Serialization;

namespace TableauSharp.Workbooks.Models;

public class TableauView
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("contentUrl")]
    public string ContentUrl { get; set; }

    [JsonPropertyName("workbookId")]
    public string WorkbookId { get; set; }

    [JsonPropertyName("ownerId")]
    public string OwnerId { get; set; }

    [JsonPropertyName("totalViews")]
    public int TotalViews { get; set; }

    [JsonPropertyName("lastViewedAt")]
    public DateTime LastViewedAt { get; set; }
}