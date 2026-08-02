namespace EnterpriseFraudRiskSystem.Models;

public class Case
{
    public int CaseID { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CaseType { get; set; } = "FraudInvestigation";
    public string CaseTitle { get; set; } = string.Empty;
    public string? CaseDescription { get; set; }
    public int CustomerID { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Severity { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public int? AssignedAnalystID { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? RootCause { get; set; }
    public string? Resolution { get; set; }
    public bool FalsePositive { get; set; } = false;

    public Customer? Customer { get; set; }
    public User? AssignedAnalyst { get; set; }
    public SLATracking? SLA { get; set; }
    public ICollection<CaseAlert> Alerts { get; set; } = new List<CaseAlert>();
    public ICollection<CaseTransaction> Transactions { get; set; } = new List<CaseTransaction>();
    public ICollection<CaseNote> Notes { get; set; } = new List<CaseNote>();
    public ICollection<CaseTimeline> Timelines { get; set; } = new List<CaseTimeline>();
    public ICollection<CaseAttachment> Attachments { get; set; } = new List<CaseAttachment>();
    public ICollection<CaseEscalation> Escalations { get; set; } = new List<CaseEscalation>();
}
