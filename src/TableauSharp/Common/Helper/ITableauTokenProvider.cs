namespace TableauSharp.Common.Helper;

public interface ITableauTokenProvider
{
    /// <summary>
    /// Gets the current Tableau authentication token.
    /// </summary>
    string GetToken();

    /// <summary>
    /// Sets or updates the Tableau authentication token.
    /// </summary>
    void SetToken(string token);

    /// <summary>
    /// Clears the current token (e.g., after sign out).
    /// </summary>
    void ClearToken();
}