namespace EnterpriseFraudRiskSystem.Models;

public class AlertHistory
{
    public int HistoryID { get; set; }
    public int AlertID { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public int? ActionBy { get; set; }
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public FRMAlert? Alert { get; set; }
    public User? ActionByUser { get; set; }
}
