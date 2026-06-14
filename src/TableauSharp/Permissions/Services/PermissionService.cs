using System.Text;
using System.Text.Json;
using TableauSharp.Common.Http;
using TableauSharp.Permissions.Models;

namespace TableauSharp.Permissions.Services;

public class PermissionService : IPermissionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauRequestBuilder _requestBuilder;

    public PermissionService(
        IHttpClientFactory httpClientFactory,
        ITableauRequestBuilder requestBuilder)
    {
        _httpClientFactory = httpClientFactory;
        _requestBuilder = requestBuilder;
    }

    public async Task<IEnumerable<TableauPermission>> GetWorkbookPermissionsAsync(string workbookId, CancellationToken cancellationToken = default)
    {
        return await GetPermissionsAsync($"workbooks/{workbookId}/permissions", cancellationToken);
    }

    public async Task<IEnumerable<TableauPermission>> GetProjectPermissionsAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await GetPermissionsAsync($"projects/{projectId}/permissions", cancellationToken);
    }

    public async Task<IEnumerable<TableauPermission>> GetDataSourcePermissionsAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        return await GetPermissionsAsync($"datasources/{dataSourceId}/permissions", cancellationToken);
    }

    private async Task<IEnumerable<TableauPermission>> GetPermissionsAsync(string endpoint, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, endpoint);
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var permissions = new List<TableauPermission>();
        if (doc.RootElement.TryGetProperty("permissions", out var permsElement))
        {
            if (permsElement.TryGetProperty("granteeCapabilities", out var granteeCapabilities))
            {
                foreach (var gc in granteeCapabilities.EnumerateArray())
                {
                    var permission = new TableauPermission();
                    
                    if (gc.TryGetProperty("user", out var userEl))
                    {
                        permission.GranteeType = "User";
                        permission.GranteeId = userEl.GetProperty("id").GetString() ?? string.Empty;
                    }
                    else if (gc.TryGetProperty("group", out var groupEl))
                    {
                        permission.GranteeType = "Group";
                        permission.GranteeId = groupEl.GetProperty("id").GetString() ?? string.Empty;
                    }

                    if (gc.TryGetProperty("capabilities", out var capsElement))
                    {
                        if (capsElement.TryGetProperty("capability", out var capabilityArray))
                        {
                            foreach (var cap in capabilityArray.EnumerateArray())
                            {
                                var capabilityName = cap.GetProperty("name").GetString() ?? string.Empty;
                                var mode = cap.GetProperty("mode").GetString() ?? string.Empty;

                                if (Enum.TryParse<Common.Models.Enums.PermissionCapability>(capabilityName, true, out var parsedCap))
                                {
                                    permission.Capabilities.Add(new PermissionCapabilityAssignment
                                    {
                                        Capability = parsedCap,
                                        Mode = mode
                                    });
                                }
                            }
                        }
                    }

                    permissions.Add(permission);
                }
            }
        }

        return permissions;
    }

    public async Task AddWorkbookPermissionAsync(string workbookId, TableauPermission permission, CancellationToken cancellationToken = default)
    {
        await AddPermissionAsync($"workbooks/{workbookId}/permissions", permission, cancellationToken);
    }

    public async Task AddProjectPermissionAsync(string projectId, TableauPermission permission, CancellationToken cancellationToken = default)
    {
        await AddPermissionAsync($"projects/{projectId}/permissions", permission, cancellationToken);
    }

    public async Task AddDataSourcePermissionAsync(string dataSourceId, TableauPermission permission, CancellationToken cancellationToken = default)
    {
        await AddPermissionAsync($"datasources/{dataSourceId}/permissions", permission, cancellationToken);
    }

    private async Task AddPermissionAsync(string endpoint, TableauPermission permission, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");

        var payload = new
        {
            permissions = new
            {
                granteeCapabilities = new[]
                {
                    new
                    {
                        user = permission.GranteeType == "User" ? new { id = permission.GranteeId } : null,
                        group = permission.GranteeType == "Group" ? new { id = permission.GranteeId } : null,
                        capabilities = new
                        {
                            capability = permission.Capabilities.Select(c => new
                            {
                                name = c.Capability.ToString(),
                                mode = c.Mode
                            })
                        }
                    }
                }
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Put, endpoint);
        request.Content = jsonContent;
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteWorkbookPermissionAsync(string workbookId, string granteeId, string granteeType, string capability, CancellationToken cancellationToken = default)
    {
        await DeletePermissionAsync($"workbooks/{workbookId}/permissions", granteeId, granteeType, capability, cancellationToken);
    }

    public async Task DeleteProjectPermissionAsync(string projectId, string granteeId, string granteeType, string capability, CancellationToken cancellationToken = default)
    {
        await DeletePermissionAsync($"projects/{projectId}/permissions", granteeId, granteeType, capability, cancellationToken);
    }

    public async Task DeleteDataSourcePermissionAsync(string dataSourceId, string granteeId, string granteeType, string capability, CancellationToken cancellationToken = default)
    {
        await DeletePermissionAsync($"datasources/{dataSourceId}/permissions", granteeId, granteeType, capability, cancellationToken);
    }

    private async Task DeletePermissionAsync(string endpoint, string granteeId, string granteeType, string capability, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        
        // Validate granteeType
        if (!granteeType.Equals("User", StringComparison.OrdinalIgnoreCase) && 
            !granteeType.Equals("Group", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("GranteeType must be either 'User' or 'Group'", nameof(granteeType));
        }
        
        var granteeTypeParam = granteeType.ToLower() + "s"; // "users" or "groups"
        var url = $"{endpoint}/{granteeTypeParam}/{granteeId}/{capability}";

        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Delete, url);
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
