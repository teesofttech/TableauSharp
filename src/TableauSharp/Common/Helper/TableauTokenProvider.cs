using TableauSharp.Common.Models;

namespace TableauSharp.Common.Helper;

public class TableauTokenProvider : ITableauTokenProvider
{
    private readonly object _gate = new();
    private AuthToken? _token;

    public string GetToken()
    {
        return GetTokenInfo().Token;
    }

    public AuthToken GetTokenInfo()
    {
        lock (_gate)
        {
            if (_token is null || string.IsNullOrWhiteSpace(_token.Token))
            {
                throw new InvalidOperationException("No Tableau auth token set. Please sign in first.");
            }

            return _token;
        }
    }

    public void SetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        SetToken(new AuthToken { Token = token });
    }

    public void SetToken(AuthToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (string.IsNullOrWhiteSpace(token.Token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        lock (_gate)
        {
            _token = token;
        }
    }

    public void ClearToken()
    {
        lock (_gate)
        {
            _token = null;
        }
    }
}
