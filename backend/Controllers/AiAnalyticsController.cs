using EnterpriseFraudRiskSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiAnalyticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AiAnalyticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("clusters")]
    public IActionResult GetClusters()
    {
        var clusters = new[]
        {
            new { clusterName = "Suspected Mule Accounts", customerCount = 14, riskCategory = "High" },
            new { clusterName = "High Velocity Shoppers", customerCount = 38, riskCategory = "Medium" },
            new { clusterName = "Low Risk Standard", customerCount = 1250, riskCategory = "Low" }
        };
        return Ok(clusters);
    }

    [HttpGet("anomalies")]
    public IActionResult GetAnomalies()
    {
        var anomalies = new[]
        {
            new { entityType = "Transaction", entityID = "TXN-9041", anomalyType = "Statistical Amount Outlier ($12,500 vs $50 avg)", confidenceScore = 0.94, time = DateTime.UtcNow.AddMinutes(-12) },
            new { entityType = "LoginSession", entityID = "SES-4012", anomalyType = "Sudden Geographic Jump (NY to Tokyo in 10 mins)", confidenceScore = 0.98, time = DateTime.UtcNow.AddMinutes(-45) }
        };
        return Ok(anomalies);
    }
}
