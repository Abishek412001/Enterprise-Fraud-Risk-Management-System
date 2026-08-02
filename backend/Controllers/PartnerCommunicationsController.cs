using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PartnerCommunicationsController : ControllerBase
{
    private readonly IWcaService _wcaService;

    public PartnerCommunicationsController(IWcaService wcaService)
    {
        _wcaService = wcaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCommunications(
        [FromQuery] int? caseId,
        [FromQuery] int? partnerId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _wcaService.SearchPartnerCommunicationsAsync(caseId, partnerId, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("/api/templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _wcaService.GetActiveTemplatesAsync();
        return Ok(templates);
    }

    [HttpGet("/api/partners")]
    public async Task<IActionResult> GetPartners()
    {
        var partners = await _wcaService.GetPartnerDirectoryAsync();
        return Ok(partners);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _wcaService.GetCommunicationByIdAsync(id);
        return result is null ? NotFound(new { error = $"Partner Communication with ID {id} not found." }) : Ok(result);
    }

    [HttpPost]
    [HttpPost("send")]
    public async Task<IActionResult> SendCommunication([FromBody] SendCommunicationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var id = await _wcaService.SendPartnerCommunicationAsync(dto);
        return Ok(new { communicationId = id, message = "Partner communication dispatched successfully." });
    }
}
