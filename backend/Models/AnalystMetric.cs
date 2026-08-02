namespace EnterpriseFraudRiskSystem.Models;

public class AnalystMetric
{
    public int AnalystMetricID { get; set; }
    public int AnalystID { get; set; }
    public DateTime MetricDate { get; set; }
    public int AssignedAlerts { get; set; }
    public int ClosedAlerts { get; set; }
    public int OpenCases { get; set; }
    public double AvgInvestigationMinutes { get; set; }
    public int Escalations { get; set; }
    public int FalsePositives { get; set; }
    public int FraudConfirmed { get; set; }
    public double SlaComplianceRate { get; set; }
    public double WorkloadScore { get; set; }

    public User? Analyst { get; set; }
}
