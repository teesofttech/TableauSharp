using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Models;

namespace TableauSharp.Auth.Service;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly TableauAuthOptions _options;
    private readonly ITableauTokenProvider _tokenProvider;

    public AuthService(HttpClient httpClient, IOptions<TableauAuthOptions> options, ITableauTokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri($"{_options.ServerUrl}/api/3.20/");
    }

    public async Task<AuthToken> SignInWithPATAsync()
    {
        var payload = new
        {
            credentials = new
            {
                personalAccessTokenName = _options.PersonalAccessTokenName,
                personalAccessTokenSecret = _options.PersonalAccessTokenSecret,
                site = new { contentUrl = _options.SiteContentUrl }
            }
        };

        var response = await _httpClient.PostAsync("auth/signin", GetJsonContent(payload));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        _tokenProvider.SetToken(doc.RootElement.GetProperty("credentials").GetProperty("token").GetString()!);
        return new AuthToken
        {
            Token = doc.RootElement.GetProperty("credentials").GetProperty("token").GetString()!,
            SiteId = doc.RootElement.GetProperty("credentials").GetProperty("site").GetProperty("id").GetString()!,
            UserId = doc.RootElement.GetProperty("credentials").GetProperty("user").GetProperty("id").GetString()!,
            Expiration = DateTime.UtcNow.AddHours(2)
        };
    }

    public async Task<AuthToken> SignInWithUserCredentialsAsync()
    {
        var payload = new
        {
            credentials = new
            {
                name = _options.Username,
                password = _options.Password,
                site = new { contentUrl = _options.SiteContentUrl }
            }
        };

        var response = await _httpClient.PostAsync("auth/signin", GetJsonContent(payload));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return new AuthToken
        {
            Token = doc.RootElement.GetProperty("credentials").GetProperty("token").GetString()!,
            SiteId = doc.RootElement.GetProperty("credentials").GetProperty("site").GetProperty("id").GetString()!,
            UserId = doc.RootElement.GetProperty("credentials").GetProperty("user").GetProperty("id").GetString()!,
            Expiration = DateTime.UtcNow.AddHours(2)
        };
    }

    public async Task SignOutAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "auth/signout");
        request.Headers.Add("X-Tableau-Auth", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static StringContent GetJsonContent(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

}