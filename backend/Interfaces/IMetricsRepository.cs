using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IMetricsRepository
{
    Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync();
    Task<FraudReportDto> GetFraudReportAsync();
    Task<List<FraudTrend>> GetFraudTrendsAsync();
    Task<List<AnalystPerformanceDto>> GetAnalystPerformanceAsync();
}
