using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Unit>()
            .HasIndex(u => u.MacAddress)
            .IsUnique();

        modelBuilder.Entity<Unit>()
            .HasOne(u => u.Account)
            .WithMany(a => a.Units)
            .HasForeignKey(u => u.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Measurement>()
            .HasOne(m => m.Unit)
            .WithMany(u => u.Measurements)
            .HasForeignKey(m => m.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Measurement>()
            .HasIndex(m => new { m.UnitId, m.MeasuredAt });

        modelBuilder.Entity<UserSettings>()
            .HasOne(s => s.Account)
            .WithOne()
            .HasForeignKey<UserSettings>(s => s.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSettings>()
            .HasIndex(s => s.AccountId)
            .IsUnique();
    }
}