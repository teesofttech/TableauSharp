namespace TableauSharp.Settings;

public class TableauOptions
{
    public string Server { get; set; } = string.Empty;
    public string Version { get; set; } = "3.23"; // default fallback
    public string Site { get; set; }
}
