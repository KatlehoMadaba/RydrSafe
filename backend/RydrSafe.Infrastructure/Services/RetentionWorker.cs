using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RydrSafe.Application.Common.Interfaces;

namespace RydrSafe.Infrastructure.Services;

/// <summary>
/// COMPLIANCE-NOTES B12. Clause 29 states retention periods; this is what enforces them.
///
/// A stated retention period with no job behind it is worse than saying nothing — it is a
/// representation to users and to the Regulator that is simply untrue. Each pass logs what it
/// purged so the periods are evidenced rather than asserted.
///
/// Periods are configuration, not constants, because clause 29 may be shortened on legal advice
/// without a code change.
/// </summary>
public class RetentionWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    IRetentionPolicy retentionPolicy,
    ILogger<RetentionWorker> logger) : BackgroundService
{
    private const int BatchSize = 500;

    private TimeSpan Interval =>
        TimeSpan.FromHours(config.GetValue("Retention:IntervalHours", 6));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let the app finish starting — migrations run first.
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // A failed purge must not take the worker down — the next pass retries.
                logger.LogError(ex, "Retention pass failed. Will retry in {Interval}.", Interval);
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    internal async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var now = DateTime.UtcNow;

        var verifications = scope.ServiceProvider.GetRequiredService<IVerificationHistoryRepository>();
        var consents = scope.ServiceProvider.GetRequiredService<IConsentRepository>();
        var accessLogs = scope.ServiceProvider.GetRequiredService<IDriverAccessLogRepository>();

        var purgedOcr = await verifications.PurgeExpiredOcrDataAsync(now, BatchSize);
        var purgedIps = await consents.PurgeIpAddressesOlderThanAsync(now.AddMonths(-retentionPolicy.ConsentIpMonths));
        var purgedLogs = await accessLogs.PurgeOlderThanAsync(now.AddDays(-retentionPolicy.AccessLogDays));

        if (purgedOcr + purgedIps + purgedLogs > 0)
        {
            logger.LogInformation(
                "Retention pass complete. OCR rows purged: {Ocr}; consent IPs cleared: {Ips}; access logs removed: {Logs}.",
                purgedOcr, purgedIps, purgedLogs);
        }

        // OCR retention is expressed in months in clause 29; surfaced here so a misconfiguration
        // is visible in the logs rather than silently retaining forever.
        logger.LogDebug(
            "Retention policy in force: verification OCR {OcrMonths}m, consent IP {IpMonths}m, access logs {LogDays}d.",
            retentionPolicy.VerificationOcrMonths, retentionPolicy.ConsentIpMonths, retentionPolicy.AccessLogDays);
    }
}
