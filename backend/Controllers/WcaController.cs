using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WcaController : ControllerBase
{
    private readonly IWcaService _wcaService;

    public WcaController(IWcaService wcaService)
    {
        _wcaService = wcaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWcaInteractions(
        [FromQuery] int? customerId,
        [FromQuery] int? caseId,
        [FromQuery] string? actionType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _wcaService.SearchWcaInteractionsAsync(customerId, caseId, actionType, page, pageSize);
        return Ok(result);
    }

    [HttpGet("summary-stats")]
    public async Task<IActionResult> GetSummaryStats()
    {
        var stats = await _wcaService.GetSummaryStatsAsync();
        return Ok(stats);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _wcaService.GetInteractionByIdAsync(id);
        return result is null ? NotFound(new { error = $"WCA Interaction with ID {id} not found." }) : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> RecordInteraction([FromBody] RecordWcaInteractionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var id = await _wcaService.RecordWcaInteractionAsync(dto);
        return Ok(new { interactionId = id, message = "WCA Action recorded in audit log." });
    }
}
