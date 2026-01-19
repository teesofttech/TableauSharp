using TableauSharp.DataSources.Models;

namespace TableauSharp.DataSources.Services;

/// <summary>
/// Service for managing Tableau data sources
/// </summary>
public interface IDataSourceService
{
    /// <summary>
    /// Get all data sources in the site
    /// </summary>
    Task<IEnumerable<TableauDataSource>> GetAllAsync();

    /// <summary>
    /// Get a specific data source by ID
    /// </summary>
    Task<TableauDataSource> GetByIdAsync(string dataSourceId);

    /// <summary>
    /// Publish a data source to a project
    /// </summary>
    Task<TableauDataSource> PublishAsync(DataSourcePublishRequest request);

    /// <summary>
    /// Delete a data source
    /// </summary>
    Task DeleteAsync(string dataSourceId);

    /// <summary>
    /// Refresh a data source extract
    /// </summary>
    Task RefreshAsync(string dataSourceId);
}
