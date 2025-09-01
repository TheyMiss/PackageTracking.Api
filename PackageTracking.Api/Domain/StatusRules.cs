namespace PackageTracking.Api.Domain;

public static class StatusRules
{
    private static readonly Dictionary<PackageStatus, PackageStatus[]> Next = new()
    {
        { PackageStatus.Created, new[] { PackageStatus.Sent, PackageStatus.Canceled } },
        { PackageStatus.Sent, new[] { PackageStatus.Accepted, PackageStatus.Returned, PackageStatus.Canceled } },
        { PackageStatus.Returned, new[] { PackageStatus.Sent, PackageStatus.Canceled } },
        { PackageStatus.Accepted, Array.Empty<PackageStatus>() },
        { PackageStatus.Canceled, Array.Empty<PackageStatus>() },
    };

    public static bool CanTransition(PackageStatus from, PackageStatus to) =>
        Next.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static IEnumerable<PackageStatus> AllowedNext(PackageStatus from) =>
        Next.TryGetValue(from, out var allowed) ? allowed : Array.Empty<PackageStatus>();
}