namespace EnterpriseFraudRiskSystem.Models;

public class CustomerSession
{
    public int SessionID { get; set; }
    public int CustomerID { get; set; }
    public int? DeviceID { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    public DateTime? LogoutTime { get; set; }
    public string Country { get; set; } = "Unknown";
    public string Browser { get; set; } = "Unknown";
    public string OperatingSystem { get; set; } = "Unknown";
    public string AuthenticationMethod { get; set; } = "Password";
    public string LoginStatus { get; set; } = "Success";
    public int RiskScore { get; set; }

    public Customer? Customer { get; set; }
    public Device? Device { get; set; }
}
