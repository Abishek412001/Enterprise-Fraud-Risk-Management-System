using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly ISentinelService _sentinelService;

    public IncidentsController(ISentinelService sentinelService)
    {
        _sentinelService = sentinelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetIncidents(
        [FromQuery] string? severity,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sentinelService.SearchIncidentsAsync(severity, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdDetail(int id)
    {
        var incident = await _sentinelService.GetIncidentByIdDetailAsync(id);
        return incident is null ? NotFound(new { error = $"Sentinel Incident with ID {id} not found." }) : Ok(incident);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignIncidentDto dto)
    {
        await _sentinelService.AssignIncidentAsync(dto);
        return Ok(new { message = "Sentinel Incident assigned successfully." });
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] CloseIncidentDto dto)
    {
        await _sentinelService.CloseIncidentAsync(dto);
        return Ok(new { message = "Sentinel Incident closed successfully." });
    }
}
