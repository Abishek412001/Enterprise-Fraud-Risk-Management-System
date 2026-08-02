using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SentinelAlertsController : ControllerBase
{
    private readonly ISentinelService _sentinelService;

    public SentinelAlertsController(ISentinelService sentinelService)
    {
        _sentinelService = sentinelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSentinelAlerts(
        [FromQuery] string? severity,
        [FromQuery] string? status,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sentinelService.SearchSentinelAlertsAsync(severity, status, q, page, pageSize);
        return Ok(result);
    }

    [HttpGet("summary-stats")]
    public async Task<IActionResult> GetSummaryStats()
    {
        var stats = await _sentinelService.GetSummaryStatsAsync();
        return Ok(stats);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var alert = await _sentinelService.GetAlertByIdAsync(id);
        return alert is null ? NotFound(new { error = $"Sentinel Alert with ID {id} not found." }) : Ok(alert);
    }
}
