namespace EnterpriseFraudRiskSystem.Models;

public class SecurityEvent
{
    public int EventID { get; set; }
    public int CustomerID { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public int? DeviceID { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
    public string Result { get; set; } = "Success";
    public string Application { get; set; } = "EFRS Portal";
    public string OperatingSystem { get; set; } = "Windows 11";

    public Customer? Customer { get; set; }
    public Device? Device { get; set; }
}
