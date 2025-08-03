namespace TableauSharp.Permissions.Models;

public class PermissionSyncRequest
{
    public string TargetId { get; set; }
    public List<TableauPermission> Permissions { get; set; } = new();
}