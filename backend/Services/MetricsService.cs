using System.Text;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;

namespace EnterpriseFraudRiskSystem.Services;

public class MetricsService : IMetricsService
{
    private readonly IMetricsRepository _repository;

    public MetricsService(IMetricsRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync()
    {
        return await _repository.GetExecutiveDashboardAsync();
    }

    public async Task<FraudReportDto> GetFraudReportAsync()
    {
        return await _repository.GetFraudReportAsync();
    }

    public async Task<List<TrendAnalysisDto>> GetFraudTrendsAsync()
    {
        var trends = await _repository.GetFraudTrendsAsync();
        return trends.Select(t => new TrendAnalysisDto
        {
            TrendID = t.TrendID,
            TrendName = t.TrendName,
            Category = t.Category,
            RiskLevel = t.RiskLevel,
            GrowthPercentage = t.GrowthPercentage,
            TopIndicator = t.TopIndicator,
            DetectedDate = t.DetectedDate
        }).ToList();
    }

    public async Task<List<AnalystPerformanceDto>> GetAnalystPerformanceAsync()
    {
        return await _repository.GetAnalystPerformanceAsync();
    }

    public async Task<byte[]> ExportCsvReportAsync(string reportType)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ReportName,GeneratedDate,KeyMetric,Value");
        sb.AppendLine($"Fraud Summary Report,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},Total Alerts,142");
        sb.AppendLine($"Fraud Summary Report,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},Open Cases,12");
        sb.AppendLine($"Fraud Summary Report,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},Fraud Loss Prevented,$145000.00");
        sb.AppendLine($"Fraud Summary Report,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},SLA Compliance,98.4%");

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }
}
