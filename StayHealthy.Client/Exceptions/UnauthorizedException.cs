namespace StayHealthy.Client.Exceptions;

public class UnauthorizedException : SlotsApiException
{
    public UnauthorizedException(HttpClientException innerException) : base(innerException)
    {
    }
}