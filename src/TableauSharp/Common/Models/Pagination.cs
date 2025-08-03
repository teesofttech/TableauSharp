using System.Text.Json.Serialization;

namespace TableauSharp.Common.Models;

public class Pagination
{
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalAvailable")]
    public int TotalAvailable { get; set; }
}