namespace TableauSharp.Common.Helper;

public class TableauTokenProvider : ITableauTokenProvider
{
    private string _token;

    public string GetToken()
    {
        if (string.IsNullOrWhiteSpace(_token))
            throw new InvalidOperationException("No Tableau auth token set. Please sign in first.");

        return _token;
    }

    public void SetToken(string token)
    {
        _token = token ?? throw new ArgumentNullException(nameof(token));
    }

    public void ClearToken()
    {
        _token = null;
    }
}