namespace EnterpriseFraudRiskSystem.Models;

public class ATOAlert
{
    public int ATOAlertID { get; set; }
    public string ATOAlertNumber { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public int? SessionID { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = "High";
    public string Priority { get; set; } = "High";
    public int RiskScore { get; set; } = 70;
    public string Status { get; set; } = "Open";
    public int? AssignedAnalystID { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedDate { get; set; }
    public string? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }

    public Customer? Customer { get; set; }
    public CustomerSession? Session { get; set; }
    public User? AssignedAnalyst { get; set; }
}
