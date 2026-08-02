using System.Security.Claims;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FrmAlertsController : ControllerBase
{
    private readonly IFRMAlertService _frmAlertService;

    public FrmAlertsController(IFRMAlertService frmAlertService)
    {
        _frmAlertService = frmAlertService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? severity,
        [FromQuery] int? analystId,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _frmAlertService.SearchAlertsAsync(status, priority, severity, analystId, q, page, pageSize);
        return Ok(result);
    }

    [HttpGet("summary-stats")]
    public async Task<IActionResult> GetSummaryStats()
    {
        var stats = await _frmAlertService.GetSummaryStatsAsync();
        return Ok(stats);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var alert = await _frmAlertService.GetByIdDetailAsync(id);
        return alert is null ? NotFound(new { error = $"FRM Alert with ID {id} not found." }) : Ok(alert);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFrmAlertDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var alert = await _frmAlertService.CreateAlertAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = alert.AlertID }, alert);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateFrmAlertStatusDto dto)
    {
        if (id != dto.AlertID) return BadRequest(new { error = "Route ID and Body AlertID must match." });

        var userId = GetCurrentUserId();
        await _frmAlertService.UpdateStatusAsync(dto, userId);
        return NoContent();
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignFrmAlertDto dto)
    {
        var userId = GetCurrentUserId();
        await _frmAlertService.AssignAlertAsync(dto, userId);
        return Ok(new { message = "Alert assigned successfully." });
    }

    [HttpPost("escalate")]
    public async Task<IActionResult> Escalate([FromBody] EscalateFrmAlertDto dto)
    {
        var userId = GetCurrentUserId();
        await _frmAlertService.EscalateAlertAsync(dto, userId);
        return Ok(new { message = "Alert escalated successfully." });
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] CloseFrmAlertDto dto)
    {
        var userId = GetCurrentUserId();
        await _frmAlertService.CloseAlertAsync(dto, userId);
        return Ok(new { message = "Alert closed successfully." });
    }

    [HttpPost("comment")]
    public async Task<IActionResult> AddComment([FromBody] AddFrmAlertCommentDto dto)
    {
        var userId = GetCurrentUserId();
        await _frmAlertService.AddCommentAsync(dto, userId);
        return Ok(new { message = "Comment added successfully." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _frmAlertService.DeleteAlertAsync(id);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(subClaim, out var userId) ? userId : 1;
    }
}
