using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AtoAlertsController : ControllerBase
{
    private readonly IATOAlertService _atoService;

    public AtoAlertsController(IATOAlertService atoService)
    {
        _atoService = atoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAtoAlerts(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? severity,
        [FromQuery] int? analystId,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _atoService.SearchAtoAlertsAsync(status, priority, severity, analystId, q, page, pageSize);
        return Ok(result);
    }

    [HttpGet("summary-stats")]
    public async Task<IActionResult> GetSummaryStats()
    {
        var stats = await _atoService.GetSummaryStatsAsync();
        return Ok(stats);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var alert = await _atoService.GetByIdDetailAsync(id);
        return alert is null ? NotFound(new { error = $"ATO Alert with ID {id} not found." }) : Ok(alert);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignAtoAlertDto dto)
    {
        await _atoService.AssignAtoAlertAsync(dto);
        return Ok(new { message = "ATO Alert assigned successfully." });
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] CloseAtoAlertDto dto)
    {
        await _atoService.CloseAtoAlertAsync(dto);
        return Ok(new { message = "ATO Alert closed successfully." });
    }
}
