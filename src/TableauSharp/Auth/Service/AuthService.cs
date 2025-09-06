using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Models;
using TableauSharp.Settings;

namespace TableauSharp.Auth.Service;

public class AuthService(IHttpClientFactory httpClientFactory,
    IOptions<TableauAuthOptions> options,
    ITableauTokenProvider tokenProvider) : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly TableauAuthOptions _options = options.Value;
    private readonly ITableauTokenProvider _tokenProvider = tokenProvider;

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
        var client = _httpClientFactory.CreateClient("TableauClient");
        var response = await client.PostAsync("auth/signin", GetJsonContent(payload));
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
        var client = _httpClientFactory.CreateClient("TableauClient");
        var response = await client.PostAsync("auth/signin", GetJsonContent(payload));
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
        var client = _httpClientFactory.CreateClient("TableauClient");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static StringContent GetJsonContent(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

}