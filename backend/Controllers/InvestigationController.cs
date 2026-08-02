using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvestigationController : ControllerBase
{
    private readonly IInvestigationService _service;

    public InvestigationController(IInvestigationService service)
    {
        _service = service;
    }

    [HttpGet("customer360/{customerId:int}")]
    public async Task<IActionResult> GetCustomer360(int customerId)
    {
        var result = await _service.GetCustomer360Async(customerId);
        return result is null ? NotFound(new { error = $"Customer ID {customerId} not found." }) : Ok(result);
    }

    [HttpGet("riskhistory/{customerId:int}")]
    public async Task<IActionResult> GetRiskHistory(int customerId)
    {
        var result = await _service.GetRiskHistoryAsync(customerId);
        return Ok(result);
    }

    [HttpGet("timeline/{customerId:int}")]
    public async Task<IActionResult> GetTimeline(int customerId)
    {
        var result = await _service.GetTimelineAsync(customerId);
        return Ok(result);
    }

    [HttpGet("summary-stats")]
    public async Task<IActionResult> GetSummaryStats()
    {
        var stats = await _service.GetSummaryStatsAsync();
        return Ok(stats);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] AnalystActionDto dto)
    {
        var sessionId = await _service.StartInvestigationAsync(dto.CustomerID, dto.AnalystID);
        return Ok(new { sessionId, message = "Investigation session started." });
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] AnalystActionDto dto)
    {
        await _service.CloseInvestigationAsync(dto.ActionID, dto.Reason);
        return Ok(new { message = "Investigation session closed." });
    }

    [HttpPost("/api/account/freeze")]
    public async Task<IActionResult> FreezeAccount([FromBody] FreezeAccountDto dto)
    {
        await _service.FreezeCustomerAccountAsync(dto);
        return Ok(new { message = $"Customer ID {dto.CustomerID} account frozen successfully." });
    }

    [HttpPost("/api/account/unfreeze")]
    public async Task<IActionResult> UnfreezeAccount([FromBody] FreezeAccountDto dto)
    {
        await _service.UnfreezeCustomerAccountAsync(dto);
        return Ok(new { message = $"Customer ID {dto.CustomerID} account unfrozen." });
    }

    [HttpPost("/api/card/suspend")]
    public async Task<IActionResult> SuspendCard([FromBody] SuspendCardDto dto)
    {
        await _service.SuspendCardAsync(dto);
        return Ok(new { message = $"Card ID {dto.CardID} suspended." });
    }

    [HttpPost("/api/card/activate")]
    public async Task<IActionResult> ActivateCard([FromBody] SuspendCardDto dto)
    {
        await _service.ActivateCardAsync(dto);
        return Ok(new { message = $"Card ID {dto.CardID} activated." });
    }

    [HttpPost("/api/device/block")]
    public async Task<IActionResult> BlockDevice([FromBody] DeviceActionDto dto)
    {
        await _service.BlockDeviceAsync(dto);
        return Ok(new { message = $"Device ID {dto.DeviceID} blocked." });
    }

    [HttpPost("/api/device/trust")]
    public async Task<IActionResult> TrustDevice([FromBody] DeviceActionDto dto)
    {
        await _service.TrustDeviceAsync(dto);
        return Ok(new { message = $"Device ID {dto.DeviceID} marked as trusted." });
    }

    [HttpPost("note")]
    public async Task<IActionResult> RecordNote([FromBody] AnalystActionDto dto)
    {
        await _service.RecordAnalystActionAsync(dto);
        return Ok(new { message = "Analyst action recorded in investigation workspace." });
    }
}
