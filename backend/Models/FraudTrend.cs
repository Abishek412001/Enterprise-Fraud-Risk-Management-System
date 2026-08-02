namespace EnterpriseFraudRiskSystem.Models;

public class FraudTrend
{
    public int TrendID { get; set; }
    public string TrendName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "High";
    public double GrowthPercentage { get; set; }
    public string TopIndicator { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; } = DateTime.UtcNow;
}
