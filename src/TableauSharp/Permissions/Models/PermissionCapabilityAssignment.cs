using System.Text.Json.Serialization;
using TableauSharp.Common.Models.Enums;

namespace TableauSharp.Permissions.Models;

public class PermissionCapabilityAssignment
{
    [JsonPropertyName("capability")]
    public PermissionCapability Capability { get; set; }

    [JsonPropertyName("mode")]
    public string Mode { get; set; } // "Allow" or "Deny"
}