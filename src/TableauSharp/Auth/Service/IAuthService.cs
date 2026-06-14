using TableauSharp.Common.Models;

namespace TableauSharp.Auth.Service;

public interface IAuthService
{
    Task<AuthToken> SignInWithPATAsync(CancellationToken cancellationToken = default);
    Task<AuthToken> SignInWithUserCredentialsAsync(CancellationToken cancellationToken = default);
    Task SignOutAsync(string token, CancellationToken cancellationToken = default);
    Task<AuthToken> SignInWithJWTAsync(string username, CancellationToken cancellationToken = default);
}
