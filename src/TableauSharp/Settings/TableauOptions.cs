namespace TableauSharp.Settings;

public class TableauOptions
{
    public string Url { get; set; } = string.Empty;
    public string Version { get; set; } = "3.21"; // default fallback
}
