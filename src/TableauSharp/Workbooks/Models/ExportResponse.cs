namespace TableauSharp.Workbooks.Models;

public class ExportResponse
{
    public byte[] FileContent { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
