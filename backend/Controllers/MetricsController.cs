using EnterpriseFraudRiskSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseFraudRiskSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _service;

    public MetricsController(IMetricsService service)
    {
        _service = service;
    }

    [HttpGet]
    [HttpGet("/api/dashboard")]
    [HttpGet("/api/executive")]
    public async Task<IActionResult> GetExecutiveDashboard()
    {
        var result = await _service.GetExecutiveDashboardAsync();
        return Ok(result);
    }

    [HttpGet("/api/reports")]
    public async Task<IActionResult> GetFraudReport()
    {
        var result = await _service.GetFraudReportAsync();
        return Ok(result);
    }

    [HttpGet("/api/trends")]
    public async Task<IActionResult> GetTrends()
    {
        var result = await _service.GetFraudTrendsAsync();
        return Ok(result);
    }

    [HttpGet("/api/performance")]
    [HttpGet("/api/analysts")]
    public async Task<IActionResult> GetAnalystPerformance()
    {
        var result = await _service.GetAnalystPerformanceAsync();
        return Ok(result);
    }

    [HttpGet("/api/export/csv")]
    [HttpGet("/api/export/excel")]
    public async Task<IActionResult> ExportCsv([FromQuery] string type = "executive")
    {
        var bytes = await _service.ExportCsvReportAsync(type);
        return File(bytes, "text/csv", $"Fraud_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("/api/export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] string type = "executive")
    {
        var bytes = await _service.ExportCsvReportAsync(type);
        return File(bytes, "application/pdf", $"Fraud_Report_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
