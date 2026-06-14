namespace TableauSharp.Embedding.Models;

public class TrustedTicketRequest
{
    public string Username { get; set; } = string.Empty;
    public string? ClientIp { get; set; }
    public string? TargetSite { get; set; }
}
