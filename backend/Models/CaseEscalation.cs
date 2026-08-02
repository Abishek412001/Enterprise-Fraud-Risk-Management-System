namespace EnterpriseFraudRiskSystem.Models;

public class CaseEscalation
{
    public int EscalationID { get; set; }
    public int CaseID { get; set; }
    public int EscalatedTo { get; set; }
    public string EscalationReason { get; set; } = string.Empty;
    public DateTime EscalationDate { get; set; } = DateTime.UtcNow;

    public Case? Case { get; set; }
    public User? EscalatedToUser { get; set; }
}
