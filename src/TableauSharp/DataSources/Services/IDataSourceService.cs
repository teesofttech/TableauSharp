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
    Task<IEnumerable<TableauDataSource>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific data source by ID
    /// </summary>
    Task<TableauDataSource> GetByIdAsync(string dataSourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a data source to a project
    /// </summary>
    Task<TableauDataSource> PublishAsync(DataSourcePublishRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a data source
    /// </summary>
    Task DeleteAsync(string dataSourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh a data source extract
    /// </summary>
    Task RefreshAsync(string dataSourceId, CancellationToken cancellationToken = default);
}
