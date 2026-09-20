using System.Net;
using System.Text.Json;
using RydrSafe.Application.Common.Exceptions;

namespace RydrSafe.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        // Scanning being unavailable is not a client error, and the provider's own message
        // is never echoed back. The flag lets the frontend offer manual entry instead of
        // showing a dead end — verification stays possible without signing in.
        if (ex is OcrUnavailableException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Photo scanning is temporarily unavailable. You can enter the registration number manually.",
                ocrUnavailable = true
            }));
        }

        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
            ForbiddenException => (HttpStatusCode.Forbidden, ex.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            RateLimitedException => (HttpStatusCode.TooManyRequests, ex.Message),
            CategoryAProcessingDisabledException => (HttpStatusCode.ServiceUnavailable, ex.Message),
            InvalidStateTransitionException => (HttpStatusCode.Conflict, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var body = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(body);
    }
}
