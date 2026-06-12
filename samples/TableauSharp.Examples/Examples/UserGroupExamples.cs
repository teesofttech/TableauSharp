using Microsoft.Extensions.Logging;
using TableauSharp.Auth.Service;
using TableauSharp.Users.Models;
using TableauSharp.Users.Services;

namespace TableauSharp.Examples.Examples;

// Demonstrates user and group management.
// All operations are site-scoped and require a valid auth token.
// Sign in before calling any of these methods.
public class UserGroupExamples(
    IAuthService authService,
    IUserService userService,
    IGroupService groupService,
    ILogger<UserGroupExamples> logger)
{
    public async Task RunAsync()
    {
        Console.WriteLine("--- User & Group Examples ---");
        Console.WriteLine();

        await authService.SignInWithPATAsync();

        await ListUsersAsync();
        await CreateAndUpdateUserAsync();
        await CreateGroupAndManageMembersAsync();
    }

    private async Task ListUsersAsync()
    {
        Console.WriteLine("[1] List all users");
        try
        {
            var users = await userService.GetAllAsync();
            foreach (var u in users)
                Console.WriteLine($"    {u.Id}  {u.Name,-30}  {u.SiteRole}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "List users failed");
        }

        Console.WriteLine();
    }

    private async Task CreateAndUpdateUserAsync()
    {
        Console.WriteLine("[2] Create and update a user");
        try
        {
            var created = await userService.CreateAsync(new UserCreateRequest
            {
                Name = "sdk.example@company.com",
                Email = "sdk.example@company.com",
                SiteRole = "Viewer"
            });
            Console.WriteLine($"    Created: {created.Id}  {created.Name}  {created.SiteRole}");

            var updated = await userService.UpdateAsync(created.Id, new UserUpdateRequest
            {
                SiteRole = "Explorer",
                IsActive = true
            });
            Console.WriteLine($"    Updated role to: {updated.SiteRole}");

            await userService.DeleteAsync(created.Id);
            Console.WriteLine("    Deleted.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User CRUD failed");
        }

        Console.WriteLine();
    }

    private async Task CreateGroupAndManageMembersAsync()
    {
        Console.WriteLine("[3] Create group and manage members");
        try
        {
            var group = await groupService.CreateAsync(new GroupCreateRequest { Name = "SDK-Example-Group" });
            Console.WriteLine($"    Group created: {group.Id}  {group.Name}");

            // Add first available user to the group
            var users = await userService.GetAllAsync();
            var user = users.FirstOrDefault();
            if (user is not null)
            {
                await groupService.AddUserToGroupAsync(group.Id, user.Id);
                Console.WriteLine($"    Added user '{user.Name}' to group.");

                await groupService.RemoveUserFromGroupAsync(group.Id, user.Id);
                Console.WriteLine("    Removed user from group.");
            }

            await groupService.DeleteAsync(group.Id);
            Console.WriteLine("    Group deleted.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Group management failed");
        }

        Console.WriteLine();
    }
}
