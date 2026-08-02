namespace EnterpriseFraudRiskSystem.Models;

public class SLATracking
{
    public int SLAID { get; set; }
    public int CaseID { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime TargetResolution { get; set; }
    public DateTime? ActualResolution { get; set; }
    public string SLAStatus { get; set; } = "OnTrack"; // OnTrack | NearBreach | Breached | Met

    public Case? Case { get; set; }
}
