namespace EnterpriseFraudRiskSystem.DTOs;

public class CaseResponseDto
{
    public int CaseID { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public string? CaseDescription { get; set; }
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedAnalystID { get; set; }
    public string? AssignedAnalystName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DueDate { get; set; }
    public string SLAStatus { get; set; } = string.Empty;
    public int AlertsCount { get; set; }
    public int TransactionsCount { get; set; }
    public int AgeHours { get; set; }
}

public class CaseDetailResponseDto : CaseResponseDto
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime? ResolvedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? RootCause { get; set; }
    public string? Resolution { get; set; }
    public bool FalsePositive { get; set; }

    public List<CaseAlertDto> LinkedAlerts { get; set; } = new();
    public List<CaseTransactionDto> LinkedTransactions { get; set; } = new();
    public List<CaseNoteDto> Notes { get; set; } = new();
    public List<CaseTimelineDto> Timeline { get; set; } = new();
    public List<CaseAttachmentDto> Attachments { get; set; } = new();
    public List<CaseEscalationDto> Escalations { get; set; } = new();
}

public class CaseAlertDto
{
    public int CaseAlertID { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public int AlertID { get; set; }
    public DateTime LinkedDate { get; set; }
}

public class CaseTransactionDto
{
    public int CaseTransactionID { get; set; }
    public long TransactionID { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LinkedDate { get; set; }
}

public class CaseNoteDto
{
    public int NoteID { get; set; }
    public int AnalystID { get; set; }
    public string AnalystName { get; set; } = string.Empty;
    public string NoteType { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class CaseTimelineDto
{
    public int TimelineID { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? ActionBy { get; set; }
    public string? ActionByName { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}

public class CaseAttachmentDto
{
    public int AttachmentID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int UploadedBy { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
}

public class CaseEscalationDto
{
    public int EscalationID { get; set; }
    public int EscalatedTo { get; set; }
    public string EscalatedToName { get; set; } = string.Empty;
    public string EscalationReason { get; set; } = string.Empty;
    public DateTime EscalationDate { get; set; }
}

public class CreateCaseDto
{
    public string CaseType { get; set; } = "FraudInvestigation";
    public string CaseTitle { get; set; } = string.Empty;
    public string? CaseDescription { get; set; }
    public int CustomerID { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Severity { get; set; } = "Medium";
    public int? CreatedBy { get; set; }
}

public class AssignCaseDto
{
    public int CaseID { get; set; }
    public int AnalystID { get; set; }
    public int? AssignedBy { get; set; }
}

public class EscalateCaseDto
{
    public int CaseID { get; set; }
    public int EscalatedTo { get; set; }
    public string EscalationReason { get; set; } = string.Empty;
    public int? ActionBy { get; set; }
}

public class CloseCaseDto
{
    public int CaseID { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public bool FalsePositive { get; set; } = false;
    public int? ActionBy { get; set; }
}

public class AddCaseNoteDto
{
    public int CaseID { get; set; }
    public int AnalystID { get; set; }
    public string NoteType { get; set; } = "InvestigationNote";
    public string Comment { get; set; } = string.Empty;
}

public class AddCaseAttachmentDto
{
    public int CaseID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = "Document";
    public int UploadedBy { get; set; }
}

public class UpdateCaseStatusDto
{
    public int CaseID { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ActionBy { get; set; }
}

public class CaseSummaryStatsDto
{
    public int OpenCasesCount { get; set; }
    public int MyCasesCount { get; set; }
    public int CriticalCasesCount { get; set; }
    public int SlaBreachesCount { get; set; }
    public int EscalatedCasesCount { get; set; }
    public int ClosedTodayCount { get; set; }
    public double AverageResolutionHours { get; set; }
}
