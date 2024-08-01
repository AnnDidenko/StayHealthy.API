using System.Net;

namespace StayHealthy.Client.Exceptions;

public class SlotsApiException : ApplicationException
{
    private const string ExceptionMessage = "Error sending a request to Slots API. See inner Exception for details.";
  
    public string Uri { get; private set; }
    public HttpStatusCode StatusCode { get; private set; }
    public string Error { get; set; }
    
    public SlotsApiException(HttpClientException innerException) 
        : base(ExceptionMessage, innerException)
    {
        Uri = innerException.RequestedUrl;
        StatusCode = innerException.StatusCode;
        Error = innerException.ResponseContent;
    }
    
    public SlotsApiException(HttpStatusCode statusCode, Exception innerException, string uri) 
        : base(ExceptionMessage, innerException)
    {
        StatusCode = statusCode;
        Uri = uri;
        Error = innerException.Message;
    }
}