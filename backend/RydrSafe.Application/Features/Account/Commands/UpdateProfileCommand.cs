using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Account.Commands;

/// <summary>
/// Clause 4 and POPIA s24 — the account holder correcting what we hold about them. Only the
/// fields a user is entitled to change themselves: the role is not one of them.
/// </summary>
public record UpdateProfileCommand(Guid UserId, string FullName, string Email) : IRequest<UserDto>;

/// <summary>
/// Registered, but nothing runs it yet — there is no MediatR validation behavior in the pipeline
/// (issue #39). Until that lands, <see cref="UpdateProfileCommandHandler"/> enforces the same
/// rules itself. Keep the two in step.
/// </summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class UpdateProfileCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateProfileCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // Duplicated from the validator on purpose; see the note there.
        var fullName = request.FullName?.Trim() ?? string.Empty;

        if (fullName.Length < 2 || fullName.Length > 255)
            throw new InvalidOperationException("Your full name must be between 2 and 255 characters.");

        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (!new EmailAddressAttribute().IsValid(email))
            throw new InvalidOperationException("Enter a valid email address.");

        var user = await userRepository.GetByIdAsync(request.UserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        // The unique index would catch this anyway, but as a 500 rather than something the form
        // can show next to the field.
        if (email != user.Email && await userRepository.EmailTakenAsync(email, user.Id))
            throw new InvalidOperationException("That email address is already registered to another account.");

        user.FullName = fullName;
        user.Email = email;
        await userRepository.UpdateAsync(user);

        return new UserDto(
            user.Id.ToString(), user.FullName, user.Email,
            user.Role.ToString().ToLower(), user.CreatedAt.ToString("o"));
    }
}
