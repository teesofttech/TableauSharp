namespace TableauSharp.Projects.Models;

public class ProjectCreateRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ParentProjectId { get; set; }
}