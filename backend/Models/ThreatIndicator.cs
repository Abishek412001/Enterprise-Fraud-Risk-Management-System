namespace EnterpriseFraudRiskSystem.Models;

public class ThreatIndicator
{
    public int IndicatorID { get; set; }
    public string IndicatorType { get; set; } = string.Empty;
    public string IndicatorValue { get; set; } = string.Empty;
    public string ThreatLevel { get; set; } = "High";
    public string Source { get; set; } = "AbuseIPDB";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
