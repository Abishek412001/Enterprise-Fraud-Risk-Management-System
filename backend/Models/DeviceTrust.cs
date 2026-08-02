namespace EnterpriseFraudRiskSystem.Models;

public class DeviceTrust
{
    public int TrustID { get; set; }
    public int DeviceID { get; set; }
    public int TrustScore { get; set; } = 50;
    public string Status { get; set; } = "Untrusted";
    public DateTime LastEvaluated { get; set; } = DateTime.UtcNow;

    public Device? Device { get; set; }
}
