using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Dealer> Dealers => Set<Dealer>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>()
            .Property(a => a.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Dealer)
            .WithMany(d => d.Accounts)
            .HasForeignKey(a => a.DealerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Accounts)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Dealer>()
            .HasMany(d => d.Customers)
            .WithOne(c => c.Dealer)
            .HasForeignKey(c => c.DealerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Units)
            .WithOne(u => u.Account)
            .HasForeignKey(u => u.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Unit>()
            .HasIndex(u => u.MacAddress)
            .IsUnique();

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