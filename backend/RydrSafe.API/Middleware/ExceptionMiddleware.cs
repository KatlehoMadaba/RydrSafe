using System.Net;
using System.Text.Json;
using FluentValidation;
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
            if (IsClientError(ex))
                logger.LogInformation("Request rejected: {Message}", ex.Message);
            else
                logger.LogError(ex, "Unhandled exception");

            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Failures caused by what the caller sent. They are expected traffic, not incidents — a
    /// mistyped password should not land in the same log stream as an unhandled null reference.
    /// </summary>
    private static bool IsClientError(Exception ex) => ex
        is ValidationException
        or UnauthorizedAccessException
        or ForbiddenException
        or KeyNotFoundException
        or RateLimitedException
        or InvalidStateTransitionException
        or CredentialConfirmationException
        or InvalidOperationException;

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

        // Validation failures carry per-field detail, which the forms show inline. The summary
        // in `error` is there so a caller that ignores `errors` still gets something useful.
        if (ex is ValidationException validation)
        {
            var errors = validation.Errors
                .GroupBy(f => ToCamelCase(f.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = validation.Errors.FirstOrDefault()?.ErrorMessage
                        ?? "Some of the details you entered are not valid.",
                errors
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
            CredentialConfirmationException => (HttpStatusCode.BadRequest, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var body = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(body);
    }

    /// <summary>
    /// FluentValidation reports "DateOfBirth"; the forms bind to "dateOfBirth". Matching the
    /// client's casing is what lets an error attach itself to the right field.
    /// </summary>
    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName) || char.IsLower(propertyName[0]))
            return propertyName;

        // Nested paths arrive as "Consents[0].ConsentKey" — camel-case each segment.
        var segments = propertyName.Split('.');

        for (var i = 0; i < segments.Length; i++)
            if (segments[i].Length > 0 && char.IsUpper(segments[i][0]))
                segments[i] = char.ToLowerInvariant(segments[i][0]) + segments[i][1..];

        return string.Join('.', segments);
    }
}
