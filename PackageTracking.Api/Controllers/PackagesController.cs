using Microsoft.AspNetCore.Mvc;
using PackageTracking.Api.Application;
using PackageTracking.Api.Domain;

namespace PackageTracking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : Controller
{
    private readonly IPackageService _svc;
    public PackagesController(IPackageService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<Object>> List([FromQuery] string? tracking, [FromQuery] PackageStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await _svc.ListAsync(tracking, status, (page - 1) * pageSize, pageSize, ct);

        return Ok(new { items, page, pageSize, total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PackageDetailsDto>> GetById(int id, CancellationToken ct)
    {
        var p = await _svc.GetAsync(id, ct);
        return p is null ? NotFound() : Ok(ToDetailsDto(p));

    }

    [HttpGet("by-tracking/{trackingNumber}")]
    public async Task<ActionResult<PackageDetailsDto>> GetByTracking(string trackingNumber, CancellationToken ct)
    {
        var p = await _svc.GetByTrackingAsync(trackingNumber, ct);
        return p is null ? NotFound() : Ok(ToDetailsDto(p));
    }

    [HttpPost]
    public async Task<ActionResult<PackageDetailsDto>> Create([FromBody] CreatePackageRequest req, CancellationToken ct)
    {
        var p = await _svc.CreateAsync(req.Sender, req.Recipient, ct);
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, ToDetailsDto(p));
    }

    [HttpPost("{id:int}/status")]
    public async Task<ActionResult> ChangeStatus(int id, [FromBody] UpdateStatusRequest req, CancellationToken ct)
    {
        var (ok, error) = await _svc.TryChangeStatusAsync(id, req.NewStatus, ct);
        return ok ? NoContent() : BadRequest(new { error });
    }

    private static PackageDetailsDto ToDetailsDto(PackageTracking.Api.Domain.Package p) =>
      new(
          p.Id,
          p.TrackingNumber,
          new PersonDto(p.Sender.Name, p.Sender.Address, p.Sender.Phone),
          new PersonDto(p.Recipient.Name, p.Recipient.Address, p.Recipient.Phone),
          p.CurrentStatus,
          p.CreatedAt,
          p.History.OrderBy(h => h.changedAt).Select(h => new StatusChangeDto(h.Status, h.changedAt)).ToList(),
          StatusRules.AllowedNext(p.CurrentStatus).ToList()
      );
}
