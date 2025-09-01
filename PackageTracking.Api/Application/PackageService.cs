using Microsoft.EntityFrameworkCore;
using PackageTracking.Api.Domain;
using PackageTracking.Api.Infrastructure;

namespace PackageTracking.Api.Application;

public class PackageService : IPackageService
{
    private readonly AppDbContext _db;
    private readonly ITrackingNumberGenerator _gen;

    public PackageService(AppDbContext db, ITrackingNumberGenerator gen)
    {
        _db = db;
        _gen = gen;
    }

    public async Task<Package> CreateAsync(PersonDto sender, PersonDto recipient, CancellationToken ct)
    {
        var pkg = new Package
        {
            TrackingNumber = _gen.Generate(),
            Sender = new PersonInfo
            {
                Name = sender.Name,
                Address = sender.Address,
                Phone = sender.Phone
            },
            Recipient = new PersonInfo
            {
                Name = recipient.Name,
                Address = recipient.Address,
                Phone = recipient.Phone
            },
            CurrentStatus = PackageStatus.Created,
            History = new() { new() { Status = PackageStatus.Created, changedAt = DateTimeOffset.UtcNow } }
        };

        _db.Packages.Add(pkg);
        await _db.SaveChangesAsync(ct);
        return pkg;
    }

    public async Task<(IReadOnlyList<Package> Items, int Total)> ListAsync(string? tracking, PackageStatus? status, int skip, int take, CancellationToken ct)
    {
        var q = _db.Packages.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tracking))
            q = q.Where(p => p.TrackingNumber.Contains(tracking));

        if (status.HasValue)
            q = q.Where(p => p.CurrentStatus == status);


        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Package?> GetAsync(int id, CancellationToken ct) =>
        _db.Packages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Package?> GetByTrackingAsync(string trackingNumber, CancellationToken ct) =>
        _db.Packages.AsNoTracking().FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber, ct);

    public async Task<(bool Ok, string? Error)> TryChangeStatusAsync(int id, PackageStatus newStatus, CancellationToken ct)
    {
        var pkg = await _db.Packages.Include(p => p.History).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pkg is null) return (false, "Package not found");

        if (pkg.CurrentStatus == newStatus) return (true, null);
        if (!StatusRules.CanTransition(pkg.CurrentStatus, newStatus))
            return (false, $"Invalid status transition {pkg.CurrentStatus} -> {newStatus}");

        pkg.CurrentStatus = newStatus;
        pkg.History.Add(new StatusChange { Status = newStatus, changedAt = DateTimeOffset.UtcNow });
        await _db.SaveChangesAsync(ct);
        return (true, null);
    }
}
