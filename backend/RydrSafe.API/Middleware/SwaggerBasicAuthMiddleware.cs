using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace RydrSafe.API.Middleware;

/// <summary>
/// Puts HTTP Basic auth in front of the Swagger UI so it can be exposed outside development
/// without publishing a map of every moderation and admin route to anyone who looks.
///
/// Basic auth rather than the app's own JWT: Swagger UI is a plain page the browser loads
/// directly, so there is no opportunity to attach a bearer token before it renders. Basic is
/// the one scheme a browser will prompt for on its own. It sends the password base64-encoded,
/// which is not encryption — this is only safe because Render terminates TLS, and the guard
/// refuses to run over plain HTTP below.
///
/// This protects documentation, not data. Every endpoint it lists still enforces its own
/// <c>[Authorize]</c> rules; someone who gets through here can read the schema, not the records.
/// </summary>
public class SwaggerBasicAuthMiddleware(RequestDelegate next, IConfiguration config, ILogger<SwaggerBasicAuthMiddleware> logger)
{
    private const string PathPrefix = "/swagger";
    private const string Realm = "RydrSafe API docs";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(PathPrefix))
        {
            await next(context);
            return;
        }

        var expectedUser = config["Swagger:Username"];
        var expectedPassword = config["Swagger:Password"];

        // Fail closed. A missing or blank credential must not mean "no check" — that is how
        // documentation ends up public by accident after a config change.
        if (string.IsNullOrWhiteSpace(expectedUser) || string.IsNullOrWhiteSpace(expectedPassword))
        {
            logger.LogWarning("Swagger request refused: Swagger:Username/Password are not configured.");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        // Basic sends the password recoverably, so over plain HTTP it is simply a leak. Render
        // forwards the original scheme in X-Forwarded-Proto, which UseForwardedHeaders applies
        // before this runs.
        if (!context.Request.IsHttps)
        {
            logger.LogWarning("Swagger request refused: connection is not HTTPS.");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Swagger is only available over HTTPS.");
            return;
        }

        if (TryReadCredentials(context, out var user, out var password)
            && FixedTimeEquals(user, expectedUser)
            && FixedTimeEquals(password, expectedPassword))
        {
            await next(context);
            return;
        }

        // The prompt the browser shows comes from this header; without it the user just sees
        // a bare 401 page and has no way to supply anything.
        context.Response.Headers.WWWAuthenticate = $"Basic realm=\"{Realm}\", charset=\"UTF-8\"";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private static bool TryReadCredentials(HttpContext context, out string user, out string password)
    {
        user = string.Empty;
        password = string.Empty;

        var header = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header)) return false;

        if (!AuthenticationHeaderValue.TryParse(header, out var parsed)
            || !string.Equals(parsed.Scheme, "Basic", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(parsed.Parameter))
        {
            return false;
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(parsed.Parameter));
        }
        catch (FormatException)
        {
            return false;
        }

        // Split on the first colon only: a password may legitimately contain one.
        var separator = decoded.IndexOf(':');
        if (separator < 0) return false;

        user = decoded[..separator];
        password = decoded[(separator + 1)..];
        return true;
    }

    /// <summary>
    /// Compares in time independent of how many characters match, so the response time cannot
    /// be used to guess the credential one character at a time.
    /// </summary>
    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
