using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Shipments;
using ShipSharp.Application.Shipments.DTOs;

namespace ShipSharp.API.Shipments;

[ApiController]
[Route("api/shipments")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int per_page = 10, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _shipmentService.GetPagedAsync(page, per_page, cancellationToken);
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
        var pagination = PaginationMeta.Create(page, per_page, totalCount, baseUrl);
        var meta = ApiMeta.Create(pagination: pagination);
        return Ok(ApiResponse<IReadOnlyList<ShipmentResponse>>.Success(items, meta));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ShipmentResponse>.Success(shipment));
    }

    [HttpGet("track/{trackingNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackByNumber(string trackingNumber, CancellationToken cancellationToken)
    {
        var trackingInfo = await _shipmentService.TrackByNumberAsync(trackingNumber, cancellationToken);
        return Ok(ApiResponse<ShipmentTrackingResponse>.Success(trackingInfo));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var shipment = await _shipmentService.CreateAsync(request, username, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, ApiResponse<ShipmentResponse>.Success(shipment));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipmentRequest request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ShipmentResponse>.Success(shipment));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusRequest request, CancellationToken cancellationToken)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var shipment = await _shipmentService.UpdateStatusAsync(id, request.Status, username, cancellationToken);
        return Ok(ApiResponse<ShipmentResponse>.Success(shipment));
    }
}
