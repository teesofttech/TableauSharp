using TableauSharp.Permissions.Models;

namespace TableauSharp.Permissions.Services;

/// <summary>
/// Service for managing Tableau permissions
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Get permissions for a workbook
    /// </summary>
    Task<IEnumerable<TableauPermission>> GetWorkbookPermissionsAsync(string workbookId);

    /// <summary>
    /// Get permissions for a project
    /// </summary>
    Task<IEnumerable<TableauPermission>> GetProjectPermissionsAsync(string projectId);

    /// <summary>
    /// Get permissions for a data source
    /// </summary>
    Task<IEnumerable<TableauPermission>> GetDataSourcePermissionsAsync(string dataSourceId);

    /// <summary>
    /// Add or update permission capability for a workbook
    /// </summary>
    Task AddWorkbookPermissionAsync(string workbookId, TableauPermission permission);

    /// <summary>
    /// Add or update permission capability for a project
    /// </summary>
    Task AddProjectPermissionAsync(string projectId, TableauPermission permission);

    /// <summary>
    /// Add or update permission capability for a data source
    /// </summary>
    Task AddDataSourcePermissionAsync(string dataSourceId, TableauPermission permission);

    /// <summary>
    /// Delete permission for a workbook
    /// </summary>
    Task DeleteWorkbookPermissionAsync(string workbookId, string granteeId, string granteeType, string capability);

    /// <summary>
    /// Delete permission for a project
    /// </summary>
    Task DeleteProjectPermissionAsync(string projectId, string granteeId, string granteeType, string capability);

    /// <summary>
    /// Delete permission for a data source
    /// </summary>
    Task DeleteDataSourcePermissionAsync(string dataSourceId, string granteeId, string granteeType, string capability);
}
