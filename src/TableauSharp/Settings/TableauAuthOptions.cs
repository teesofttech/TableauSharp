namespace TableauSharp.Settings;

public class TableauAuthOptions
{
    public string SiteContentUrl { get; set; } = string.Empty;
    public string? PersonalAccessTokenName { get; set; }
    public string? PersonalAccessTokenSecret { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UsePAT { get; set; } = true;
}