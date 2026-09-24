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

    /// <summary>
    /// Falls back on blank as well as null: an empty string in configuration is how this
    /// setting looks when someone copies the sample file and leaves the value alone, and
    /// returning "" would show the user an explanation-shaped hole.
    /// </summary>
    public string DisabledReason
    {
        get
        {
            var configured = config["CategoryA:DisabledReason"];
            return string.IsNullOrWhiteSpace(configured) ? DefaultDisabledReason : configured;
        }
    }

    /// <summary>
    /// Written for the person staring at a greyed-out dropdown, not for a lawyer.
    ///
    /// The previous wording claimed "we have applied to the Information Regulator". That
    /// application has not been made — COMPLIANCE-NOTES section 3 still has it unchecked — so the
    /// notice was telling users something untrue about the platform's legal position. Saying
    /// plainly that permission has not been obtained is both accurate and the stronger signal.
    /// </summary>
    private const string DefaultDisabledReason =
        "Some report types are switched off at the moment. Anything that accuses a driver of a "
           + "crime — assault, theft, fraud, harassment, reckless or drunk driving — counts as "
           + "special personal information under POPIA, and we need written permission from the "
           + "Information Regulator before we are allowed to collect it. We do not have that "
           + "permission yet, so we will not take those reports, even to store them. "
           + "You can still report an unsafe vehicle. "
           + "If a crime has been committed, please report it to SAPS on 10111 — they can act on "
           + "it and we cannot.";
}
