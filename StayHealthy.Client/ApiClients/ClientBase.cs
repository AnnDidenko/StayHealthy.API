using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using StayHealthy.Client.Exceptions;

namespace StayHealthy.Client.ApiClients;

public abstract class ClientBase
{
    private readonly HttpClient _httpClient;

    protected ClientBase(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    protected async Task<T> GetAsync<T>(string endpointUrl, string userToken, int withRetries)
        where T : class
    {
        try
        {
            var result = await RunWithRetriesAsync(
                async (c, t) =>
                {
                    using var message = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
                    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", t);
                    return await c.SendAsync(message);
                },
                userToken, withRetries, ParsingTask);

            return (T)result!;
        }
        catch (HttpClientException ex)
        {
            var isUnauthorized = ex is { StatusCode: HttpStatusCode.Unauthorized };

            throw isUnauthorized
                ? new UnauthorizedException(ex)
                : new BadRequestException(ex);
        }
        catch (Exception ex)
        {
            throw new SlotsApiException(HttpStatusCode.InternalServerError, ex, endpointUrl);
        }

        static async Task<object> ParsingTask(HttpResponseMessage response) =>
            await response.Content.ReadAsAsync<T>();
    }

    protected async Task PostAsync<T>(
        string endpointUrl,
        T body,
        string userToken,
        int withRetries)
    {
        try
        {
            await RunWithRetriesAsync(
                async (c, t) =>
                {
                    using var message = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
                    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", t);
                    message.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8,
                        "application/json");
                    return await c.SendAsync(message);
                },
                userToken, withRetries);
        }
        catch (HttpClientException ex)
        {
            var isUnauthorized = ex is { StatusCode: HttpStatusCode.Unauthorized };

            throw isUnauthorized
                ? new UnauthorizedException(ex)
                : new BadRequestException(ex);
        }
        catch (Exception ex)
        {
            throw new SlotsApiException(HttpStatusCode.InternalServerError, ex, endpointUrl);
        }
    }

    private async Task<object?> RunWithRetriesAsync(
        Func<HttpClient, string, Task<HttpResponseMessage>> func,
        string userToken,
        int retries = 0,
        Func<HttpResponseMessage, Task<object>>? parsingAction = null)
    {
        for (var i = 0; i <= retries; i++)
        {
            try
            {
                var response = await func.Invoke(_httpClient, userToken);
                if (response.IsSuccessStatusCode)
                {
                    if (parsingAction is null)
                    {
                        return null;
                    }


                    var result = await parsingAction.Invoke(response);
                    return result;
                }

                var content = await response.Content.ReadAsStringAsync();
                throw new HttpClientException(response.StatusCode,
                    response.RequestMessage.Method.Method,
                    response.RequestMessage.RequestUri,
                    content);
            }
            catch (HttpClientException e)
            {
                if (i == retries)
                {
                    throw;
                }

                await Task.Delay(500);
            }
        }

        throw new Exception("Unexpected flow");
    }
}