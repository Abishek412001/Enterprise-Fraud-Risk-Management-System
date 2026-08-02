using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityEventsController : ControllerBase
{
    private readonly ISentinelService _sentinelService;

    public SecurityEventsController(ISentinelService sentinelService)
    {
        _sentinelService = sentinelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSecurityEvents(
        [FromQuery] int? customerId,
        [FromQuery] string? eventType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sentinelService.SearchSecurityEventsAsync(customerId, eventType, page, pageSize);
        return Ok(result);
    }

    [HttpGet("/api/threatindicators")]
    public async Task<IActionResult> GetThreatIndicators(
        [FromQuery] string? threatLevel,
        [FromQuery] string? source,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sentinelService.SearchThreatIndicatorsAsync(threatLevel, source, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> RecordSecurityEvent([FromBody] CreateSecurityEventDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var eventId = await _sentinelService.RecordSecurityEventAsync(dto);
        return Ok(new { eventId, message = "Security event ingested and correlated by Sentinel SIEM Engine." });
    }
}
