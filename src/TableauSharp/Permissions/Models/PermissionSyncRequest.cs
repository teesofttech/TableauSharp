namespace TableauSharp.Permissions.Models;

public class PermissionSyncRequest
{
    public string TargetId { get; set; } = string.Empty;
    public List<TableauPermission> Permissions { get; set; } = new();
}
