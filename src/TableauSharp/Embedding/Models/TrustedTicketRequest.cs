namespace TableauSharp.Embedding.Models;

public class TrustedTicketRequest
{
    public string Username { get; set; }
    public string ClientIp { get; set; }
    public string TargetSite { get; set; }
}