using PackageTracking.Api.Domain;

namespace PackageTracking.Api.Application;

public interface IPackageService
{
    Task<Package> CreateAsync(PersonDto sender, PersonDto recipient, CancellationToken ct);
    Task<(IReadOnlyList<Package> Items, int Total)> ListAsync(string? tracking, PackageStatus? status, int skip, int take, CancellationToken ct);
    Task<Package?> GetAsync(int id, CancellationToken ct);
    Task<Package?> GetByTrackingAsync(string trackingNumber, CancellationToken ct);
    Task<(bool Ok, string? Error)> TryChangeStatusAsync(int id, PackageStatus newStatus, CancellationToken ct);
}
