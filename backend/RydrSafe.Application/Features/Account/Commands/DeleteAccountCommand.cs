using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Exceptions;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Account.Commands;

/// <summary>
/// Clause 29 and POPIA s24 — the account holder deleting their own account, immediately and
/// without asking anyone.
///
/// Deletion is real and irreversible: the user row goes, and with it the consents, notifications,
/// followed drivers and verification history that hang off it. Reports are the exception. They
/// describe other people, and clause 29.2 keeps them as an unlinked historical record so a driver
/// disputing one still has something to dispute.
/// </summary>
public record DeleteAccountCommand(Guid UserId, string Password, string? Reason)
    : IRequest<DeleteAccountResponse>;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.Password).NotEmpty()
            .WithMessage("Enter your password to confirm you want to delete your account.");

        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public class DeleteAccountCommandHandler(
    IUserRepository userRepository,
    IReportRepository reportRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<DeleteAccountCommand, DeleteAccountResponse>
{
    public async Task<DeleteAccountResponse> Handle(
        DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        // 400, not 401: a mistyped confirmation password must not sign the user out mid-dialog.
        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new CredentialConfirmationException(
                "That password is incorrect. Your account has not been deleted.");

        var email = user.Email;

        // Order matters. The reporter FK is Restrict, so the reports have to be unlinked before
        // the user row will delete at all — which is the point of using Restrict rather than a
        // cascade that would take the reports with it.
        var deIdentified = await reportRepository.DeIdentifyByUserAsync(user.Id);

        await userRepository.DeleteAsync(user.Id);

        return new DeleteAccountResponse(
            email,
            deIdentified,
            DateTime.UtcNow,
            deIdentified == 0
                ? "Your account has been deleted. You can sign up again with this email address at any time."
                : $"Your account has been deleted. {deIdentified} report(s) you submitted have been kept "
                  + "but are no longer linked to you, because they concern other people (clause 29.2). "
                  + "You can sign up again with this email address at any time.");
    }
}
