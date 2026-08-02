namespace EnterpriseFraudRiskSystem.Models;

public class CaseTimeline
{
    public int TimelineID { get; set; }
    public int CaseID { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? ActionBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }

    public Case? Case { get; set; }
    public User? ActionByUser { get; set; }
}
