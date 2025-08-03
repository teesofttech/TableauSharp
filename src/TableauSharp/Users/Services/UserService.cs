using System.Text;
using System.Text.Json;
using TableauSharp.Users.Models;

namespace TableauSharp.Users.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<TableauUser>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("users");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var users = new List<TableauUser>();
        foreach (var u in doc.RootElement.GetProperty("users").EnumerateArray())
        {
            users.Add(new TableauUser
            {
                Id = u.GetProperty("id").GetString(),
                Name = u.GetProperty("name").GetString(),
                Email = u.GetProperty("email").GetString(),
                SiteRole = u.GetProperty("siteRole").GetString()
            });
        }

        return users;
    }

    public async Task<TableauUser> GetByIdAsync(string userId)
    {
        var response = await _httpClient.GetAsync($"users/{userId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var u = doc.RootElement.GetProperty("user");

        return new TableauUser
        {
            Id = u.GetProperty("id").GetString(),
            Name = u.GetProperty("name").GetString(),
            Email = u.GetProperty("email").GetString(),
            SiteRole = u.GetProperty("siteRole").GetString()
        };
    }

    public async Task<TableauUser> CreateAsync(UserCreateRequest request)
    {
        var payload = new { user = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("users", jsonContent);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var u = doc.RootElement.GetProperty("user");

        return new TableauUser
        {
            Id = u.GetProperty("id").GetString(),
            Name = u.GetProperty("name").GetString(),
            Email = u.GetProperty("email").GetString(),
            SiteRole = u.GetProperty("siteRole").GetString()
        };
    }

    public async Task<TableauUser> UpdateAsync(string userId, UserUpdateRequest request)
    {
        var payload = new { user = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"users/{userId}", jsonContent);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var u = doc.RootElement.GetProperty("user");

        return new TableauUser
        {
            Id = u.GetProperty("id").GetString(),
            Name = u.GetProperty("name").GetString(),
            Email = u.GetProperty("email").GetString(),
            SiteRole = u.GetProperty("siteRole").GetString()
        };
    }

    public async Task DeleteAsync(string userId)
    {
        var response = await _httpClient.DeleteAsync($"users/{userId}");
        response.EnsureSuccessStatusCode();
    }
}