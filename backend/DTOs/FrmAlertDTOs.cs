namespace EnterpriseFraudRiskSystem.DTOs;

public class CreateFrmAlertDto
{
    public int CustomerID { get; set; }
    public int AccountID { get; set; }
    public long? TransactionID { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string AlertCategory { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public int RiskScore { get; set; } = 50;
    public string? ResolutionNotes { get; set; }
}

public class AssignFrmAlertDto
{
    public int AlertID { get; set; }
    public int AnalystID { get; set; }
}

public class UpdateFrmAlertStatusDto
{
    public int AlertID { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? Comments { get; set; }
}

public class EscalateFrmAlertDto
{
    public int AlertID { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CloseFrmAlertDto
{
    public int AlertID { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class AddFrmAlertCommentDto
{
    public int AlertID { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class FrmAlertResponseDto
{
    public int AlertID { get; set; }
    public string AlertNumber { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int AccountID { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public long? TransactionID { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string AlertCategory { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public int? AssignedAnalystID { get; set; }
    public string? AssignedAnalystName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? Resolution { get; set; }
}

public class FrmAlertDetailResponseDto : FrmAlertResponseDto
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string NationalIdNumber { get; set; } = string.Empty;
    public decimal AccountBalance { get; set; }
    public string AccountStatus { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
    public List<CardSummaryDto> Cards { get; set; } = new();
    public List<TransactionSummaryDto> RecentTransactions { get; set; } = new();
    public List<AlertHistoryDto> History { get; set; } = new();
    public List<AlertCommentDto> Comments { get; set; } = new();
}

public class CardSummaryDto
{
    public int CardId { get; set; }
    public string CardNumberMasked { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
}

public class TransactionSummaryDto
{
    public long TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string MerchantName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionAt { get; set; }
}

public class AlertHistoryDto
{
    public int HistoryID { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? ActionByUsername { get; set; }
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AlertCommentDto
{
    public int CommentID { get; set; }
    public string AnalystUsername { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class FrmAlertSummaryStatsDto
{
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int AssignedAlerts { get; set; }
    public int ClosedAlerts { get; set; }
    public double AverageAlertAgeHours { get; set; }
}
