namespace EnterpriseFraudRiskSystem.DTOs;

public class SentinelAlertResponseDto
{
    public int AlertID { get; set; }
    public string AlertNumber { get; set; } = string.Empty;
    public string AlertName { get; set; } = string.Empty;
    public string AlertCategory { get; set; } = string.Empty;
    public string AlertSource { get; set; } = string.Empty;
    public string AlertRule { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? AssignedAnalystID { get; set; }
    public string? AssignedAnalystName { get; set; }
    public int? IncidentID { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class SentinelIncidentResponseDto
{
    public int IncidentID { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedAnalystID { get; set; }
    public string? AssignedAnalystName { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CorrelatedAlertsCount { get; set; }
}

public class SentinelIncidentDetailResponseDto : SentinelIncidentResponseDto
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public List<SentinelAlertResponseDto> CorrelatedAlerts { get; set; } = new();
    public List<SecurityEventDto> SecurityEvents { get; set; } = new();
    public List<ThreatIndicatorDto> MatchedThreatIndicators { get; set; } = new();
}

public class ThreatIndicatorDto
{
    public int IndicatorID { get; set; }
    public string IndicatorType { get; set; } = string.Empty;
    public string IndicatorValue { get; set; } = string.Empty;
    public string ThreatLevel { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class SecurityEventDto
{
    public int EventID { get; set; }
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public string Result { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
}

public class CreateSecurityEventDto
{
    public int CustomerID { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public int? DeviceID { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Result { get; set; } = "Success";
    public string Application { get; set; } = "EFRS Portal";
    public string OperatingSystem { get; set; } = "Windows 11";
}

public class AssignIncidentDto
{
    public int IncidentID { get; set; }
    public int AnalystID { get; set; }
}

public class CloseIncidentDto
{
    public int IncidentID { get; set; }
}

public class SentinelSummaryStatsDto
{
    public int OpenIncidentsCount { get; set; }
    public int CriticalIncidentsCount { get; set; }
    public int HighRiskDevicesCount { get; set; }
    public int ActiveThreatIndicatorsCount { get; set; }
    public int SecurityEventsTodayCount { get; set; }
}
