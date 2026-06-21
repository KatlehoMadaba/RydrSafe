using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateRefreshToken(string token, string storedHash);
}
