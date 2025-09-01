using Microsoft.EntityFrameworkCore;
using PackageTracking.Api.Domain;

namespace PackageTracking.Api.Infrastructure;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Package> Packages => Set<Package>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Package>().OwnsOne(p => p.Sender);
        modelBuilder.Entity<Package>().OwnsOne(p => p.Recipient);
        modelBuilder.Entity<Package>().OwnsMany(p => p.History);
        modelBuilder.Entity<Package>().HasIndex(p => p.TrackingNumber).IsUnique();
    }
}
