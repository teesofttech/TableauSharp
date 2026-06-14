using System.Text.Json.Serialization;

namespace TableauSharp.Permissions.Models;

public class TableauPermission
{
    [JsonPropertyName("granteeId")]
    public string GranteeId { get; set; } = string.Empty;

    [JsonPropertyName("granteeType")]
    public string GranteeType { get; set; } = string.Empty; // "User" or "Group"

    [JsonPropertyName("capabilities")]
    public List<PermissionCapabilityAssignment> Capabilities { get; set; } = new();
}
