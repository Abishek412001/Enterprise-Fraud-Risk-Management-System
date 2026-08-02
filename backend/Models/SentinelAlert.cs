namespace EnterpriseFraudRiskSystem.Models;

public class SentinelAlert
{
    public int AlertID { get; set; }
    public string AlertNumber { get; set; } = string.Empty;
    public string AlertName { get; set; } = string.Empty;
    public string AlertCategory { get; set; } = string.Empty;
    public string AlertSource { get; set; } = "Microsoft Sentinel";
    public string AlertRule { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public int? UserID { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string Country { get; set; } = "Unknown";
    public int? DeviceID { get; set; }
    public string Severity { get; set; } = "High";
    public string Priority { get; set; } = "High";
    public int RiskScore { get; set; } = 75;
    public string Status { get; set; } = "Open";
    public int? AssignedAnalystID { get; set; }
    public int? IncidentID { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedDate { get; set; }
    public string? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }

    public Customer? Customer { get; set; }
    public User? User { get; set; }
    public Device? Device { get; set; }
    public User? AssignedAnalyst { get; set; }
    public SentinelIncident? Incident { get; set; }
}
