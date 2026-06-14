using System.Text;
using System.Text.Json;
using TableauSharp.Common.Http;
using TableauSharp.Users.Models;

namespace TableauSharp.Users.Services;

public class UserService : IUserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITableauRequestBuilder _requestBuilder;

    public UserService(IHttpClientFactory httpClientFactory, ITableauRequestBuilder requestBuilder)
    {
        _httpClientFactory = httpClientFactory;
        _requestBuilder = requestBuilder;
    }

    public async Task<IEnumerable<TableauUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, "users");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
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

    public async Task<TableauUser> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Get, $"users/{userId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
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

    public async Task<TableauUser> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new { user = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("TableauClient");
        using var httpRequest = _requestBuilder.CreateSiteRequest(HttpMethod.Post, "users");
        httpRequest.Content = jsonContent;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
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

    public async Task<TableauUser> UpdateAsync(string userId, UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new { user = request };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("TableauClient");
        using var httpRequest = _requestBuilder.CreateSiteRequest(HttpMethod.Put, $"users/{userId}");
        httpRequest.Content = jsonContent;
        var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
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

    public async Task DeleteAsync(string userId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("TableauClient");
        using var request = _requestBuilder.CreateSiteRequest(HttpMethod.Delete, $"users/{userId}");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
