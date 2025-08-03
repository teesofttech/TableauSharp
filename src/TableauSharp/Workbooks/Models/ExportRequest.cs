namespace TableauSharp.Workbooks.Models;

public class ExportRequest
{
    public string WorkbookId { get; set; }
    public string ViewId { get; set; }
    public string Format { get; set; } // "PDF", "PNG", "CSV"
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
}