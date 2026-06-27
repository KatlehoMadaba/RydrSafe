using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Infrastructure.Services;

public class OcrService(IConfiguration config, HttpClient httpClient) : IOcrService
{
    private readonly string _apiKey = config["GoogleVision:ApiKey"]
        ?? throw new InvalidOperationException("GoogleVision:ApiKey is not configured.");

    public async Task<OcrResult> ExtractAsync(Stream imageStream)
    {
        var bytes = await ReadAllBytesAsync(imageStream);
        var base64 = Convert.ToBase64String(bytes);

        var requestBody = new
        {
            requests = new[]
            {
                new
                {
                    image = new { content = base64 },
                    features = new[] { new { type = "TEXT_DETECTION", maxResults = 1 } }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(
            $"https://vision.googleapis.com/v1/images:annotate?key={_apiKey}", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Google Vision API error {(int)response.StatusCode}: {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(responseJson);

        var responses = doc.RootElement.GetProperty("responses")[0];

        if (!responses.TryGetProperty("fullTextAnnotation", out var annotation))
            return new OcrResult(null, null, null, null, null);

        var rawText = annotation.GetProperty("text").GetString() ?? string.Empty;
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
        var match = Regex.Match(text, @"\b([A-Z]{2,3}\s?\d{1,4}\s?[A-Z]{2,4})\b");
        return match.Success ? Regex.Replace(match.Value, @"\s", "").ToUpper() : null;
    }

    private static string? ExtractPhoneNumber(string text)
    {
        var match = Regex.Match(text, @"\b(0[6-8]\d[\s\-]?\d{3}[\s\-]?\d{4})\b");
        return match.Success ? Regex.Replace(match.Value, @"[\s\-]", "") : null;
    }

    private static readonly string[] LocationNoise =
    [
        "road", "street", "avenue", "community", "church", "complex",
        "centre", "center", "park", "drive", "place", "square", "mall",
        "metro", "station", "meet", "pick", "spot", "route", "arrive",
        "trip", "fastest", "details", "message", "send"
    ];

    private static string? ExtractDriverName(string[] lines)
    {
        // Explicit label: "Driver: John" or "Name: John"
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

        // Uber/Bolt format: "Freddie • Silver Nissan Almera"
        foreach (var line in lines)
        {
            var bulletIndex = line.IndexOfAny(['•', '·', '|', '—']);
            if (bulletIndex > 1)
            {
                var candidate = line[..bulletIndex].Trim();
                var wordCount = candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                if (wordCount <= 3 && candidate.Length > 1 && Regex.IsMatch(candidate, @"^[A-Z][a-zA-Z\s]+$"))
                    return candidate;
            }
        }

        // Vehicle make anchor: name appears before a known make on the same line
        var knownMakes = new[] { "Toyota", "Honda", "Ford", "BMW", "Mercedes", "Volkswagen", "Hyundai", "Kia", "Nissan", "Suzuki" };
        foreach (var line in lines)
        {
            foreach (var make in knownMakes)
            {
                var makeIdx = line.IndexOf(make, StringComparison.OrdinalIgnoreCase);
                if (makeIdx > 2)
                {
                    var candidate = line[..makeIdx].Trim().TrimEnd('•', '·', '|', '—', '.', ' ');
                    var wordCount = candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    if (wordCount <= 3 && candidate.Length > 1 && Regex.IsMatch(candidate, @"^[A-Z][a-zA-Z\s]+$"))
                        return candidate;
                }
            }
        }

        // Last resort: single or two-word title-case line, skip noise
        foreach (var line in lines)
        {
            var lower = line.ToLower();
            if (LocationNoise.Any(w => lower.Contains(w))) continue;
            if (line.Contains('@') || line.Contains('.')) continue;
            if (Regex.IsMatch(line, @"^[A-Z][a-z]+(?: [A-Z][a-z]+)?$"))
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
