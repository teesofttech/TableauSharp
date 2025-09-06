namespace TableauSharp.Settings;

public class TableauAuthOptions
{
    public string SiteContentUrl { get; set; } = string.Empty;
    public string? PersonalAccessTokenName { get; set; }
    public string? PersonalAccessTokenSecret { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UsePAT { get; set; } = true;
    public string? SecretValue { get; set; }
    public string? SecretId { get; set; }
    public int Jwt_Expiry_Minutes { get; set; }
    public string Jwt_Audience { get; set; } = "tableau";
    public string Scopes { get; set; } = "tableau:views:read tableau:workbooks:read";
}