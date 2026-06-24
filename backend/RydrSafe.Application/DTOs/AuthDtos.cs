namespace RydrSafe.Application.DTOs;

public record RegisterRequest(string FullName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record UserDto(string Id, string FullName, string Email, string Role, string CreatedAt);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string FullName,
    string Email,
    string Role
);
