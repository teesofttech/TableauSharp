using System.Text;
using System.Text.Json;
using TableauSharp.Common.Http;
using TableauSharp.Projects.Models;

namespace TableauSharp.Projects.Services;

public class ProjectService : IProjectService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauRequestBuilder _requestBuilder;

    public ProjectService(IHttpClientFactory httpClientFactory, ITableauRequestBuilder requestBuilder)
    {
        _httpClientFactory = httpClientFactory;
        _requestBuilder = requestBuilder;
    }

    public async Task<IEnumerable<TableauProject>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, "projects");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var projects = new List<TableauProject>();
        foreach (var p in doc.RootElement.GetProperty("projects").EnumerateArray())
        {
            projects.Add(new TableauProject
            {
                Id = p.GetProperty("id").GetString() ?? string.Empty,
                Name = p.GetProperty("name").GetString() ?? string.Empty,
                Description = p.TryGetProperty("description", out var description) ? description.GetString() : null,
                ParentProjectId = p.TryGetProperty("parentProjectId", out var parent) ? parent.GetString() : null,
                OwnerId = p.GetProperty("owner").GetProperty("id").GetString()
            });
        }

        return projects;
    }

    public async Task<TableauProject> GetByIdAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, $"projects/{projectId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var p = doc.RootElement.GetProperty("project");

        return new TableauProject
        {
            Id = p.GetProperty("id").GetString() ?? string.Empty,
            Name = p.GetProperty("name").GetString() ?? string.Empty,
            Description = p.TryGetProperty("description", out var description) ? description.GetString() : null,
            ParentProjectId = p.TryGetProperty("parentProjectId", out var parent) ? parent.GetString() : null,
            OwnerId = p.GetProperty("owner").GetProperty("id").GetString()
        };
    }

    public async Task<TableauProject> CreateAsync(ProjectCreateRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new { project = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("TableauClient");
        using var httpRequest = _requestBuilder.CreateSiteRequest(HttpMethod.Post, "projects");
        httpRequest.Content = jsonContent;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var p = doc.RootElement.GetProperty("project");

        return new TableauProject
        {
            Id = p.GetProperty("id").GetString() ?? string.Empty,
            Name = p.GetProperty("name").GetString() ?? string.Empty,
            Description = p.TryGetProperty("description", out var description) ? description.GetString() : null,
            ParentProjectId = p.TryGetProperty("parentProjectId", out var parent) ? parent.GetString() : null,
            OwnerId = p.GetProperty("owner").GetProperty("id").GetString()
        };
    }

    public async Task<TableauProject> UpdateAsync(string projectId, ProjectUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new { project = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("TableauClient");
        using var httpRequest = _requestBuilder.CreateSiteRequest(HttpMethod.Put, $"projects/{projectId}");
        httpRequest.Content = jsonContent;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var p = doc.RootElement.GetProperty("project");

        return new TableauProject
        {
            Id = p.GetProperty("id").GetString() ?? string.Empty,
            Name = p.GetProperty("name").GetString() ?? string.Empty,
            Description = p.TryGetProperty("description", out var description) ? description.GetString() : null,
            ParentProjectId = p.TryGetProperty("parentProjectId", out var parent) ? parent.GetString() : null,
            OwnerId = p.GetProperty("owner").GetProperty("id").GetString()
        };
    }

    public async Task DeleteAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Delete, $"projects/{projectId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
