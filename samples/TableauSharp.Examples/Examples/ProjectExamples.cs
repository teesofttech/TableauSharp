using Microsoft.Extensions.Logging;
using TableauSharp.Auth.Service;
using TableauSharp.Projects.Models;
using TableauSharp.Projects.Services;

namespace TableauSharp.Examples.Examples;

// Demonstrates project management.
// Projects are the top-level containers for workbooks and data sources.
// Nested projects (sub-folders) are supported via ParentProjectId.
public class ProjectExamples(
    IAuthService authService,
    IProjectService projectService,
    ILogger<ProjectExamples> logger)
{
    public async Task RunAsync()
    {
        Console.WriteLine("--- Project Examples ---");
        Console.WriteLine();

        await authService.SignInWithPATAsync();

        await ListProjectsAsync();
        await CreateUpdateDeleteProjectAsync();
        await CreateNestedProjectAsync();
    }

    private async Task ListProjectsAsync()
    {
        Console.WriteLine("[1] List all projects");
        try
        {
            var projects = await projectService.GetAllAsync();
            foreach (var p in projects)
                Console.WriteLine($"    {p.Id}  {p.Name,-30}  parent={p.ParentProjectId ?? "none"}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "List projects failed");
        }

        Console.WriteLine();
    }

    private async Task CreateUpdateDeleteProjectAsync()
    {
        Console.WriteLine("[2] Create, update, and delete a project");
        try
        {
            var created = await projectService.CreateAsync(new ProjectCreateRequest
            {
                Name = "SDK-Example-Project",
                Description = "Created by TableauSharp SDK example"
            });
            Console.WriteLine($"    Created: {created.Id}  {created.Name}");

            var updated = await projectService.UpdateAsync(created.Id, new ProjectUpdateRequest
            {
                Name = "SDK-Example-Project-Renamed",
                Description = "Updated by TableauSharp SDK example"
            });
            Console.WriteLine($"    Renamed to: {updated.Name}");

            await projectService.DeleteAsync(created.Id);
            Console.WriteLine("    Deleted.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Project CRUD failed");
        }

        Console.WriteLine();
    }

    // Sub-projects allow hierarchical organisation of content on Tableau Server.
    private async Task CreateNestedProjectAsync()
    {
        Console.WriteLine("[3] Create a nested project (sub-folder)");
        try
        {
            var parent = await projectService.CreateAsync(new ProjectCreateRequest { Name = "SDK-Parent" });
            Console.WriteLine($"    Parent: {parent.Id}  {parent.Name}");

            var child = await projectService.CreateAsync(new ProjectCreateRequest
            {
                Name = "SDK-Child",
                ParentProjectId = parent.Id
            });
            Console.WriteLine($"    Child : {child.Id}  {child.Name}  (parent={child.ParentProjectId})");

            await projectService.DeleteAsync(child.Id);
            await projectService.DeleteAsync(parent.Id);
            Console.WriteLine("    Cleaned up.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Nested project example failed");
        }

        Console.WriteLine();
    }
}
