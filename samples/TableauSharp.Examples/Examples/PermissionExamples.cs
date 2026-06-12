using Microsoft.Extensions.Logging;
using TableauSharp.Auth.Service;
using TableauSharp.Common.Models.Enums;
using TableauSharp.Permissions.Models;
using TableauSharp.Permissions.Services;
using TableauSharp.Users.Services;
using TableauSharp.Workbooks.Services;

namespace TableauSharp.Examples.Examples;

// Demonstrates permission management across workbooks, projects, and data sources.
// Tableau uses a capability model: each capability (View, Export, etc.) is either
// Allowed or Denied for a specific user or group on a specific resource.
public class PermissionExamples(
    IAuthService authService,
    IPermissionService permissionService,
    IWorkbookService workbookService,
    IUserService userService,
    ILogger<PermissionExamples> logger)
{
    public async Task RunAsync()
    {
        Console.WriteLine("--- Permission Examples ---");
        Console.WriteLine();

        await authService.SignInWithPATAsync();

        await GetWorkbookPermissionsAsync();
        await GrantAndRevokePermissionAsync();
    }

    private async Task GetWorkbookPermissionsAsync()
    {
        Console.WriteLine("[1] Get permissions for first workbook");
        try
        {
            var workbooks = await workbookService.GetAllAsync();
            var first = workbooks.FirstOrDefault();
            if (first is null) { Console.WriteLine("    No workbooks."); return; }

            Console.WriteLine($"    Workbook: {first.Name}");
            var permissions = await permissionService.GetWorkbookPermissionsAsync(first.Id);
            foreach (var p in permissions)
            {
                Console.WriteLine($"    {p.GranteeType} {p.GranteeId}");
                foreach (var cap in p.Capabilities)
                    Console.WriteLine($"      {cap.Capability,-20} {cap.Mode}");
            }

            if (!permissions.Any())
                Console.WriteLine("    No permissions set.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Get permissions failed");
        }

        Console.WriteLine();
    }

    // This example grants View permission to the first user on the first workbook,
    // then immediately revokes it to leave no side-effects.
    private async Task GrantAndRevokePermissionAsync()
    {
        Console.WriteLine("[2] Grant View permission to a user, then revoke it");
        try
        {
            var workbooks = await workbookService.GetAllAsync();
            var workbook = workbooks.FirstOrDefault();
            var users = await userService.GetAllAsync();
            var user = users.FirstOrDefault();

            if (workbook is null || user is null)
            {
                Console.WriteLine("    Needs at least one workbook and one user.");
                return;
            }

            var permission = new TableauPermission
            {
                GranteeType = "User",
                GranteeId = user.Id,
                Capabilities =
                [
                    new PermissionCapabilityAssignment { Capability = PermissionCapability.View, Mode = "Allow" }
                ]
            };

            await permissionService.AddWorkbookPermissionAsync(workbook.Id, permission);
            Console.WriteLine($"    Granted View/Allow to user '{user.Name}' on workbook '{workbook.Name}'.");

            await permissionService.DeleteWorkbookPermissionAsync(
                workbook.Id, user.Id, "User", PermissionCapability.View.ToString());
            Console.WriteLine("    Revoked.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Grant/revoke permission failed");
        }

        Console.WriteLine();
    }
}
