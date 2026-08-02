namespace EnterpriseFraudRiskSystem.Models;

public class CustomerRiskHistory
{
    public int HistoryID { get; set; }
    public int CustomerID { get; set; }
    public int OldRiskScore { get; set; }
    public int NewRiskScore { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public int? ChangedBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
    public User? ChangedByUser { get; set; }
}
