using System.Text.Json.Serialization;

namespace TableauSharp.Common.Models;

public class ErrorResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }
}