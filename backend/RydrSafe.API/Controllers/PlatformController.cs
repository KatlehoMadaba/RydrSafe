using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Enums;
using RydrSafe.Domain.Services;

namespace RydrSafe.API.Controllers;

/// <summary>
/// What the client needs to render the right form: the agreement version to record consent
/// against, and which report categories are currently accepted.
/// </summary>
[ApiController]
[Route("api/platform")]
public class PlatformController(ICategoryAGate categoryAGate) : ControllerBase
{
    public const string AgreementVersion = "1.0";

    [HttpGet("config")]
    [AllowAnonymous]
    public IActionResult GetConfig()
    {
        var categories = Enum.GetValues<ReportCategory>()
            .Select(c => new
            {
                value = c.ToString(),
                classification = ReportClassificationPolicy.Classify(c).ToString(),
                // The form disables rather than hides these, so a user can see that the
                // category exists and why it is not available (clause 24.4).
                available = ReportClassificationPolicy.Classify(c) == ReportClassification.CategoryB
                            || categoryAGate.IsProcessingEnabled
            })
            .ToList();

        return Ok(new
        {
            agreementVersion = AgreementVersion,
            categoryAProcessingEnabled = categoryAGate.IsProcessingEnabled,
            categoryAPublicationEnabled = categoryAGate.IsPublicationEnabled,
            categoryADisabledReason = categoryAGate.IsProcessingEnabled ? null : categoryAGate.DisabledReason,
            reportCategories = categories,
            requiredConsents = Domain.Entities.ConsentKeys.Required
        });
    }
}
