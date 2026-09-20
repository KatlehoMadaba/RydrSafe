namespace RydrSafe.Domain.Entities;

public class VerificationHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string Status { get; set; } = "Safe";
    public int RiskScore { get; set; }
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Clause 25.3. SHA-256 of each uploaded image, comma-separated. The images themselves are
    /// never written to disk or to the database — this hash is the only thing that survives, and
    /// it exists so a duplicate upload can be recognised without keeping an image library.
    /// </summary>
    public string? ImageHashes { get; set; }

    /// <summary>
    /// Clause 25.2. When the in-memory image buffers were released. Recorded so the deletion
    /// promise is verifiable rather than merely asserted.
    /// </summary>
    public DateTime? ImagesDiscardedAt { get; set; }

    /// <summary>
    /// Clause 29. When the OCR-derived fields on this row become due for purging by the
    /// retention worker.
    /// </summary>
    public DateTime? OcrDataRetainedUntil { get; set; }

    /// <summary>Set by the retention worker once the OCR-derived fields have been cleared.</summary>
    public DateTime? OcrDataPurgedAt { get; set; }

    public User User { get; set; } = null!;
}
