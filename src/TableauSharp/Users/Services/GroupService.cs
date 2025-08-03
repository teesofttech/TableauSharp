using System.Text;
using System.Text.Json;
using TableauSharp.Users.Models;

namespace TableauSharp.Users.Services;

public class GroupService : IGroupService
{
    private readonly HttpClient _httpClient;

    public GroupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<TableauGroup>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("groups");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
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

    public async Task<TableauGroup> GetByIdAsync(string groupId)
    {
        var response = await _httpClient.GetAsync($"groups/{groupId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var g = doc.RootElement.GetProperty("group");

        return new TableauGroup
        {
            Id = g.GetProperty("id").GetString(),
            Name = g.GetProperty("name").GetString()
        };
    }

    public async Task<TableauGroup> CreateAsync(GroupCreateRequest request)
    {
        var payload = new { group = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("groups", jsonContent);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var g = doc.RootElement.GetProperty("group");

        return new TableauGroup
        {
            Id = g.GetProperty("id").GetString(),
            Name = g.GetProperty("name").GetString()
        };
    }

    public async Task AddUserToGroupAsync(string groupId, string userId)
    {
        var payload = new { user = new { id = userId } };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"groups/{groupId}/users", jsonContent);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveUserFromGroupAsync(string groupId, string userId)
    {
        var response = await _httpClient.DeleteAsync($"groups/{groupId}/users/{userId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string groupId)
    {
        var response = await _httpClient.DeleteAsync($"groups/{groupId}");
        response.EnsureSuccessStatusCode();
    }
}