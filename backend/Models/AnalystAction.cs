namespace EnterpriseFraudRiskSystem.Models;

public class AnalystAction
{
    public int ActionID { get; set; }
    public int CustomerID { get; set; }
    public int AnalystID { get; set; }
    public int? SessionID { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }

    public Customer? Customer { get; set; }
    public User? Analyst { get; set; }
    public InvestigationSession? Session { get; set; }
}
