using System.Net;

namespace StayHealthy.Client.Exceptions;

public class HttpClientException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public string HttpMethod { get; }

    public string RequestedUrl { get; }

    public string ResponseContent { get; }
    
    public HttpClientException(
        HttpStatusCode statusCode,
        string httpMethod,
        Uri requestedUrl,
        string responseContent)
        : base(string.Format("HTTP {0:D} ({1:G}) when '{2}' to '{3}'.\nBody:'{4}'", statusCode, statusCode, httpMethod, requestedUrl, responseContent))
    {
        StatusCode = statusCode;
        HttpMethod = httpMethod;
        RequestedUrl = requestedUrl.ToString();
        ResponseContent = responseContent;
    }
}