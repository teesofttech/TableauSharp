using System.Text;
using System.Text.Json;
using TableauSharp.Projects.Models;

namespace TableauSharp.Projects.Services;

public class ProjectService : IProjectService
{
    private readonly HttpClient _httpClient;

    public ProjectService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<TableauProject>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("projects");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var projects = new List<TableauProject>();
        foreach (var p in doc.RootElement.GetProperty("projects").EnumerateArray())
        {
            projects.Add(new TableauProject
            {
                Id = p.GetProperty("id").GetString(),
                Name = p.GetProperty("name").GetString(),
                Description = p.GetProperty("description").GetString(),
                ParentProjectId = p.TryGetProperty("parentProjectId", out var parent) ? parent.GetString() : null,
                OwnerId = p.GetProperty("owner").GetProperty("id").GetString()
            });
        }

        return projects;
    }

    public async Task<TableauProject> GetByIdAsync(string projectId)
    {
        var response = await _httpClient.GetAsync($"projects/{projectId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var p = doc.RootElement.GetProperty("project");

        return new TableauProject
        {
            Id = p.GetProperty("id").GetString(),
            Name = p.GetProperty("name").GetString(),
            Description = p.GetProperty("description").GetString(),
            ParentProjectId = p.TryGetProperty("parentProjectId", out var parent) ? parent.GetString() : null,
            OwnerId = p.GetProperty("owner").GetProperty("id").GetString()
        };
    }

    public async Task<TableauProject> CreateAsync(ProjectCreateRequest request)
    {
        var payload = new { project = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("projects", jsonContent);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var p = doc.RootElement.GetProperty("project");

        return new TableauProject
        {
            Id = p.GetProperty("id").GetString(),
            Name = p.GetProperty("name").GetString(),
            Description = p.GetProperty("description").GetString(),
            ParentProjectId = p.TryGetProperty("parentProjectId", out var parent) ? parent.GetString() : null,
            OwnerId = p.GetProperty("owner").GetProperty("id").GetString()
        };
    }

    public async Task<TableauProject> UpdateAsync(string projectId, ProjectUpdateRequest request)
    {
        var payload = new { project = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"projects/{projectId}", jsonContent);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var p = doc.RootElement.GetProperty("project");

        return new TableauProject
        {
            Id = p.GetProperty("id").GetString(),
            Name = p.GetProperty("name").GetString(),
            Description = p.GetProperty("description").GetString(),
            ParentProjectId = p.TryGetProperty("parentProjectId", out var parent) ? parent.GetString() : null,
            OwnerId = p.GetProperty("owner").GetProperty("id").GetString()
        };
    }

    public async Task DeleteAsync(string projectId)
    {
        var response = await _httpClient.DeleteAsync($"projects/{projectId}");
        response.EnsureSuccessStatusCode();
    }
}