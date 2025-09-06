using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TableauSharp.Common.Helper;
using TableauSharp.Common.Models;
using TableauSharp.Settings;

namespace TableauSharp.Auth.Service;

public class AuthService(IHttpClientFactory httpClientFactory,
    IOptions<TableauAuthOptions> options,
    IOptions<TableauOptions> tableauOptions,
    ITableauTokenProvider tokenProvider) : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly TableauAuthOptions _options = options.Value;
    private readonly ITableauTokenProvider _tokenProvider = tokenProvider;
    private readonly TableauOptions _tableauOptions = tableauOptions.Value;

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
            Expiration = DateTime.UtcNow.AddMinutes(_options.Jwt_Expiry_Minutes)
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

    public async Task<AuthToken> SignInWithJWTAsync(string username, CancellationToken cancellationToken)
    {
        var payload = new
        {
            credentials = new
            {
                jwt = CreateJwtToken(username),
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

    private string CreateJwt(string username, string[] scopes)
    {
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Audience = _options.Jwt_Audience,
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("sub", username),
                new System.Security.Claims.Claim("jti", Guid.NewGuid().ToString()),
                new System.Security.Claims.Claim("scopes", string.Join(" ", scopes))
            }),
            Expires = DateTime.UtcNow.AddMinutes(_options.Jwt_Expiry_Minutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretValue)) { KeyId = _options.SecretId }, SecurityAlgorithms.HmacSha256Signature)
        };

        JsonWebTokenHandler jsonWebTokenHandler = new();
        return jsonWebTokenHandler.CreateToken(tokenDescriptor);
    }

    private TableauJWT CreateJwtToken(string username)
    {
        string[] scopes = _options.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new TableauJWT
        {
            ExpiryMinutes = _options.Jwt_Expiry_Minutes,
            Server = _tableauOptions.Url,
            Site = _options.SiteContentUrl,
            Token = CreateJwt(username, scopes)
        };
    }
}