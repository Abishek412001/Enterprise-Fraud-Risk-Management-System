using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var newUserId = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new { id = newUserId }, new { userId = newUserId });
        }
        catch (Exception ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request, ipAddress);

        if (result is null)
            return Unauthorized(new { error = "Invalid username or password." });

        return Ok(result);
    }
}
