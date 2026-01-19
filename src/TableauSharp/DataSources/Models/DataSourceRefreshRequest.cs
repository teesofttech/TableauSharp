namespace TableauSharp.DataSources.Models;

/// <summary>
/// Request model for refreshing a data source
/// </summary>
public class DataSourceRefreshRequest
{
    public string DataSourceId { get; set; } = string.Empty;
}
