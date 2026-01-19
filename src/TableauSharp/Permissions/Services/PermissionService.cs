using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TableauSharp.Common.Helper;
using TableauSharp.Permissions.Models;
using TableauSharp.Settings;

namespace TableauSharp.Permissions.Services;

public class PermissionService : IPermissionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauTokenProvider _tokenProvider;
    private readonly TableauAuthOptions _authOptions;
    private readonly TableauOptions _tableauOptions;

    public PermissionService(
        IHttpClientFactory httpClientFactory,
        ITableauTokenProvider tokenProvider,
        IOptions<TableauAuthOptions> authOptions,
        IOptions<TableauOptions> tableauOptions)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
        _authOptions = authOptions.Value;
        _tableauOptions = tableauOptions.Value;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        client.BaseAddress = new Uri($"{_tableauOptions.Server}/api/{_tableauOptions.Version}/sites/{_authOptions.SiteContentUrl}/");
        client.DefaultRequestHeaders.Add("X-Tableau-Auth", _tokenProvider.GetToken());
        return client;
    }

    public async Task<IEnumerable<TableauPermission>> GetWorkbookPermissionsAsync(string workbookId)
    {
        return await GetPermissionsAsync($"workbooks/{workbookId}/permissions");
    }

    public async Task<IEnumerable<TableauPermission>> GetProjectPermissionsAsync(string projectId)
    {
        return await GetPermissionsAsync($"projects/{projectId}/permissions");
    }

    public async Task<IEnumerable<TableauPermission>> GetDataSourcePermissionsAsync(string dataSourceId)
    {
        return await GetPermissionsAsync($"datasources/{dataSourceId}/permissions");
    }

    private async Task<IEnumerable<TableauPermission>> GetPermissionsAsync(string endpoint)
    {
        using var client = CreateClient();
        var response = await client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
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

    public async Task AddWorkbookPermissionAsync(string workbookId, TableauPermission permission)
    {
        await AddPermissionAsync($"workbooks/{workbookId}/permissions", permission);
    }

    public async Task AddProjectPermissionAsync(string projectId, TableauPermission permission)
    {
        await AddPermissionAsync($"projects/{projectId}/permissions", permission);
    }

    public async Task AddDataSourcePermissionAsync(string dataSourceId, TableauPermission permission)
    {
        await AddPermissionAsync($"datasources/{dataSourceId}/permissions", permission);
    }

    private async Task AddPermissionAsync(string endpoint, TableauPermission permission)
    {
        using var client = CreateClient();

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

        var response = await client.PutAsync(endpoint, jsonContent);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteWorkbookPermissionAsync(string workbookId, string granteeId, string granteeType, string capability)
    {
        await DeletePermissionAsync($"workbooks/{workbookId}/permissions", granteeId, granteeType, capability);
    }

    public async Task DeleteProjectPermissionAsync(string projectId, string granteeId, string granteeType, string capability)
    {
        await DeletePermissionAsync($"projects/{projectId}/permissions", granteeId, granteeType, capability);
    }

    public async Task DeleteDataSourcePermissionAsync(string dataSourceId, string granteeId, string granteeType, string capability)
    {
        await DeletePermissionAsync($"datasources/{dataSourceId}/permissions", granteeId, granteeType, capability);
    }

    private async Task DeletePermissionAsync(string endpoint, string granteeId, string granteeType, string capability)
    {
        using var client = CreateClient();
        
        // Validate granteeType
        if (!granteeType.Equals("User", StringComparison.OrdinalIgnoreCase) && 
            !granteeType.Equals("Group", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("GranteeType must be either 'User' or 'Group'", nameof(granteeType));
        }
        
        var granteeTypeParam = granteeType.ToLower() + "s"; // "users" or "groups"
        var url = $"{endpoint}/{granteeTypeParam}/{granteeId}/{capability}";

        var response = await client.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }
}
