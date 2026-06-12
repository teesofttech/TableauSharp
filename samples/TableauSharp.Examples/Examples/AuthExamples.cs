using Microsoft.Extensions.Logging;
using TableauSharp.Auth.Service;

namespace TableauSharp.Examples.Examples;

// Demonstrates the three authentication flows supported by TableauSharp.
// Only one sign-in method should be called per session. After sign-in the
// token is stored automatically in ITableauTokenProvider and injected into
// all other services — you do not need to pass it manually.
public class AuthExamples(IAuthService authService, ILogger<AuthExamples> logger)
{
    public async Task RunAsync()
    {
        Console.WriteLine("--- Auth Examples ---");
        Console.WriteLine();

        await SignInWithPATAsync();
        await SignOutAsync();
    }

    // Personal Access Token is the recommended flow for server-side applications.
    // Configure PersonalAccessTokenName and PersonalAccessTokenSecret in appsettings.json.
    private async Task SignInWithPATAsync()
    {
        Console.WriteLine("[1] Sign in with Personal Access Token");
        try
        {
            var token = await authService.SignInWithPATAsync();
            Console.WriteLine($"    Token    : {token.Token[..8]}...");
            Console.WriteLine($"    SiteId   : {token.SiteId}");
            Console.WriteLine($"    UserId   : {token.UserId}");
            Console.WriteLine($"    Expires  : {token.Expiration:u}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PAT sign-in failed");
        }

        Console.WriteLine();
    }

    // Sign out invalidates the session on the Tableau server.
    // Always sign out when done to avoid leaking server sessions.
    private async Task SignOutAsync()
    {
        Console.WriteLine("[2] Sign out");
        try
        {
            var token = await authService.SignInWithPATAsync();
            await authService.SignOutAsync(token.Token);
            Console.WriteLine("    Signed out successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sign-out failed");
        }

        Console.WriteLine();
    }
}
