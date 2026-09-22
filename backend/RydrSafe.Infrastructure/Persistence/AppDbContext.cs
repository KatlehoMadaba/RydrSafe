using Microsoft.EntityFrameworkCore;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;

namespace RydrSafe.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<VerificationHistory> VerificationHistories => Set<VerificationHistory>();
    public DbSet<DriverFollow> DriverFollows => Set<DriverFollow>();
    public DbSet<ReportStatusAudit> ReportStatusAudits => Set<ReportStatusAudit>();
    public DbSet<DriverStatusAudit> DriverStatusAudits => Set<DriverStatusAudit>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<DriverAppeal> DriverAppeals => Set<DriverAppeal>();
    public DbSet<DriverRecordAccessLog> DriverRecordAccessLogs => Set<DriverRecordAccessLog>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.FullName).HasMaxLength(255).IsRequired();
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
            e.Property(u => u.AcceptedAgreementVersion).HasMaxLength(20);
            e.Property(u => u.ClosureReason).HasMaxLength(500);
        });

        modelBuilder.Entity<Driver>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.DriverName).HasMaxLength(255).IsRequired();
            e.Property(d => d.PhoneNumber).HasMaxLength(50);
            e.Property(d => d.Status).HasConversion<string>();
            e.Property(d => d.RightOfReplyResponse).HasMaxLength(4000);

            // GetByPhoneNumberAsync filters on this during every verification.
            e.HasIndex(d => d.PhoneNumber);
            // GetAllAsync orders by it.
            e.HasIndex(d => d.RiskScore);

            e.HasMany(d => d.Vehicles).WithOne(v => v.Driver).HasForeignKey(v => v.DriverId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(d => d.Reports).WithOne(r => r.Driver).HasForeignKey(r => r.DriverId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(d => d.Appeals).WithOne(a => a.Driver).HasForeignKey(a => a.DriverId).OnDelete(DeleteBehavior.Cascade);

            // Computed from other columns — never stored.
            e.Ignore(d => d.PublicStatus);
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.RegistrationNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(v => v.RegistrationNumber).IsUnique();
            e.Property(v => v.Make).HasMaxLength(100);
            e.Property(v => v.Model).HasMaxLength(100);
            e.Property(v => v.Color).HasMaxLength(50);
        });

        modelBuilder.Entity<Report>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Category).HasConversion<string>();
            e.Property(r => r.Severity).HasConversion<string>();
            e.Property(r => r.Status).HasConversion<string>();
            e.Property(r => r.Classification).HasConversion<string>();
            e.Property(r => r.CorroborationPath).HasConversion<string>();
            e.Property(r => r.IncidentDatePrecision).HasConversion<string>();
            e.Property(r => r.Description).IsRequired();
            e.Property(r => r.OfficialReference).HasMaxLength(100);
            e.Property(r => r.PublicRecordSourceType).HasMaxLength(50);
            e.Property(r => r.PublicRecordSourceUrl).HasMaxLength(2000);
            e.Property(r => r.PublicRecordSourceReference).HasMaxLength(500);
            e.Property(r => r.CorroborationRevokedReason).HasMaxLength(1000);

            // 64 hex chars of SHA-256.
            e.Property(r => r.SubmissionIpHash).HasMaxLength(64);
            e.Property(r => r.SubmissionDeviceHash).HasMaxLength(64);
            e.Property(r => r.ReporterKeyHash).HasMaxLength(64);

            // The account-deletion sweep updates every report for one reporter.
            e.HasIndex(r => r.UserId);

            // Every public read filters driver + status; the corroboration pass filters the same way.
            e.HasIndex(r => new { r.DriverId, r.Status });
            e.HasIndex(r => r.Status);
            e.HasIndex(r => r.CreatedAt);

            e.Ignore(r => r.IsLive);

            // Restrict, not SetNull: deleting an account must go through DeleteAccountCommand,
            // which de-identifies the reports deliberately and counts what it touched. A silent
            // cascade would make clause 29.2 depend on a database setting nobody reads.
            e.HasOne(r => r.User).WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(r => r.StatusAudits).WithOne(a => a.Report).HasForeignKey(a => a.ReportId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReportStatusAudit>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.FromStatus).HasConversion<string>();
            e.Property(a => a.ToStatus).HasConversion<string>();
            e.Property(a => a.Reason).HasMaxLength(2000).IsRequired();
            e.HasIndex(a => new { a.ReportId, a.CreatedAt });
            e.HasIndex(a => a.ActorUserId);
        });

        modelBuilder.Entity<DriverStatusAudit>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.FromStatus).HasConversion<string>();
            e.Property(a => a.ToStatus).HasConversion<string>();
            e.Property(a => a.Reason).HasMaxLength(2000).IsRequired();
            e.HasIndex(a => new { a.DriverId, a.CreatedAt });
            e.HasIndex(a => a.ActorUserId);

            e.HasOne(a => a.Driver).WithMany().HasForeignKey(a => a.DriverId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserConsent>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.ConsentKey).HasMaxLength(100).IsRequired();
            e.Property(c => c.AgreementVersion).HasMaxLength(20).IsRequired();
            e.Property(c => c.Locale).HasMaxLength(20).IsRequired();
            e.Property(c => c.CollectionSurface).HasMaxLength(100).IsRequired();
            // IPv6 in full form is 45 characters.
            e.Property(c => c.IpAddress).HasMaxLength(45);

            e.HasIndex(c => new { c.UserId, c.ConsentKey, c.AcceptedAt });
            // The retention worker scans on this.
            e.HasIndex(c => c.AcceptedAt);

            e.HasOne(c => c.User).WithMany(u => u.Consents).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DriverAppeal>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Status).HasConversion<string>();
            e.Property(a => a.Grounds).HasMaxLength(50).IsRequired();
            e.Property(a => a.Detail).HasMaxLength(4000).IsRequired();
            e.Property(a => a.ContactEmail).HasMaxLength(255).IsRequired();
            e.Property(a => a.ContactPhone).HasMaxLength(50);
            e.Property(a => a.IdentityEvidenceNote).HasMaxLength(2000).IsRequired();
            e.Property(a => a.Outcome).HasMaxLength(4000);

            e.HasIndex(a => new { a.DriverId, a.Status });
            e.HasIndex(a => a.ContactEmail);
            e.HasIndex(a => a.CreatedAt);
        });

        modelBuilder.Entity<DriverRecordAccessLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.RequesterIpHash).HasMaxLength(64).IsRequired();
            e.Property(l => l.Surface).HasMaxLength(50).IsRequired();
            e.Property(l => l.LookupTermHash).HasMaxLength(64);

            // The rate-limit check reads exactly this.
            e.HasIndex(l => new { l.RequesterIpHash, l.CreatedAt });
            e.HasIndex(l => l.CreatedAt);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Title).HasMaxLength(255).IsRequired();
            e.Property(n => n.Message).IsRequired();
            e.HasIndex(n => new { n.UserId, n.CreatedAt });
            e.HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DriverFollow>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasIndex(f => new { f.UserId, f.DriverId }).IsUnique();
            e.HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(f => f.Driver).WithMany().HasForeignKey(f => f.DriverId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Recommendation>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Category).HasConversion<string>();
            e.Property(r => r.Status).HasConversion<string>();
            e.Property(r => r.Subject).HasMaxLength(120).IsRequired();
            e.Property(r => r.Message).HasMaxLength(2000).IsRequired();
            e.HasIndex(r => new { r.UserId, r.CreatedAt });
            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VerificationHistory>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Status).HasMaxLength(50).IsRequired();
            e.Property(v => v.DriverName).HasMaxLength(255);
            e.Property(v => v.RegistrationNumber).HasMaxLength(50);
            // Several comma-separated SHA-256 hex digests.
            e.Property(v => v.ImageHashes).HasMaxLength(1000);

            e.HasIndex(v => new { v.UserId, v.VerifiedAt });
            // The retention worker scans on this.
            e.HasIndex(v => v.OcrDataRetainedUntil);

            e.HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
