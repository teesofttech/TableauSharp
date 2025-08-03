namespace TableauSharp.Users.Models;

public class UserCreateRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string SiteRole { get; set; }
}