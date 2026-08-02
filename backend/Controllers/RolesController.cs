using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly ISecurityService _securityService;

    public RolesController(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _securityService.GetRolesAsync();
        return Ok(roles);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        await _securityService.AssignRoleAsync(dto);
        return Ok(new { message = $"Role '{dto.RoleName}' assigned to user ID {dto.UserId} successfully." });
    }
}
