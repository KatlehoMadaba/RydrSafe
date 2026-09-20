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
        var user = await userRepository.GetByIdAsync(request.UserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        var email = request.Email.Trim().ToLowerInvariant();

        // The unique index would catch this anyway, but as a 500 rather than something the form
        // can show next to the field.
        if (email != user.Email && await userRepository.EmailTakenAsync(email, user.Id))
            throw new InvalidOperationException("That email address is already registered to another account.");

        user.FullName = request.FullName.Trim();
        user.Email = email;
        await userRepository.UpdateAsync(user);

        return new UserDto(
            user.Id.ToString(), user.FullName, user.Email,
            user.Role.ToString().ToLower(), user.CreatedAt.ToString("o"));
    }
}
