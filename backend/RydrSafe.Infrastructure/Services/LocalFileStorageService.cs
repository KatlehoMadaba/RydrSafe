using Microsoft.Extensions.Configuration;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Infrastructure.Services;

public class LocalFileStorageService(IConfiguration config) : IFileStorageService
{
    private readonly string _root = config["FileStorage:Path"]
        ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "evidence", "reports");

    public async Task<string> SaveAsync(Stream stream, string fileName)
    {
        Directory.CreateDirectory(_root);
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var stored = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_root, stored);
        await using var fs = File.Create(fullPath);
        await stream.CopyToAsync(fs);
        return $"/evidence/reports/{stored}";
    }
}
