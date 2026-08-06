using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipSharp.Application.Common.Models;
using ShipSharp.Application.Ports;
using ShipSharp.Application.Ports.DTOs;

namespace ShipSharp.API.Ports;

[ApiController]
[Route("ports")]
[Authorize]
public class PortsController : ControllerBase
{
    private readonly IPortService _portService;

    public PortsController(IPortService portService)
    {
        _portService = portService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var ports = await _portService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PortResponse>>.Success(ports));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var port = await _portService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PortResponse>.Success(port));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePortRequest request, CancellationToken cancellationToken)
    {
        var port = await _portService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = port.Id }, ApiResponse<PortResponse>.Success(port));
    }
}
