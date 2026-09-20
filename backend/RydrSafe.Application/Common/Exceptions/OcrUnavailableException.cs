namespace RydrSafe.Application.Common.Exceptions;

/// <summary>
/// The OCR provider could not be reached or has refused us — quota exhausted, billing
/// disabled, rate limited. Distinct from an image we simply could not read: this says
/// scanning is unavailable right now, not that the photo was bad.
/// <para>
/// Verification is deliberately anonymous, so quota can run out through ordinary use.
/// When it does, callers should fall back to manual registration entry rather than
/// telling the passenger their request was invalid.
/// </para>
/// </summary>
public class OcrUnavailableException(string message) : Exception(message);
