namespace TableauSharp.Users.Models;

public class UserUpdateRequest
{
    public string SiteRole { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
