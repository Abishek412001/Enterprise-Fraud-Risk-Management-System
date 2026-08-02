using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RulesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RulesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRules()
    {
        var rules = await _context.Set<FraudRule>().ToListAsync();
        return Ok(rules);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] FraudRule rule)
    {
        _context.Set<FraudRule>().Add(rule);
        await _context.SaveChangesAsync();
        return Ok(rule);
    }
}
