using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CasesController : ControllerBase
{
    private readonly ICaseService _caseService;

    public CasesController(ICaseService caseService)
    {
        _caseService = caseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCases(
        [FromQuery] string? priority,
        [FromQuery] string? severity,
        [FromQuery] string? status,
        [FromQuery] int? analystId,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _caseService.SearchCasesAsync(priority, severity, status, analystId, q, page, pageSize);
        return Ok(result);
    }

    [HttpGet("open")]
    public async Task<IActionResult> GetOpenCases()
    {
        var result = await _caseService.GetOpenCasesAsync();
        return Ok(result);
    }

    [HttpGet("critical")]
    public async Task<IActionResult> GetCriticalCases()
    {
        var result = await _caseService.GetCriticalCasesAsync();
        return Ok(result);
    }

    [HttpGet("analyst/{id:int}")]
    public async Task<IActionResult> GetCasesByAnalyst(int id)
    {
        var result = await _caseService.GetCasesByAnalystAsync(id);
        return Ok(result);
    }

    [HttpGet("summary-stats")]
    public async Task<IActionResult> GetSummaryStats()
    {
        var stats = await _caseService.GetSummaryStatsAsync();
        return Ok(stats);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var caseDetail = await _caseService.GetCaseByIdDetailAsync(id);
        return caseDetail is null ? NotFound(new { error = $"Fraud Case with ID {id} not found." }) : Ok(caseDetail);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCase([FromBody] CreateCaseDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _caseService.CreateCaseAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.CaseID }, created);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignCase([FromBody] AssignCaseDto dto)
    {
        await _caseService.AssignCaseAsync(dto);
        return Ok(new { message = "Case assigned successfully." });
    }

    [HttpPost("escalate")]
    public async Task<IActionResult> EscalateCase([FromBody] EscalateCaseDto dto)
    {
        await _caseService.EscalateCaseAsync(dto);
        return Ok(new { message = "Case escalated successfully." });
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseCase([FromBody] CloseCaseDto dto)
    {
        await _caseService.CloseCaseAsync(dto);
        return Ok(new { message = "Case resolved and closed successfully." });
    }

    [HttpPost("note")]
    public async Task<IActionResult> AddNote([FromBody] AddCaseNoteDto dto)
    {
        await _caseService.AddCaseNoteAsync(dto);
        return Ok(new { message = "Case note added." });
    }

    [HttpPost("attachment")]
    public async Task<IActionResult> AddAttachment([FromBody] AddCaseAttachmentDto dto)
    {
        await _caseService.AddAttachmentAsync(dto);
        return Ok(new { message = "Attachment linked to case." });
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateCaseStatusDto dto)
    {
        await _caseService.UpdateCaseStatusAsync(dto);
        return Ok(new { message = "Case status updated." });
    }
}
