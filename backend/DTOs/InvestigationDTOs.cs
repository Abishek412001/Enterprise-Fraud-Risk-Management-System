namespace EnterpriseFraudRiskSystem.DTOs;

public class Customer360Dto
{
    public int CustomerID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string KycStatus { get; set; } = string.Empty;
    public string AmlRiskLevel { get; set; } = string.Empty;
    public bool IsFrozen { get; set; }
    public DateTime CustomerSince { get; set; }
    public int CurrentRiskScore { get; set; }
    public string RiskCategory { get; set; } = string.Empty;

    public int TotalAccounts { get; set; }
    public int TotalCards { get; set; }
    public int TotalTransactions { get; set; }
    public int FrmAlertsCount { get; set; }
    public int AtoAlertsCount { get; set; }
    public int SentinelAlertsCount { get; set; }
    public int OpenCasesCount { get; set; }
    public int RegisteredDevicesCount { get; set; }
}

public class InvestigationSessionDto
{
    public int SessionID { get; set; }
    public int CustomerID { get; set; }
    public int AnalystID { get; set; }
    public string AnalystName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SummaryNotes { get; set; }
}

public class AnalystActionDto
{
    public int ActionID { get; set; }
    public int CustomerID { get; set; }
    public int AnalystID { get; set; }
    public string AnalystName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}

public class FreezeAccountDto
{
    public int CustomerID { get; set; }
    public int AnalystID { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SuspendCardDto
{
    public int CardID { get; set; }
    public int AnalystID { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class DeviceActionDto
{
    public int DeviceID { get; set; }
    public int AnalystID { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class InvestigationSummaryStatsDto
{
    public int CustomersUnderInvestigationCount { get; set; }
    public int AccountsFrozenCount { get; set; }
    public int CardsSuspendedCount { get; set; }
    public int DevicesBlockedCount { get; set; }
    public int InvestigationsTodayCount { get; set; }
    public double AverageInvestigationMinutes { get; set; }
}
