using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Vessels;
using ShipSharp.Application.Vessels.DTOs;

namespace ShipSharp.API.Vessels;

[ApiController]
[Route("api/vessels")]
[Authorize]
public class VesselsController : ControllerBase
{
    private readonly IVesselService _vesselService;

    public VesselsController(IVesselService vesselService)
    {
        _vesselService = vesselService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetPaged([FromQuery] bool? is_active, [FromQuery] int page = 1, [FromQuery] int per_page = 10, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _vesselService.GetPagedAsync(is_active, page, per_page, cancellationToken);
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
        var pagination = PaginationMeta.Create(page, per_page, totalCount, baseUrl);
        var meta = ApiMeta.Create(pagination: pagination);
        return Ok(ApiResponse<IReadOnlyList<VesselResponse>>.Success(items, meta));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vessel = await _vesselService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<VesselResponse>.Success(vessel));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateVesselRequest request, CancellationToken cancellationToken)
    {
        var vessel = await _vesselService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = vessel.Id }, ApiResponse<VesselResponse>.Success(vessel));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVesselRequest request, CancellationToken cancellationToken)
    {
        var vessel = await _vesselService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<VesselResponse>.Success(vessel));
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var vessel = await _vesselService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponse<VesselResponse>.Success(vessel));
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var vessel = await _vesselService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponse<VesselResponse>.Success(vessel));
    }
}
