namespace EnterpriseFraudRiskSystem.DTOs;

public class ExecutiveDashboardDto
{
    public int TotalOpenAlerts { get; set; }
    public int OpenCases { get; set; }
    public int FrozenAccounts { get; set; }
    public int ActiveIncidents { get; set; }
    public double SlaComplianceRate { get; set; }
    public decimal FraudLossPrevented { get; set; }
    public List<string> RuleBasedInsights { get; set; } = new();
}

public class FraudReportDto
{
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public int TotalTransactionsAnalyzed { get; set; }
    public int TotalAlertsTriggered { get; set; }
    public int FraudCasesOpened { get; set; }
    public decimal TotalFraudLossPrevented { get; set; }
    public double FalsePositiveRate { get; set; }
    public List<string> TopFraudMerchants { get; set; } = new();
    public List<string> TopFraudCountries { get; set; } = new();
}

public class TrendAnalysisDto
{
    public int TrendID { get; set; }
    public string TrendName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public double GrowthPercentage { get; set; }
    public string TopIndicator { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; }
}

public class AnalystPerformanceDto
{
    public int AnalystID { get; set; }
    public string AnalystName { get; set; } = string.Empty;
    public int AssignedAlerts { get; set; }
    public int ClosedAlerts { get; set; }
    public int OpenCases { get; set; }
    public double AvgInvestigationMinutes { get; set; }
    public int Escalations { get; set; }
    public double SlaComplianceRate { get; set; }
    public double WorkloadScore { get; set; }
}
