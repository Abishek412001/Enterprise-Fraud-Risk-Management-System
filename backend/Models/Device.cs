namespace EnterpriseFraudRiskSystem.Models;

public class Device
{
    public int DeviceID { get; set; }
    public int CustomerID { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string DeviceName { get; set; } = "Unknown Device";
    public string Browser { get; set; } = "Unknown Browser";
    public string OperatingSystem { get; set; } = "Unknown OS";
    public string IPAddress { get; set; } = string.Empty;
    public string Country { get; set; } = "Unknown";
    public string? City { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public bool IsTrusted { get; set; }
    public bool IsBlocked { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<CustomerSession> Sessions { get; set; } = new List<CustomerSession>();
}
