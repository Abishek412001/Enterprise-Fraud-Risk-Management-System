namespace EnterpriseFraudRiskSystem.Models;

public class SentinelIncident
{
    public int IncidentID { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = "High";
    public string Status { get; set; } = "New";
    public int? AssignedAnalystID { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedDate { get; set; }

    public User? AssignedAnalyst { get; set; }
    public ICollection<SentinelAlert> Alerts { get; set; } = new List<SentinelAlert>();
}
