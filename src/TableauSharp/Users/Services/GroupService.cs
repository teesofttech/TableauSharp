using System.Text;
using System.Text.Json;
using TableauSharp.Common.Http;
using TableauSharp.Users.Models;

namespace TableauSharp.Users.Services;

public class GroupService : IGroupService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauRequestBuilder _requestBuilder;

    public GroupService(IHttpClientFactory httpClientFactory, ITableauRequestBuilder requestBuilder)
    {
        _httpClientFactory = httpClientFactory;
        _requestBuilder = requestBuilder;
    }

    public async Task<IEnumerable<TableauGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, "groups");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var groups = new List<TableauGroup>();
        foreach (var g in doc.RootElement.GetProperty("groups").EnumerateArray())
        {
            groups.Add(new TableauGroup
            {
                Id = g.GetProperty("id").GetString(),
                Name = g.GetProperty("name").GetString()
            });
        }

        return groups;
    }

    public async Task<TableauGroup> GetByIdAsync(string groupId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, $"groups/{groupId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var g = doc.RootElement.GetProperty("group");

        return new TableauGroup
        {
            Id = g.GetProperty("id").GetString(),
            Name = g.GetProperty("name").GetString()
        };
    }

    public async Task<TableauGroup> CreateAsync(GroupCreateRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new { group = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("TableauClient");
        using var httpRequest = _requestBuilder.CreateSiteRequest(HttpMethod.Post, "groups");
        httpRequest.Content = jsonContent;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var g = doc.RootElement.GetProperty("group");

        return new TableauGroup
        {
            Id = g.GetProperty("id").GetString(),
            Name = g.GetProperty("name").GetString()
        };
    }

    public async Task AddUserToGroupAsync(string groupId, string userId, CancellationToken cancellationToken = default)
    {
        var payload = new { user = new { id = userId } };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Post, $"groups/{groupId}/users");
        request.Content = jsonContent;
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveUserFromGroupAsync(string groupId, string userId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Delete, $"groups/{groupId}/users/{userId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string groupId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Delete, $"groups/{groupId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
