namespace TableauSharp.Workbooks.Models;

public class WorkbookPublishRequest
{
    public string Name { get; set; }
    public string ProjectId { get; set; }
    public string FilePath { get; set; }
    public bool Overwrite { get; set; }
}