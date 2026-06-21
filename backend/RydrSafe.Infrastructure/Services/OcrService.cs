using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using RydrSafe.Application.Common.Interfaces;
using Tesseract;

namespace RydrSafe.Infrastructure.Services;

public class OcrService(IConfiguration config) : IOcrService
{
    private readonly string _tessDataPath = config["Tesseract:DataPath"] ?? "./tessdata";

    public async Task<OcrResult> ExtractAsync(Stream imageStream)
    {
        var bytes = await ReadAllBytesAsync(imageStream);
        string rawText;

        using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
        using var pix = Pix.LoadFromMemory(bytes);
        using var page = engine.Process(pix);
        rawText = page.GetText();

        return ParseText(rawText);
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    private static OcrResult ParseText(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var registrationNumber = ExtractRegistrationNumber(text);
        var phoneNumber = ExtractPhoneNumber(text);
        var driverName = ExtractDriverName(lines);
        var (make, model) = ExtractVehicle(lines);

        return new OcrResult(driverName, registrationNumber, phoneNumber, make, model);
    }

    private static string? ExtractRegistrationNumber(string text)
    {
        // SA number plate patterns: ABC 123 GP, ABC123GP, etc.
        var match = Regex.Match(text, @"\b([A-Z]{2,3}\s?\d{1,3}\s?[A-Z]{2,3})\b");
        return match.Success ? Regex.Replace(match.Value, @"\s", "").ToUpper() : null;
    }

    private static string? ExtractPhoneNumber(string text)
    {
        // SA mobile numbers: 0XX XXX XXXX
        var match = Regex.Match(text, @"\b(0[6-8]\d[\s\-]?\d{3}[\s\-]?\d{4})\b");
        return match.Success ? Regex.Replace(match.Value, @"[\s\-]", "") : null;
    }

    private static string? ExtractDriverName(string[] lines)
    {
        foreach (var line in lines)
        {
            var lower = line.ToLower();
            if (lower.StartsWith("driver") || lower.StartsWith("name") || lower.StartsWith("captain"))
            {
                var parts = line.Split(':', 2);
                if (parts.Length == 2 && parts[1].Trim().Length > 2)
                    return parts[1].Trim();
            }
        }

        // Fallback: first title-case line that looks like a name
        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, @"^[A-Z][a-z]+ [A-Z][a-z]+"))
                return line.Trim();
        }

        return null;
    }

    private static (string? make, string? model) ExtractVehicle(string[] lines)
    {
        var knownMakes = new[] { "Toyota", "Honda", "Ford", "BMW", "Mercedes", "Volkswagen", "Hyundai", "Kia", "Nissan", "Suzuki" };

        foreach (var line in lines)
        {
            foreach (var make in knownMakes)
            {
                if (line.Contains(make, StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var makeIndex = Array.FindIndex(parts, p => p.Equals(make, StringComparison.OrdinalIgnoreCase));
                    var model = makeIndex + 1 < parts.Length ? parts[makeIndex + 1] : null;
                    return (make, model);
                }
            }
        }

        return (null, null);
    }
}
