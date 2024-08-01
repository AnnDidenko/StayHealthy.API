namespace StayHealthy.Client.Exceptions;

public class BadRequestException : SlotsApiException
{
    public BadRequestException(HttpClientException innerException) : base(innerException)
    {
    }
}