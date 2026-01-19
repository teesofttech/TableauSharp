namespace TableauSharp.DataSources.Models;

/// <summary>
/// Represents a Tableau data source
/// </summary>
public class TableauDataSource
{
    /// <summary>
    /// Unique identifier for the data source
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the data source
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Project ID containing this data source
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Owner user ID
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Type of the data source (e.g., "sqlserver", "postgres")
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// When the data source was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the data source was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Whether the data source is certified
    /// </summary>
    public bool IsCertified { get; set; }

    /// <summary>
    /// Content URL for accessing the data source
    /// </summary>
    public string? ContentUrl { get; set; }
}
