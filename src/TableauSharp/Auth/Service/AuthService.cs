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

    public async Task<AuthToken> SignInWithPATAsync(CancellationToken cancellationToken = default)
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
        var response = await client.PostAsync("auth/signin", GetJsonContent(payload), cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var authToken = ParseAuthToken(json);
        _tokenProvider.SetToken(authToken);
        return authToken;
    }

    public async Task<AuthToken> SignInWithUserCredentialsAsync(CancellationToken cancellationToken = default)
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
        var response = await client.PostAsync("auth/signin", GetJsonContent(payload), cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var authToken = ParseAuthToken(json);
        _tokenProvider.SetToken(authToken);
        return authToken;
    }

    public async Task SignOutAsync(string token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "auth/signout");
        request.Headers.Add("X-Tableau-Auth", token);
        var client = _httpClientFactory.CreateClient("TableauClient");
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        _tokenProvider.ClearToken();
    }

    private static StringContent GetJsonContent(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public async Task<AuthToken> SignInWithJWTAsync(string username, CancellationToken cancellationToken = default)
    {
        string[] scopes = _options.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var payload = new
        {
            credentials = new
            {
                jwt = CreateJwt(username, scopes),
                site = new { contentUrl = GetSiteContentUrl() }
            }
        };
        var client = _httpClientFactory.CreateClient("TableauClient");
        var response = await client.PostAsync("auth/signin", GetJsonContent(payload), cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var authToken = ParseAuthToken(json);
        _tokenProvider.SetToken(authToken);
        return authToken;
    }

    private AuthToken ParseAuthToken(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var credentials = doc.RootElement.GetProperty("credentials");
        var site = credentials.GetProperty("site");
        var user = credentials.GetProperty("user");

        return new AuthToken
        {
            Token = credentials.GetProperty("token").GetString() ?? string.Empty,
            SiteId = site.GetProperty("id").GetString() ?? string.Empty,
            SiteContentUrl = site.TryGetProperty("contentUrl", out var contentUrl)
                ? contentUrl.GetString()
                : GetSiteContentUrl(),
            UserId = user.GetProperty("id").GetString() ?? string.Empty,
            Expiration = GetExpiration(credentials)
        };
    }

    private string CreateJwt(string username, string[] scopes)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretValue))
        {
            throw new InvalidOperationException("TableauAuthOptions.SecretValue must be configured for JWT sign-in.");
        }

        if (string.IsNullOrWhiteSpace(_options.SecretId))
        {
            throw new InvalidOperationException("TableauAuthOptions.SecretId must be configured for JWT sign-in.");
        }

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Audience = string.IsNullOrWhiteSpace(_options.Jwt_Audience)
                ? _tableauOptions.Server
                : _options.Jwt_Audience,
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

    private DateTime GetExpiration(JsonElement credentials)
    {
        if (credentials.TryGetProperty("estimatedTimeToExpiration", out var expirationElement))
        {
            var value = expirationElement.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var parts = value.Split(':');
                if (parts.Length == 3
                    && int.TryParse(parts[0], out var days)
                    && int.TryParse(parts[1], out var hours)
                    && int.TryParse(parts[2], out var minutes))
                {
                    return DateTime.UtcNow.Add(new TimeSpan(days, hours, minutes, 0));
                }
            }
        }

        var fallbackMinutes = _options.Jwt_Expiry_Minutes > 0 ? _options.Jwt_Expiry_Minutes : 120;
        return DateTime.UtcNow.AddMinutes(fallbackMinutes);
    }

    private string GetSiteContentUrl()
        => !string.IsNullOrWhiteSpace(_options.SiteContentUrl)
            ? _options.SiteContentUrl
            : _tableauOptions.Site;
}
