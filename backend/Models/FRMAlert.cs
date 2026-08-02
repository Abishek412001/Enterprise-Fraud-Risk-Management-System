namespace EnterpriseFraudRiskSystem.Models;

public class FRMAlert
{
    public int AlertID { get; set; }
    public string AlertNumber { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public int AccountID { get; set; }
    public long? TransactionID { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string AlertCategory { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Severity { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public int RiskScore { get; set; } = 50;
    public int? AssignedAnalystID { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedDate { get; set; }
    public string? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }

    public Customer? Customer { get; set; }
    public Account? Account { get; set; }
    public Transaction? Transaction { get; set; }
    public User? AssignedAnalyst { get; set; }

    public ICollection<AlertAssignment> Assignments { get; set; } = new List<AlertAssignment>();
    public ICollection<AlertHistory> History { get; set; } = new List<AlertHistory>();
    public ICollection<AlertComment> Comments { get; set; } = new List<AlertComment>();
}
