using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Infrastructure.Services;

/// <summary>
/// One-way hashes for values we need to compare but must not keep in the clear.
///
/// The salt matters here. An IP address hashed without one is trivially reversible — the whole
/// IPv4 space is four billion entries, which is minutes of work. A per-deployment secret salt
/// makes the stored value useless to anyone who takes the database and nothing else.
/// </summary>
public class HashingService : IHashingService
{
    private readonly byte[] _salt;

    public HashingService(IConfiguration config)
    {
        var salt = config["Hashing:Salt"];

        if (string.IsNullOrWhiteSpace(salt) || salt.Length < 32)
            throw new InvalidOperationException(
                "Hashing:Salt must be set to a random value of at least 32 characters. "
                + "Without it, stored IP hashes are reversible by brute force.");

        _salt = Encoding.UTF8.GetBytes(salt);
    }

    public string? HashIdentifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var input = Encoding.UTF8.GetBytes(value.Trim().ToLowerInvariant());
        var buffer = new byte[_salt.Length + input.Length];
        _salt.CopyTo(buffer, 0);
        input.CopyTo(buffer, _salt.Length);

        return Convert.ToHexString(SHA256.HashData(buffer)).ToLowerInvariant();
    }

    /// <summary>
    /// Image hashes are deliberately unsalted: the point is to recognise the same screenshot
    /// uploaded by two different people, which a per-deployment salt would still allow but a
    /// per-value one would not. The hash of an image reveals nothing about its content.
    /// </summary>
    public string HashBytes(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
