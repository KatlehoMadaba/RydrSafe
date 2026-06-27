using MediatR;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;

namespace RydrSafe.Application.Features.Auth.Queries;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;

public class GetCurrentUserQueryHandler(IUserRepository userRepository) : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        return new UserDto(user.Id.ToString(), user.FullName, user.Email, user.Role.ToString().ToLower(), user.CreatedAt.ToString("o"));
    }
}
