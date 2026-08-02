using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly ISecurityService _securityService;

    public AuditLogsController(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _securityService.GetAuditLogsAsync(page, pageSize);
        return Ok(result);
    }
}
