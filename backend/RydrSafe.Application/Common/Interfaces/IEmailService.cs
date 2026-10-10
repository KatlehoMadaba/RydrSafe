namespace RydrSafe.Application.Common.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends one transactional email. Implementations must not throw on a provider failure —
    /// a password reset request must look identical to the caller whether or not the address
    /// exists, and whether or not the provider was reachable. Failures are logged.
    /// </summary>
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
