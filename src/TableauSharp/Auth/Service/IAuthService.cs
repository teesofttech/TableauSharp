using TableauSharp.Common.Models;

namespace TableauSharp.Auth.Service;

public interface IAuthService
{
    Task<AuthToken> SignInWithPATAsync();
    Task<AuthToken> SignInWithUserCredentialsAsync();
    Task SignOutAsync(string token);
}