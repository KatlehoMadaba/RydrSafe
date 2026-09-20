using Microsoft.Extensions.Configuration;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Infrastructure.Services;

/// <summary>
/// COMPLIANCE-NOTES B5. Reads the Category A standstill from configuration so it can be turned
/// on the day counsel confirms a lawful path, and off again immediately if that changes,
/// without a code deployment.
///
/// Both switches default to <c>false</c>. That is deliberate: a missing or malformed setting
/// must not be the thing that starts processing allegations of criminal conduct.
/// </summary>
public class CategoryAGate(IConfiguration config) : ICategoryAGate
{
    public bool IsProcessingEnabled =>
        config.GetValue("CategoryA:ProcessingEnabled", false);

    /// <summary>
    /// Publication is a narrower permission than processing and can never exceed it, so this is
    /// an AND rather than an independent flag.
    /// </summary>
    public bool IsPublicationEnabled =>
        IsProcessingEnabled && config.GetValue("CategoryA:PublicationEnabled", false);

    public string DisabledReason =>
        config["CategoryA:DisabledReason"]
        ?? "RydrSafe is not currently accepting reports that allege criminal conduct. "
           + "We have applied to the Information Regulator for prior authorisation under POPIA "
           + "section 57(1)(b), and section 58(2) prevents us from processing these reports until "
           + "that application is decided. Reports about vehicle safety and conduct are unaffected. "
           + "If you are reporting a crime, please contact SAPS on 10111.";
}
