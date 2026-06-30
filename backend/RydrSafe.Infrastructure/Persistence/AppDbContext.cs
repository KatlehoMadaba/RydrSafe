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
        });

        modelBuilder.Entity<Driver>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.DriverName).HasMaxLength(255).IsRequired();
            e.Property(d => d.PhoneNumber).HasMaxLength(50);
            e.Property(d => d.Status).HasConversion<string>();
            e.HasMany(d => d.Vehicles).WithOne(v => v.Driver).HasForeignKey(v => v.DriverId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(d => d.Reports).WithOne(r => r.Driver).HasForeignKey(r => r.DriverId).OnDelete(DeleteBehavior.Cascade);
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
            e.Property(r => r.Description).IsRequired();
            e.HasOne(r => r.User).WithMany(u => u.Reports).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Title).HasMaxLength(255).IsRequired();
            e.Property(n => n.Message).IsRequired();
            e.HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DriverFollow>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasIndex(f => new { f.UserId, f.DriverId }).IsUnique();
            e.HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(f => f.Driver).WithMany().HasForeignKey(f => f.DriverId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VerificationHistory>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Status).HasMaxLength(50).IsRequired();
            e.Property(v => v.DriverName).HasMaxLength(255);
            e.Property(v => v.RegistrationNumber).HasMaxLength(50);
            e.HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
