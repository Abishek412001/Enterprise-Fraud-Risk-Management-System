namespace EnterpriseFraudRiskSystem.Models;

public class InvestigationTimeline
{
    public int TimelineID { get; set; }
    public int CustomerID { get; set; }
    public string EventCategory { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? PerformedBy { get; set; }

    public Customer? Customer { get; set; }
    public User? PerformedByUser { get; set; }
}
