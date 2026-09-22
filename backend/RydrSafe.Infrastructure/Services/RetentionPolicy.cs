using Microsoft.Extensions.Configuration;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Infrastructure.Services;

/// <summary>
/// Clause 29 periods, from configuration. The defaults here are the ones stated in the user
/// agreement; if the agreement is shortened, change the configuration and the worker follows on
/// its next pass.
/// </summary>
public class RetentionPolicy(IConfiguration config) : IRetentionPolicy
{
    public int VerificationOcrMonths => config.GetValue("Retention:VerificationOcrMonths", 12);

    public int ConsentIpMonths => config.GetValue("Retention:ConsentIpMonths", 24);

    public int AccessLogDays => config.GetValue("Retention:AccessLogDays", 90);

    public DateTime VerificationOcrExpiry(DateTime from) => from.AddMonths(VerificationOcrMonths);
}
