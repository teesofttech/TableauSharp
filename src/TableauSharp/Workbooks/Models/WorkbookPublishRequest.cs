namespace TableauSharp.Workbooks.Models;

public class WorkbookPublishRequest
{
    public string Name { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool Overwrite { get; set; }
}
