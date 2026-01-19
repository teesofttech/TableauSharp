namespace TableauSharp.DataSources.Models;

/// <summary>
/// Request model for publishing a data source
/// </summary>
public class DataSourcePublishRequest
{
    public string Name { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool Overwrite { get; set; } = false;
    public string? Description { get; set; }
}
