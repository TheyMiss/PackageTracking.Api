using PackageTracking.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace PackageTracking.Api.Application;

public record PersonDto(
    [param:Required] string Name,
    [param: Required] string Address,
    [param: Required] string Phone
    );

public record CreatePackageRequest(
    [param: Required] PersonDto Sender,
    [param: Required] PersonDto Recipient
    );

public record PackageListItem(
    int Id,
    string TrackingNumber,
    PackageStatus CurrentStatus,
    string SenderName,
    string RecipientName,
    DateTimeOffset CreatedAt
    );

public record StatusChangeDto(PackageStatus Status, DateTimeOffset changedAt);

public record PackageDetailsDto(
    int Id, 
    string TrackingNumber,
    PersonDto Sender,
    PersonDto Recipient,
    PackageStatus CurrentStatus,
    DateTimeOffset CreatedAt,
    IReadOnlyList<StatusChangeDto> History,
    IReadOnlyList<PackageStatus> AllowedNextStatuses
    );

public record UpdateStatusRequest([param: Required] PackageStatus NewStatus);