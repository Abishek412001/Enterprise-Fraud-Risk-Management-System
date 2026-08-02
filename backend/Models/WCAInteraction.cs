namespace EnterpriseFraudRiskSystem.Models;

public class WCAInteraction
{
    public int InteractionID { get; set; }
    public int? CaseID { get; set; }
    public int? AlertID { get; set; }
    public int CustomerID { get; set; }
    public int AnalystID { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ActionCategory { get; set; } = "InvestigationAction";
    public string ActionDescription { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string? StatusBefore { get; set; }
    public string? StatusAfter { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Case? Case { get; set; }
    public Customer? Customer { get; set; }
    public User? Analyst { get; set; }
}
