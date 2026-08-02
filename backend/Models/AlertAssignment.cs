namespace EnterpriseFraudRiskSystem.Models;

public class AlertAssignment
{
    public int AssignmentID { get; set; }
    public int AlertID { get; set; }
    public int AnalystID { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public int? AssignedBy { get; set; }

    public FRMAlert? Alert { get; set; }
    public User? Analyst { get; set; }
    public User? AssignedByUser { get; set; }
}
