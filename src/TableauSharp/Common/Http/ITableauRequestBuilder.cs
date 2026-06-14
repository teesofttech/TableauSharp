namespace TableauSharp.Common.Http;

public interface ITableauRequestBuilder
{
    HttpRequestMessage CreateSiteRequest(HttpMethod method, string relativePath);
    HttpRequestMessage CreateServerRequest(HttpMethod method, string relativePath);
}
