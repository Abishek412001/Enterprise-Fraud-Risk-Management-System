namespace EnterpriseFraudRiskSystem.Models;

public class FraudMetric
{
    public int MetricID { get; set; }
    public DateTime MetricDate { get; set; }
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int ClosedAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int FrmAlertsCount { get; set; }
    public int AtoAlertsCount { get; set; }
    public int SentinelAlertsCount { get; set; }
    public int FraudConfirmedCount { get; set; }
    public int FalsePositivesCount { get; set; }
    public int AccountsFrozenCount { get; set; }
    public int CasesCreatedCount { get; set; }
    public int CasesClosedCount { get; set; }
    public double AvgResolutionMinutes { get; set; }
    public double SlaComplianceRate { get; set; }
    public decimal FraudLossPrevented { get; set; }
}
