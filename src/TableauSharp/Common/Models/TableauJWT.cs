namespace TableauSharp.Common.Models;

public sealed class TableauJWT
{
    public string Token { get; set; }
    public int ExpiryMinutes { get; set; }
    public string Site { get; set; }
    public string Server { get; set; }
}
