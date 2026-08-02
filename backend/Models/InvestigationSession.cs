namespace EnterpriseFraudRiskSystem.Models;

public class InvestigationSession
{
    public int SessionID { get; set; }
    public int CustomerID { get; set; }
    public int AnalystID { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = "Active";
    public string? SummaryNotes { get; set; }

    public Customer? Customer { get; set; }
    public User? Analyst { get; set; }
    public ICollection<AnalystAction> Actions { get; set; } = new List<AnalystAction>();
    public ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();
}
