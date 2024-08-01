using System.Net;
using FluentValidation;
using StayHealthy.Application.Exceptions;
using StayHealthy.Client.Exceptions;

namespace StayHealthy.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, exception.Message);

        ExceptionResponse response = exception switch
        {
            BadRequestException _ => new ExceptionResponse(HttpStatusCode.BadRequest, "Bad request. Please check your request."),
            UnauthorizedException _ => new ExceptionResponse(HttpStatusCode.Unauthorized, "Unauthorized. Please check your credentials."),
            SlotsApiException slotsApiException => new ExceptionResponse(slotsApiException.StatusCode, slotsApiException.Error),
            HttpClientException httpClientException => new ExceptionResponse(httpClientException.StatusCode, httpClientException.Message),
            TimeSlotConflictException timeSlotConflictException => new ExceptionResponse(HttpStatusCode.Conflict, timeSlotConflictException.Message),
            ValidationException validationException => new ExceptionResponse(HttpStatusCode.BadRequest, $"{validationException.Message} Please select another time slot."),
            _ => new ExceptionResponse(HttpStatusCode.InternalServerError, "Internal server error. Please retry later.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }

    private class ExceptionResponse
    {
        public ExceptionResponse(HttpStatusCode statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }

        public HttpStatusCode StatusCode { get; }
        public string Message { get; }
    }
}