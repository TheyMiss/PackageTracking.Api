using System.ComponentModel.DataAnnotations;

namespace PackageTracking.Api.Domain;

public class PersonInfo
{
    [Required] public string Name { get; set; } = default!;
    [Required] public string Address { get; set; } = default!;
    [Required] public string Phone { get; set; } = default!;
}

public class StatusChange
{
    public int Id { get; set; }
    public PackageStatus Status { get; set; }
    public DateTimeOffset changedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Package
{
    public int Id { get; set; }
    [Required] public string TrackingNumber { get; set; } = default!;
    [Required] public PersonInfo Sender { get; set; } = new();
    [Required] public PersonInfo Recipient { get; set; } = new();

    public PackageStatus CurrentStatus { get; set; } = PackageStatus.Created;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<StatusChange> History { get; set; } = new();
}