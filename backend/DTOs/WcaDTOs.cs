namespace EnterpriseFraudRiskSystem.DTOs;

public class WcaInteractionDto
{
    public int InteractionID { get; set; }
    public int? CaseID { get; set; }
    public int? AlertID { get; set; }
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int AnalystID { get; set; }
    public string AnalystName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionCategory { get; set; } = string.Empty;
    public string ActionDescription { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string? StatusBefore { get; set; }
    public string? StatusAfter { get; set; }
    public DateTime Timestamp { get; set; }
}

public class RecordWcaInteractionDto
{
    public int? CaseID { get; set; }
    public int? AlertID { get; set; }
    public int CustomerID { get; set; }
    public int AnalystID { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ActionCategory { get; set; } = "InvestigationAction";
    public string ActionDescription { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string? StatusBefore { get; set; }
    public string? StatusAfter { get; set; }
}

public class PartnerCommunicationDto
{
    public int CommunicationID { get; set; }
    public int? CaseID { get; set; }
    public int PartnerID { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string CommunicationType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class SendCommunicationDto
{
    public int? CaseID { get; set; }
    public int PartnerID { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string CommunicationType { get; set; } = "InformationRequest";
    public string Channel { get; set; } = "Email";
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CommunicationTemplateDto
{
    public int TemplateID { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class PartnerDirectoryDto
{
    public int PartnerID { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EscalationContact { get; set; }
}

public class WcaSummaryStatsDto
{
    public int TodayWcaActionsCount { get; set; }
    public int PendingPartnerResponsesCount { get; set; }
    public int CommunicationsSentCount { get; set; }
    public int CommunicationsReceivedCount { get; set; }
    public double AveragePartnerResponseHours { get; set; }
}
