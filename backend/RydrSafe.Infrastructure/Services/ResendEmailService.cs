using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Infrastructure.Services;

/// <summary>
/// Transactional email via Resend (https://resend.com).
/// </summary>
public class ResendEmailService(
    HttpClient httpClient,
    IConfiguration config,
    ILogger<ResendEmailService> logger) : IEmailService
{
    private readonly string? _apiKey = config["Resend:ApiKey"];

    /// <summary>
    /// Must be an address on a domain verified in Resend. Resend rejects anything else, and
    /// the rejection arrives as a 4xx on send rather than at startup.
    /// </summary>
    private readonly string _from = config["Resend:FromAddress"] ?? "RydrSafe <noreply@rydrsafe.co.za>";

    public async Task SendAsync(
        string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        // Unconfigured is a deployment state, not a user error. Log loudly and return: the
        // caller is a password reset, which must behave identically whether or not mail works.
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            logger.LogError(
                "Resend:ApiKey is not configured — no email sent to {Recipient}. Password reset "
                + "links cannot be delivered until it is set.", Redact(toEmail));
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = JsonContent.Create(new
                {
                    from = _from,
                    to = new[] { toEmail },
                    subject,
                    html = htmlBody
                })
            };
            request.Headers.Authorization = new("Bearer", _apiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Resend rejected the email to {Recipient}: {Status} {Body}",
                    Redact(toEmail), (int)response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            // Swallowed deliberately. Surfacing this would turn a mail outage into a 500 on
            // the reset endpoint, which also tells an attacker the address existed.
            logger.LogError(ex, "Failed to send email to {Recipient}.", Redact(toEmail));
        }
    }

    /// <summary>Logs enough to trace a delivery problem without writing addresses to the log.</summary>
    private static string Redact(string email)
    {
        var at = email.IndexOf('@');
        return at <= 1 ? "***" : $"{email[0]}***{email[at..]}";
    }
}
