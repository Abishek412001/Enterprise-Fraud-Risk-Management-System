using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly ISecurityService _securityService;

    public PermissionsController(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _securityService.GetPermissionsAsync();
        return Ok(permissions);
    }
}
