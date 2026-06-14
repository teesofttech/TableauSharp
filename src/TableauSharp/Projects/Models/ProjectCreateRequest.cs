namespace TableauSharp.Projects.Models;

public class ProjectCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentProjectId { get; set; }
}
