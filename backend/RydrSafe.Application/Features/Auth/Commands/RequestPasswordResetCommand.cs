using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Features.Auth.Commands;

public record RequestPasswordResetCommand(string Email, string ResetUrlBase) : IRequest;

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

/// <summary>
/// Issues a reset link, if the address belongs to an account.
/// <para>
/// The handler returns successfully either way and says nothing about whether the address was
/// found. An endpoint that answers "no such account" is an account-enumeration oracle: anyone
/// could test an address list against it and learn who uses RydrSafe. On a platform where
/// users report alleged criminal conduct, that is not a small leak.
/// </para>
/// </summary>
public class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IEmailService emailService,
    IHashingService hashingService) : IRequestHandler<RequestPasswordResetCommand>
{
    /// <summary>
    /// Short on purpose. A reset link is a bearer credential sitting in an inbox; an hour is
    /// long enough to act on and short enough that a stale forwarded email is worthless.
    /// </summary>
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email);

        // No account: stop here, silently. Same response, same timing characteristics as a hit.
        if (user is null) return;

        // 256 bits from a CSPRNG. URL-safe so it survives being pasted out of an email client.
        var token = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        await tokenRepository.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hashingService.HashIdentifier(token)!,
            ExpiresAt = DateTime.UtcNow.Add(Lifetime)
        });
        await tokenRepository.SaveChangesAsync();

        var link = $"{request.ResetUrlBase.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";

        await emailService.SendAsync(
            user.Email,
            "Reset your RydrSafe password",
            BuildEmail(user.FullName, link),
            cancellationToken);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string BuildEmail(string fullName, string link) =>
        $"""
         <p>Hi {System.Net.WebUtility.HtmlEncode(fullName)},</p>
         <p>Someone asked to reset the password for your RydrSafe account. If that was you,
            use the link below. It expires in one hour and can only be used once.</p>
         <p><a href="{link}">Reset your password</a></p>
         <p>If it wasn't you, you can ignore this email — your password has not changed.</p>
         <p>— RydrSafe</p>
         """;
}
