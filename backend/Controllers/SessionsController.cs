using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly IATOAlertService _atoService;

    public SessionsController(IATOAlertService atoService)
    {
        _atoService = atoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions(
        [FromQuery] int? customerId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _atoService.SearchSessionsAsync(customerId, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("/api/devices")]
    public async Task<IActionResult> GetDevices(
        [FromQuery] int? customerId,
        [FromQuery] bool? isBlocked,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _atoService.SearchDevicesAsync(customerId, isBlocked, page, pageSize);
        return Ok(result);
    }

    [HttpPost("login-simulation")]
    public async Task<IActionResult> RecordLogin([FromBody] RecordCustomerLoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var sessionId = await _atoService.RecordCustomerLoginAsync(dto);
        return Ok(new { sessionId, message = "Customer login attempt processed by ATO Risk Engine." });
    }

    [HttpPost("/api/devices/{id:int}/block")]
    public async Task<IActionResult> BlockDevice(int id)
    {
        await _atoService.SetDeviceStatusAsync(id, isBlocked: true, isTrusted: false);
        return Ok(new { message = $"Device ID {id} has been blocked." });
    }

    [HttpPost("/api/devices/{id:int}/trust")]
    public async Task<IActionResult> TrustDevice(int id)
    {
        await _atoService.SetDeviceStatusAsync(id, isBlocked: false, isTrusted: true);
        return Ok(new { message = $"Device ID {id} has been marked as Trusted." });
    }
}
