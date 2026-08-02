using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IMetricsService
{
    Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync();
    Task<FraudReportDto> GetFraudReportAsync();
    Task<List<TrendAnalysisDto>> GetFraudTrendsAsync();
    Task<List<AnalystPerformanceDto>> GetAnalystPerformanceAsync();
    Task<byte[]> ExportCsvReportAsync(string reportType);
}
