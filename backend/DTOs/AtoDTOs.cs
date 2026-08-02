namespace EnterpriseFraudRiskSystem.DTOs;

public class AtoAlertResponseDto
{
    public int ATOAlertID { get; set; }
    public string ATOAlertNumber { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? SessionID { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? AssignedAnalystID { get; set; }
    public string? AssignedAnalystName { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class AtoAlertDetailResponseDto : AtoAlertResponseDto
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }
    public DeviceDto? CurrentDevice { get; set; }
    public List<DeviceDto> PreviousDevices { get; set; } = new();
    public List<CustomerSessionDto> RecentSessions { get; set; } = new();
}

public class DeviceDto
{
    public int DeviceID { get; set; }
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsTrusted { get; set; }
    public bool IsBlocked { get; set; }
}

public class CustomerSessionDto
{
    public int SessionID { get; set; }
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? DeviceID { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string AuthenticationMethod { get; set; } = string.Empty;
    public string LoginStatus { get; set; } = string.Empty;
    public int RiskScore { get; set; }
}

public class RecordCustomerLoginDto
{
    public int CustomerID { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string Country { get; set; } = "USA";
    public string Browser { get; set; } = "Chrome 122.0";
    public string OperatingSystem { get; set; } = "Windows 11";
    public string LoginStatus { get; set; } = "Success";
    public bool IsTorVpn { get; set; }
}

public class AssignAtoAlertDto
{
    public int ATOAlertID { get; set; }
    public int AnalystID { get; set; }
}

public class CloseAtoAlertDto
{
    public int ATOAlertID { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class AtoSummaryStatsDto
{
    public int TotalAtoAlerts { get; set; }
    public int OpenAtoAlerts { get; set; }
    public int HighRiskLoginsToday { get; set; }
    public int FailedLoginsToday { get; set; }
    public int SuspiciousDevicesCount { get; set; }
}
